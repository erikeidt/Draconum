
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	partial class VariableTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", Value.ToString () );
				break;
			case EvaluationIntention.ValueOrNode:
				return this;
			case EvaluationIntention.SideEffectsOnly:
				break;
			case EvaluationIntention.AddressOrNode:
				return this;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class LongIntegerTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", string.Format ( "#{0}", Value ) );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class StringTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// NB: a C string's value is its address.  (There is no notion of a string's address.)
			switch ( purpose ) {
			case EvaluationIntention.Value:
				context.GenerateInstruction ( "PUSH", string.Format ( "\"{0}\"", Value ) );
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

	partial class AssignmentTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			// Old form evaluates Right first then Left
			//	This is great for the stack machine since only a simple DUP is required
			//	However, it evaluates the arguments out of order instead of left to right
			//Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
			//switch ( purpose ) {
			//case EvaluationIntention.Value:
			//case EvaluationIntention.ValueOrNode:
			//	context.GenerateInstruction ( "DUP" );
			//	break;
			//case EvaluationIntention.SideEffectsOnly:
			//	break;
			//default:
			//	throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			//}
			//var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			//if ( target == null ) {
			//	context.GenerateInstruction ( "IPOP" );
			//} else if ( target is VariableTreeNode variable ) {
			//	context.GenerateInstruction ( "POP", variable.Value.ToString () );
			//} else {
			//	throw new AssertionFailedException ( "unexpected result for LHS" );
			//}

			var target = Left.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );

			Right.GenerateCodeForValue ( context, EvaluationIntention.Value );

			var keep = false;
			switch ( purpose ) {
			case EvaluationIntention.Value:
			case EvaluationIntention.ValueOrNode:
				keep = true;
				break;
			case EvaluationIntention.SideEffectsOnly:
				break;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			if ( target == null ) {
				if ( keep ) {
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftY = RightTop
					//		and pops only Left off the stack, leaving Right
					context.GenerateInstruction ( "ISTORE" );
				} else {
					// Stack has LeftY | RightTop <-- stack top
					//	This instruction does *LeftYT = RightTop
					//		and pops both Left and Right off the stack
					context.GenerateInstruction ( "IPOP" );
				}
			} else if ( target is VariableTreeNode variable ) {
				if ( keep ) {
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does not pop Right
					context.GenerateInstruction ( "STORE", variable.Value.ToString () );
				}
				else {
					// Stack has RightTop <-- stack top
					//	This instruction does var = RightTop
					//		and does pop Right off the stack
					context.GenerateInstruction ( "POP", variable.Value.ToString () );
				}
			} else {
				throw new AssertionFailedException ( "unexpected result for LHS" );
			}

			return null;
		}
	}

	partial class AddressOfTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.SideEffectsOnly:
				Arg.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				break;
			case EvaluationIntention.Value:
				var ans = Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );

				if ( ans != null ) {
					if ( ans is VariableTreeNode variable ) {
						context.GenerateInstruction ( "PUSH", "&" + variable.Value.ToString () );
					} else {
						throw new AssertionFailedException ( "unexpected result for &" );
					}
				}
				break;
			case EvaluationIntention.ValueOrNode:
				return Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
			return null;
		}

	}

	partial class BinaryOperatorTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.SideEffectsOnly:
				Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				return null;
			case EvaluationIntention.Value:
			case EvaluationIntention.ValueOrNode:
				Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
				Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
				context.GenerateInstruction ( Op.ToString () );
				return null;
			case EvaluationIntention.AddressOrNode:
				Left.GenerateCodeForValue ( context, EvaluationIntention.Value );
				Right.GenerateCodeForValue ( context, EvaluationIntention.Value );
				context.GenerateInstruction ( "AddIndex" );
				return null;
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}
		}
	}

	partial class PostfixIncrementTreeNode
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

			var what = Arg.GenerateCodeForValue ( context, EvaluationIntention.AddressOrNode );
			if ( what == null ) {
				if ( !so )
					context.GenerateInstruction ( "DUP" );
				context.GenerateInstruction ( "IINC" );
				if ( !so )
					context.GenerateInstruction ( "DEREF" );
			} else if ( what is VariableTreeNode variable ) {
				context.GenerateInstruction ( "PUSH", variable.Value.ToString () );
				if ( !so )
					context.GenerateInstruction ( "DUP" );
				context.GenerateInstruction ( "INC" );
				context.GenerateInstruction ( "POP", variable.Value.ToString () );
			}
			return null;
		}
	}

	partial class ExpressionSeparatorTreeNode
	{
		public override AbstractSyntaxTree GenerateCodeForValue ( CodeGenContext context, EvaluationIntention purpose )
		{
			switch ( purpose ) {
			case EvaluationIntention.SideEffectsOnly:
				Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				Right.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				break;
			case EvaluationIntention.Value:
			case EvaluationIntention.ValueOrNode:
				Left.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
				return Right.GenerateCodeForValue ( context, purpose );
			default:
				throw new AssertionFailedException ( "unexpected evaluation intention" + purpose );
			}

			return null;
		}
	}

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