namespace com.erikeidt.Draconum
{
	partial class LogicalNotTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			Arg.GenerateCodeForValue ( context, purpose );
			switch ( purpose ) {
				case EvaluationIntention.SideEffectsOnly:
					Arg.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
					return null;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					Arg.GenerateCodeForValue ( context, EvaluationIntention.Value );
					context.GenerateInstruction ( "NEG" );
					return null;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}

		public override void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			Arg.GenerateCodeForConditionalBranch ( context, label, !reverse );
		}
	}
}