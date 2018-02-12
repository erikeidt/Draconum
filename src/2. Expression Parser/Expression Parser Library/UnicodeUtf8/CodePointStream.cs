
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

using IO=System.IO;

namespace com.erikeidt.Draconum
{
	/// <summary>
	///		Provides a stream of CodePoints, and implemented for UTF8.  
	///		(Could/should be a separated into the general Unicode interface and various implemenations (UTF8, other).)
	/// </summary>
	class CodePointStream : System.IDisposable
	{
		private ByteString _buffer;
		private int _cursor;

		public static CodePointStream FromStream ( IO.Stream st )
		{
			var llength = st.Seek ( 0, IO.SeekOrigin.End );
			var length = checked((int) llength);
			var bytes = new byte [ length ];
			var read = st.Read ( bytes, 0, length );
			if ( read != length )
				throw new IO.IOException ( "did not read full file" );
			return new CodePointStream ( new ByteString ( bytes, 0, length, false ) );
		}

		public static CodePointStream FromString ( string str )
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes ( str );
			return new CodePointStream ( new ByteString ( bytes, 0, bytes.Length, false ) );
		}

		public CodePointStream ( ByteString bs, int cursor = 0 )
		{
			_buffer = bs;
			_cursor = cursor;
		}

		public CodePointStream Reset ()
		{
			return new CodePointStream ( _buffer );
		}

		public CodePointStream FromCursor ( int cursor )
		{
			return new CodePointStream ( _buffer, cursor );
		}

		public bool AtEOF ()
		{
			return _cursor == _buffer.Length;
		}

		public CodePoint Read ()
		{
			if ( AtEOF () )
				return CodePoint.Eof;

			var b = ReadByte ();

			if ( (b & 0xC0) == 0xC0 )
				throw new Utf8Exception ( "invalid UTF-8 character in stream at: " + _cursor + ": 0x" + b.ToString ( "X" ) );

			if ( b < 0x7F )
				return new CodePoint ( b );

			var c = ReadByte ();
			if ( (b & 0xE0) == 0xC0 )
				return new CodePoint ( b, c );

			var d = ReadByte ();
			if ( (b & 0xF0) == 0xE0 )
				return new CodePoint ( b, c, d );

			var e = ReadByte ();
			return new CodePoint ( b, c, d, e );
		}

		public ByteStreamMark Mark ( int cursor )
		{
			return new ByteStreamMark ( _buffer.Mark.Buffer, _buffer.Mark.Start + cursor );
		}

		public ByteStreamMark Mark ()
		{
			return new ByteStreamMark ( _buffer.Mark.Buffer, _buffer.Mark.Start + _cursor );
		}

		public void Dispose ()
		{
		}

		private byte ReadByte ()
		{
			if ( AtEOF () )
				return (byte) '\0';

			return _buffer [ _cursor++ ];
		}
	}
}