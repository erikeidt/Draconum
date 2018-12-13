
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
using System.IO;

// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum
{
	abstract partial class AbstractStatementNode : Dump
	{
	}

	partial class EmptyStatement : AbstractStatementNode
	{
		public EmptyStatement ()
		{
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( "Empty Statement", prolog );
		}
	}

	partial class DeclarationStatement : AbstractStatementNode
	{
		public readonly List<AbstractSyntaxTree> Declarations;

		public DeclarationStatement ( List<AbstractSyntaxTree> declarations )
		{
			Declarations = declarations;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			WriteLine ( "Declaration Statement", prolog, Declarations.Count );
		}

		public override void PrettyPrintBody ()
		{
			for ( int i = 0; i < Declarations.Count; i++ )
			{
				Declarations [ i ].PrettyPrint ( ".decl" );
			}
		}
	}

	partial class ExpressionStatement : AbstractStatementNode
	{
		public readonly AbstractSyntaxTree Expression;

		public ExpressionStatement ( AbstractSyntaxTree expression )
		{
			Expression = expression;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			WriteLine ( "Expression Statement", prolog, 1 );
		}

		public override void PrettyPrintBody ()
		{
			Expression.PrettyPrint ();
		}
	}

	partial class BlockStatement : AbstractStatementNode
	{
		public readonly List<AbstractStatementNode> Statements;

		public BlockStatement ( List<AbstractStatementNode> statements )
		{
			Statements = statements;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			WriteLine ( "Block Statement", prolog, Statements.Count );
		}

		public override void PrettyPrintBody ()
		{
			for ( int i = 0; i < Statements.Count; i++ )
			{
				Statements [ i ].PrettyPrint ();
			}
		}
	}

	partial class IfStatement : AbstractStatementNode
	{
		public readonly AbstractSyntaxTree Condition;
		public readonly AbstractStatementNode ThenPart;
		public readonly AbstractStatementNode ElsePart;

		public IfStatement ( AbstractSyntaxTree condition, AbstractStatementNode thenPart, AbstractStatementNode elsePart )
		{
			Condition = condition;
			ThenPart = thenPart;
			ElsePart = elsePart;
		}

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			int arity = ElsePart == null ? 2 : 3;
			WriteLine ( arity == 3 ? "If-Then-Else Statement" : "If-Then Statement", prolog, arity );
		}

		public override void PrettyPrintBody ()
		{
			Condition.PrettyPrint ( "Condition\t" );
			ThenPart.PrettyPrint ( "ThenPart\t" );
			ElsePart?.PrettyPrint ( "ElsePart\t" );
		}
	}

	// continuable statement & switches use this base class
	abstract partial class BreakableStatement : AbstractStatementNode
	{
		public bool HasBreak;
	}

	// loops use this base class
	abstract partial class ContinueableStatement : BreakableStatement
	{
		public bool HasContinue;
	}

	partial class ForStatement : ContinueableStatement
	{
		public readonly AbstractSyntaxTree Initialization;
		public readonly AbstractSyntaxTree Condition;
		public readonly AbstractSyntaxTree Increment;
		public AbstractStatementNode Body;

		public ForStatement ( AbstractSyntaxTree initialization, AbstractSyntaxTree condition, AbstractSyntaxTree increment )
		{
			Initialization = initialization;
			Condition = condition;
			Increment = increment;
		}

		private string initText = "Initialization\t";
		private string condText = "Condition\t";
		private string incrText = "Increment\t";
		private string bodyText = "Body\t";

		public override void PrettyPrintHeader ( string prolog = "" )
		{
			int arity = 1 + ( Initialization != null ? 1 : 0 ) + ( Condition != null ? 1 : 0 ) + ( Increment != null ? 1 : 0 );
			WriteLine ( "For Statement", prolog, arity );
		}

		public override void PrettyPrintBody () {
			Initialization?.PrettyPrint ( initText );
			Condition?.PrettyPrint ( condText );
			Increment?.PrettyPrint ( incrText );
			Body.PrettyPrint ( bodyText );
		}
	}

	partial class ReturnStatement : AbstractStatementNode
	{
		public readonly AbstractSyntaxTree ReturnValue;

		public ReturnStatement ( AbstractSyntaxTree returnValue )
		{
			ReturnValue = returnValue;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			if ( ReturnValue == null )
				WriteLine ( "Return Statement (Void)", prolog );
			else
				WriteLine ( "Return Statement", prolog, 1 );
		}

		public override void PrettyPrintBody () {
			ReturnValue?.PrettyPrint ( "Return Value\t" );
		}
	}

	abstract partial class BranchingStatement : AbstractStatementNode
	{
	}

	partial class BreakStatement : BranchingStatement
	{
		public readonly BreakableStatement EnclosingStatement;

		public BreakStatement ( BreakableStatement enclosingStatement )
		{
			EnclosingStatement = enclosingStatement;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			WriteLine ( "Break Statement", prolog );
		}
	}

	partial class ContinueStatement : BranchingStatement
	{
		public readonly ContinueableStatement EnclosingStatement;

		public ContinueStatement ( ContinueableStatement enclosingStatement )
		{
			EnclosingStatement = enclosingStatement;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			WriteLine ( "Continue Statement", prolog );
		}
	}

	partial class UserLabel
	{
		public readonly ByteString Name;

		public UserLabel ( ByteString name )
		{
			Name = name;
		}
	}

	partial class GotoStatement : AbstractStatementNode
	{
		public readonly UserLabel GotoTarget;

		public GotoStatement ( UserLabel gotoTarget )
		{
			GotoTarget = gotoTarget;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			WriteLine ( "Goto: " + GotoTarget.Name, prolog );
		}
	}

	partial class LabelStatement : AbstractStatementNode
	{
		public readonly UserLabel Label;
		public readonly AbstractStatementNode Statement;

		public LabelStatement ( UserLabel label, AbstractStatementNode statement )
		{
			Label = label;
			Statement = statement;
		}

		public override void PrettyPrintHeader ( string prolog )
		{
			if ( Statement == null )
			{
				// case of label at the end of a method, applies to no actual statement
				WriteLine ( "Label: " + Label.Name, prolog );
			}
			else
			{
				WriteLine ( "Label: " + Label.Name, prolog, 1 );
			}
		}

		public override void PrettyPrintBody ()
		{
			Statement?.PrettyPrint ( "Statement\t" );
		}
	}
}