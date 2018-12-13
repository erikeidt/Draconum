using System;
using System.Collections.Generic;

namespace com.erikeidt.Draconum
{
	abstract class Dump : IDump
	{
		public static string LastIntro;

		// List of levels indicating how many children are left at each level.
		private static readonly List <int> Levels = new List <int> ();

		private static System.IO.TextWriter _printTextWriter;
		private static string Intro1 = "    ";
		private static string Intro2 = "        ";

		public void PrettyPrint ( System.IO.TextWriter to, string prolog = "" )
		{
			_printTextWriter = to;
			SmallFormat ();
			PrettyPrintHeader ( prolog );
			PrettyPrintBody ();
		}

		public void PrettyPrint ( string prolog = "" )
		{
			PrettyPrintHeader ( prolog );
			PrettyPrintBody ();
		}

		public abstract void PrettyPrintHeader ( string prolog = "" );
		public virtual void PrettyPrintBody () { }

		public static void WriteLine ( string str, string prolog = "", int arity = 0 )
		{
			int level = Levels.Count;

			Levels.Add ( arity );

			var intro = Intro1;
			LastIntro = intro;
			// does the operator at each level have more siblings?
			if ( level > 0 )
			{
				intro = Intro2;
				for ( int i = 0; i < level - 1; i++ )  {
					var count = Levels [ i ];
					if ( count == 0 ) {
						intro += "    ";
					} else {
						intro += "|   ";
					}
				}

				if ( Levels [ Levels.Count - 2 ] > 1 )
					LastIntro = intro + "|";
				else
					LastIntro = intro;

				// decrement parent count
				var currLevel = Levels [ level - 1 ];
				var parCount = currLevel;
				if ( parCount == 0 )
					throw new ArgumentOutOfRangeException ();
				Levels [ level - 1 ] = --parCount;
			}

			while ( Levels.Count > 0 && Levels [ Levels.Count - 1 ] == 0 )
				Levels.RemoveAt ( Levels.Count - 1 );


			_printTextWriter.WriteLine ( "{0}+---{1}{2}", intro, prolog, str );
			//System.Console.WriteLine ( "{0}+---{1}{2}", intro, prolog, str );
		}

		private static void SmallFormat ()
		{
			Intro1 = "    ";
			Intro2 = "        ";
			LastIntro = null;
		}

		public static void CommentFormat ()
		{
			Intro1 = "                             ";
			Intro2 = "                                 ";
			LastIntro = Intro1;
		}
	}
}