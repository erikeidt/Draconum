
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
	///		A simple class that holds the identity of a label, as in an assembly language label or branch target.
	/// </summary>
	struct BranchTargetLabel
	{
		public readonly int Id;

		public BranchTargetLabel ( int id )
		{
			Id = id;
		}
	}
}