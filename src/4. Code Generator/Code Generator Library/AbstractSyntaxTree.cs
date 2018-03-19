
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-10-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

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

	partial class AbstractSyntaxTree
	{
		/// <summary>
		///		Generates code for the given tree node
		/// </summary>
		/// <param name="context">
		///		where the generated code is output to, and some helper functions
		/// </param>
		/// <param name="purpose">
		/// </param>
		public virtual AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			throw new System.NotImplementedException ();
		}

		/// <summary>
		///		Generates code for the given tree node, with the purpose of accomplishing a conditional branch to target
		///			or falling thru to whatever comes after this generated code.
		/// </summary>
		/// <param name="context">
		///		where the generated code is output to, and some helper functions
		/// </param>
		/// <param name="label">
		///		the target of the conditional branch
		/// </param>
		/// <param name="reverse">
		///		The normal approach for an if-statement, e.g. in:
		///				if ( a ) { b=1; }
		///			is that we evaluate "a" and 
		///				conditionally branch around the then-part on false, or else,
		///				fall thru to then-part (e.g. b=1) on true.
		///		Thus:
		///			false --> jump on test condition evaluates to false, fall thru otherwise
		///							so here, false means use the normal sense for an if-statement
		///								relative to the then-part
		///			true  --> jump on test condition evaluates to true, fall thru otherwise
		///							and true, means use the reverse sense
		/// 
		///		True is appropriate, for example, for
		///				if ( !a ) { b=1; }
		///		We would start by evaluating !a with false, meaning branch on false, and fall thru on true.
		///		However, the ! will handle by reversing the condition rather than generating any negation code.
		///		
		///		If statements and While statements and For statements evaluate their conditional for 
		///			branch (around the then or the loop) on false, and fall-thru (into the then or loop) on true.  
		///		However, do while statements evaluate the conditional for branch oppositely:
		///			branch (backward, to repeat the loop) on true, and fall-thrue (exiting the loop) on false.
		///		So, do while also evaluates condition expression with reverse=true.
		/// 
		/// </param>
		public virtual void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			// This implementation is used by any operator that does not customize the evaluation for conditional branch:
			// This default implementation generates the value to be tested, and then does a test & branch on that value.
			// 
			GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalBranch ( label, reverse );

			// Note: ! && || operators can generate substantially better code using custom implementation rather than this generic one.
			// Negation is discussed above: 
			//	rather than generating code to negate a boolean and test & branch on that,
			//		negation simply evaluates the negated expression with reversed branch condition.
			// The short-circut operators also need not generate a boolean value to be tested & branched on, 
			//	and can instead directly branch on the conditions of the left-hand and right-hand side expressions.

		}
	}
}