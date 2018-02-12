
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
		///		SideEffectsOnly:	if the value is unwanted, as would be the case in expression statement: a=b;
		///								for stack machine, this means not leaving anything on the stack
		///								so that the caller (who doesn't want a value) does not have anything to pop off the stack
		///		Value:				place the value at the most accessible location (e.g for stack machine, top of stack)
		///		ValueOrNode:		compute value if necessary, otherwise return the constant value or variable
		///								computation is necessary if the expression involves most any operator
		///									but not if the expression is simply a variable or constant
		///		Address:			place the address at the most accessible location (e.g for stack machine, top of stack)
		///		AddressOrNode:		compute address if necessary, otherwise return variable
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
		///		The normal approach for an if-statement 
		///				e.g. if ( a ) { b=1; }
		///			is that we have to evaluate "a" and conditional branch around the then-part on false
		///				and fall thru to b=1 on true.
		///		Thus:
		///			false --> jump on test condition evaluates to false, fall thru otherwise
		///						so here, false means use the normal sense for an if-statement
		///			true  --> jump on test condition evaluates to true, fall thru otherwise
		///						and true, means use the reverse sense
		/// </param>
		public virtual void GenerateCodeForConditionalBranch ( CodeGenContext context, BranchTargetLabel label, bool reverse )
		{
			GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateConditionalBranch ( label, reverse );
		}
	}
}