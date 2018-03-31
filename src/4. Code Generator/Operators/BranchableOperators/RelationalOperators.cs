
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
	partial class LessThanTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.LessThan, label, reverse );
		}
	}

	partial class LessOrEqualTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.LessOrEqual, label, reverse );
		}
	}

	partial class NotEqualTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.NotEqual, label, reverse );
		}
	}

	partial class EqualEqualTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.Equal, label, reverse );
		}
	}

	partial class GreaterOrEqualTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.GreaterOrEqual, label, reverse );
		}
	}

	partial class GreaterThanTreeNode
	{
		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalCompareAndBranch ( CodeGenContext.OrderingRelation.GreaterThan, label, reverse );
		}
	}
}