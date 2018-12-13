using System;
using System.Collections.Generic;
using System.Text;

namespace com.erikeidt.Draconum
{
	partial interface ISymbolTable
	{
		void EnterScope ();
		void ExitScope ();
	}
}
