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
							// PEA == Push Effective Address
							context.GenerateInstruction ( "PEA", variable.Value.ToString () );
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