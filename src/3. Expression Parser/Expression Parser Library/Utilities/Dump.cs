using System.Collections.Generic;

namespace com.erikeidt.Draconum
{
	abstract class Dump : IDump
	{
		private static readonly List<int> Levels = new List<int> ();

		public abstract void PrettyPrint ( System.IO.TextWriter to, int level, string prolog  = "");

		public void PrettyPrint ( System.IO.TextWriter to, string prolog = "" )
		{
			Levels.Add ( 0 );
			PrettyPrint ( to, 0, prolog );
			Levels.Clear ();
		}

		public static void WriteLine ( System.IO.TextWriter to, int level, string str, string prolog = "", int arity = 0 )
		{
			while ( Levels.Count < level + 2 )
				Levels.Add ( 0 );
			Levels [ level + 1 ] = arity;

			var intro = "    ";
			// does the operator at each level have more sibilings
			for ( int i = 0 ; i < level ; i++ ) {
				var count = Levels [ i ];
				if ( count == 0 ) {
					intro += "    ";
				} else {
					intro += "|   ";
				}
			}

			// decrement parent count
			if ( level >= 0 ) {
				var parCount = Levels [ level ];
				if ( parCount > 0 )
					Levels [ level ] = --parCount;
			}

			intro += "+---";
			to.WriteLine ( "{0}{1}{2}", intro, prolog, str );
		}

	}
}