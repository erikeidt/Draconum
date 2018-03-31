
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
	class CodeGenContext : IDisposable
	{
		public enum OrderingRelation { LessThan, LessOrEqual, NotEqual, Equal, GreaterThan, GreaterOrEqual };

		// normal is reverse=false -- this means branching on the false condition, e.g. if condition branching around the then part
		private string [] normalReverseFalse = { "B.GE", "B.GT", "B.EQ", "B.NE", "B.LE", "B.LT" };

		// opposite is reverse=true; and this means branching on the said condition
		private string [] oppositeReverseTrue = { "B.LT", "B.LE", "B.NE", "B.EQ", "B.GT", "B.GE" };

		private readonly List<bool> _labelList = new List<bool> ();
		private readonly System.IO.TextWriter _output;
		private int _nextLabelId;
		private bool _isDisposed;

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
			_output.WriteLine ( "L{0}:", label.Id );
		}

		public void GenerateInstruction ( string opcode )
		{
			_output.WriteLine ( "\t{0}", opcode );
		}

		public void GenerateInstruction ( string opcode, string arg )
		{
			_output.WriteLine ( "\t{0}\t{1}", opcode, arg );
		}

		public void GenerateInstruction ( string opcode, string arg1, string arg2 )
		{
			_output.WriteLine ( "\t{0}\t{1},{2}", opcode, arg1, arg2 );
		}

		public void GenerateUnconditionalBranch ( BranchTargetLabel label )
		{
			_output.WriteLine ( "\t{0}\tL{1}", "JUMP", label.Id );
		}

		public void GenerateConditionalBranch ( BranchTargetLabel label, bool reverse )
		{
			_output.WriteLine ( "\t{0}\tL{1}", reverse ? "B.TRUE" : "B.FALSE", label.Id );
		}

		public void GenerateConditionalCompareAndBranch ( OrderingRelation type, BranchTargetLabel label, bool reverse )
		{
			_output.WriteLine ( "\t{0}\tL{1}", reverse ? oppositeReverseTrue [ (int) type ] : normalReverseFalse [ (int) type ], label.Id );
		}

		public void InsertComment ( string comment )
		{
			_output.WriteLine ( "#\t{0}", comment );
		}

		/// <summary>
		///		Test two conditions, and conditionally branch vs. fall through: 
		///			either branch to target or fall through to subsequent code;
		///			conditions are or'ed together: either one constitutes "true".
		///		Another way of thinking of this is condition disjunction (either).
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
		///		false	--> normal case, fall through on at least one true, otherwise on false: branch to target
		///		true	--> reversed case, fall through both false, otherwise on true: branch to target
		/// </param>
		public void EvalEither ( AbstractSyntaxTree left, AbstractSyntaxTree right, BranchTargetLabel target, bool reverse )
		{
			var around = CreateLabel ();
			left.GenerateCodeForConditionalBranch ( this, around, !reverse );
			right.GenerateCodeForConditionalBranch ( this, target, reverse );
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
			left.GenerateCodeForConditionalBranch ( this, target, reverse );
			right.GenerateCodeForConditionalBranch ( this, target, reverse );
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
				return 0;

			if ( arg is ArgumentSeparatorTreeNode comma ) {
				var count = EvaluateArgumentList ( comma.Left );
				comma.Right.GenerateCodeForValue ( this, EvaluationIntention.Value );
				return count + 1;
			}

			arg.GenerateCodeForValue ( this, EvaluationIntention.Value );
			return 1;
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