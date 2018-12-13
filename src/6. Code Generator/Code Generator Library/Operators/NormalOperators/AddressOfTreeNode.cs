/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-10-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	partial class AddressOfTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// To take the address of a child expression, we capitalize on having an evaluation intention mode that matches this.
			//	We have this mode to support assignment and address of operations.
			//		Note that we must have a notion of evaluating an expression for its location rather than for its value, as
			//			we could not evaluate the left hand side of an assignment for value, and then take its address -- that doesn't work!
			//	By using the address evaluation intention for the child, there is then sometimes no code to generate for this operator.
			//	When it does need to generate code, the case of &a, for example, the it uses the PEA instruction
			//	Also note that this approach automatically detects and optimizes a sequence like &*
			//
			// Contrast this above with the Indirection operator
			//		The other operator, Indirection, needs to special case *& to collapse these two.
			//		This is because we don't have an evaluation intention mode for indirection,
			//		Instead we generate the indirection's value and the use an Indirection instruction to dereference it.
			//	
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					break;
				case EvaluationIntention.Value:
					var ans = Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );

					if ( ans != null ) {
						if ( ans is VariableTreeNode variable ) {
							// PEA == Push Effective Address
							context.GenerateInstruction ( "PEA", variable.Value.ToString () );
						} else {
							throw new AssertionFailedException ( "unexpected result for &" );
						}
					}
					break;
				case EvaluationIntention.ValueOrNode:
					return Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
			return null;
		}

	}
}