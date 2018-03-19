
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
	/// <summary>
	/// 
	/// </summary>
	partial class AbstractStatementNode
	{
		/// <summary>
		///		Generates code for a statement, into a context.
		/// </summary>
		/// <param name="context">
		///		a standard CodeGenContext that receives the generated code.
		/// </param>
		public abstract void GenerateCode ( CodeGenContext context );

		/// <summary>
		///		Certain statements are like unconditional branches: break, continue, goto.
		/// 
		///		We could handle them just like any other code, but this results in some inefficient code generation: 
		///			conditional branches that branch around unconditional branches,
		///				-instead of-
		///			(reversed) conditional branches to the right target in the first place.
		/// </summary>
		/// <returns>
		///		The branch target, if this statement is such a kind, or, null if not a branching statement.
		/// </returns>
		public virtual BranchTargetLabel GetBranchTarget ()
		{
			return null;
		}

		/// <summary>
		///		Most statements fall thru to other statements, but not all do:
		///			break, continue, goto, return
		///		do not fall through to the next statement.
		/// 
		///		Other constructs require some clean up code:
		///			if ( a ) { then-part; } else { else-part }
		///		here, the then-part would normally be followed by an unconditional branch
		///		around the else-part.
		/// 
		///		However, in the case that the then-part ends with a branching statement
		///		then the clean up branch would be dead code and so we will omit it.
		/// 
		///		This method tells us whether clean-up code would be dead anyway.
		/// </summary>
		/// <returns>
		///		true	--> the statemnt falls through, so requires such clean-up
		///		false	--> the statement does not fall through, so does not require such clean-up
		/// </returns>
		public virtual bool EndsLive ()
		{
			return true;
		}
	}
}