
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-10-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	partial class LogicalNotTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			Arg.GenerateCodeForValue ( context, purpose );
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Arg.GenerateCodeForValue ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( "NEG" );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}

		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Arg.GenerateCodeForConditionalBranch ( context, label, !reverse );
		}
	}

	partial class ShortCircutAndTreeNode
	{
		// NB: Short Circut Evaluation (Left && Right): if "Left" evaluates to false we must not evaluate "Right"

		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					// we have something like:  a && b;
					var joinPoint1 = context.CreateLabel ();
					Left.GenerateCodeForConditionalBranch ( context, joinPoint1, false );
					Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					context.PlaceLabelHere ( joinPoint1 );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					// we have an expression like (a && b) + 3 so we treat that like: ((a && b) ? 1 : 0) + 3;
					var zero = context.CreateLabel ();
					context.EvalBoth ( Left, Right, zero, false );
					context.GenerateInstruction ( "PUSH", "#1" );
					var joinPoint2 = context.CreateLabel ();
					context.GenerateUnconditionalBranch ( joinPoint2 );
					context.GenerateInstruction ( "PUSH", "#0" );
					context.PlaceLabelHere ( joinPoint2 );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}

		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel target, bool reverse )
		{
			if ( reverse )
				context.EvalEither ( Left, Right, target, false );
			else
				context.EvalBoth ( Left, Right, target, false );
		}
	}

	partial class ShortCircutOrTreeNode
	{
		// NB: Short Circut Evaluation (Left || Right): if "Left" evaluates to true we must not evaluate "Right"

		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					// we have something like:  a || b;
					var joinPoint1 = context.CreateLabel ();
					Left.GenerateCodeForConditionalBranch ( context, joinPoint1, true );
					Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					context.PlaceLabelHere ( joinPoint1 );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					// we have an expression like (a || b) + 3 so we treat that like: ((a || b) ? 1 : 0) + 3;
					var zero = context.CreateLabel ();
					context.EvalEither ( Left, Right, zero, false );
					context.GenerateInstruction ( "PUSH", "#1" );
					var joinPoint2 = context.CreateLabel ();
					context.GenerateUnconditionalBranch ( joinPoint2 );
					context.GenerateInstruction ( "PUSH", "#0" );
					context.PlaceLabelHere ( joinPoint2 );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}

		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel target, bool reverse )
		{
			if ( reverse )
				context.EvalBoth ( Left, Right, target, false );
			else
				context.EvalEither ( Left, Right, target, false );
		}
	}


}