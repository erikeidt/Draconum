
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


// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum {
	using static Operators;

	partial class PostfixIncrementTreeNode : UnaryOperatorTreeNode {
		public PostfixIncrementTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class PostfixDecrementTreeNode : UnaryOperatorTreeNode {
		public PostfixDecrementTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class FunctionCallTreeNode : BinaryOperatorTreeNode {
		public FunctionCallTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class SubscriptTreeNode : BinaryOperatorTreeNode {
		public SubscriptTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class SelectionTreeNode : BinaryOperatorTreeNode {
		public SelectionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class IndirectSelectionTreeNode : BinaryOperatorTreeNode {
		public IndirectSelectionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class PrefixIncrementTreeNode : UnaryOperatorTreeNode {
		public PrefixIncrementTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class PrefixDecrementTreeNode : UnaryOperatorTreeNode {
		public PrefixDecrementTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class FixPointTreeNode : UnaryOperatorTreeNode {
		public FixPointTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class NegationTreeNode : UnaryOperatorTreeNode {
		public NegationTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class LogicalNotTreeNode : UnaryOperatorTreeNode {
		public LogicalNotTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class BitwiseComplementTreeNode : UnaryOperatorTreeNode {
		public BitwiseComplementTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class IndirectionTreeNode : UnaryOperatorTreeNode {
		public IndirectionTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class AddressOfTreeNode : UnaryOperatorTreeNode {
		public AddressOfTreeNode ( Operator op, AbstractSyntaxTree arg ) : base ( op, arg ) { }
	}

	partial class SelectionReferenceTreeNode : BinaryOperatorTreeNode {
		public SelectionReferenceTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class IndirectSelectionReferenceTreeNode : BinaryOperatorTreeNode {
		public IndirectSelectionReferenceTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class MultiplicationTreeNode : BinaryOperatorTreeNode {
		public MultiplicationTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class DivisionTreeNode : BinaryOperatorTreeNode {
		public DivisionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class ModuloTreeNode : BinaryOperatorTreeNode {
		public ModuloTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AdditionTreeNode : BinaryOperatorTreeNode {
		public AdditionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class SubtractionTreeNode : BinaryOperatorTreeNode {
		public SubtractionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class BitwiseLeftShiftTreeNode : BinaryOperatorTreeNode {
		public BitwiseLeftShiftTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class BitwiseRightShiftTreeNode : BinaryOperatorTreeNode {
		public BitwiseRightShiftTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class OrderTreeNode : BinaryOperatorTreeNode {
		public OrderTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class LessThanTreeNode : RelationalOperatorTreeNode {
		public LessThanTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class LessOrEqualTreeNode : RelationalOperatorTreeNode {
		public LessOrEqualTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class GreaterThanTreeNode : RelationalOperatorTreeNode {
		public GreaterThanTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class GreaterOrEqualTreeNode : RelationalOperatorTreeNode {
		public GreaterOrEqualTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class EqualEqualTreeNode : RelationalOperatorTreeNode {
		public EqualEqualTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class NotEqualTreeNode : RelationalOperatorTreeNode {
		public NotEqualTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class BitwiseAndTreeNode : BinaryOperatorTreeNode {
		public BitwiseAndTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class BitwiseXorTreeNode : BinaryOperatorTreeNode {
		public BitwiseXorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class BitwiseOrTreeNode : BinaryOperatorTreeNode {
		public BitwiseOrTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class ShortCircutAndTreeNode : BinaryOperatorTreeNode {
		public ShortCircutAndTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class ShortCircutOrTreeNode : BinaryOperatorTreeNode {
		public ShortCircutOrTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class TernaryChoiceTreeNode : TernaryOperatorTreeNode {
		public TernaryChoiceTreeNode ( Operator op, AbstractSyntaxTree pre, AbstractSyntaxTree mid, AbstractSyntaxTree post ) : base ( op, pre, mid, post ) { }
	}

	partial class AssignmentTreeNode : BinaryOperatorTreeNode {
		public AssignmentTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentMultiplicationTreeNode : BinaryOperatorTreeNode {
		public AssignmentMultiplicationTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentDivisionTreeNode : BinaryOperatorTreeNode {
		public AssignmentDivisionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentModuloTreeNode : BinaryOperatorTreeNode {
		public AssignmentModuloTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentAdditionTreeNode : BinaryOperatorTreeNode {
		public AssignmentAdditionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentSubtractionTreeNode : BinaryOperatorTreeNode {
		public AssignmentSubtractionTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentBitwiseAndTreeNode : BinaryOperatorTreeNode {
		public AssignmentBitwiseAndTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentBitwiseXorTreeNode : BinaryOperatorTreeNode {
		public AssignmentBitwiseXorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentBitwiseOrTreeNode : BinaryOperatorTreeNode {
		public AssignmentBitwiseOrTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentBitwiseLeftShiftTreeNode : BinaryOperatorTreeNode {
		public AssignmentBitwiseLeftShiftTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class AssignmentBitwiseRightShiftTreeNode : BinaryOperatorTreeNode {
		public AssignmentBitwiseRightShiftTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class ExpressionSeparatorTreeNode : BinaryOperatorTreeNode {
		public ExpressionSeparatorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

	partial class ArgumentSeparatorTreeNode : BinaryOperatorTreeNode {
		public ArgumentSeparatorTreeNode ( Operator op, AbstractSyntaxTree left, AbstractSyntaxTree right ) : base ( op, left, right ) { }
	}

}
