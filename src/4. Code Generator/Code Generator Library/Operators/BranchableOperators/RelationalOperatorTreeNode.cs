using System;
using System.Collections.Generic;
using System.Text;

namespace com.erikeidt.Draconum
{
    partial class RelationalOperatorTreeNode
    {
	    public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
	    {
		    Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
		    Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
		    context.GenerateConditionalCompareAndBranch ( Op, label, reverse );
	    }
    }
}
