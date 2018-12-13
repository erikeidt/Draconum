/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 11-16-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

using System.Collections.Generic;
using System.IO;

// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum
{
	partial interface ISymbolTable
	{
		VariableTreeNode LookupSymbol ( ByteString symbolName );
	}
}
