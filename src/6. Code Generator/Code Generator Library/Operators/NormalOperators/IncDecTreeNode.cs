namespace com.erikeidt.Draconum
{
	partial class PostfixIncrementTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			bool evaluateForSideEffectsOnly = false;
			switch ( purpose )
			{
				case EvaluationIntention.SideEffectsOnly:
					evaluateForSideEffectsOnly = true;
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			var what = Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
			if ( what == null )
			{
				// it is a temporary (and lvalue), so must be an address
				if ( evaluateForSideEffectsOnly )	
				{
					// value not needed for other expression

					// stack has: address
					context.GenerateInstruction ( "IINC" );
					// stack has: ...
				}
				else
				{
					// stack has: address
					context.GenerateInstruction ( "DUP" );		// copy the address
					// stack has: address, address
					context.GenerateInstruction ( "Indirection" );
					// stack has: address, value
					context.GenerateInstruction ( "SWAP" );
					// stack has: value, address
					context.GenerateInstruction ( "IINC" );
					// stack has: value
				}
			}
			else if ( what is VariableTreeNode variable )
			{
				context.GenerateInstruction ( "PUSH", variable.Value.ToString ( ) );
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );	// copy original value to leave on the stack
				context.GenerateInstruction ( "INC" );
				context.GenerateInstruction ( "POP", variable.Value.ToString ( ) );
			}
			else
			{
				throw new AssertionFailedException ( "unexpected target" + what.ToString () );
			}

			return null;
		}
	}

	partial class PostfixDecrementTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			bool evaluateForSideEffectsOnly = false;
			switch ( purpose )
			{
				case EvaluationIntention.SideEffectsOnly:
					evaluateForSideEffectsOnly = true;
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			var what = Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
			if ( what == null )
			{
				// it is a temporary (and lvalue), so must be an address
				if ( evaluateForSideEffectsOnly )	
				{
					// value not needed for other expression

					// stack has: address
					context.GenerateInstruction ( "IDEC" );
					// stack has: ...
				}
				else
				{
					// stack has: address
					context.GenerateInstruction ( "DUP" );		// copy the address
					// stack has: address, address
					context.GenerateInstruction ( "Indirection" );
					// stack has: address, value
					context.GenerateInstruction ( "SWAP" );
					// stack has: value, address
					context.GenerateInstruction ( "IDEC" );
					// stack has: value
				}
			}
			else if ( what is VariableTreeNode variable )
			{
				context.GenerateInstruction ( "PUSH", variable.Value.ToString ( ) );
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );	// copy original value to leave on the stack
				context.GenerateInstruction ( "DEC" );
				context.GenerateInstruction ( "POP", variable.Value.ToString ( ) );
			}
			else
			{
				throw new AssertionFailedException ( "unexpected target" + what.ToString () );
			}

			return null;
		}
	}

	partial class PrefixIncrementTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			bool evaluateForSideEffectsOnly = false;
			switch ( purpose )
			{
				case EvaluationIntention.SideEffectsOnly:
					evaluateForSideEffectsOnly = true;
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			var what = Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
			if ( what == null )
			{
				// it is a temporary (and lvalue), so must be an address

				// stack has: address
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );		// stack has: address, address
				context.GenerateInstruction ( "IINC" );			// stack has: address
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "Indirection" );	// stack has: value
			}
			else if ( what is VariableTreeNode variable )
			{
				context.GenerateInstruction ( "PUSH", variable.Value.ToString ( ) );
				// stack has: value
				context.GenerateInstruction ( "INC" );
				// stack has: value+1
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );	// stack has value+1, value+1
				context.GenerateInstruction ( "POP", variable.Value.ToString ( ) );
			}
			else
			{
				throw new AssertionFailedException ( "unexpected target" + what.ToString () );
			}

			return null;
		}
	}

	partial class PrefixDecrementTreeNode
	{
		protected override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			bool evaluateForSideEffectsOnly = false;
			switch ( purpose )
			{
				case EvaluationIntention.SideEffectsOnly:
					evaluateForSideEffectsOnly = true;
					break;
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			var what = Arg.GenerateCodeForValueWithPrettyPrint ( context, EvaluationIntention.AddressOrNode );
			if ( what == null )
			{
				// it is a temporary (and lvalue), so must be an address

				// stack has: address
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );		// stack has: address, address
				context.GenerateInstruction ( "IDEC" );			// stack has: address
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "Indirection" );	// stack has: value
			}
			else if ( what is VariableTreeNode variable )
			{
				context.GenerateInstruction ( "PUSH", variable.Value.ToString ( ) );
				// stack has: value
				context.GenerateInstruction ( "DEC" );
				// stack has: value+1
				if ( ! evaluateForSideEffectsOnly )
					context.GenerateInstruction ( "DUP" );	// stack has value+1, value+1
				context.GenerateInstruction ( "POP", variable.Value.ToString ( ) );
			}
			else
			{
				throw new AssertionFailedException ( "unexpected target" + what.ToString () );
			}

			return null;
		}
	}
}