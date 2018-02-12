
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

using System;
using System.Collections.Generic;

namespace com.erikeidt.Draconum.ExpressionGenerator
{
	using static Program.Section;
	using static Program.Arity;
	using static Program.Associativity;

	class Program
	{
		const string OutputFilePath = @"..\2. Expression Parser\Expression Parser Library\";

		static readonly List<string> CopyrightText = new List<string> {
			null,
			"/*",
			" *",
			" * Copyright (c) 2018, Erik L. Eidt",
			" * All rights Reserved.",
			" *",
			" * Author: Erik L. Eidt",
			" * Created: 01-11-2018",
			" *",
			" * License: No License: no permissions are granted to use, modify, or share. See COPYRIGHT.md for more details.",
			" *",
			" */",
			null,
		};

		public enum Section
		{
			NoState,
			UnaryState,
			BinaryState,
		}

		public enum Arity
		{
			Neither,
			Unary,
			Binary,
			Ternary
		}

		public enum Associativity
		{
			NotApplic,
			LeftRight,
			RightLeft,
		}

		class SemanticOperator
		{
			public readonly Section Section;
			public readonly Arity Arity;
			public readonly int Precedence;
			public readonly Associativity Lr;
			public readonly string Token;
			public readonly string [] Names;
			public readonly bool HasTreeNodes;

			public SemanticOperator ( Section section, Arity arity, int precedence, Associativity lr, string token, string [] names, bool hasTreeNodes )
			{
				Section = section;
				Arity = arity;
				Precedence = precedence;
				Lr = lr;
				Token = token;
				Names = names;
				HasTreeNodes = hasTreeNodes;
			}
		}

		private readonly HashSet<Tuple<Section, string>> _syntacticTokens = new HashSet<Tuple<Section, string>> ();
		private readonly HashSet<string> _operatorNames = new HashSet<string> ();

		private readonly List<SemanticOperator> _semanticOperators = new List<SemanticOperator> ();

		void AddSemanticOperator ( Section kind, Arity arity, int precedence, Associativity lr, string token, string semanticOperatorName, bool hasNodes = true )
		{
			AddSemanticOperator ( kind, arity, precedence, lr, token, new string [] { semanticOperatorName }, hasNodes );
		}

		void AddSemanticOperator ( Section kind, Arity arity, int precedence, Associativity lr, string token, string [] semanticOperatorNames, bool hasNodes = true )
		{
			// this will fail if there's a duplicate, and that's the idea.
			_syntacticTokens.Add ( new Tuple<Section, string> ( kind, token ) );

			// just ensuring uniqueness here
			_operatorNames.Add ( (kind == UnaryState ? "Unary" : "Binary") + token );

			_semanticOperators.Add ( new SemanticOperator ( kind, arity, precedence, lr, token, semanticOperatorNames, hasNodes ) );
		}

		static void Main ( string [] args )
		{
			var p = new Program ();
			p.Run ();
		}

		void Run ()
		{
			AddSemanticOperator ( NoState, Neither, 0, NotApplic, "", "None", false );

			AddSemanticOperator ( UnaryState, Neither, 0, NotApplic, "(", "PrecedenceGroupingParenthesis", false );

			// NB: 
			//	These two postfix operators materialize in the Binary State, however, they are unary operators!
			//		This is reason for separate notion of state vs arity.	
			AddSemanticOperator ( BinaryState, Unary, 2, LeftRight, "++", "PostfixIncrement" );
			AddSemanticOperator ( BinaryState, Unary, 2, LeftRight, "--", "PostfixDecrement" );

			AddSemanticOperator ( BinaryState, Binary, 2, LeftRight, "(", "FunctionCall" );
			AddSemanticOperator ( UnaryState, Neither, 0, NotApplic, ")", "EmptyArgumentList", false );
			AddSemanticOperator ( BinaryState, Neither, 0, NotApplic, ")", "CloseParenOrArgList", false );
			AddSemanticOperator ( BinaryState, Binary, 2, LeftRight, "[", "Subscript" );
			AddSemanticOperator ( UnaryState, Neither, 0, NotApplic, "]", "EmptySubscript", false );
			AddSemanticOperator ( BinaryState, Neither, 0, NotApplic, "]", "CloseSubscript", false );

			AddSemanticOperator ( BinaryState, Binary, 2, LeftRight, ".", "Selection" );                    // binary but requires special parsing of right operand as member name
			AddSemanticOperator ( BinaryState, Binary, 2, LeftRight, "->", "IndirectSelection" );           // binary but required special parsing of right operand as member name

			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "++", "PrefixIncrement" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "--", "PrefixDecrement" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "+", "FixPoint" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "-", "Negation" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "!", "LogicalNot" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "~", "BitwiseComplement" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "*", "Indirection" );
			AddSemanticOperator ( UnaryState, Unary, 3, RightLeft, "&", "AddressOf" );

			AddSemanticOperator ( BinaryState, Binary, 4, LeftRight, ".*", "SelectionReference" );          // binary but requires special parsing of right operand as member name
			AddSemanticOperator ( BinaryState, Binary, 4, LeftRight, "->*", "IndirectSelectionReference" ); // binary but required special parsing of right operand as member name

			AddSemanticOperator ( BinaryState, Binary, 5, LeftRight, "*", "Multiplication" );
			AddSemanticOperator ( BinaryState, Binary, 5, LeftRight, "/", "Division" );
			AddSemanticOperator ( BinaryState, Binary, 5, LeftRight, "%", "Modulo" );

			AddSemanticOperator ( BinaryState, Binary, 6, LeftRight, "+", "Addition" );
			AddSemanticOperator ( BinaryState, Binary, 6, LeftRight, "-", "Subtraction" );

			AddSemanticOperator ( BinaryState, Binary, 7, LeftRight, "<<", "BitwiseLeftShift" );
			AddSemanticOperator ( BinaryState, Binary, 7, LeftRight, ">>", "BitwiseRightShift" );

			AddSemanticOperator ( BinaryState, Binary, 8, LeftRight, "<=>", "Order" );

			AddSemanticOperator ( BinaryState, Binary, 9, LeftRight, "<", "LessThan" );
			AddSemanticOperator ( BinaryState, Binary, 9, LeftRight, "<=", "LessOrEqual" );
			AddSemanticOperator ( BinaryState, Binary, 9, LeftRight, ">", "GreaterThan" );
			AddSemanticOperator ( BinaryState, Binary, 9, LeftRight, ">=", "GreaterOrEqual" );

			AddSemanticOperator ( BinaryState, Binary, 10, LeftRight, "==", "EqualEqual" );
			AddSemanticOperator ( BinaryState, Binary, 10, LeftRight, "!=", "NotEqual" );

			AddSemanticOperator ( BinaryState, Binary, 11, LeftRight, "&", "BitwiseAnd" );
			AddSemanticOperator ( BinaryState, Binary, 12, LeftRight, "^", "BitwiseXor" );
			AddSemanticOperator ( BinaryState, Binary, 13, LeftRight, "|", "BitwiseOr" );

			AddSemanticOperator ( BinaryState, Binary, 14, LeftRight, "&&", "ShortCircutAnd" );
			AddSemanticOperator ( BinaryState, Binary, 15, LeftRight, "||", "ShortCircutOr" );

			AddSemanticOperator ( BinaryState, Binary, 16, RightLeft, "?", "TernaryTest", false );
			AddSemanticOperator ( BinaryState, Ternary, 16, RightLeft, ":", "TernaryChoice" );

			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "=", "Assignment" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "*=", "AssignmentMultiplication" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "/=", "AssignmentDivision" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "%=", "AssignmentModulo" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "+=", "AssignmentAddition" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "-=", "AssignmentSubtraction" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "&=", "AssignmentBitwiseAnd" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "^=", "AssignmentBitwiseXor" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "|=", "AssignmentBitwiseOr" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, "<<=", "AssignmentBitwiseLeftShift" );
			AddSemanticOperator ( BinaryState, Binary, 17, RightLeft, ">>=", "AssignmentBitwiseRightShift" );

			AddSemanticOperator ( BinaryState, Binary, 18, LeftRight, ",", new [] { "ExpressionSeparator", "ArgumentSeparator" } );

			var namespacePath = "com.erikeidt.Draconum";

			List<string> operators = new List<string> ();
			List<string> precedence = new List<string> ();
			List<string> arity = new List<string> ();
			List<string> treeNodes = new List<string> ();
			List<string> unaryOperators = new List<string> ();
			List<string> binaryOperators = new List<string> ();
			List<string> ternaryOperators = new List<string> ();
			List<Tuple<string, string>> unaryStateTokenSet = new List<Tuple<string, string>> ();
			List<Tuple<string, string>> binaryStateTokenSet = new List<Tuple<string, string>> ();

			foreach ( var so in _semanticOperators ) {
				foreach ( var on in so.Names )
				{
					switch ( so.Section )
					{
					case UnaryState:
						unaryStateTokenSet.Add ( new Tuple<string, string> ( so.Token, on ) );
						break;
					case BinaryState:
						binaryStateTokenSet.Add ( new Tuple<string, string> ( so.Token, on ) );
						break;
					}

					operators.Add ( on );
					precedence.Add ( (so.Precedence * 2 + (so.Lr == RightLeft ? 0 : 1)).ToString () );
					if ( so.HasTreeNodes ) {
						treeNodes.Add ( "partial class " + on + "TreeNode : " + so.Arity.ToString () + "OperatorTreeNode {" );
						switch ( so.Arity ) {
						case Unary:
							arity.Add ( "1" );
							unaryOperators.Add ( "case " + on + ": " + "res = new " + on + "TreeNode ( op, arg ); break;" );
							treeNodes.Add ( "\tpublic " + on + "TreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }" );
							break;
						case Binary:
							arity.Add ( "2" );
							binaryOperators.Add ( "case " + on + ": " + "res = new " + on + "TreeNode ( op, left, right ); break;" );
							treeNodes.Add ( "\tpublic " + on + "TreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }" );
							break;
						case Ternary:
							arity.Add ( "3" );
							ternaryOperators.Add ( "case " + on + ": " + "res = new " + on + "TreeNode ( op, pre, mid, post ); break;" );
							treeNodes.Add ( "\tpublic " + on + "TreeNode ( Operator op, AbstractSyntaxTree pre, AbstractSyntaxTree mid, AbstractSyntaxTree post ) : base ( op, pre, mid, post ) { }" );
							break;
						default:
							arity.Add ( "0" );
							break;
						}
						treeNodes.Add ( "}" );
						treeNodes.Add ( null );
					}
					else
					{
						arity.Add ( "0" );
					}
				}
			}

			System.IO.TextWriter file = System.IO.File.CreateText ( OutputFilePath + @"Operators.gen.cs" );
			SendStrings ( file, "{0}", CopyrightText );
			file.WriteLine ( "namespace " + namespacePath + " {" );
			file.WriteLine ( "\tclass Operators {" );
			file.WriteLine ( "\t\tpublic enum Operator : byte {" );
			SendStrings ( file, "\t\t\t{0},", operators );
			file.WriteLine ( "\t\t}" );
			file.WriteLine ();
			file.WriteLine ( "\t\tpublic static byte [] Precedence = {" );
			SendStrings ( file, "\t\t\t{0},", precedence );
			file.WriteLine ( "\t\t};" );
			file.WriteLine ();
			file.WriteLine ( "\t\tpublic static byte [] Arity = {" );
			SendStrings ( file, "\t\t\t{0},", arity );
			file.WriteLine ( "\t\t};" );
			file.WriteLine ( "\t}" );
			file.WriteLine ( "}" );
			file.Dispose ();

			file = System.IO.File.CreateText ( OutputFilePath + @"ExpressionParser.gen.cs" );
			SendStrings ( file, "{0}", CopyrightText );
			file.WriteLine ();
			file.WriteLine ( "namespace " + namespacePath + " {" );
			file.WriteLine ( "\tusing static Operators;" );
			file.WriteLine ( "\tusing static Operators.Operator;" );
			file.WriteLine ();
			file.WriteLine ( "\tpartial class ExpressionParser {" );
			file.WriteLine ( "\t\tpublic AbstractSyntaxTree BuildUnaryTreeNode ( Operator op )" );
			file.WriteLine ( "\t\t{" );
			file.WriteLine ( "\t\t\tvar arg = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar res = (AbstractSyntaxTree) null;" );
			file.WriteLine ( "\t\t\tswitch ( op ) {" );
			SendStrings ( file, "\t\t\t\t{0}", unaryOperators );
			file.WriteLine ( "\t\t\t}" );
			file.WriteLine ( "\t\t\treturn res;" );
			file.WriteLine ( "\t\t}" );
			file.WriteLine ( "\t\tpublic AbstractSyntaxTree BuildBinaryTreeNode ( Operator op )" );
			file.WriteLine ( "\t\t{" );
			file.WriteLine ( "\t\t\tvar right = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar left = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar res = (AbstractSyntaxTree) null;" );
			file.WriteLine ( "\t\t\tswitch ( op ) {" );
			SendStrings ( file, "\t\t\t\t{0}", binaryOperators );
			file.WriteLine ( "\t\t\t}" );
			file.WriteLine ( "\t\t\treturn res;" );
			file.WriteLine ( "\t\t}" );
			file.WriteLine ();
			file.WriteLine ( "\t\tpublic AbstractSyntaxTree BuildTernaryTreeNode ( Operator op )" );
			file.WriteLine ( "\t\t{" );
			file.WriteLine ( "\t\t\tvar post = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar mid = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar pre = _operandStack.Pop ();" );
			file.WriteLine ( "\t\t\tvar res = (AbstractSyntaxTree) null;" );
			file.WriteLine ( "\t\t\tswitch ( op ) {" );
			SendStrings ( file, "\t\t\t\t{0}", ternaryOperators );
			file.WriteLine ( "\t\t\t}" );
			file.WriteLine ( "\t\t\treturn res;" );
			file.WriteLine ( "\t\t}" );
			file.WriteLine ( "\t}" );
			file.WriteLine ( "}" );
			file.WriteLine ();
			file.WriteLine ();
	
			file.WriteLine ( "/*" );
			var soa = unaryStateTokenSet.ToArray ();
			Array.Sort ( soa, TokenComparer );
			file.WriteLine ( "Unary State Tokens & Operators" );
			SendStrings ( file, "\t\t{0}", Array.ConvertAll<Tuple<string,string>,string> ( soa, x => x.Item1 + "\t" + x.Item2 ) );
			soa = binaryStateTokenSet.ToArray ();
			Array.Sort ( soa, TokenComparer );
			file.WriteLine ();
			file.WriteLine ( "Binary State Tokens & Operators" );
			SendStrings ( file, "\t\t{0}", Array.ConvertAll<Tuple<string,string>,string> ( soa, x => x.Item1 + "\t" + x.Item2 ) );
			file.WriteLine ( "*/" );

			file.Dispose ();

			file = System.IO.File.CreateText ( OutputFilePath + @"AbstractSyntaxTree.gen.cs" );
			SendStrings ( file, "{0}", CopyrightText );
			file.WriteLine ();
			file.WriteLine ( "// ReSharper disable PartialTypeWithSinglePart" );
			file.WriteLine ();
			file.WriteLine ( "namespace " + namespacePath + " {" );
			file.WriteLine ( "	using static Operators;" );
			file.WriteLine ();
			SendStrings ( file, "\t{0}", treeNodes );
			file.WriteLine ( "}" );
			file.Dispose ();
		}

		private static int TokenComparer ( Tuple<string, string> one, Tuple<string, string> two )
		{
			var a = one.Item1;
			var b = two.Item1;
			var first = string.Compare ( a, b, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.CompareOptions.Ordinal );
			if ( first != 0 )
				return first;
			var c = one.Item2;
			var d = two.Item2;
			return string.Compare ( c, d, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.CompareOptions.Ordinal );
		}

		void SendStrings ( System.IO.TextWriter file, string format, IEnumerable<string> strings )
		{
			foreach ( var line in strings ) {
				if ( String.IsNullOrEmpty ( line ) )
					file.WriteLine ();
				else
					file.WriteLine ( format, line );
			}
		}

	}
}

#if false
		public static string [] AsciiNames = new string [ 128 ];

		static void AsciiName ( char ch, string name )
		{
			AsciiNames [ ch ] = name;
		}

		static void AddToken ( Section kind, string token, string name )
		{
			var key = new Tuple<Section, string> ( kind, token );
			if ( _syntacticTokens.ContainsKey ( key ) )
				throw new System.ArgumentException ( "duplicate: " + token );
			_syntacticTokens.Add ( key, name );
		}


			// give some names to these individual characters, probably won't use except for verification...

			AsciiName ( '!', "Bang" );
			AsciiName ( '#', "HashTag" );
			AsciiName ( '%', "Percent" );
			AsciiName ( '&', "And" );
			AsciiName ( '(', "OpenParen" );
			AsciiName ( ')', "CloseParen" );
			AsciiName ( '*', "Star" );
			AsciiName ( '+', "Plus" );
			AsciiName ( ',', "Comma" );
			AsciiName ( '-', "Minus" );
			AsciiName ( '.', "Dot" );
			AsciiName ( '/', "Slash" );
			AsciiName ( ':', "Colon" );
			AsciiName ( ';', "SemiColon" );
			AsciiName ( '<', "Less" );
			AsciiName ( '=', "Equal" );
			AsciiName ( '>', "Greater" );
			AsciiName ( '?', "QuestionMark" );
			AsciiName ( '.', "Dot" );
			AsciiName ( '@', "AtSign" );
			AsciiName ( '[', "OpenSquareBracket" );
			AsciiName ( '^', "Hat" );
			AsciiName ( ']', "CloseSquareBracket" );
			AsciiName ( '|', "VerticalBar" );
			AsciiName ( '~', "Tilde" );

			// declare the Unary State Tokens
			//	for later analysis & verification, in particular to detect syntactic ambiguity

			AddToken ( UnaryState, "!", "Bang" );
			AddToken ( UnaryState, "&", "And" );
			AddToken ( UnaryState, "(", "OpenParen" );
			AddToken ( UnaryState, ")", "CloseParen" );
			AddToken ( UnaryState, "]", "CloseSquareBracket" );
			AddToken ( UnaryState, "*", "Star" );
			AddToken ( UnaryState, "+", "Plus" );
			AddToken ( UnaryState, "++", "PlusPlus" );
			AddToken ( UnaryState, "-", "Minus" );
			AddToken ( UnaryState, "--", "MinusMinus" );
			AddToken ( UnaryState, ".", "NumericDot" );
			AddToken ( UnaryState, "~", "Tilde" );

			// declare the Binary State Tokens
			//	for later analysis & verification, in particular to detect syntactic ambiguity

			AddToken ( BinaryState, "!=", "NotEqual" );
			AddToken ( BinaryState, "%", "Percent" );
			AddToken ( BinaryState, "%=", "Percent" );
			AddToken ( BinaryState, "&", "And" );
			AddToken ( BinaryState, "&&", "AndAnd" );
			AddToken ( BinaryState, "&=", "AssignmentAnd" );
			AddToken ( BinaryState, "(", "Invocation" );
			AddToken ( BinaryState, ")", "CloseParen" );
			AddToken ( BinaryState, "*", "Multiply" );
			AddToken ( BinaryState, "*=", "AssignemntMultiply" );
			AddToken ( BinaryState, "+", "Plus" );
			AddToken ( BinaryState, "+=", "AssignmentPlus" );
			AddToken ( BinaryState, "++", "PlusPlus" );
			AddToken ( BinaryState, ",", "ExpressionSeparator" );
			AddToken ( BinaryState, "-", "Minus" );
			AddToken ( BinaryState, "--", "MinusMinus" );
			AddToken ( BinaryState, "-=", "AssignmentMinus" );
			AddToken ( BinaryState, "->", "OverArrow" );
			AddToken ( BinaryState, "->*", "OverArrowStar" );
			AddToken ( BinaryState, ".", "Dot" );
			AddToken ( BinaryState, ".*", "DotStar" );
			AddToken ( BinaryState, "/", "Divide" );
			AddToken ( BinaryState, "/=", "AssignmentDivide" );
			AddToken ( BinaryState, ":", "Colon" );
			AddToken ( BinaryState, "<", "LessThan" );
			AddToken ( BinaryState, "<<", "LeftShift" );
			AddToken ( BinaryState, "<=", "LessOrEqual" );
			AddToken ( BinaryState, "<=>", "LessEqualGreater" );
			AddToken ( BinaryState, "<<=", "AssignmentLeftShift" );
			AddToken ( BinaryState, "=", "Equal" );
			AddToken ( BinaryState, "==", "EqualEqual" );
			AddToken ( BinaryState, ">", "GreaterThan" );
			AddToken ( BinaryState, ">>", "RightShift" );
			AddToken ( BinaryState, ">=", "GreaterOrEqual" );
			AddToken ( BinaryState, ">>=", "AssignmentRightShift" );
			AddToken ( BinaryState, "?", "QuestionMark" );
			AddToken ( BinaryState, "[", "OpenSubscript" );
			AddToken ( BinaryState, "^", "Xor" );
			AddToken ( BinaryState, "^=", "AssignmentXor" );
			AddToken ( BinaryState, "]", "CloseSubscript" );
			AddToken ( BinaryState, "|", "Or" );
			AddToken ( BinaryState, "|=", "AssignmentOr" );
			AddToken ( BinaryState, "||", "OrOr" );

			var charFile = System.IO.File.CreateText ( path + "Character.gen.cs" );
			charFile.WriteLine ( nspace );
			charFile.WriteLine ( "\tpublic Enum Characters : byte {" );
			for ( int i = 32 ; i < 128 ; i++ ) {
				if ( AsciiNames [ i ] != null ) {
					charFile.WriteLine ( "\t\t/* " + i.ToString ( "000" ) + " */\t" + AsciiNames [ i ] + "," );
				}
			}
			charFile.WriteLine ( "\t}" );
			charFile.WriteLine ( "}" );
			charFile.Dispose ();

			var astOpeatorFile = System.IO.File.CreateText ( path + "AstOperators.gen.cs" );
			astOpeatorFile.WriteLine ( nspace );
			astOpeatorFile.WriteLine ( "\tpublic Enum Tokens : byte {" );
			var aoa = _astOperators.ToArray ();
			Array.Sort ( aoa, ( x, y ) => String.Compare ( x.Key, y.Key, StringComparison.InvariantCulture ) );
			for ( int i = 0 ; i < aoa.Length ; i++ ) {
				semanticOpeatorFile.WriteLine ( "\t\t" + aoa [ i ].Value + "/* " + i.ToString ( "000" ) + " */," );
			}
			astOpeatorFile.WriteLine ( "\t}" );
			astOpeatorFile.WriteLine ( "}" );
			astOpeatorFile.Dispose ();

#endif
