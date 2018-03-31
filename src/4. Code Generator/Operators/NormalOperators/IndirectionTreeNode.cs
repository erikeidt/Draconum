namespace com.erikeidt.Draconum
{
	partial class IndirectionTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// might this be a case of *&(expr)
			var addrOf = Arg as AddressOfTreeNode;

			// NB: a C string's value is its address.  
			//	(There is no notion of a string's address as that would be an address of an address.)
			switch ( purpose ) {
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					if ( addrOf != null ) {
						// 3404
						// 3401: collapse *&(expr) on RHS and just generate code for the underlying expr
						return addrOf.Arg.GenerateCodeForValue ( context, purpose );
					} else {
						Arg.GenerateCodeForValue ( context, EvaluationIntention.Value );
						context.GenerateInstruction ( "DEREF" );
					}
					break;
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					break;
				case EvaluationIntention.AddressOrNode:
					if ( addrOf != null ) {
						// 3401: collapse *&(expr) on LHS and just generate code for the underlying expr
						return addrOf.Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
					} else {
						// NB: if we did ValueOrNode here, we'd have to handle Node results specially
						//	as we cannot allow a Node from ValueOrNode unmodified as AddressOrNode
						Arg.GenerateCodeForValue ( context, EvaluationIntention.Value );
					}
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}
}