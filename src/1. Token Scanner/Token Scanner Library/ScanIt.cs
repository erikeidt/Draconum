
/*
 *
 * Copyright (c) 2012-2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 09-10-2012
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 */

using System.Collections.Generic;

namespace com.erikeidt.Draconum
{

	/// <summary>
	/// 	This class provides the very simplistic (though efficient) Draconum "scanner".
	///			
	///		Background:
	///			In general, parsers typically either parse text (characters) directly, or, parse tokens, which
	///			are provided by a scanner that converts a stream of text into a stream of tokens.
	///
	///			The division of labor between the "scanner" and parser is different from most implemenations.
	///
	/// 		Draconum opts for the former approach, parsing text more-or-less directly rather than tokenizing 
	///			input and then parsing tokens.  However, handling comments, whitespace, and #if/#else/#endif is 
	///			challenging without some help.
	/// 
	///			Thus, Draconum's idea of the scanner is instead a helper that classifies the next token without 
	///			fully tokenizing it and without creating a generalized token stream.  The parser parses the 
	///			individual characters for symbols like -> ++ .*  etc..
	///
	/// 	This "scanner" is used on demand by the parser, which allows the parser to parse context sensitive tokens.
	///
	///  	The scanner expects Unicode text input, by depending on a CodePointStream.
	///	
	/// 	The scanner provides methods for retrieving characters, identifiers, and literals. 	For other multi-character 
	///		tokens, it let's the parser parse them as characters.
	///
	/// 	However simple, this scanner also counts line numbers, skips over whitespace, comments, 
	///		and C#-style #if/#elif/#else condtional compilation directives, and provide error reporting methods.
	///		(Much of the complexity (e.g. the line of code count for this class) is in this handling.  A simpler
	///		version, without some of these features, might be useful in an educational setting.)	
	///		In order to do this, and for example, since /* and // are two characters, this scanner helper internally 
	///		maintains a buffer of two code points, "curr" and "next".  This two-code-point buffering is not externally 
	///		visible (to the parser).  It also maintains a preprocessing mode for determining how to handle #if/#elif/#else
	///		since these mean sometimes (and sometimes not) skipping until further directives depending on context.
	///
	/// 	It also allows for internally pushing back one identifier, which allows the consuming client parser ask 
	///		if the next characters match some identifier, without necessarily consuming it if it doesn't.  
	///		This capability is exposed via HaveIdentifier.  Other push back is not at present, though could be.
	/// </summary>
	class ScanIt : System.IDisposable // make callers use this in the using design pattern.
	{
		enum TokenModeEnum { Illegal, Normal, SingleLineToken, MultiLineComment, IfDefFalseSection, };

		//public static readonly int ScannerLID = Logger.RegisterSubsystem ( "ScanIt" );
		private readonly CodePointStream _cps;
		private readonly string _job;
		private readonly System.IO.TextWriter _lg;

		private int _line;
		private int _indentationLevel;
		private bool _nonBlankSeen;

		private CodePoint _curr;
		private int _cursor;
		private int _currTokenCursorStart;
		private int _currLineMark;

		private int _nextCursor;
		private int _nextLineMark;

		private CodePoint _next;

		private ByteString? _identifierPushedBack;

		private readonly Stack<TokenModeEnum> _tokenModeStack;
		private readonly HashSet<ByteString> _defdDict;

		private readonly CodePoint [] _idStarts = { new CodePoint ( (byte) '_' ), new CodePoint ( (byte) '.' ) };
		private readonly CodePoint [] _idMores = { new CodePoint ( (byte) '_' ), new CodePoint ( (byte) '.' ) }; //, new CodePoint ( (byte) '-' ) };

		public ScanIt ( CodePointStream cps, string job, System.IO.TextWriter lg )
		{
			_defdDict = new HashSet<ByteString> { new ByteString ( "true" ), new ByteString ( "1" ) };

			_cps = cps;
			_job = job;
			_lg = lg;
			_line = 0;
			_currTokenCursorStart = _cursor = _currLineMark = -2;
			_nextCursor = _nextLineMark = -1;
			_indentationLevel = 0;
			_tokenModeStack = new Stack<TokenModeEnum> ();
			PushMode ( TokenModeEnum.Illegal );
			PushMode ( TokenModeEnum.Normal );
			_curr = new CodePoint ( (byte) ' ' );
			_next = new CodePoint ( (byte) '\n' );
			Advance ();
			Advance ();
		}

		public void Dispose ()
		{
			_cps.Dispose ();
		}

		public void Message ( string msg, bool fromMark = false )
		{
			int offset = _cursor - _currLineMark;
			if ( fromMark ) {
				var advance = _cursor - _currTokenCursorStart;
				if ( offset >= advance )
					offset -= advance;
			}

			if ( offset < 0 ) offset = 0; // happens on newline inside an error string

			var err = _job + ": " + msg + " on line " + _line + " (character offset " + offset + ")";

			var cps = _cps.FromCursor ( _currLineMark );
			var mark = cps.Mark ();
			int lineLength = LengthToEndOfLine ( cps );
			var line = mark.OfLength ( lineLength );
			var srcText = line.ToString ();
			//var srcText = ReadLineByLineNum ( _cps, _line );

			_lg.WriteLine ( srcText );
			_lg.WriteLine ( "{0}{1}", "".PadLeft ( offset ), "^" );
			_lg.WriteLine ( err );
		}

		/// <summary>
		///		Issue error message indicating the current cursor position.
		/// </summary>
		/// <param name="msg">
		///		error message to issue
		/// </param>
		public void Error ( string msg )
		{
			Message ( msg );
			throw new CompilationException ( msg );
		}

		/// <summary>
		///		Issue error message at the start of the last token.
		/// </summary>
		/// <param name="msg">
		///		error message to issue
		/// </param>
		public void ErrorAtMark ( string msg )
		{
			Message ( msg, true );
			throw new CompilationException ( msg );
		}

		private void ReadIntoNext ()
		{
			if ( !_next.AtEOF () ) {
				_nextCursor++;
				_next = _cps.Read ();
			}
		}

		public void Advance ()
		{
			if ( ! _curr.AtEOF (  ) )
			{
				_cursor = _nextCursor;
				_curr = _next;
				_currLineMark = _nextLineMark;
				ReadIntoNext ( );

				switch ( _curr.Value )
				{
					case '\r':
						_curr = new CodePoint ( ( byte ) '\n' );
						if ( _next.Value == '\n' )
							ReadIntoNext ( );
						goto case '\n';
					case '\n':
						_line++;
						_nextLineMark = _nextCursor;
						_indentationLevel = 0;
						_nonBlankSeen = false;
						HandlePPDirectivesFromNormalMode ( );
						break;
					case ' ':
						_indentationLevel++;
						break;
					case '\t':
						if ( (_indentationLevel & 7) != 0 )
							throw new CompilationException ( "tab after space is illegal for indentation" );
						_indentationLevel += 8;	// tab level
						break;
					default:
						_nonBlankSeen = true;
						break;
				}
			}
		}

		public bool IsEOF ()
		{
			Trim ();
			return _curr.AtEOF ();
		}

		public bool WhiteSpace ()
		{
			return Trim ();
		}

		private bool Trim ()
		{
			var ans = false;
			for ( ; ; )
			{
				if ( _curr.Value == '/' ) {
					if ( _next.Value == '/' ) {
						Advance ();
						Advance ();
						while ( !_curr.AtEOF () && _curr.Value != '\r' && _curr.Value != '\n' )
							Advance ();
						if ( _curr.AtEOF () )
							Error ( "EOF during // comment" );
						Advance ();
						continue;
					}

					if ( _next.Value == '*' ) {
						PushMode ( TokenModeEnum.MultiLineComment );
						Advance ();
						Advance ();
						while ( !_curr.AtEOF () && (_curr.Value != '*' || _next.Value != '/') )
							Advance ();
						if ( _curr.AtEOF () )
							Error ( "EOF during /* comment" );
						Advance ();
						Advance ();
						PopMode ();
						continue;
					}
				} else if ( ans && _curr.Value == '-' && _next.Value == '-' ) // seen whitespace, then -, and -
				  {
					Advance ();
					Advance ();
					while ( !_curr.AtEOF () && _curr.Value != '\r' && _curr.Value != '\n' )
						Advance ();
					if ( _curr.AtEOF () )
						Error ( "EOF during -- comment" );
					Advance ();
					continue;
				}

				if ( _curr.AtEOF () )
					break;

				// there is no whitespace at U+10000 or higher
				if ( _curr.Value <= 0xFFFF && System.Char.IsWhiteSpace ( unchecked((char) _curr.Value) ) ) {
					ans = true;
					Advance ();
					continue;
				}

				break;
			}
			return ans;
		}

		/// <summary>
		///		Classifies the upcoming character into token type, skipping whitespace first.
		///		Does not consume the next charater (except whitepsace).  Use
		///			.Advance () to consume single characters, or,
		///			.GetIdentifier () to consume identifiers, or,
		///			.GetStringLiteral () to consume string & character literals, or,
		///			.GetNubmer () to consume numeric literals...
		/// </summary>
		/// <returns>
		///		'A' for identifiers
		///		'0' for numbers
		///		'\"' for string literals
		///		'\'' for character literals
		///		the starting character, otherwise
		/// </returns>
		public CodePoint Token ()
		{
			if ( _identifierPushedBack.HasValue )
				return new CodePoint ( (byte) 'A' );

			Trim ();
			_currTokenCursorStart = _cursor;
			if ( _curr.AtEOF () )
				return CodePoint.Eof;
			if ( IdStart () )
				return new CodePoint ( (byte) 'A' );
			if ( _curr.IsDigit () )
				return new CodePoint ( (byte) '0' );
			return _curr;
		}

		private void PushBackIdentifier ( ByteString id )
		{
			if ( _identifierPushedBack.HasValue )
				throw new AssertionFailedException ( "double push back not supported" );
			_identifierPushedBack = id;
		}

		/// <summary>
		///		Checks to see of the next token matches a particular identifier
		///		Consumes the identifier on match.
		///		Leaves the identifier unconsumed otherwise (using push back)
		/// </summary>
		/// <param name="idToMatch">
		///		ByteString corresponding to the identifier to match
		/// </param>
		/// <returns>
		///		true, if matched (also consumed)
		///		false, if not matched.
		/// </returns>
		public bool HaveIdentifier ( ByteString idToMatch )
		{
			if ( _identifierPushedBack.HasValue ) {
				if ( idToMatch == _identifierPushedBack.Value ) {
					_identifierPushedBack = null;
					return true;
				}
				return false;
			}

			var t = Token ();
			if ( t.Value == 'A' ) {
				var newId = GetIdentifier ();
				if ( idToMatch == newId )
					return true;
				PushBackIdentifier ( newId );
			}

			return false;
		}

		private bool IdStart ()
		{
			if ( _curr.IsLetter () )
				return true;
			var cnt = _idStarts.Length;
			for ( var i = 0 ; i < cnt ; i++ )
				if ( _curr == _idStarts [ i ] )
					return true;
			return false;
		}

		private bool IdMore ()
		{
			if ( _curr.IsLetterOrDigit () )
				return true;
			var cnt = _idMores.Length;
			for ( var i = 0 ; i < cnt ; i++ )
				if ( _curr == _idMores [ i ] )
					return true;
			return false;
		}

		/// <summary>
		///		Fetches an identifier, given that the caller knows it the next token is an identifier
		/// </summary>
		/// <returns>
		///		ByteString corresponding to the identifier found
		/// </returns>
		public ByteString GetIdentifier ()
		{
			if ( _identifierPushedBack.HasValue ) {
				var ans = _identifierPushedBack.Value;
				_identifierPushedBack = null;
				return ans;
			}

			if ( !IdStart () )
				throw new AssertionFailedException ( "thought we'd found an identifier" );

			var mark = _cps.Mark ( _cursor );
			int len = 0;
			unchecked {
				for ( ; ; )
				{
					if ( _curr.AtEOF () )
						break;
					if ( !IdMore () )
						break;
					len++;
					Advance ();
				}
			}
			return mark.OfLength ( len );
		}

		/// <summary>
		///		Demands an identifier, or else error message
		/// </summary>
		/// <param name="msg">
		///		the error message if not found
		/// </param>
		/// <returns>
		///		the identifier, if found
		/// </returns>
		public ByteString ExpectIdentifier ( string msg = null )
		{
			if ( _identifierPushedBack.HasValue ) {
				var ans = _identifierPushedBack.Value;
				_identifierPushedBack = null;
				return ans;
			}

			Trim ();
			if ( _curr.AtEOF () || !IdStart () ) {
				if ( msg != null )
					Error ( msg + " instead of: '" + _curr.Value + "'" );
				else
					Error ( "Expected Identifier instead of: '" + _curr.Value + "'" );
			}
			return GetIdentifier ();
		}

		private int HexDigitValue ()
		{
			if ( _curr.IsDigit () )
				return _curr.Value - '0';
			if ( _curr.Value >= 'A' && _curr.Value <= 'F' )
				return _curr.Value - ('A' - 10);
			if ( _curr.Value >= 'a' && _curr.Value <= 'f' )
				return _curr.Value - ('a' - 10);
			return -1;
		}

		/// <summary>
		///		Gathers a numeric literal, given that the caller knows it is a number
		/// </summary>
		/// <returns>
		///		value of literal
		/// </returns>
		public long GetNonNegativeIntegralLiteral ()
		{
			long ans = 0;
			Trim ();

			unchecked {
				if ( _curr.Value == '#' ) {
					Advance ();
					var digit = HexDigitValue ();
					if ( digit < 0 )
						Error ( "Expected Number instead of: '" + (char) _curr.Value + "'" );

					while ( digit >= 0 ) {
						try {
							ans = checked(ans * 16 + digit);
						} catch ( System.OverflowException ) {
							Error ( "Overflow on hex number: " + (char) _curr.Value + "'" );
						}
						Advance ();
						digit = HexDigitValue ();
					}
				} else {
					if ( !_curr.IsDigit () )
						Error ( "Expected Number instead of: '" + (char) _curr.Value + "'" );

					while ( _curr.IsDigit () ) {
						try {
							ans = checked(ans * 10 + (_curr.Value - '0'));
						} catch ( System.OverflowException ) {
							Error ( "Overflow on decimal number: " + (char) _curr.Value + "'" );
						}
						Advance ();
					}
				}
			}
			return ans;
		}

		public long GetSignedInteger ()
		{
			Trim ();
			var neg = false;
			if ( _curr.Value == '-' ) {
				neg = true;
				Advance ();
			} else if ( _curr.Value == '+' ) {
				Advance ();
			}
			var ans = GetNonNegativeIntegralLiteral ();
			return neg ? -ans : ans;
		}

		/// <summary>
		///		fetches a String Literal, given that the caller knows it is one already
		/// </summary>
		/// <returns>
		///		a ByteString containing the string literal's characters 
		///			(the string's hashed, is computed, in support of interning)
		/// </returns>
		public ByteString GetStringLiteral ()
		{
			int terminatingCharacter = _curr.Value;

			if ( terminatingCharacter != '\"' && terminatingCharacter != '\'' )
				throw new AssertionFailedException ( "thought we were parsing a string here" );

			Advance ();
			PushMode ( TokenModeEnum.SingleLineToken );
			var mark = _cps.Mark ( _cursor );
			int length = 0;
			List<byte> copy = null;

			var esc = false;
			for ( ; ; Advance () ) {
				if ( _curr.AtEOF () )
					break;
				if ( _curr.Value == terminatingCharacter )
					break;
				var val = (char) _curr.Value;
				if ( esc ) {
					esc = false;
					if ( val == '\n' )
						continue;
					switch ( val ) {
					case '0':
						val = '\0';
						break;
					case 't':
						val = '\t';
						break;
					case 'b':
						val = '\b';
						break;
					case 'r':
						val = '\r';
						break;
					case 'n':
						val = '\n';
						break;
					}
				} else {
					if ( val == '\\' ) {
						if ( copy == null )
							copy = mark.MakeList ( length );
						esc = true;
						continue;
					}
					if ( val == '\n' )
						Error ( "Literal not terminated by end of line" );
				}

				length++;

				// this will have to upgrade to support CodePoints being added to the list here
				//	CodePoints larger than 1 UTF8 Code Unit will have to each be added in encoded form
				copy?.Add ( unchecked((byte) val) );
			}
			if ( _curr.AtEOF () )
				Error ( terminatingCharacter == '"' ? "Unterminated String at EOF" : "Unterminated Quoted Identifier at EOF" );
			Advance ();
			PopMode ();
			if ( copy != null )
				mark = new ByteStreamMark ( copy.ToArray (), 0 );
			return mark.OfLength ( length );
		}

		public bool IfTokenNoAdvance ( char what )
		{
			Trim ();
			if ( _curr.Value != what )
				return false;
			return true;
		}

		/// <summary>
		///		used to consume single character tokens, e.g.
		///				if ( expression ) 
		///					assignment;
		///		both the ( and the ) and the ; can be consumed this way
        ///		NOTE: differs from IfCharacter by trimming whitespace
		/// </summary>
		/// <param name="what"></param>
		/// <returns>
		///		true, if the parameter what was found (and if found it will be consumed)
		///		false, if the parameter what was not found
		/// </returns>
		public bool IfToken ( char what )
		{
			Trim ();
			if ( _curr.Value != what )
				return false;
			Advance ();
			return true;
		}

		/// <summary>
		///		Used for subsequent parsing characters in multi-character token, as
		///		it does not trim whitespace before looking at the character.
		///		Consumes the character if it matches.
		/// </summary>
		/// <param name="what">
		///		the character being checked for.
		/// </param>
		/// <returns>
		///		true, if matches the parameter (also consumes the character)
		///		false, if doesn't match the parameter
		/// </returns>
		public bool IfCharacter ( char what )   // 
		{
			if ( _curr.Value != what )
				return false;
			Advance ();
			return true;
		}

		/// <summary>
		///		Demand a one character token, or else parse error.
		/// </summary>
		/// <param name="what">
		///		that one character token to be demanded
		/// </param>
		public void ExpectToken ( char what )
		{
			Trim ();
			if ( _curr.Value != what ) {
				var val = unchecked((char) _curr.Value);
				Error ( "Expected character: '" + what + "' instead of: '" + val + "'" );
			}
			Advance ();
		}

		//public void Token ( ByteString what )
		//{
		//	if ( Token ().Value != 'A' )
		//		Error ( "Expected identifier: '" + what + "' instead of: '" + unchecked((char) _curr.Value) + "'" );
		//	var idToMatch = ExpectIdentifier ();
		//	if ( !idToMatch.Equals ( what ) )
		//		Error ( "Expected identifier: '" + what + "' instead of: '" + idToMatch + "'" );
		//}

		private void PushMode ( TokenModeEnum tme )
		{
			_tokenModeStack.Push ( tme );
		}

		private TokenModeEnum TopMode ()
		{
			return _tokenModeStack.Peek ();
		}

		private void PopMode ()
		{
			if ( _tokenModeStack.Peek () == TokenModeEnum.Illegal )
				Error ( "if def processing failed." );
			_tokenModeStack.Pop ();
		}

		// Assumes mode is SingleLineToken
		private void FinishPPLine ()
		{
			while ( _curr.Value != '\n' )
				Advance ();
			if ( _curr.AtEOF () )
				Error ( "EOF during preprocessing-directive handling." );
		}

		private static readonly ByteString KeywordIf = new ByteString ( "if" );
		private static readonly ByteString KeywordElIf = new ByteString ( "elif" );
		private static readonly ByteString KeywordElse = new ByteString ( "else" );
		private static readonly ByteString KeywordEndif = new ByteString ( "endif" );

		private void HandlePPDirectivesFromNormalMode ()
		{
			while ( TopMode () == TokenModeEnum.Normal && _next.Value == '#' )
				HandlePPDirectiveFromNormalMode ();
		}

		private void HandlePPDirectiveFromNormalMode ()
		{
			PushMode ( TokenModeEnum.SingleLineToken );
			Advance ();
			Advance ();
			var ppid = ExpectIdentifier ();
			if ( ppid == KeywordIf ) {
				// evaluate condition; for now that will be simply an idToMatch, no expression supported
				var defid = ExpectIdentifier ();
				if ( _defdDict.Contains ( defid ) ) {
					// go into processing mode because the #if evaluated to true
					FinishPPLine ();
					PopMode ();
					PushMode ( TokenModeEnum.IfDefFalseSection );
					PushMode ( TokenModeEnum.Normal );
				} else {
					// go into skipping mode because the #if evaluated to false
					SkipTillElseOrEndif ();
				}
			} else if ( ppid == KeywordElIf || ppid == KeywordElse ) {
				// go into skipping mode because we were in normal mode, so we're skipping until #endif
				SkipTillEndif ();
			} else if ( ppid == KeywordEndif ) {
				// consume the endif and resume normal processing
				PopMode ();
				PopMode ();
				if ( TopMode () != TokenModeEnum.IfDefFalseSection )
					Error ( "unexpected #endif" );
				PopMode ();
			}
		}

		private void SkipTillElseOrEndif ()
		{
			var stack = 0;
			for ( ; ; )
			{
				FinishPPLine ();
				if ( _next.Value != '#' )
					Advance ();
				else {
					Advance ();
					Advance ();
					var ppid = ExpectIdentifier ();
					if ( ppid == KeywordIf )
						stack++;
					else if ( ppid == KeywordElIf ) {
						if ( stack == 0 ) {
							var defid = ExpectIdentifier ();
							if ( _defdDict.Contains ( defid ) ) {
								PopMode ();
								PushMode ( TokenModeEnum.IfDefFalseSection );
								PushMode ( TokenModeEnum.Normal );
								return;
							}
						}
					} else if ( ppid == KeywordElse ) {
						if ( stack == 0 ) {
							PopMode ();
							PushMode ( TokenModeEnum.IfDefFalseSection );
							PushMode ( TokenModeEnum.Normal );
							return;
						}
					} else if ( ppid == KeywordEndif ) {
						if ( stack == 0 ) {
							PopMode ();
							return;
							/*
							PopMode ();
							if ( TopMode () != TokenModeEnum.IfDefFalseSection )
								Error ( "unexpected #endif" );
							PopMode ();
							return;
							*/
						}
						stack--;
					}
				}
			}
		}

		// Normal processing mode and we encounter #elif/#else, so we just skip to #endif
		private void SkipTillEndif ()
		{
			var stack = 0;
			for ( ; ; )
			{
				FinishPPLine ();
				if ( _next.Value != '#' )
					Advance ();
				else {
					Advance ();
					Advance ();
					var ppid = ExpectIdentifier ();
					if ( ppid == KeywordIf )
						stack++;
					else if ( ppid == KeywordEndif ) {
						if ( stack == 0 )
							break;
						stack--;
					}
				}
			}
			PopMode ();
			PopMode ();
			if ( TopMode () != TokenModeEnum.IfDefFalseSection )
				Error ( "unexpected #endif" );
			PopMode ();
		}

		private static int LengthToEndOfLine ( CodePointStream cps )
		{
			int lineLength = 0;
			for ( ; ; )
			{
				var cp = cps.Read ();
				if ( cp.Value == '\r' || cp.Value == '\n' || cp.AtEOF () )
					break;
				lineLength++;
			}
			return lineLength;
		}
	}
}
