/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace com.erikeidt.Draconum
{
	using static Operators;

	partial class CodeGenContext : IDisposable
	{
		private readonly List<bool> _labelList = new List<bool> ();
		private readonly System.IO.TextWriter _output;
		private int _nextLabelId;
		private bool _isDisposed;
		private string _prettyPrintProlog;

		public CodeGenContext ( TextWriter output )
		{
			_output = output;
		}

		public BranchTargetLabel CreateLabel ()
		{
			_labelList.Add ( false );
			return new BranchTargetLabel ( _nextLabelId++ );
		}

		public void PlaceLabelHere ( BranchTargetLabel label )
		{
			if ( _labelList [ label.Id ] )
				throw new AssertionFailedException ( "label already defined: L" + label.Id );
			_labelList [ label.Id ] = true;
			OutLine ( String.Format ( "L{0}:", label.Id ) );
		}

		public void GenerateInstruction ( string opcode )
		{
			OutLine ( String.Format ( "    {0}", opcode ) );
		}

		public void GenerateInstruction ( string opcode, string arg )
		{
			OutLine ( String.Format ( "    {0}{2}{1}", opcode, arg, _argumentPosition.Substring ( opcode.Length ) ) );
		}

		public void GenerateInstruction ( string opcode, string arg1, string arg2 )
		{
			OutLine ( String.Format ( "    {0}{3}{1}, {2}", opcode, arg1, arg2, _argumentPosition.Substring ( opcode.Length ) ) );
		}

		public void GenerateUnconditionalBranch ( BranchTargetLabel label )
		{
			var opcode = "JUMP";
			OutLine ( String.Format ( "    {0}{2}L{1}", opcode, label.Id, _argumentPosition.Substring ( opcode.Length ) ) );
		}

		public void GenerateUnaryTestConditionalBranch ( BranchTargetLabel label, bool reverse )
		{
			var opcode = reverse ? "B.TRUE" : "B.FALSE";
			OutLine ( String.Format ( "    {0}{2}L{1}", opcode, label.Id, _argumentPosition.Substring ( opcode.Length ) ) );
		}

		public void GenerateBinaryCompareConditionalBranch ( Operator op, BranchTargetLabel label, bool reverse )
		{
			//Handle the Relational Operators: LessThan,LessOrEqual,GreaterThan,GreaterOrEqual,EqualEqual,NotEqual,
			var branchOpcode = "";
			switch ( op )
			{
				case Operator.LessThan:
					branchOpcode = reverse ? "B.LT" : "B.GE";
					break;
				case Operator.LessOrEqual:
					branchOpcode = reverse ? "B.LE" : "B.GT";
					break;
				case Operator.GreaterThan:
					branchOpcode = reverse ? "B.GT" : "B.LE";
					break;
				case Operator.GreaterOrEqual:
					branchOpcode = reverse ? "B.GE" : "B.LT";
					break;
				case Operator.EqualEqual:
					branchOpcode = reverse ? "B.EQ" : "B.NE";
					break;
				case Operator.NotEqual:
					branchOpcode = reverse ? "B.NE" : "B.EQ";
					break;
				default:
					throw new AssertionFailedException ( "Operator is not one of the relational operators: " + op.ToString () );
			}

			OutLine ( String.Format ( "    {0}{2}L{1}", branchOpcode, label.Id, _argumentPosition.Substring ( branchOpcode.Length ) ) );
		}

		public void InsertComment ( string comment )
		{
			OutLine ( String.Format ( "# {0}", comment ) );
		}

		public void GenerateVariableDeclaration ( VariableTreeNode node )
		{
			var opcode = ".decl";
			OutLine ( String.Format ( "    {0}{2}{1}", opcode, node.Value, _argumentPosition.Substring ( opcode.Length ) ) );
		}

		/// <summary>
		///		Test two conditions, and conditionally branch vs. fall through: 
		///			either branch to target or fall through to subsequent code;
		///			conditions are or'ed together: either one constitutes "true".
		///		Another way of thinking of this is condition disjunction (i.e. either).
		/// </summary>
		/// <param name="left">
		///		Initial/left test condition.
		/// </param>
		/// <param name="right">
		///		Initial/right test condition -- possibly short-circuted.
		/// </param>
		/// <param name="target">
		///		Conditional branch target (or else fall thru).
		/// </param>
		/// <param name="reverse">
		///		false	--> normal case, fall through to following code on at least one true, otherwise on false: branch to target
		///		true	--> reversed case, fall through both false, otherwise on true: branch to target
		/// </param>
		public void EvalEither ( AbstractSyntaxTree left, AbstractSyntaxTree right, BranchTargetLabel target, bool reverse )
		{
			// NB: "Either" means evaluating left/first operand, and on true, taking branch around the right/second operand
			//		and into the fall thru code of the then-part or loop-body!
			//	This requires use of an intermediate label, and a reversal of the evaluation condition, but only for the left/first operand.
			//	The second operand is evaluated as per normal, take brach around the then-part or loop-body on condition false.
			var around = CreateLabel ();
			left.GenerateCodeForConditionalBranchWithPrettyPrint ( this, around, !reverse );
			right.GenerateCodeForConditionalBranchWithPrettyPrint ( this, target, reverse );
			PlaceLabelHere ( around );
		}

		/// <summary>
		///		Test two conditions, and conditionally branch vs. fall through: 
		///			either branch to target or fall through to subsequent code;
		///			conditions are and'ed together: both together constitute "true".
		///		Another way of thinking of this is condition conjunction (both).
		/// </summary>
		/// <param name="left">
		///		Initial/left test condition.
		/// </param>
		/// <param name="right">
		///		Initial/right test condition -- possibly short-circuted.
		/// </param>
		/// <param name="target">
		///		Conditional branch target (or else fall thru).
		/// </param>
		/// <param name="reverse">
		///		false	--> normal case, fall through on both true, otherwise on false: branch to target
		///		true	--> reversed case, fall through at least one false, otherwise on true: branch to target
		/// </param>
		public void EvalBoth ( AbstractSyntaxTree left, AbstractSyntaxTree right, BranchTargetLabel target, bool reverse )
		{
			// NB: "Both" means evaluating left/first and right/second operands in the normal way.
			//	Each operand is evaluated to branch around the then-part or loop-body on condition false,
			//		and fall thru into the then-part or loop-body on condition true (both sub conditions being true).
			left.GenerateCodeForConditionalBranchWithPrettyPrint ( this, target, reverse );
			right.GenerateCodeForConditionalBranchWithPrettyPrint ( this, target, reverse );
		}

		/// <summary>
		///		For evalutating function call arguments.
		/// 
		///		NB: function calls have 0 or more arguments, but they are all stored in a binary tree-type structure.
		/// 
		///		There is a special operator, ArgumentSeparatorTreeNode, for that.
		///		These will only appear (at the top or) as the left operand of itself.
		/// 
		///		They are evaluated like the comma operator except they stack the operands for the invocation
		///			whereas the regular comma operator ignores left-hand side results.
		/// </summary>
		/// <param name="arg">
		///		AST to evalutate as parameter list
		/// </param>
		/// <returns>
		///		Count of arguments passed to function, for processing of the stack frame.
		/// </returns>
		public int EvaluateArgumentList ( AbstractSyntaxTree arg )
		{
			if ( arg == null )
			{
				Dump.WriteLine ( "<empty>" );
				return 0;
			}

			if ( arg is ArgumentSeparatorTreeNode comma )
			{
				var count = EvaluateArgumentList ( comma.Left );
				comma.Right.GenerateCodeForValueWithPrettyPrint ( this, EvaluationIntention.Value );
				return count + 1;
			}

			arg.GenerateCodeForValueWithPrettyPrint ( this, EvaluationIntention.Value );
			return 1;
		}

		public void SetPrettyPrintProlog ( string prolog )
		{
			_prettyPrintProlog = prolog;
		}

		public void PrettyPrint ( Dump node )
		{
			node.PrettyPrintHeader ( _prettyPrintProlog );
			_prettyPrintProlog = "";
		}

		private string _argumentPosition = "            ";

		private void OutLine ( string arg )
		{
#if false
			// for tree on the left and assembly code on the right
			_output.WriteLine ( String.Format ( "{0}{1}","                                       ", arg ) );
#else
			// for code on left and tree on the right
			var pad = Dump.LastIntro == null ? "" : Dump.LastIntro.Substring ( arg.Length );
			_output.WriteLine ( String.Format ( "{0}{1}", arg, pad ) );
#endif
		}

		public void Dispose ()
		{
			if ( _isDisposed )
				return;
			_isDisposed = true;
			for ( var i = 0 ; i < _labelList.Count ; i++ )
				if ( !_labelList [ i ] )
					throw new AssertionFailedException ( "label not defined: L" + i );
		}
	}
}