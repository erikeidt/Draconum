
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 *
 * Author: Erik L. Eidt
 * Created: 01-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share. See COPYRIGHT.md for more details.
 *
 */


namespace com.erikeidt.Draconum {
	using static Operators;
	using static Operators.Operator;

	partial class ExpressionParser {
		public AbstractSyntaxTree BuildUnaryTreeNode ( Operator op )
		{
			var arg = _operandStack.Pop ();
			var res = (AbstractSyntaxTree) null;
			switch ( op ) {
				case PostfixIncrement: res = new PostfixIncrementTreeNode ( op, arg ); break;
				case PostfixDecrement: res = new PostfixDecrementTreeNode ( op, arg ); break;
				case PrefixIncrement: res = new PrefixIncrementTreeNode ( op, arg ); break;
				case PrefixDecrement: res = new PrefixDecrementTreeNode ( op, arg ); break;
				case FixPoint: res = new FixPointTreeNode ( op, arg ); break;
				case Negation: res = new NegationTreeNode ( op, arg ); break;
				case LogicalNot: res = new LogicalNotTreeNode ( op, arg ); break;
				case BitwiseComplement: res = new BitwiseComplementTreeNode ( op, arg ); break;
				case Indirection: res = new IndirectionTreeNode ( op, arg ); break;
				case AddressOf: res = new AddressOfTreeNode ( op, arg ); break;
			}
			return res;
		}
		public AbstractSyntaxTree BuildBinaryTreeNode ( Operator op )
		{
			var right = _operandStack.Pop ();
			var left = _operandStack.Pop ();
			var res = (AbstractSyntaxTree) null;
			switch ( op ) {
				case FunctionCall: res = new FunctionCallTreeNode ( op, left, right ); break;
				case Subscript: res = new SubscriptTreeNode ( op, left, right ); break;
				case Selection: res = new SelectionTreeNode ( op, left, right ); break;
				case IndirectSelection: res = new IndirectSelectionTreeNode ( op, left, right ); break;
				case SelectionReference: res = new SelectionReferenceTreeNode ( op, left, right ); break;
				case IndirectSelectionReference: res = new IndirectSelectionReferenceTreeNode ( op, left, right ); break;
				case Multiplication: res = new MultiplicationTreeNode ( op, left, right ); break;
				case Division: res = new DivisionTreeNode ( op, left, right ); break;
				case Modulo: res = new ModuloTreeNode ( op, left, right ); break;
				case Addition: res = new AdditionTreeNode ( op, left, right ); break;
				case Subtraction: res = new SubtractionTreeNode ( op, left, right ); break;
				case BitwiseLeftShift: res = new BitwiseLeftShiftTreeNode ( op, left, right ); break;
				case BitwiseRightShift: res = new BitwiseRightShiftTreeNode ( op, left, right ); break;
				case Order: res = new OrderTreeNode ( op, left, right ); break;
				case LessThan: res = new LessThanTreeNode ( op, left, right ); break;
				case LessOrEqual: res = new LessOrEqualTreeNode ( op, left, right ); break;
				case GreaterThan: res = new GreaterThanTreeNode ( op, left, right ); break;
				case GreaterOrEqual: res = new GreaterOrEqualTreeNode ( op, left, right ); break;
				case EqualEqual: res = new EqualEqualTreeNode ( op, left, right ); break;
				case NotEqual: res = new NotEqualTreeNode ( op, left, right ); break;
				case BitwiseAnd: res = new BitwiseAndTreeNode ( op, left, right ); break;
				case BitwiseXor: res = new BitwiseXorTreeNode ( op, left, right ); break;
				case BitwiseOr: res = new BitwiseOrTreeNode ( op, left, right ); break;
				case ShortCircutAnd: res = new ShortCircutAndTreeNode ( op, left, right ); break;
				case ShortCircutOr: res = new ShortCircutOrTreeNode ( op, left, right ); break;
				case Assignment: res = new AssignmentTreeNode ( op, left, right ); break;
				case AssignmentMultiplication: res = new AssignmentMultiplicationTreeNode ( op, left, right ); break;
				case AssignmentDivision: res = new AssignmentDivisionTreeNode ( op, left, right ); break;
				case AssignmentModulo: res = new AssignmentModuloTreeNode ( op, left, right ); break;
				case AssignmentAddition: res = new AssignmentAdditionTreeNode ( op, left, right ); break;
				case AssignmentSubtraction: res = new AssignmentSubtractionTreeNode ( op, left, right ); break;
				case AssignmentBitwiseAnd: res = new AssignmentBitwiseAndTreeNode ( op, left, right ); break;
				case AssignmentBitwiseXor: res = new AssignmentBitwiseXorTreeNode ( op, left, right ); break;
				case AssignmentBitwiseOr: res = new AssignmentBitwiseOrTreeNode ( op, left, right ); break;
				case AssignmentBitwiseLeftShift: res = new AssignmentBitwiseLeftShiftTreeNode ( op, left, right ); break;
				case AssignmentBitwiseRightShift: res = new AssignmentBitwiseRightShiftTreeNode ( op, left, right ); break;
				case ExpressionSeparator: res = new ExpressionSeparatorTreeNode ( op, left, right ); break;
				case ArgumentSeparator: res = new ArgumentSeparatorTreeNode ( op, left, right ); break;
			}
			return res;
		}

		public AbstractSyntaxTree BuildTernaryTreeNode ( Operator op )
		{
			var post = _operandStack.Pop ();
			var mid = _operandStack.Pop ();
			var pre = _operandStack.Pop ();
			var res = (AbstractSyntaxTree) null;
			switch ( op ) {
				case TernaryChoice: res = new TernaryChoiceTreeNode ( op, pre, mid, post ); break;
			}
			return res;
		}
	}
}


/*
Unary State Tokens & Operators
		!	LogicalNot
		&	AddressOf
		(	PrecedenceGroupingParenthesis
		)	EmptyArgumentList
		*	Indirection
		+	FixPoint
		++	PrefixIncrement
		-	Negation
		--	PrefixDecrement
		]	EmptySubscript
		~	BitwiseComplement

Binary State Tokens & Operators
		!=	NotEqual
		%	Modulo
		%=	AssignmentModulo
		&	BitwiseAnd
		&&	ShortCircutAnd
		&=	AssignmentBitwiseAnd
		(	FunctionCall
		)	CloseParenOrArgList
		*	Multiplication
		*=	AssignmentMultiplication
		+	Addition
		++	PostfixIncrement
		+=	AssignmentAddition
		,	ArgumentSeparator
		,	ExpressionSeparator
		-	Subtraction
		--	PostfixDecrement
		-=	AssignmentSubtraction
		->	IndirectSelection
		->*	IndirectSelectionReference
		.	Selection
		.*	SelectionReference
		/	Division
		/=	AssignmentDivision
		:	TernaryChoice
		<	LessThan
		<<	BitwiseLeftShift
		<<=	AssignmentBitwiseLeftShift
		<=	LessOrEqual
		<=>	Order
		=	Assignment
		==	EqualEqual
		>	GreaterThan
		>=	GreaterOrEqual
		>>	BitwiseRightShift
		>>=	AssignmentBitwiseRightShift
		?	TernaryTest
		[	Subscript
		]	CloseSubscript
		^	BitwiseXor
		^=	AssignmentBitwiseXor
		|	BitwiseOr
		|=	AssignmentBitwiseOr
		||	ShortCircutOr
*/
