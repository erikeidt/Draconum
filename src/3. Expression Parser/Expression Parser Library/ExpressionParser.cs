
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 01-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */


using System.Collections.Generic;
using System.Linq;

namespace com.erikeidt.Draconum
{
	using Exceptions = Draconum;
	using static Operators;

	partial class ExpressionParser
	{
		private readonly Stack<Operator> _operatorStack = new Stack<Operator> ();
		private readonly Stack<AbstractSyntaxTree> _operandStack = new Stack<AbstractSyntaxTree> ();
		private readonly ScanIt _scanner;

		public ExpressionParser ( ScanIt scanner )
		{
			_scanner = scanner;
		}

		/// <summary>
		///		Attempts to parse.  Does not throw on error.
		/// </summary>
		/// <returns>
		///		Either the result or a list of errors
		///		(Could/should return list of warnings with results as well.)
		/// </returns>
		public ResultOrErrorList<AbstractSyntaxTree> TryParse ()
		{
			return TryParse ( CodePoint.Eof );
		}

		/// <summary>
		///		Attempts to parse.  Does not throw on error.
		/// </summary>
		/// <param name="terminatingCharacter">
		///		Stops parsing if it sees this CodePoint in an unnested context.
		/// </param>
		/// <returns>
		///		Either the result or a list of errors
		///		(Could/should return list of warnings with results as well.)
		/// </returns>
		public ResultOrErrorList<AbstractSyntaxTree> TryParse ( CodePoint terminatingCharacter )
		{
			_operatorStack.Clear ();
			_operatorStack.Push ( Operator.None );  // make Peek () safe to do without first checking if empty
			_operandStack.Clear ();

			try {
				return new ResultOrErrorList<AbstractSyntaxTree> ( ParseToResult ( terminatingCharacter ) );
			} catch ( Exceptions.CompilationException p ) {
				return new ResultOrErrorList<AbstractSyntaxTree> ( new List<Exceptions.CompilationException> () { p } );
			}
		}

		/// <summary>
		///		Parses ok, or throws
		/// </summary>
		/// <returns>
		///		AbstractSyntaxTree representing parse, or throws
		/// </returns>
		public AbstractSyntaxTree Parse ()
		{
			return Parse ( CodePoint.Eof );
		}

		/// <summary>
		///		Parses ok, or throws
		/// </summary>
		/// <param name="terminatingCharacter">
		///		Stops parsing if it sees this CodePoint in an unnested context.
		/// </param>
		/// <returns>
		///		AbstractSyntaxTree representing parse, or throws
		/// </returns>
		public AbstractSyntaxTree Parse ( CodePoint terminatingCharacter )
		{
			_operatorStack.Clear ();
			_operatorStack.Push ( Operator.None ); // make Peek () safe to do without first checking if empty
			_operandStack.Clear ();
			return ParseToResult ( terminatingCharacter );
		}

		private AbstractSyntaxTree ParseToResult ( CodePoint terminatingCharacter )
		{
			ParseExpressionToStack ( terminatingCharacter );
			while ( _operatorStack.Peek () != Operator.None )
				ReduceTopOperator ();
			if ( _operandStack.Count != 1 )
				throw new AssertionFailedException ( "operand stack should have only 1 operand" );
			return _operandStack.Pop ();
		}

		private void ParseExpressionToStack ( CodePoint terminatingCharacter )
		{
			for ( ; ; )     // Alternate between the States
			{
				// initial State: Unary State
				for ( ; ; ) // Unary State
				{
					var token = _scanner.Token ();
					switch ( token.Value ) {
					case '(':
						_scanner.Advance ();
						_operatorStack.Push ( Operator
							.PrecedenceGroupingParenthesis ); // nothing to reduce, as '(' has highest precedence
						continue;

					case ')':
						_scanner.Advance ();
						if ( _operatorStack.Peek () != Operator.FunctionCall )
							_scanner.ErrorAtMark ( "unexpected ) in unary state" );
						_operatorStack.Pop ();
						// NB: We have a function call with no argument list, and we can either:
						//	provide a unary function call operator
						//		but our automated operator generators don't have that concept, or,
						//  push an empty operand onto the stack so there's two operands for it to work
						_operandStack.Push ( null );
						BuildAndPushOperator ( Operator.FunctionCall );
						break;

					case '!':
						_scanner.Advance ();
						_operatorStack.Push ( Operator.LogicalNot ); // prefix unary operators have implicit highest precedence
						continue;
					case '&':
						_scanner.Advance ();
						_operatorStack.Push ( Operator.AddressOf );
						continue;
					case '*':
						_scanner.Advance ();
						_operatorStack.Push ( Operator.Indirection );
						continue;
					case '+':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '+', Operator.PrefixIncrement, Operator.FixPoint );
						continue;
					case '-':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '-', Operator.PrefixDecrement, Operator.Negation );
						continue;
					case '~':
						_scanner.Advance ();
						_operatorStack.Push ( Operator.BitwiseComplement );
						continue;
					case 'A':
						var id = _scanner.GetIdentifier ();
						// we can look up the variable in a symbol table here
						_operandStack.Push ( new VariableTreeNode ( id ) );
						break;
					case '0':
						var num = _scanner.GetNonNegativeIntegralLiteral ();
						_operandStack.Push ( new LongIntegerTreeNode ( num ) );
						break;
					case '\"':
						var str = _scanner.GetStringLiteral ();
						_operandStack.Push ( new StringTreeNode ( str ) );
						break;
					case '\'':
						var chStr = _scanner.GetStringLiteral ();
						var cps = new CodePointStream ( chStr );
						var cp1 = cps.Read ();
						if ( cp1.AtEOF () )
							_scanner.ErrorAtMark ( "not enough characters in character literal" );
						var cp2 = cps.Read ();
						if ( !cp2.AtEOF () )
							_scanner.ErrorAtMark ( "too many characters in character literal" );
						_operandStack.Push ( new CharacterTreeNode ( cp1.Value ) );
						break;

					default:
						_scanner.ErrorAtMark ( "unrecognized token: " + (char) token.Value );
						return;
					}

					break; // switch to Binary State
				} // for (;;) { end of Unary State
				for ( ; ; ) // Binary State
				{
					var cp = _scanner.Token ();
					if ( cp.AtEOF () || cp == terminatingCharacter && AtTopLevelAndUnblocked () )
						return;

					switch ( cp.Value ) {
					case '(':
						_scanner.Advance ();
						ReduceThenPushOperator ( Operator.FunctionCall );
						break;
					case ',':
						_scanner.Advance ();
						AcceptComma ();
						break;
					case ')':
						_scanner.Advance ();
						AcceptCloseParen ();
						continue; // stay in Binary State

					case '[':
						_scanner.Advance ();
						ReduceThenPushOperator ( Operator.Subscript );
						break;
					case ']':
						_scanner.Advance ();
						// we have a [ b ], so we reduce until we reach the '['
						ReduceUntilMatch ( Operator.Subscript );
						// reduction pops the operator, and, leaves us with a & b on the operand stack
						BuildAndPushOperator ( Operator.Subscript );
						continue; // stay in Binary State

					case '!':
						_scanner.Advance ();
						if ( !_scanner.IfCharacter ( '=' ) )
							_scanner.ErrorAtMark ( "expected = for !=" );
						ReduceThenPushOperator ( Operator.NotEqual );
						break;

					case '*':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '=', Operator.AssignmentMultiplication, Operator.Multiplication );
						break;
					case '/':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '=', Operator.AssignmentDivision, Operator.Division );
						break;
					case '%':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '=', Operator.AssignmentModulo, Operator.Modulo );
						break;
					case '=':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '=', Operator.EqualEqual, Operator.Assignment );
						break;
					case '^':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '=', Operator.AssignmentBitwiseXor, Operator.BitwiseXor );
						break;

					case '+':
						_scanner.Advance ();
						if ( _scanner.IfCharacter ( '+' ) ) {
							// C/C++ postfix ++
							ReduceThenPushOperator ( Operator.PostfixIncrement );
							continue; // stay in Binary State
						}

						ReduceAndPushByChoice ( '=', Operator.AssignmentAddition, Operator.Addition );
						break;
					case '-':
						_scanner.Advance ();
						if ( _scanner.IfCharacter ( '>' ) ) {
							// at least ->
							if ( _scanner.IfCharacter ( '*' ) ) {
								// C++: member pointer: ->*
								// ToDo: parse rhs of ->* (which is a member name) and generate an binary node to push on the operand stack
								throw new System.NotImplementedException ( "->*" );
							} else {
								// C/C++ indirect selection: ->
								var memberName = _scanner.ExpectIdentifier ( "member identifier" );
								// ToDo: parse rhs of -> (which is a member name) and generate an operand to push on
								throw new System.NotImplementedException ( "->" );
							}
						} else {
							ReduceAndPushByChoice ( '-', Operator.PostfixDecrement, '=', Operator.AssignmentSubtraction,
								Operator.Subtraction );
						}

						break;

					case '&':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '&', Operator.ShortCircutAnd, '=', Operator.AssignmentBitwiseAnd, Operator.BitwiseAnd );
						break;
					case '|':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '|', Operator.ShortCircutOr, '=', Operator.AssignmentBitwiseOr, Operator.BitwiseOr );
						break;

					case '.':
						throw new System.NotImplementedException ( ". operator" );

					case '?':
						_scanner.Advance ();
						// Operator.TernaryTest is used for grouping of the expressions during parsing,
						//	This operator won't appear in the final tree (like Operator.GroupingParen)
						ReduceThenPushOperator ( Operator.TernaryTest );
						//// NB: we fake an open paren here:
						////	this is necessary to handle: a?b,c:d, which should parse ok as:
						////			a?(b,c):d 
						////			   -not-
						////			(a?b),(c:d)
						////			   as this makes no sense and thus is not helpful
						//_operatorStack.Push ( Operator.None );
						break;
					case ':':
						_scanner.Advance ();
						if ( _scanner.IfCharacter ( ':' ) ) {
							throw new System.NotImplementedException ( ":: operator" );
						} else {
							//// NB: this close ')' is the dual to the fake '(' inserted by '?'
							//ReduceUntilMatch ( Operator.None );
							// We reduce.  We're looing for Operator.TernaryTest 
							//	similar to how ']' reduces until we find the matching '['
							ReduceUntilMatch ( Operator.TernaryTest );
							// this will leave two operands on the stack, so for
							//		a ? b :			-- which is what we have so far
							//	a and b will be on the operand stack.
							// now we push the true ternary operator:
							_operatorStack.Push ( Operator.TernaryChoice );
							// when it gets reduced later, so for
							//		a ? b : c;
							// reducing this operator will consume a, b, and c.
						}

						break;

					case '<':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '<', '=', Operator.AssignmentBitwiseLeftShift, Operator.BitwiseLeftShift, '=',
							Operator.LessOrEqual, Operator.LessThan );
						break;
					case '>':
						_scanner.Advance ();
						ReduceAndPushByChoice ( '>', '=', Operator.AssignmentBitwiseRightShift, Operator.BitwiseRightShift, '=',
							Operator.GreaterOrEqual, Operator.GreaterThan );
						break;

					default:
						return; // let caller deal with the "unexpected" input, like ;
					}

					break; // switch to Unary State
				} // end of for (;;) { Binary State
			}   // end of for(;;) { Alternating between the states
		}

		private void AcceptComma ()
		{
			/************ Discussion:
			So we would like to simply do this, but it isn't sufficiently accurate:
					ReduceThenPushOperator ( Operator.ExpressionSeparator );

			We have to know whether the comma is at the top-level inside a function call argument list.
				See these examples:
					a.	f(1,2)				-- the comma is an argument separator, there are two actual arguments being passed.
					b.	f((1),(2))			-- same as above
					c.	f ((1,2))			-- the comma is an expression separator, there is only one actual argument being passed ( and it's value is 2)
					d.	f ((1,2),(3,4))		-- the first and last comma operators are expression separators, whereas the middle is an argument separator

			The distinctions between the above can only be detected here before reduction of the overall expression
					i.e. while parsing the ',' as once the tree is built for the closing ')', you can't see the difference anymore!

			 We need to know if the operator stack has an Operator.FunctionCall on, however,
				we are not intersted in it if it is under Operator.GoupingParen, or Operator.Subscript...

			 So, rather than searching the operator stack for the appropriate conditions,
				we simply reduce the left hand side of the comma operator and then see what's on top.
			************/

			// reduce left argument to comma operator
			ReduceByPrecedence ( Operator.ExpressionSeparator );

			// now we can decide which comma operator to push 
			//	the tree differentiates between argument separating commas and expression separating commas
			var op = _operatorStack.Peek ();
#if MultiDimensionalArrays
			_operatorStack.Push ( op == Operator.FunctionCall || op == Operator.Subscript ? Operator.ArgumentSeparator : Operator.ExpressionSeparator );
#else
			_operatorStack.Push ( op == Operator.FunctionCall ? Operator.ArgumentSeparator : Operator.ExpressionSeparator );
#endif
		}

		private void AcceptCloseParen ()
		{
			/************ Discussion:
			We don't know whether a close paren ')' matches a regular parenthesis or a function call parenthesis.
			So, we'll reduce the operator stack one element at a time until we know!
			************/

			for ( ; ; )
			{
				var op = _operatorStack.Peek ();
				if ( op == Operator.None )
					_scanner.ErrorAtMark ( "unexpected ')'" );

				switch ( op ) {
				case Operator.FunctionCall:
					_operatorStack.Pop ();
					BuildAndPushOperator ( Operator.FunctionCall );
					return;
				case Operator.PrecedenceGroupingParenthesis:
					_operatorStack.Pop ();
					return;
				}

				ReduceTopOperator ();
			}
		}

		private void ReduceAndPushByChoice ( char testFor, Operator onMatch, Operator otherwise )
		{
			if ( _scanner.IfCharacter ( testFor ) )
				ReduceThenPushOperator ( onMatch );
			else
				ReduceThenPushOperator ( otherwise );
		}

		private void ReduceAndPushByChoice ( char test1, Operator match1, char test2, Operator match2, Operator otherwise )
		{
			if ( _scanner.IfCharacter ( test1 ) )
				ReduceThenPushOperator ( match1 );
			else if ( _scanner.IfCharacter ( test2 ) )
				ReduceThenPushOperator ( match2 );
			else
				ReduceThenPushOperator ( otherwise );
		}

		private void ReduceAndPushByChoice ( char test1, char test2, Operator match2A, Operator match2B, char test3, Operator match3A, Operator match3B )
		{
			if ( _scanner.IfCharacter ( test1 ) ) {
				if ( _scanner.IfCharacter ( test2 ) )
					ReduceThenPushOperator ( match2A );
				else
					ReduceThenPushOperator ( match2B );
			} else if ( _scanner.IfCharacter ( test3 ) )
				ReduceThenPushOperator ( match3A );
			else
				ReduceThenPushOperator ( match3B );
		}

		private void ReduceThenPushOperator ( Operator newOp )
		{
			ReduceByPrecedence ( newOp );
			_operatorStack.Push ( newOp );
		}

		private void ReduceByPrecedence ( Operator newOp )
		{
			// the incoming operator has precedence directly from the table
			//	a stack operator has precedence from the table, but lowered by right associativity
			//	thus when an operator with right associativity is compared to itself, 
			//		the new one is higher precedence than an already stacked one of the same
			//		hence the removal of the last bit & ~1;

			var newOpPrecedence = Precedence [ (int) newOp ];
			for ( ; ; ) {
				var stackedOp = _operatorStack.Peek ();
				if ( _operatorStack.Peek () == Operator.None )
					break;

				// NB: the precence of the following operators on the operator stack mean that
				//	we are in a nested context, so we don't try to reduce past these, despite precedences
				//	instead, the parser is waiting for the proper matching close
				if ( stackedOp == Operator.FunctionCall ||
					 stackedOp == Operator.PrecedenceGroupingParenthesis ||
#if MultidimensionalArrays
					 stackedOp == Operator.Subscript ||
#endif
					 stackedOp == Operator.TernaryTest )
					break;

				var stackedPrecedence = Precedence [ (int) stackedOp ] & ~1;
				if ( stackedPrecedence >= newOpPrecedence )
					break;
				ReduceTopOperator ();
			}
		}

		// for matching brackets of various kinds
		private void ReduceUntilMatch ( Operator opToMatch )
		{
			for ( ; ; )
			{
				var op = _operatorStack.Peek ();
				if ( op == opToMatch )
					break;

				if ( op == Operator.None ) {
					if ( opToMatch == Operator.Subscript )
						_scanner.ErrorAtMark ( "unexpected ']'" );
					else if ( opToMatch == Operator.TernaryTest )
						_scanner.ErrorAtMark ( "unexpected : (no matching ?)" );
					else
						_scanner.ErrorAtMark ( "unexpected ')'" );
				}

				ReduceTopOperator ();
			}

			// and finally, pop the matching bracket
			_operatorStack.Pop ();
		}

		private void ReduceTopOperator ()
		{
			// first check balancing operators
			var op = _operatorStack.Pop ();

			// we can't reduce certain operators, 
			//	they're supposed to be matched by their counterparts instead of being reduced
			switch ( op ) {
			case Operator.FunctionCall:
			case Operator.PrecedenceGroupingParenthesis:
				_scanner.ErrorAtMark ( "unmatched '('" );
				break;
			case Operator.Subscript:
				_scanner.ErrorAtMark ( "unmatched '['" );
				break;
			case Operator.TernaryTest:
				_scanner.ErrorAtMark ( "missing : for ? operator" );
				break;
			}

			BuildAndPushOperator ( op );
		}

		void BuildAndPushOperator ( Operator op )
		{
			AbstractSyntaxTree res;
			switch ( Arity [ (int) op ] ) {
			case 1:
				res = BuildUnaryTreeNode ( op );
				break;
			case 2:
				res = BuildBinaryTreeNode ( op );
				break;
			case 3:
				res = BuildTernaryTreeNode ( op );
				break;
			default:
				throw new System.Exception ( "arity not understood: " + op.ToString () );
			}

			_operandStack.Push ( res );
		}

		bool AtTopLevelAndUnblocked ()
		{
			foreach ( var op in _operatorStack.Reverse () ) {
				switch ( op ) {
				case Operator.FunctionCall:
				case Operator.PrecedenceGroupingParenthesis:
				case Operator.TernaryTest:
				case Operator.Subscript:
					return false;
				}
			}

			return true;
		}
	}

	static class Extensions
	{
		public static bool AnyExceptTerminal<T> ( this Stack<T> me )
		{
			return me.Count > 1;
		}
	}
}