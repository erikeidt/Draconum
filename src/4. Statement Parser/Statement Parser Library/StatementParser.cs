
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-08-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

using System;
using System.Collections.Generic;

namespace com.erikeidt.Draconum
{
	class StatementParser
	{
		private static readonly ByteString KeywordIf = new ByteString ( "if" );
		private static readonly ByteString KeywordElse = new ByteString ( "else" );
		private static readonly ByteString KeywordFor = new ByteString ( "for" );
		private static readonly ByteString KeywordWhile = new ByteString ( "while" );
		private static readonly ByteString KeywordReturn = new ByteString ( "return" );
		private static readonly ByteString KeywordBreak = new ByteString ( "break" );
		private static readonly ByteString KeywordContinue = new ByteString ( "continue" );
		private static readonly ByteString KeywordGoto = new ByteString ( "goto" );

		private readonly ScanIt _scanner;
		private readonly ExpressionParser _expressionParser;
		private readonly ISymbolTable _symbolTable;

		private readonly Stack<BreakableStatement> _breakables = new Stack<BreakableStatement> ();
		private readonly Stack<ContinueableStatement> _continueables = new Stack<ContinueableStatement> ();

		public StatementParser ( ScanIt scanner, ISymbolTable symbolTable )
		{
			_scanner = scanner;
			_expressionParser = new ExpressionParser ( scanner, symbolTable );
			_symbolTable = symbolTable;
		}

		public ResultOrErrorList<AbstractStatementNode> TryParse ()
		{
			try {
				return new ResultOrErrorList<AbstractStatementNode> ( ParseStatement () );
			} catch ( CompilationException ce ) {
				return new ResultOrErrorList<AbstractStatementNode> ( new List<CompilationException> () { ce } );
			}
		}

		private AbstractStatementNode ParseStatement ()
		{
			for ( ; ; )
			{
				switch ( _scanner.Token ().Value ) {

				case '{':
					return ParseBlockStatement ();

				case 'A':
					if ( _scanner.HaveIdentifier ( KeywordIf ) )
						return ParseIfStatement ();
					else if ( _scanner.HaveIdentifier ( KeywordFor ) )
						return ParseForStatement ();
					else if ( _scanner.HaveIdentifier ( KeywordReturn ) )
						return ParseReturnStatement ();
					else if ( _scanner.HaveIdentifier ( KeywordBreak ) )
						return ParseBreakStatement ();
					else if ( _scanner.HaveIdentifier ( KeywordContinue ) )
						return ParseContinueStatement ();
					else if ( _scanner.HaveIdentifier ( KeywordGoto ) )
						return ParseGotoStatement ();

					// look for labeled statement as in label:
					// 
					var ans = _expressionParser.Parse ( terminatingCharacter: new CodePoint ( (byte) ':' ) );
					// this a pattern that admits
					//		Label:
					//	if the pattern doesn't fit, then the ExpectToken (';') just below will issue an error
					if ( ans is VariableTreeNode label && _scanner.IfToken ( ':' ) ) {
						if ( _scanner.IfTokenNoAdvance ( '}' ) )
						{
							// the following it is not legal or really acceptable:
							//		{ if ( cond ) L1: }
							//	so some grammars disallow
							//		L1 : }
							//	requiring input like this instead to resolve issues
							//		L1: ; }
							// however, when a label is used at the end of the block that contains it, e.g.
							//		abc; ... L1 : }
							//	this seems like an acceptable construct, and indeed some languages/grammars allow this.
							return new LabelStatement ( new UserLabel ( label.Value ), null );
						}
						var stmt = ParseStatement ();
						return new LabelStatement ( new UserLabel ( label.Value ), stmt );
					}

					_scanner.ExpectToken ( ';' );
					return new ExpressionStatement ( ans );

				case ';':
					_scanner.Advance ();
					return new EmptyStatement ();

				case CodePoint.EofValue:
					_scanner.Error ( "expected statement" );
					break;

				default:
					return ParseExpressionStatement ();
				}
			}
		}

		public AbstractStatementNode ParseExpressionStatement ()
		{
			// we could use Parse ( ';' ) but it is not necessary as ';' is not a valid operator token
			//	so the expression parser will stop on that character anyway
			var expr = _expressionParser.Parse ();
			_scanner.ExpectToken ( ';' );
			return new ExpressionStatement ( expr );
		}

		private AbstractStatementNode ParseBlockStatement ()
		{
			_symbolTable.EnterScope ();
			var list = new List<AbstractStatementNode> ();

			_scanner.ExpectToken ( '{' ); // "{ expected for block statement" )

			for ( ; ; )
			{
				if ( _scanner.IfToken ( '}' ) )
				{
					_symbolTable.ExitScope ();
					return new BlockStatement ( list );
				}

				var stmt = ParseStatement ();
				list.Add ( stmt );
			}
		}

		private AbstractStatementNode ParseIfStatement ()
		{
			_scanner.ExpectToken ( '(' );
			var condition = _expressionParser.Parse ( new CodePoint ( (byte) ')' ) );
			_scanner.ExpectToken ( ')' );

			var thenPart = ParseStatement ();

			AbstractStatementNode elsePart = null;
			if ( _scanner.HaveIdentifier ( KeywordElse ) ) {
				elsePart = ParseStatement ();
			}

			return new IfStatement ( condition, thenPart, elsePart );
		}

		private AbstractStatementNode ParseForStatement ()
		{
			_scanner.ExpectToken ( '(' );

			AbstractSyntaxTree initializer = null;
			AbstractSyntaxTree condition = null;
			AbstractSyntaxTree increment = null;

			if ( !_scanner.IfToken ( ';' ) ) {
				initializer = _expressionParser.Parse ();
				_scanner.ExpectToken ( ';' );
			}

			if ( !_scanner.IfToken ( ';' ) ) {
				condition = _expressionParser.Parse ();
				_scanner.ExpectToken ( ';' );
			}

			if ( !_scanner.IfToken ( ')' ) ) {
				increment = _expressionParser.Parse ( new CodePoint ( (byte) ')' ) );
				_scanner.ExpectToken ( ')' );
			}


			var forLoop = new ForStatement ( initializer, condition, increment );

			_breakables.Push ( forLoop );
			_continueables.Push ( forLoop );

			var body = ParseStatement ();

			_breakables.Pop ();
			_continueables.Pop ();

			forLoop.Body = body;
			return forLoop;
		}

		private AbstractStatementNode ParseBreakStatement ()
		{
			_scanner.ExpectToken ( ';' );
			if ( _breakables.Count == 0 )
				throw new CompilationException ( "break not nested within a break'able statement" );

			var breakable = _breakables.Peek ();
			breakable.HasBreak = true;
			return new BreakStatement ( breakable );
		}

		private AbstractStatementNode ParseContinueStatement ()
		{
			_scanner.ExpectToken ( ';' );
			if ( _continueables.Count == 0 )
				throw new CompilationException ( "break not nested within a break'able statement" );

			var continueable = _continueables.Peek ();
			continueable.HasContinue = true;
			return new ContinueStatement ( continueable );
		}

		private AbstractStatementNode ParseReturnStatement ()
		{
			if ( _scanner.IfToken ( ';' ) )
				return new ReturnStatement ( null );
			var expr = _expressionParser.Parse ();
			_scanner.ExpectToken ( ';' );
			return new ReturnStatement ( expr );
		}

		private AbstractStatementNode ParseGotoStatement ()
		{
			var id = _scanner.ExpectIdentifier ();
			_scanner.ExpectToken ( ';' );
			return new GotoStatement ( new UserLabel ( id ) );
		}
	}
}