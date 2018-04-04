
/*
 *
 * Copyright (c) 2018, Erik L. Eidt
 * All rights Reserved.
 * 
 * Author: Erik L. Eidt
 * Created: 01-11-2018
 *
 * License: No License: no permissions are granted to use, modify, or share this content. See COPYRIGHT.md for more details.
 *
 */

namespace com.erikeidt.Draconum
{
	using Exceptions = Draconum;

	struct CodePoint
	{
		public const int CodePointEofValue = -1;

		public readonly int Value;

		public static readonly CodePoint Eof = new CodePoint ( CodePointEofValue );

		public CodePoint ( byte cp )
		{
			if ( cp > 0x7F )
				throw new Exceptions.Utf8Exception ( "bad code point" );
			Value = cp;
		}

		private CodePoint ( int cp )
		{
			Value = cp;
		}

		public bool AtEOF ()
		{
			return Value == CodePointEofValue;
		}

		public CodePoint ( byte b, byte c )
		{
			if ( (b & 0xE0) != 0xC0 || (c & 0xC0) != 0xC0 )
				throw new Exceptions.Utf8Exception ( "bad code point" );
			Value = ((b & 0x1F) << 6) | (c & 0x3F);
		}

		public CodePoint ( byte b, byte c, byte d )
		{
			if ( (b & 0xF0) != 0xE0 || (c & 0xC0) != 0xC0 || (d & 0xC0) != 0xC0 )
				throw new Exceptions.Utf8Exception ( "bad code point" );

			Value = (((b & 0x3F) << 6) | (c & 0x3F) << 6) | (d & 0x3F);
		}

		public CodePoint ( byte b, byte c, byte d, byte e )
		{
			if ( (b & 0xF8) != 0xF0 || (c & 0xC0) != 0xC0 || (d & 0xC0) != 0xC0 || (e & 0xC0) != 0xC0 )
				throw new Exceptions.Utf8Exception ( "bad code point" );

			Value = ((((b & 0x07) << 6) | (c & 0x3F) << 6) | (d & 0x3F) << 6) | (e & 0x3F);
		}

		public bool IsWhitespace ()
		{
			if ( Value > 0xFFFF )
				return false;
			return System.Char.IsWhiteSpace ( unchecked((char) Value) );
		}

		public bool IsDigit ()
		{
			return Value >= '0' && Value <= '9';
		}

		public bool IsLetter ()
		{
			if ( Value > 0xFFFF )
				return true;
			return System.Char.IsLetter ( unchecked((char) Value) );
		}

		public bool IsLetterOrDigit ()
		{
			if ( Value > 0xFFFF )
				return true;
			return System.Char.IsLetterOrDigit ( unchecked((char) Value) );
		}

		public static bool operator== ( CodePoint a, CodePoint b )
		{
			return a.Value == b.Value;
		}

		public static bool operator!= ( CodePoint a, CodePoint b )
		{
			return a.Value != b.Value;
		}

		public override bool Equals ( object obj )
		{
			if ( obj is CodePoint other )
				return this == other;
			return false;
		}

		public override int GetHashCode ()
		{
			return Value;
		}
	}
}