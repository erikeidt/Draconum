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

		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					// we have something like an expression statement:
					//		a && b;
					//	or perhaps function argument, like:
					//		f(&&b);
					//	This requires differentiated evaluation for left/first and right/second as follows:
					//		a's truth value determines whether b is executed or skipped (short-circuted).
					//			if a is false, then b is skipped; otherwise if a is true, b is evaluated.
					//		thus, we need to evaluate "a" for conditional branch, and to branch around "b"!
					//		however, b does not affect anything further (and we don't need a final value for (a&&b) either).
					//		Thus, b is evaluated for side effects only.
					var joinPoint1 = context.CreateLabel ();
					Left.GenerateCodeForConditionalBranchWithPrettyPrint ( context, joinPoint1, false );
					Right.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
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

		protected override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel target, bool reverse )
		{
			// NB: reverse=false is the normal case as would be used in:
			//			if ( a && b ) { c=1; }
			//		where we branch around the then-part ( {c=1; } ) when the sub-expression a && b is false
			//	
			//	Reverse=true would apply in situation such as:
			//			if ( ! (a && b) ) { c=1; }
			//		where we would apply reverse=true toward the inner evaluation of (a || b)
			//	or many other cases involving reversal of evaluation purpose due to the && expression's larger context.
			if ( reverse )
				context.EvalEither ( Left, Right, target, true );
			else
				context.EvalBoth ( Left, Right, target, false );
		}
	}
}