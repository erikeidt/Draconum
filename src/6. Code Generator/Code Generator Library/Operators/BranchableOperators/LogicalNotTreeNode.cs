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
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			Arg.GenerateCodeForValueWithPrettyPrint ( context, purpose );
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( "NEG" );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}

		protected override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Arg.GenerateCodeForConditionalBranchWithPrettyPrint ( context, label, !reverse );
		}
	}
}