/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 11-19-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */
namespace com.erikeidt.Draconum
{
	partial class UnaryOperatorTreeNode 
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					if ( Op != Operators.Operator.FixPoint )
						context.GenerateInstruction ( Op.ToString () );
					return null;
//				case EvaluationIntention.AddressOrNode:
//					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
//					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}
	}
}
