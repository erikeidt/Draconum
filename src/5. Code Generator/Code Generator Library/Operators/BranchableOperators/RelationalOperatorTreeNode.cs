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
    partial class RelationalOperatorTreeNode
    {
	    public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
	    {
		    Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
		    Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
		    context.GenerateBinaryCompareConditionalBranch ( Op, label, reverse );
	    }
    }
}
