
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

using System.ComponentModel.Design.Serialization;
using System.Net.Mime;

namespace com.erikeidt.Draconum
{
	partial class AbstractSyntaxTree
	{
		/// <summary>
		///		Generates code for the given tree node.
		/// </summary>
		/// <param name="context">
		///		where the generated code is output to, and some helper functions
		/// </param>
		/// <param name="purpose">
		///		A tree can be evalutated for code generation:
		///			for side effects only:
		///				used to evaluate statements that discard results
		///				expect null for return value
		///			for the value produced by the expression (unconditionally):
		///				used to evaluate most operators in an expression context
		///				expect null for return value meaning the result is in the "temporary" location
		///			for the value produced by the expression when complex, or the node when a simple variable
		///				used to evaluate function expressions
		///				expect null meaning the result is in the "temporary" location, or,
		///				a variable if the expression evaluates to a simple variable
		///			for the address of the expression when complex, or the node when a simple variable
		///				used to evaluate left-hand-side of assignment operators
		///				expect null when the result is in the "temporary" location, or,
		///				variable if the result is the address of a variable
		///		Note: a tree can also be evalutated for conditional branch, but that is done with GenerateCodeForConditionalBranch, below.
		///		See EvaluationIntention for more details.
		/// </param>
		/// <returns>
		///		The return value takes on meaning in the context of the intended purpose of evaluation.
		///		It is a tree node that captures the mode of the loaded value:
		///			null		-- indicating the expression result is a "temporary", or is nothing
		///			variable	-- if the temporary result is a variable or address
		/// </returns>
		/// This is meant to be overridden by subclasses, but not called; hence protected!
		protected abstract AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose );

		// This on is to be invoked; it performs pretty printing, and then invokes the protected (real/overridden) one.
		public AbstractSyntaxTree GenerateCodeForValueWithPrettyPrint ( CodeGenContext context, EvaluationIntention purpose )
		{
			context.PrettyPrint ( this );
			return GenerateCodeForValue ( context, purpose );
		}

		/// <summary>
		///		Generates code for the given tree node, with the purpose of accomplishing a conditional branch to target
		///			or falling thru to whatever comes after this generated code.
		/// </summary>
		/// <param name="context">
		///		where the generated code is output to, and some helper functions
		/// </param>
		/// <param name="label">
		///		the desired target of the conditional branch
		/// </param>
		/// <param name="reverse">
		/// 	the reverse parameter's meaning:
		/// 
		///			false --> use the normal if-then-style evaluation:
		///						take branch on condition false and fall thru on condition true
		/// 
		///			true  --> use the reverse of the normal sense of if-then evaluation:
		///						take branch on condition true and fall thru on condition false
		///						one might use this, if for some reason
		///							we had choosen to swap the code ordering of the then-part and else-part
		///						or, we need to evaluate something involving negation
		///						or, we're evaluating a do ... while instead of while.
		/// 
		///		NB: The normal approach for an if-then-statement, e.g. in:
		///				if ( a ) { b=1; }
		///			is that we evaluate "a" and 
		///				conditionally branch: take branch around the then-part on "a" being false, or else,
		///					fall thru to then-part (e.g. { b=1; } ) on "a" being true.
		///
		/// 	*If* statements and *While* statements and *For* statements evaluate their condition to 
		///			take the branch (around the then or the loop) on the expressed condition being false, and
		///				fall-thru (into the then-part or loop-body) on it being true.
		/// 
		///		Sometimes we need to reverse things, e.g.:
		///			To accomplish negation operator as in
		///					if ( ! a ) { b=1; }
		///				We would start by evaluating "! a" as with any if-statemetn: for conditional branch with reverse=false, 
		///					(meaning take branch on false, and fall thru on true)
		///				However, the "!" will be handled by reversing the evaluation condition (for the further evaluation of "a")
		///					rather than generating any actual negation code.
		/// 
		///			Also, *do while* statements evaluate the conditional for branch oppositely than regular while:
		///				take branch (backward, to repeat the loop) on true, and fall-thru (exiting the loop) on false.
		///			So, *do while* should evaluate its conditional expression using reverse=true.
		///
		///			Further note that the short-circut boolean || operator requires 
		///				evaluation of the left/first operand under reversed conditions:
		///					this in order to branch around the right/second operand (short-circuting it).
		///				So, to accomplish the fall thru to the then-part or loop-body, 
		///					the left/first operand is evaluated to take branch (condition true) into the then-part
		///					and the right/second operand is evaluated to take branch (condition false) around the then-part
		/// 
		///			Combinations of &&, || and ! will reverse evaluation conditions as needed.
		///
		///			See the overrides:
		///				ShortCircutAndTreeNode.GenerateCodeForConditionalBranch, and,
		///				ShortCircutOrTreeNode.GenerateCodeForConditionalBranch
		///
		/// </param>
		protected virtual void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			//
			// This implementation is used by any operator that does not customize the evaluation for conditional branch:
			// This default implementation generates the value to be tested, and then does a test & branch on that value.
			// 
			GenerateCodeForValue/*WithPrettyPrint*/ ( context, EvaluationIntention.Value );	// pretty print already done at this level
			context.GenerateUnaryTestConditionalBranch ( label, reverse );

			// Note: ! && || operators can generate substantially better code using custom implementation rather than this generic one.
			// Negation is discussed above: 
			//	rather than generating code to negate a boolean and test & branch on that,
			//		negation simply evaluates the negated expression with reversed branch condition.
			// The short-circut operators also need not generate a boolean value to be tested & branched on, 
			//	and can instead directly branch on the conditions of the left-hand and right-hand side expressions.
		}

		public virtual void GenerateCodeForConditionalBranchWithPrettyPrint ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			context.PrettyPrint ( this );	// prints && operator
			GenerateCodeForConditionalBranch ( context, label, reverse );
		}
	}
}