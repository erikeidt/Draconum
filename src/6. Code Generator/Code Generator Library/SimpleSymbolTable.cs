
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace com.erikeidt.Draconum.Code_Generator_Library
{
	partial class Symbol
	{
		public readonly int Scope;
		public readonly VariableTreeNode TreeNode;

		public Symbol ( int scope, VariableTreeNode node )
		{
			Scope = scope;
			TreeNode = node ?? throw new ArgumentNullException ( nameof ( node ) );
		}
	}

	partial class SimpleSymbolTable : ISymbolTable
	{
		private Dictionary < ByteString, Symbol > _variables = new Dictionary < ByteString, Symbol > ();
		private List < Symbol > _symbolsInScope = new List < Symbol > ();
		private List < VariableTreeNode > _symbolsOutOfScope = new List < VariableTreeNode > ( );
		private int _scopeLevel;

		public void EnterScope ()
		{
			_scopeLevel++;
		}

		public void ExitScope ()
		{
			_scopeLevel--;
			for ( ;; )
			{
				int count = _symbolsInScope.Count;
				if ( count == 0 )
					break;
				count--;
				var sym = _symbolsInScope [ count ];
				if ( sym.Scope <= _scopeLevel )
					break;
				_symbolsInScope.RemoveAt ( count );
				var node = sym.TreeNode;
				_variables.Remove ( node.Value );
				_symbolsOutOfScope.Add ( node );
			}
		}

		public VariableTreeNode AddSymbol (  ByteString symbolName )
		{
			throw new NotImplementedException();
		}

		private Symbol AddSymbol ( ByteString symbolName, bool check )
		{
			var found = new Symbol ( _scopeLevel, new VariableTreeNode ( symbolName ) );
			_variables [ symbolName ] = found;
			_symbolsInScope.Add ( found );
			return found;
		}

		public VariableTreeNode LookupSymbol ( ByteString symbolName )
		{
			_variables.TryGetValue ( symbolName, out var found );
			if ( found == null )
				found = AddSymbol ( symbolName, false );
			return found.TreeNode;
		}

		public void GenerateVariables ( CodeGenContext context )
		{
			ExitScope ();
			for ( int i = _symbolsOutOfScope.Count; --i >= 0 ; )
				context.GenerateVariableDeclaration ( _symbolsOutOfScope [ i ] );
		}
	}
}
