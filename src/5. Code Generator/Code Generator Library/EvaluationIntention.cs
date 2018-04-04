namespace com.erikeidt.Draconum
{
	/// <summary>
	/// This enum indicates the purpose for which the caller is evaluating an expression.
	///
	///
	///		SideEffectsOnly:	
	///			Used for: expression statements, since their value is simply discarded, e.g.
	///				1; -or- a; -or- a && b; -or- foo();
	///				even a=b; // we want b to assign to a but after that we discard the result.
	///			For a stack machine, this intention means not leaving anything on the stack,
	///			so that the caller (who doesn't want a value) will not have anything to pop off the stack.
	/// 
	///			NB: this is an optimization, and not strictly required.  Without it, expression 
	///			statements would use Value for the intention and then pop the unwanted value off the stack.  
	///			A simple assignment a=b; would 
	///					push b; store a; pop; 
	///			instead of the more desirable
	///					push b; pop a;
	///
	///					
	///		Value:				
	///			Used for: any operation where the actual value is required, for example,
	///				the right hand side of an assignment statement, or, an argument to a function call.
	/// 
	///			Requests placing the value of the expression at the most accessible location, e.g.
	///						for stack machine: on the top of stack
	///						for an accumulator machine: in the accumulator
	///						for a register machine, in a register
	///
	/// 
	///		ValueOrNode:		
	///			Used for: when you would need the value but not when it is just a simple variable.
	///
	///			Currently used to evaluate the function target of a function call node, because
	///				without it, all function calls would push the target address, then use
	///					indirect calls through the generated address.  
	///					So, this is another optimization that is not strictly necessary.
	/// 
	///			Compute value, if necessary, otherwise return the constant value or variable.
	///			Computation is necessary if the expression involves most any operator,
	///			but not if the expression is simply a variable (or constant)
	/// 
	///
	///		The next two are used for evaluating the left hand side of an assignment type operator
	/// 
	///		Address:	place the address at the most accessible location (e.g for stack machine, top of stack)
	///					NB: this is unimplemented at present.
	///
	///		AddressOrNode:	
	///			Used for: left hand side of assignment.	
	///			Compute address of expression if necessary, otherwise return variable.
	///			(We could think of this one as an optimization, but we never implemented the simpler "Address".)
	///			Without this, a simple assignment a=b; would require indirection as follows:
	///				pea a; push b; ipop;
	///			instead of the more desireable
	///				push b; pop a;
	/// 
	///		Note there is one more evaluation intention, which is for conditional branching.
	///		However, this is handled separately so does not have an entry in this enum.
	///
	///		(Conditional branching evaluation is handled separately because evaluation 
	///			for conditional branching requires additional parameters that other evaluation
	///			intentions do not need,	and, because as it turns out, the overrides for the 
	///			virtual methods are provided in different parent classes.)
	/// 
	/// 
	/// </summary>
	enum EvaluationIntention { SideEffectsOnly, Value, ValueOrNode, AddressOrNode }
}