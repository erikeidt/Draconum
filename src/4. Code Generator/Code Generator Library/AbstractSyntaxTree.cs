
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