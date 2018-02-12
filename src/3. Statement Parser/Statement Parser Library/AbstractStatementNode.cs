
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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Empty Statement", prolog );
		}
	}


	partial class DeclarationStatement : AbstractStatementNode
	{
		public readonly List<AbstractSyntaxTree> Declartions;

		public DeclarationStatement ( List<AbstractSyntaxTree> declartions )
		{
			Declartions = declartions;
		}

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Declaration Statement", prolog, Declartions.Count );

			foreach ( var decl in Declartions )
				decl.PrettyPrint ( to, level + 1, null );
		}
	}

	partial class ExpressionStatement : AbstractStatementNode
	{
		public readonly AbstractSyntaxTree Expression;

		public ExpressionStatement ( AbstractSyntaxTree expression )
		{
			Expression = expression;
		}

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Expression Statement", prolog, 1 );
			Expression.PrettyPrint ( to, level + 1, null );
		}
	}

	partial class BlockStatement : AbstractStatementNode
	{
		public readonly List<AbstractStatementNode> Statements;

		public BlockStatement ( List<AbstractStatementNode> statements )
		{
			Statements = statements;
		}

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Block Statement", prolog, Statements.Count );
			foreach ( var stmt in Statements )
				stmt.PrettyPrint ( to, level + 1, null );
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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			int arity = ElsePart == null ? 2 : 3;
			WriteLine ( to, level, arity == 3 ? "If-Then-Else Statement" : "If-Then Statement", prolog, arity );
			Condition.PrettyPrint ( to, level + 1, "Condition\t" );
			ThenPart.PrettyPrint ( to, level + 1, "ThenPart\t" );
			ElsePart?.PrettyPrint ( to, level + 1, "ElsePart\t" );
		}
	}

	abstract partial class BreakableStatement : AbstractStatementNode
	{
		public bool HasBreak;
	}

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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			int arity = 1 + (Initialization != null ? 1 : 0) + (Condition != null ? 1 : 0) + (Increment != null ? 1 : 0);
			WriteLine ( to, level, "For Statement", prolog, arity );
			Initialization?.PrettyPrint ( to, level + 1, "Initialization\t" );
			Condition?.PrettyPrint ( to, level + 1, "Condition\t" );
			Increment?.PrettyPrint ( to, level + 1, "Increment\t" );
			Body.PrettyPrint ( to, level + 1, "Body\t" );
		}
	}

	partial class ReturnStatement : AbstractStatementNode
	{
		public readonly AbstractSyntaxTree ReturnValue;

		public ReturnStatement ( AbstractSyntaxTree returnValue )
		{
			ReturnValue = returnValue;
		}

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			if ( ReturnValue == null )
				WriteLine ( to, level, "Return Statement (Void)", prolog );
			else
			{
				WriteLine ( to, level, "Return Statement", prolog, 1 );
				ReturnValue.PrettyPrint ( to, level + 1, "Return Value\t" );
			}
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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Break Statement", prolog );
		}
	}

	partial class ContinueStatement : BranchingStatement
	{
		public readonly ContinueableStatement EnclosingStatement;

		public ContinueStatement ( ContinueableStatement enclosingStatement )
		{
			EnclosingStatement = enclosingStatement;
		}

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Continue Statement", prolog );
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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			WriteLine ( to, level, "Goto: " + GotoTarget.Name, prolog );
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

		public override void PrettyPrint ( TextWriter to, int level, string prolog = "" )
		{
			if ( Statement == null )
			{
				// case of label at the end of a method, applies to no actual statement
				WriteLine ( to, level, "Label: " + Label.Name, prolog );
			}
			else
			{
				WriteLine ( to, level, "Label: " + Label.Name, prolog, 1 );
				Statement.PrettyPrint ( to, level + 1, "Statement\t" );
			}
		}
	}

}