namespace com.erikeidt.Draconum
{
	partial class FunctionCallTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			var functionToCall = Left.GenerateCodeForValue ( context, EvaluationIntention.ValueOrNode );

			// NB: Function calls are binary operators in the sense that they have two arguments.
			// The first argument is the function to call, and the second is:
			//	possibly null -- if no arguments are supplied, or,
			//	a single parameter -- if one argument is supplied, or,
			//	a tree of ArgumentSeparator, whose left is 
			//		a single parameter, or,
			//		a tree of ArugmentSeparator ...
			//		and whose Right is a single parameter.

			var argCount = context.EvaluateArgumentList ( Right );
			if ( functionToCall == null )
				context.GenerateInstruction ( "ICALL", string.Format ( "#{0}", argCount ) );
			else if ( functionToCall is VariableTreeNode variable ) {
				context.GenerateInstruction ( "CALL", variable.Value.ToString (), string.Format ( "#{0}", argCount ) );
			} else {
				throw new AssertionFailedException ( "unknown function" );
			}

			if ( purpose == EvaluationIntention.SideEffectsOnly )
				context.GenerateInstruction ( "POP" );

			return null;
		}
	}
}