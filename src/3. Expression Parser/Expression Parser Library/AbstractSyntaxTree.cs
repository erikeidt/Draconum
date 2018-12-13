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

// ReSharper disable PartialTypeWithSinglePart

using System.Collections.Generic;

namespace com.erikeidt.Draconum
{
	using static Operators;
	using UnicodeUtf8 = Draconum;

	abstract partial class AbstractSyntaxTree : Dump
	{
	}

	partial class VariableTreeNode : AbstractSyntaxTree
	{
		public readonly UnicodeUtf8.ByteString Value;

		public VariableTreeNode ( UnicodeUtf8.ByteString variableName )
		{
			Value = variableName;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "Var: '{0}'", Value ), prolog );
		}
	}

	partial class LongIntegerTreeNode : AbstractSyntaxTree
	{
		public readonly long Value;

		public LongIntegerTreeNode ( long numericLiteral )
		{
			Value = numericLiteral;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "num: '{0}'", Value ), prolog );
		}
	}

	partial class StringTreeNode : AbstractSyntaxTree
	{
		public readonly UnicodeUtf8.ByteString Value;

		public StringTreeNode ( UnicodeUtf8.ByteString stringLiteral )
		{
			Value = stringLiteral;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "string: \"{0}\"", Value ), prolog );
		}
	}

	/*
	partial class CharacterTreeNode : AbstractSyntaxTree
	{
		public readonly int Value;

		public CharacterTreeNode ( int charLiteral )
		{
			Value = charLiteral;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "char: \'{0}\'", Value ), prolog );
		}
	}
	*/

	abstract partial class OperatorTreeNode : AbstractSyntaxTree
	{
		public readonly Operator Op;

		protected OperatorTreeNode ( Operator op )
		{
			Op = op;
		}
	}

	abstract partial class UnaryOperatorTreeNode : OperatorTreeNode
	{
		public readonly AbstractSyntaxTree Arg;

		public UnaryOperatorTreeNode ( Operator op, AbstractSyntaxTree arg )
			: base ( op )
		{
			Arg = arg;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "unop: \'{0}\'", Op ), prolog, 1 );
		}

		public override void PrettyPrintBody ()
		{
			Arg.PrettyPrint ();
		}
	}

	abstract partial class BinaryOperatorTreeNode : OperatorTreeNode
	{
		public readonly AbstractSyntaxTree Left;
		public readonly AbstractSyntaxTree Right;

		public BinaryOperatorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right )
			: base ( op )
		{
			Left = left;
			Right = right;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "binop: \'{0}\'", Op ), prolog, 2 );
		}

		public override void PrettyPrintBody ()
		{
			Left.PrettyPrint ();
			if ( Right != null )
				Right.PrettyPrint ();
			else
				Dump.WriteLine ( "<empty>" );
		}
	}

	abstract partial class RelationalOperatorTreeNode : BinaryOperatorTreeNode
	{
		public RelationalOperatorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right )
			: base ( op, left, right )
		{
		}
	}

	abstract partial class TernaryOperatorTreeNode : OperatorTreeNode
	{
		public readonly AbstractSyntaxTree Pre;
		public readonly AbstractSyntaxTree Mid;
		public readonly AbstractSyntaxTree Post;

		public TernaryOperatorTreeNode ( Operator op, AbstractSyntaxTree pre, AbstractSyntaxTree mid,
			AbstractSyntaxTree post )
			: base ( op )
		{
			Pre = pre;
			Mid = mid;
			Post = post;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( string.Format ( "ternop: \'{0}\'", Op ), prolog, 3 );
		}

		public override void PrettyPrintBody ()
		{
			Pre.PrettyPrint ( "Test\t" );
			Mid.PrettyPrint ( "True\t" );
			Post.PrettyPrint ( "False\t" );
		}
	}

	abstract partial class AssignmentOperatorTreeNode : BinaryOperatorTreeNode {
		public AssignmentOperatorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right )
			: base ( op, left, right )
		{
		}
	}
}
