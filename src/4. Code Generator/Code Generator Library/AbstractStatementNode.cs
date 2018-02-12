
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
	partial class AbstractStatementNode
	{
		/// <summary>
		///		Generates code to a context for the give statement
		/// </summary>
		/// <param name="context">
		///		standard CodeGenContext
		/// </param>
		public abstract void GenerateCode ( CodeGenContext context );

		public virtual BranchTargetLabel GetBranchTarget ()
		{
			return null;
		}

		public virtual bool EndsLive ()
		{
			return true;
		}
	}
}