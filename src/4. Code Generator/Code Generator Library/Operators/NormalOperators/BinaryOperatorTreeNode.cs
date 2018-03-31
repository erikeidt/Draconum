namespace com.erikeidt.Draconum
{
	partial class BinaryOperatorTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
					Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( Op.ToString () );
					return null;
				case EvaluationIntention.AddressOrNode:
					Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
					Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( "AddIndex" );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}
	}
}