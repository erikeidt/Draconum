namespace com.erikeidt.Draconum
{
	partial class ExpressionSeparatorTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					return Right.GenerateCodeForValue ( context, purpose );
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}
}