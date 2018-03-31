namespace com.erikeidt.Draconum
{
	partial class TernaryChoiceTreeNode
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
			var secondChoice = context.CreateLabel ();
			Pre.GenerateCodeForConditionalBranch ( context, secondChoice, false );

			Mid.GenerateCodeForValue ( context, so ? EvaluationIntention.SideEffectsOnly : EvaluationIntention.Value );
			var joinPoint = context.CreateLabel ();
			context.GenerateUnconditionalBranch ( joinPoint );
			context.PlaceLabelHere ( secondChoice );
			Post.GenerateCodeForValue ( context, so ? EvaluationIntention.SideEffectsOnly : EvaluationIntention.Value );
			context.PlaceLabelHere ( joinPoint );
			return null;
		}
	}
}