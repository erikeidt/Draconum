
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	partial class VariableTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", Value.ToString () );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			case EvaluationIntention.AddressOrNode:
				return this;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class LongIntegerTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", string.Format ( "#{0}", Value ) );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class StringTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// NB: a C string's value is its address.  (There is no notion of a string's address.)
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", string.Format ( "\"{0}\"", Value ) );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class AssignmentTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );

			switch ( purpose ) {
			case EvaluationIntention.Value:
			case EvaluationIntention.ValueOrNode:
				context.GenerateInstruction ( "DUP" );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );

			}

			var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			if ( target == null ) {
				context.GenerateInstruction ( "ISTORE" );
			} else if ( target is VariableTreeNode variable ) {
				context.GenerateInstruction ( "POP", variable.Value.ToString () );
			} else {
				throw new AssertionFailedException ( "unexpected result for LHS" );
			}

			return null;
		}
	}

	partial class AddressOfTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.SideEffectsOnly:
				Arg.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				break;
			case EvaluationIntention.Value:
				var ans = Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );

				if ( ans != null ) {
					if ( ans is VariableTreeNode variable ) {
						context.GenerateInstruction ( "PUSH", "&" + variable.Value.ToString () );
					} else {
						throw new AssertionFailedException ( "unexpected result for &" );
					}
				}
				break;
			case EvaluationIntention.ValueOrNode:
				return Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}

	}
}