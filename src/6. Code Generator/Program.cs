﻿
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

using com.erikeidt.Draconum.Code_Generator_Library;
using System;
using System.IO;

namespace com.erikeidt.Draconum.CodeGeneratorTest
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

			Test ( 1000, "(a && b) + 3" );  // fyi, this should throw CompileException

			Test ( 1001, "if ( a && b ) { c = 1; } else { d = 2; }" );
			Test ( 1002, "if ( a || b ) { c = 1; } else { d = 2; }" );

			Test ( 1100, "a && b;" );
			Test ( 1101, "a || b;" );

			Test ( 1150, "(a && b) + 3;" );
			Test ( 1151, "(a || b) + 3;" );

			Test ( 1200, "e = a && b;" );
			Test ( 1201, "e = a || b;" );
			Test ( 1250, "e = (a && b) + 3;" );
			Test ( 1251, "e = (a || b) + 3;" );

			Test ( 1300, "a ();" );
			Test ( 1301, "a(1,2,3);" );
			Test ( 1302, "a((1,2),3);" );

			Test ( 1400, "a() && b();" );
			Test ( 1401, "a() || b();" );
			Test ( 1450, "(a() && b()) + 3;" );
			Test ( 1451, "(a() || b()) + 3;" );

			Test ( 1600, "s += a[i];" );
			Test ( 1601, "a[i] += s; " );
			Test ( 1602, "a[i]++;" );

			Test ( 1800, "a ? b : c;" );
			Test ( 1801, "e = a ? b : c;" );
			Test ( 1802, "a(0) ? b(1) : c(2);" );
			Test ( 1803, "e = a(0) ? b(1) : c(2);" );

			Test ( 2001, "for(;;) { if ( a && b ) { c=1; break; } }" );
			Test ( 2002, "for(;;) { if ( a && b ) break; }" );
			Test ( 2003, "for(;;) { if ( a && b ) { break; } }" );
			Test ( 2004, "for(;;) { if ( a && b ) { c=1; break; } }" );

			Test ( 2005, "for(;;) { a = 1; break; }" );

			Test ( 2400, "a=b=c;" );

			Test ( 2500, "break;" );
			Test ( 2501, "continue;" );
			Test ( 2502, "return;"  );
			Test ( 2503, "return 1;" );

			Test ( 3000, "for ( i = 0; i < 100; i++ ) { if ( a[i] == 32 ) break; }" );
			Test ( 3001, "for ( i = 0; i < 100; i++ ) { if ( a[i] != 32 && a[i] != 64 ) break; }" );

			Test ( 3200, "a[i](1);" );
			Test ( 3201, "a[i] = b[i];" );
			Test ( 3202, "a[i] = b[i] = c[i];" );

			Test ( 3400, "*p = 1;" );
			Test ( 3401, "*&a = 1;" );
			Test ( 3402, "p = &a;" );
			Test ( 3403, "i = *&a;" );
			Test ( 3404, "i = &*p;" );
			Test ( 3405, "p = &a[i];" );
			Test ( 3406, "*&a[i] = i;" );
			Test ( 3407, "*(a+i) = *(b+i);" );

			Test ( 3700, "if ( a == 1 ) { c = 1; } else { d = 2; }" );
			Test ( 3701, "if ( a != 1 ) { c = 1; } else { d = 2; }" );
			Test ( 3702, "if ( a <= 1 ) { c = 1; } else { d = 2; }" );
			Test ( 3703, "if ( a < 1 ) { c = 1; } else { d = 2; }" );
			Test ( 3704, "if ( a > 1 ) { c = 1; } else { d = 2; }" );
			Test ( 3705, "if ( a >= 1 ) { c = 1; } else { d = 2; }" );

			Test ( 3800, "if ( a == 4 ) { c = 1; } else { d = 2; }" );
			Test ( 3801, "if ( a >= 1 && a <= 7 ) { c = 1; } else { d = 2; }" );
			Test ( 3802, "if ( a > 1 && a < 7 ) { c = 1; } else { d = 2; }" );
			Test ( 3803, "if ( a < 1 || a > 8 ) { c = 1; } else { d = 2; }" );
			Test ( 3804, "if ( a <= 1 || a >= 8 ) { c = 1; } else { d = 2; }" );

			Test ( 3900, "for ( i = 0; i < 100; i++ ) { if ( a[i] != b[i] ) break; s += a[i] * b[i]; }" );

			//Test ( 4000, "L1: goto L1;" );

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
				var symbolTable = new SimpleSymbolTable ();
				var parser = new StatementParser ( scanner, symbolTable );
				var result = parser.TryParse ();
				scanner.Message ( "Parse End" );
				tw.WriteLine ();
				result.Dump ( tw );
				tw.WriteLine ();
				if ( ! result.HasErrors )
				{
					using ( var context = new CodeGenContext ( tw ) ) {
						try
						{
							Dump.CommentFormat ();
							symbolTable.GenerateVariables ( context );
							//result.Result.PrettyPrint ();
							result.Result.GenerateCodeWithPrettyPrint ( context );
						} catch ( Exception ex ) {
							System.Console.WriteLine ( ex );
						}
					}
				}
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
