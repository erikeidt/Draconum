
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 04-10-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	using static Operators;
	using static Operators.Operator;

	partial class AssignmentOperatorTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// When we ask for AddressOrNode, we will get either an address
			//		e.g. for a[i] += ..., we'll get a+i on the stack, or,
			//		for i += ..., we'll get nothing on the stack, so we can pop directly into i
			var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			var variable = target as VariableTreeNode;

			if ( Op != Assignment )
				GenerateLHSValue ( context, target, variable );

			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );

			var keep = false;
			switch ( purpose )
			{
				case EvaluationIntention.Value:
				case EvaluationIntention.ValueOrNode:
					keep = true;
					break;
				case EvaluationIntention.SideEffectsOnly:
					break;
				default:
					throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			if ( Op != Assignment )
				GenerateAssignmentComputation ( context );

			if ( target == null )
			{
				// this case is that we generated an address onto the stack for the target
				//	so, we need to use indirection operators
				if ( keep )
				{
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftY = RightTop
					//		and pops only Left off the stack, leaving Right
					context.GenerateInstruction ( "ISTORE" );
				}
				else
				{
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftYT = RightTop
					//		and pops both Left and Right off the stack
					context.GenerateInstruction ( "IPOP" );
				}
			}
			else
			{
				// this case is that we generated nothing onto the stack for the left hand side
				//	so, we'll pop or store directly into 
				if ( keep )
				{
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does not pop Right
					//	This form is used when the assignment is used 
					//		as in f(a=b); in which b is assigned into a, and the value is passed to f
					context.GenerateInstruction ( "STORE", variable.Value.ToString () );
				}
				else
				{
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does pop Right off the stack, because the value is not wanted.
					context.GenerateInstruction ( "POP", variable.Value.ToString () );
				}
			}

			return null;
		}

		protected void GenerateLHSValue ( CodeGenContext context, AbstractSyntaxTree target,
			VariableTreeNode variable )
		{
			if ( target == null )
			{
				// An address was generated onto the stack.
				//	We need to use this address twice, as in a[i] += 4;
				//		we need to read a[i] once, and write a[i] once,
				//		from the same pointer computation
				context.GenerateInstruction ( "DUP" );

				// and now we'll convert one of them into the value at that location
				context.GenerateInstruction ( "Indirect" );
			}
			else if ( variable != null )
			{
				context.GenerateInstruction ( "PUSH", variable.Value.ToString () );
			}
			else
			{
				throw new AssertionFailedException ( "unexpected result for LHS" );
			}
		}

		protected void GenerateAssignmentComputation ( CodeGenContext context )
		{
			Operator op;
			switch ( Op )
			{
				case Assignment:
					return;
				case AssignmentMultiplication:
					op = Addition;
					break;
				case AssignmentDivision:
					op = Division;
					break;
				case AssignmentModulo:
					op = Modulo;
					break;
				case AssignmentAddition:
					op = Addition;
					break;
				case AssignmentSubtraction:
					op = Subtraction;
					break;
				case AssignmentBitwiseAnd:
					op = BitwiseAnd;
					break;
				case AssignmentBitwiseXor:
					op = BitwiseXor;
					break;
				case AssignmentBitwiseOr:
					op = BitwiseOr;
					break;
				case AssignmentBitwiseLeftShift:
					op = BitwiseLeftShift;
					break;
				case AssignmentBitwiseRightShift:
					op = BitwiseRightShift;
					break;
				default:
					throw new AssertionFailedException ( "unexpected assignment operator: " + Op.ToString () );
			}

			context.GenerateInstruction ( op.ToString () );
		}
	}
}

// Very old version: this one evalutated RHS first, then LHS
//public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
//{
//	// Old form evaluates Right first then Left
//	//	This is great for the stack machine since only a simple DUP is required
//	//	However, it evaluates the arguments out of order instead of left to right
//	//Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
//	//switch ( purpose ) {
//	//case EvaluationIntention.Value:
//	//case EvaluationIntention.ValueOrNode:
//	//	context.GenerateInstruction ( "DUP" );
//	//	break;
//	//case EvaluationIntention.SideEffectsOnly:
//	//	break;
//	//default:
//	//	throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
//	//}
//	//var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
//	//if ( target == null ) {
//	//	context.GenerateInstruction ( "IPOP" );
//	//} else if ( target is VariableTreeNode variable ) {
//	//	context.GenerateInstruction ( "POP", variable.Value.ToString () );
//	//} else {
//	//	throw new AssertionFailedException ( "unexpected result for LHS" );
//	//}
