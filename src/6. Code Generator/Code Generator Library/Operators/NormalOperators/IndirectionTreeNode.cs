namespace com.erikeidt.Draconum
{
	partial class IndirectionTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// NB: a C string's value is its address.  
			//	(There is no notion of a string's address as that would be an address of an address.)
			switch ( purpose ) {
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					// might be a case of *(p+i), which can be done as p[i];
					if ( Arg is AddressOfTreeNode addrOf1 ) {
						// 3404
						// 3401: collapse *&(expr) on RHS and just generate code for the underlying expr
						context.SetPrettyPrintProlog ( "<skipped> " );
						context.PrettyPrint ( Arg );
						return addrOf1.Arg.GenerateCodeForValueWithPrettyPrint ( context, purpose );
					} else {
						if ( Arg is AdditionTreeNode addition )
						{
							context.SetPrettyPrintProlog ( "<skipped> " );
							context.PrettyPrint ( Arg );
							addition.Left.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
							addition.Right.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
							context.GenerateInstruction ( "Subscript" );								
						}
						else
						{
							Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
							context.GenerateInstruction ( "Indirection" );
						}
					}
					break;
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					break;
				case EvaluationIntention.AddressOrNode:
					if ( Arg is AddressOfTreeNode addrOf2 ) {
						// 3401: collapse *&(expr) on LHS and just generate code for the underlying expr
						context.SetPrettyPrintProlog ( "<skipped> " );
						context.PrettyPrint ( Arg );
						return addrOf2.Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
					}
					else {
						// NB: if we did ValueOrNode here, we'd have to handle Node results specially
						//	as we cannot allow a Node from ValueOrNode unmodified as AddressOrNode
						Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					}
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}
}