namespace com.erikeidt.Draconum
{
	partial class AssignmentTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// Old form evaluates Right first then Left
			//	This is great for the stack machine since only a simple DUP is required
			//	However, it evaluates the arguments out of order instead of left to right
			//Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			//switch ( purpose ) {
			//case EvaluationIntention.Value:
			//case EvaluationIntention.ValueOrNode:
			//	context.GenerateInstruction ( "DUP" );
			//	break;
			//case EvaluationIntention.SideEffectsOnly:
			//	break;
			//default:
			//	throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			//}
			//var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			//if ( target == null ) {
			//	context.GenerateInstruction ( "IPOP" );
			//} else if ( target is VariableTreeNode variable ) {
			//	context.GenerateInstruction ( "POP", variable.Value.ToString () );
			//} else {
			//	throw new AssertionFailedException ( "unexpected result for LHS" );
			//}

			var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );

			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );

			var keep = false;
			switch ( purpose ) {
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					keep = true;
					break;
				case EvaluationIntention.SideEffectsOnly:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			if ( target == null ) {
				if ( keep ) {
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftY = RightTop
					//		and pops only Left off the stack, leaving Right
					context.GenerateInstruction ( "ISTORE" );
				} else {
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftYT = RightTop
					//		and pops both Left and Right off the stack
					context.GenerateInstruction ( "IPOP" );
				}
			} else if ( target is VariableTreeNode variable ) {
				if ( keep ) {
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does not pop Right
					context.GenerateInstruction ( "STORE", variable.Value.ToString () );
				} else {
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does pop Right off the stack
					context.GenerateInstruction ( "POP", variable.Value.ToString () );
				}
			} else {
				throw new AssertionFailedException ( "unexpected result for LHS" );
			}

			return null;
		}
	}
}