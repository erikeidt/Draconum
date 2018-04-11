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
					context.PlaceLabelHere ( zero );
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
				context.EvalEither ( Left, Right, target, true );
			else
				context.EvalBoth ( Left, Right, target, false );
		}
	}
}