namespace com.erikeidt.Draconum
{
	partial class PostfixIncrementTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			bool so = false;
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					so = true;
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			var what = Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			if ( what == null ) {
				if ( !so )
					context.GenerateInstruction ( "DUP" );
				context.GenerateInstruction ( "IINC" );
				if ( !so )
					context.GenerateInstruction ( "DEREF" );
			} else if ( what is VariableTreeNode variable ) {
				context.GenerateInstruction ( "PUSH", variable.Value.ToString () );
				if ( !so )
					context.GenerateInstruction ( "DUP" );
				context.GenerateInstruction ( "INC" );
				context.GenerateInstruction ( "POP", variable.Value.ToString () );
			}
			return null;
		}
	}
}