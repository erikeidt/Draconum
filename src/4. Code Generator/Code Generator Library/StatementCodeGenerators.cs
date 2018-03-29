
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

using System.IO;
using System.Collections.Generic;

// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum
{
	partial class EmptyStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			// do nothing!
		}
	}

	partial class DeclarationStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			// do nothing?
		}
	}

	partial class ExpressionStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			Expression.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
		}
	}

	partial class BlockStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			for ( int i = 0 ; i < Statements.Count ; i++ )
				Statements [ i ].GenerateCode ( context );
		}

		public override BranchTargetLabel? GetBranchTarget ()
		{
			// if the block statement is blank, then it does not provide target
			//	(if there is a larger control structure like if around the empty block statement as then part, it might)
			if ( Statements.Count == 0 )
				return null;
			// otherwise if the first 
			return Statements [ 0 ].GetBranchTarget ();
		}

		public override bool EndsLive ()
		{
			if ( Statements.Count == 0 )
				return true;
			// We're just checking the last statement in the block
			//	whereas we could also check for unreachable code inside a block...
			//		but at least we err on the conservative side.
			//		i.e. conservative == return true;
			return Statements [ Statements.Count - 1 ].EndsLive ();
		}
	}

	partial class IfStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			var target = ThenPart.GetBranchTarget ();
			if ( target.HasValue ) {
				//
				// Normally, we evaluate 
				//		if ( a ) { b = 1; }
				//	such that if "a" evaluates to false, then we branch around the then-part
				//		and if "a" evaluates to true, we fall-thru to the then-part
				// But when the then-part is itself a goto statement of some sort:
				//		if ( a ) { goto L; }
				//		if ( a ) { break; }
				//		if ( a ) { continue; }
				// Then instead of branching around a branch instruction,
				//	we reverse the evaluation condition and supply the control flow target:
				//	so that if "a" evaluates to true, then we branch (goto/break/continue),
				//		and if "a" evaluates to false, we don't branch
				//
				Condition.GenerateCodeForConditionalBranch ( context, target.Value, true );
				ElsePart?.GenerateCode ( context );
			} else if ( ElsePart == null ) {
				var joinPoint = context.CreateLabel ();
				Condition.GenerateCodeForConditionalBranch ( context, joinPoint, false );
				ThenPart.GenerateCode ( context );
				context.PlaceLabelHere ( joinPoint );
			}
			  /* else if ( ElsePart.IsBranchStatement () { }*/
			  else {
				var elsePartLabel = context.CreateLabel ();
				Condition.GenerateCodeForConditionalBranch ( context, elsePartLabel, false );
				ThenPart.GenerateCode ( context );
				var joinPoint = context.CreateLabel ();
				context.GenerateUnconditionalBranch ( joinPoint );
				context.PlaceLabelHere ( elsePartLabel );
				ElsePart.GenerateCode ( context );
				context.PlaceLabelHere ( joinPoint );
			}
		}
	}

	partial class BreakableStatement
	{
		public BranchTargetLabel? BreakLabel;
	}

	partial class ContinueableStatement
	{
		public BranchTargetLabel? ContinueLabel;
	}

	partial class ForStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			context.InsertComment ( "for statement" );

			Initialization?.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );

			var loopTop = context.CreateLabel ();
			context.PlaceLabelHere ( loopTop );

			var cl = context.CreateLabel ();
			var bl = context.CreateLabel ();

			ContinueLabel = cl;
			BreakLabel = bl;

			Condition?.GenerateCodeForConditionalBranch ( context, bl, false );
			Body.GenerateCode ( context );
			context.PlaceLabelHere ( cl );
			Increment?.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
			if ( Body.EndsLive () )
				context.GenerateUnconditionalBranch ( loopTop );

			context.PlaceLabelHere ( bl );
		}
	}

	partial class ReturnStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			ReturnValue.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateInstruction ( "RET" );
		}

		//	If there is a single sequence of exit epilog code
		//	We should use it as a return branch target, 
		//	but for now, at least, we don't know that for sure.
		//	Ultimately, the entry/epilog sequence depends somewhat on the code
		//		inside the methods, so don't necessarily know the answer at
		//			this point in code generation.
		//public override BranchTargetLabel GetBranchTarget ()
		//{
		//	if ( ReturnValue != null )
		//		return false;
		//	return ???
		//}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class BreakStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			if ( ! EnclosingStatement.BreakLabel.HasValue )
				throw new ParseException ( "break has no enclosing construct" ) ;
			context.GenerateUnconditionalBranch ( EnclosingStatement.BreakLabel.Value );
		}

		public override BranchTargetLabel? GetBranchTarget ()
		{
			var target = EnclosingStatement.BreakLabel;
			if ( ! target.HasValue )
				throw new AssertionFailedException ( "BreakLabel not set" );
			return target;
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class ContinueStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			if ( ! EnclosingStatement.ContinueLabel.HasValue )
				throw new ParseException ( "break has no enclosing construct" ) ;
			context.GenerateUnconditionalBranch ( EnclosingStatement.ContinueLabel.Value );
		}

		public override BranchTargetLabel? GetBranchTarget ()
		{
			var target = EnclosingStatement.ContinueLabel;
			if ( ! target.HasValue )
				throw new AssertionFailedException ( "ContinueLabel not set" );
			return target;
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class UserLabel
	{
		private BranchTargetLabel? _label;

		BranchTargetLabel GetBranchTargetLabel ( CodeGenContext context )
		{
			if ( ! _label.HasValue )
				_label = context.CreateLabel ();
			return _label.Value;
		}
	}

	partial class GotoStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			throw new System.NotImplementedException ();
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class LabelStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			throw new System.NotImplementedException ();
		}
	}
}