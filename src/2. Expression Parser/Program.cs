
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-06-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

using System.IO;

namespace com.erikeidt.Draconum.ExpressionParserTest
{
	class Program
	{
		private string _testDir;
		private string _masterDir;
		private int _testsRun;
		private int _testsPassed;

		public void RunTests ()
		{
			_testDir = @"bin\Debug\TestResults\";
			Directory.CreateDirectory ( _testDir );

			_masterDir = @"Master Test Results\";

			Test ( 1001, "a+b*c-d" );
			Test ( 1002, "a?b:c" );
			Test ( 1003, "a?b,c:d" );

			Test ( 2004, "(a)+b*c-d" );
			Test ( 2005, "(a+b)*(c-d)" );

			Test ( 3006, "f(a,b)" );
			Test ( 3007, "f((a,b))" );
			Test ( 3008, "f()" );
			Test ( 3009, "f()+g()" );
			Test ( 3010, "f(a,b,c)" );
			Test ( 3011, "f(a,b,c,d)" );
			Test ( 3012, "f((a,b,c),d)" );
			Test ( 3013, "f((a,b),(c,d))" );

			Test ( 3240, "a[b]" );
			Test ( 3241, "a[b,c]" );

			Test ( 3514, "a=b" );
			Test ( 3515, "a+b+c" );
			Test ( 3516, "a=b=c" );
			Test ( 3517, "a=b+c" );
			Test ( 3518, "a+=b*c" );
			Test ( 3519, "a+=b=c" );
			Test ( 3520, "a+=b+=c" );

			Test ( 5021, "a:b" );
			Test ( 5022, "a:b", ':' );
			Test ( 5023, "a", ')' );
			Test ( 5024, "a)" );
			Test ( 5025, "a)", ')' );
			Test ( 5026, "a,b" );
			Test ( 5027, "(a,b),(c,d)" );
			Test ( 5028, "a,b", ',' );
			Test ( 5029, "(a,b),(c,d)", ',' );

			Test ( 6030, "a=b+c;" );
			Test ( 6031, ";" );

			Test ( 7032, "\"a string\"" );
			Test ( 7033, "123" );
			Test ( 7034, "'I'" );
			Test ( 7035, "''" );
			Test ( 7036, "'II'" );
			Test ( 7037, "\"abc" );
			Test ( 7038, "\"\"" );
			Test ( 7039, "-404" );

			
			Test ( 9042, "// hello\n4" );
			Test ( 9043, "a/* comment */+/*comment*/b" );

			Test ( 9044, "abcdef=ghijkl" );
			Test ( 9045, "abc/**/def=444" );
			Test ( 9046, "abc def" );
			Test ( 9047, "\"hello\\nthere\"" );
			Test ( 9048, "'\n'" );
			Test ( 9049, "'\\n'" );

			System.Console.WriteLine ();
			System.Console.WriteLine ( "Tests run: {0}, Test Passed: {1}", _testsRun, _testsPassed );
		}

		void Test ( int testNum, string exprToParse, int terminatingChar = -1 )
		{
			++_testsRun;
			var testName = string.Format ( "results-test-{0}.txt", testNum );
			using ( var tw = File.CreateText ( _testDir + testName ) ) {
				string tc = terminatingChar >= 0 ? ((char) terminatingChar).ToString () : "";
				tw.WriteLine ( "------ Test: {0} ------\t{1}\t{2}", testNum, tc, exprToParse );
				tw.WriteLine ();
				var utf8Stream = CodePointStream.FromString ( exprToParse );
				var scanner = new ScanIt ( utf8Stream, testName, tw );
				var parser = new ExpressionParser ( scanner );
				var result = terminatingChar >= 0 ?
					parser.TryParse ( new CodePoint ( (byte) terminatingChar ) ) :
					parser.TryParse ();
				scanner.Message ( "Parse End" );
				tw.WriteLine ();
				result.Dump ( tw );
				tw.WriteLine ();
			}

			using ( var tr = new StreamReader ( File.OpenRead ( _testDir + testName ) ) ) {
				var trBytes = tr.ReadToEnd ();
				System.Console.WriteLine ();
				System.Console.WriteLine ();
				System.Console.WriteLine ( trBytes );
				StreamReader mr = null;
				try {
					mr = new StreamReader ( File.OpenRead ( _masterDir + testName ) );
					using ( mr ) {
						var mrBytes = mr.ReadToEnd ();
						int len = mrBytes.Length;
						if ( len != trBytes.Length ) {
							System.Console.WriteLine ( "Test {0} FAILURE: lengths differ!!!!", testNum );
							return;
						}
						for ( int i = 0 ; i < len ; i++ ) {
							if ( trBytes [ i ] != mrBytes [ i ] ) {
								System.Console.WriteLine ( "Test {0} FAILURE: bytes differ!!!!", testNum );
								return;
							}
						}
					}
				} catch ( System.Exception ) {
					System.Console.WriteLine ( "Test {0}: No Master!!!", testNum );
					return;
				}
			}

			System.Console.WriteLine ( "Test {0}: SUCCESS!!", testNum );
			++_testsPassed;
		}


		static void Main ( string [] args )
		{
			//var fileStream = System.IO.File.OpenRead ( "text.exp" );
			//var utf8Stream = CodePointStream.FromStream ( fileStream );

			var p = new Program ();
			try {
				p.RunTests ();
			} catch ( System.Exception ex ) {
				System.Console.WriteLine ( ex );
			}

			System.Console.WriteLine ( "press any key to continue..." );
			System.Console.ReadLine ();
		}
	}
}