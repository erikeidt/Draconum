
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
using System.ComponentModel.Design.Serialization;

// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum
{
	partial class EmptyStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			context.InsertComment ( "empty statement" );
			// do nothing!
		}
	}

	partial class DeclarationStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			context.InsertComment ( "declaration statement" );
			// do nothing?
		}
	}

	partial class ExpressionStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			Expression.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
		}
	}

	partial class BlockStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			for ( int i = 0 ; i < Statements.Count ; i++ )
			{
				var stmt = Statements [ i ];
				stmt.GenerateCodeWithPrettyPrint ( context );
			}
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
		protected override void GenerateCode ( CodeGenContext context )
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
				context.SetPrettyPrintProlog ( "If-Goto\t" );
				Condition.GenerateCodeForConditionalBranchWithPrettyPrint ( context, target.Value, true );
				Dump.WriteLine ( "<then part branch incorporated into preceding condition>" );

				context.SetPrettyPrintProlog ( "ElsePart\t" );
				ElsePart?.GenerateCodeWithPrettyPrint ( context );
			} else if ( ElsePart == null ) {
				context.SetPrettyPrintProlog ( "Condition\t" );
				var joinPoint = context.CreateLabel ();
				Condition.GenerateCodeForConditionalBranchWithPrettyPrint ( context, joinPoint, false );

				context.SetPrettyPrintProlog ( "ThenPart" );
				ThenPart.GenerateCodeWithPrettyPrint ( context );

				context.InsertComment ( "if-then rejoin" );
				context.PlaceLabelHere ( joinPoint );
			}
			/* else if ( ElsePart.IsBranchStatement () { }*/
			else
			{
				var elsePartLabel = context.CreateLabel ();
				context.SetPrettyPrintProlog ( "Condition\t" );

				Condition.GenerateCodeForConditionalBranchWithPrettyPrint ( context, elsePartLabel, false );
				context.SetPrettyPrintProlog ( "ThenPart\t" );
				ThenPart.GenerateCodeWithPrettyPrint ( context );

				var joinPoint = context.CreateLabel ();
				context.InsertComment ( "end of ThenPart" );
				context.GenerateUnconditionalBranch ( joinPoint );

				context.SetPrettyPrintProlog ( "ElsePart\t" );

				context.PlaceLabelHere ( elsePartLabel );

				context.InsertComment ( "start of ElsePart" );
				ElsePart.GenerateCodeWithPrettyPrint ( context );

				context.InsertComment ( "if-then-else rejoin" );
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
		protected override void GenerateCode ( CodeGenContext context )
		{
			context.SetPrettyPrintProlog ( initText );
			Initialization?.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );

			var loopTop = context.CreateLabel ();
			context.PlaceLabelHere ( loopTop );

			var cl = context.CreateLabel ();
			var bl = context.CreateLabel ();

			ContinueLabel = cl;
			BreakLabel = bl;

			context.SetPrettyPrintProlog ( condText );
			Condition?.GenerateCodeForConditionalBranchWithPrettyPrint ( context, bl, false );

			context.SetPrettyPrintProlog ( bodyText );
			Body.GenerateCodeWithPrettyPrint ( context );

			context.PlaceLabelHere ( cl );

			context.SetPrettyPrintProlog ( incrText );
			Increment?.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );

			if ( Body.EndsLive () )
				context.GenerateUnconditionalBranch ( loopTop );

			context.PlaceLabelHere ( bl );
		}
	}

	partial class ReturnStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			context.InsertComment ( "return statement" );
			ReturnValue?.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
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
		protected override void GenerateCode ( CodeGenContext context )
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
		protected override void GenerateCode ( CodeGenContext context )
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

		public BranchTargetLabel GetBranchTargetLabel ( CodeGenContext context )
		{
			if ( ! _label.HasValue )
				_label = context.CreateLabel ();
			return _label.Value;
		}
	}

	partial class GotoStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			var label = GotoTarget.GetBranchTargetLabel ( context );
			context.GenerateUnconditionalBranch ( label );
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class LabelStatement
	{
		protected override void GenerateCode ( CodeGenContext context )
		{
			var label = Label.GetBranchTargetLabel ( context );
			context.PlaceLabelHere ( label );
			Statement.GenerateCodeWithPrettyPrint ( context );
		}
	}
}