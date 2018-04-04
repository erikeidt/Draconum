namespace com.erikeidt.Draconum
{
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
}