
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 02-08-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.erikeidt.Draconum.StatementParserTest
{
	class Program
	{
		private string _testDir;
		private string _masterDir;
		private int _testsRun;
		private int _testsPassed;

		public void RunTests ()
		{
			_testDir = @"TestResults\";
			Directory.CreateDirectory ( _testDir );

			_masterDir = @"..\..\Master Test Results\";

			Test ( 1000, ";" );

			Test ( 2000, "if ( test ) { a=1; } else { b=2; }" );
			Test ( 2001, "if ( one > two ) { abc=one+1; } else { def=two+2; }" );
			Test ( 2002, "{ if ( test ) a=1; b=2; }" );
			Test ( 2003, "if ( test )" );	// getting unexpected token, should be unexpected EOF or "missing statement"
			Test ( 2004, "if ( a && b ) { c = 1; } { d = 2; }" );

			// all of these fail to get past ')'
			Test ( 3000, "for (;;) ;" );
			Test ( 3001, "for (;;) { if ( a ) b=1; }" );
			Test ( 3002, "for (;;)" );

			Test ( 4000, "L1: ;" );
			Test ( 4001, "L1: a = b;"  );
			Test ( 4002, "{ L1: }" );
			Test ( 4003, "L1: a ? b : c;" );
			Test ( 4004, "L1 + L2: ;" );


			System.Console.WriteLine ();
			System.Console.WriteLine ( "Tests run: {0}, Test Passed: {1}", _testsRun, _testsPassed );
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

		void Test ( int testNum, string exprToParse, int terminatingChar = -1 )
		{
			++_testsRun;
			var testName = string.Format ( "results-test-{0}.txt", testNum );
			using ( var tw = File.CreateText ( _testDir + testName ) ) {
				string tc = terminatingChar >= 0 ? ((char) terminatingChar).ToString () : "";
				tw.WriteLine ( "------ Test: {0} ------\t{1}\t{2}", testNum, tc, exprToParse.Replace("\n", "\r\n" ) );
				tw.WriteLine ();
				var utf8Stream = CodePointStream.FromString ( exprToParse );
				var scanner = new ScanIt ( utf8Stream, testName, tw );
				var parser = new StatementParser ( scanner, new NotMuchOfASymbolTable () );
				var result = parser.TryParse ();
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
	}
}
