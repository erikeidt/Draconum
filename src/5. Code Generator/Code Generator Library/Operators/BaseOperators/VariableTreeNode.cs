namespace com.erikeidt.Draconum
{
	partial class VariableTreeNode : AbstractSyntaxTree
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
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