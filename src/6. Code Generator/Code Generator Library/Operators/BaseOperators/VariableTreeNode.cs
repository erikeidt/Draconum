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
	partial class VariableTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.Value:
					context.GenerateInstruction ( "PUSH", Value.ToString () );
					break;
				case EvaluationIntention.ValueOrNode:
					return this;
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
}