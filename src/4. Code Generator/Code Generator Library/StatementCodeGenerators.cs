
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

using System.IO;
using System.Collections.Generic;

// ReSharper disable PartialTypeWithSinglePart

namespace com.erikeidt.Draconum
{
	partial class EmptyStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			// do nothing!
		}
	}


	partial class DeclarationStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			// do nothing?
		}
	}

	partial class ExpressionStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			Expression.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
		}
	}

	partial class BlockStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			for ( int i = 0; i < Statements.Count; i++ )
				Statements[i].GenerateCode ( context );
		}

		public override BranchTargetLabel GetBranchTarget ()
		{
			// if the block statement is blank, then it does not provide target
			//	(if there is a larger control structure like if around the empty block statement as then part, it might)
			if ( Statements.Count == 0 )
				return null;
			// otherwise if the first 
			return Statements [ 0 ].GetBranchTarget ();
		}

		public override bool EndsLive ()
		{
			if ( Statements.Count == 0 )
				return true;
			// We're just checking the last statement in the block
			//	whereas we could also check for unreachable code inside a block...
			//		but at least we err on the conservative side.
			//		i.e. conservative == return true;
			return Statements [ Statements.Count - 1 ].EndsLive ();
		}
	}

	partial class IfStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			var target = ThenPart.GetBranchTarget ();
			if ( target != null )
			{
				Condition.GenerateCodeForConditionalBranch ( context, target, false );
				ElsePart?.GenerateCode ( context );
			}
			else if ( ElsePart == null )
			{
				var joinPoint = context.CreateLabel ();
				Condition.GenerateCodeForConditionalBranch ( context, joinPoint, false );
				ThenPart.GenerateCode ( context );
				context.PlaceLabelHere ( joinPoint );
			}
			/* else if ( ElsePart.IsBranchStatement () { }*/
			else
			{
				var elsePartLabel = context.CreateLabel ();
				Condition.GenerateCodeForConditionalBranch ( context, elsePartLabel, false );
				ThenPart.GenerateCode ( context );
				var joinPoint = context.CreateLabel ();
				context.GenerateUnconditionalBranch ( joinPoint );
				context.PlaceLabelHere ( elsePartLabel );
				ElsePart.GenerateCode ( context );
				context.PlaceLabelHere ( joinPoint );
			}
		}
	}

	partial class BreakableStatement
	{
		public BranchTargetLabel BreakLabel;
	}

	partial class ContinueableStatement
	{
		public BranchTargetLabel ContinueLabel;
	}

	partial class ForStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			context.InsertComment ( "for statement" );

			Initialization?.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );

			var loopTop = context.CreateLabel ();
			context.PlaceLabelHere ( loopTop );

			ContinueLabel = context.CreateLabel ();
			BreakLabel = context.CreateLabel ();			

			Condition?.GenerateCodeForConditionalBranch ( context, BreakLabel, false );
			Body.GenerateCode ( context );
			context.PlaceLabelHere ( ContinueLabel );
			Increment?.GenerateCodeForValue ( context, EvaluationIntention.SideEffectsOnly );
			if ( Body.EndsLive () )
				context.GenerateUnconditionalBranch ( loopTop );

			context.PlaceLabelHere ( BreakLabel );
		}
	}

	partial class ReturnStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			ReturnValue.GenerateCodeForValue ( context, EvaluationIntention.Value );
			context.GenerateInstruction ( "RET" );
		}

		//	If there is a single sequence of exit epilog code
		//	We should use it as a return branch target, 
		//	but for now, at least, we don't know that for sure.
		//	Ultimately, the entry/epilog sequence depends somewhat on the code
		//		inside the methods, so don't necessarily know the answer at
		//			this point in code generation.
		//public override BranchTargetLabel GetBranchTarget ()
		//{
		//	if ( ReturnValue != null )
		//		return false;
		//	return ???
		//}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class BreakStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			context.GenerateUnconditionalBranch ( EnclosingStatement.BreakLabel );
		}

		public override BranchTargetLabel GetBranchTarget ()
		{
			var target = EnclosingStatement.BreakLabel;
			if ( target == null )
				throw new AssertionFailedException ( "BreakLabel not set" );
			return target;
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class ContinueStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			context.GenerateUnconditionalBranch ( EnclosingStatement.ContinueLabel );
		}

		public override BranchTargetLabel GetBranchTarget ()
		{
			var target = EnclosingStatement.ContinueLabel;
			if ( target == null )
				throw new AssertionFailedException ( "ContinueLabel not set" );
			return target;
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class UserLabel
	{
		private BranchTargetLabel _label;

		BranchTargetLabel GetBranchTargetLabel ( CodeGenContext context )
		{
			if ( _label == null )
				_label = context.CreateLabel ();
			return _label;
		}
	}

	partial class GotoStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			throw new System.NotImplementedException ();
		}

		public override bool EndsLive ()
		{
			return false;
		}
	}

	partial class LabelStatement
	{
		public override void GenerateCode ( CodeGenContext context )
		{
			throw new System.NotImplementedException ();
		}
	}
}