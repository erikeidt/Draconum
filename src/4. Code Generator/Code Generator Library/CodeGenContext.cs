
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
		public enum OrderingRelation { LessThan, LessOrEqual, NotEqual, Equal, GreaterOrEqual, GreaterThan };

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

			string [] fwd = { "B.GE", "B.GT", "B.EQ", "B.NE", "B.LE", "B.LT" };
			_output.WriteLine ( "\t{0}\tL{1}", reverse ? fwd [ 5 - (int) type ] : fwd [ (int) type ], label.Id );
		}

		public void InsertComment ( string comment )
		{
			_output.WriteLine ( "#\t{0}", comment );
		}

		public void EvalEither ( AbstractSyntaxTree left, AbstractSyntaxTree right, BranchTargetLabel target, bool reverse )
		{
			var around = CreateLabel ();
			left.GenerateCodeForConditionalBranch ( this, around, !reverse );
			right.GenerateCodeForConditionalBranch ( this, target, reverse );
			PlaceLabelHere ( around );
		}

		public void EvalBoth ( AbstractSyntaxTree left, AbstractSyntaxTree right, BranchTargetLabel target, bool reverse )
		{
			left.GenerateCodeForConditionalBranch ( this, target, reverse );
			right.GenerateCodeForConditionalBranch ( this, target, reverse );
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
	}
}