using System;
using System.Collections.Generic;
using System.Text;

namespace com.erikeidt.Draconum
{
	partial class NotMuchOfASymbolTable : ISymbolTable
	{
		public NotMuchOfASymbolTable ()
		{
		}



		public VariableTreeNode LookupSymbol ( ByteString symbolName )
		{
			return new VariableTreeNode ( symbolName );
		}
	}
}
