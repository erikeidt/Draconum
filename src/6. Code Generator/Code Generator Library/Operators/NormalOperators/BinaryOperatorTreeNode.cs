namespace com.erikeidt.Draconum
{
	partial class BinaryOperatorTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Left.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					Right.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Left.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					Right.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( Op.ToString () );
					return null;
				case EvaluationIntention.AddressOrNode:
					Left.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					Right.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( "AddIndex" );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}
	}
}