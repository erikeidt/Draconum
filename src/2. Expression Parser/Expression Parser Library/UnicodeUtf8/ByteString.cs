
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

using System;

namespace com.erikeidt.Draconum
{
	public struct ByteString : IEquatable<ByteString>
	{
		public readonly ByteStreamMark Mark;
		public readonly int Length;
		public readonly int Hash;

		public ByteString ( byte [] bytes, int start, int length, bool hash = true )
		{
			Mark = new ByteStreamMark ( bytes, start );
			Length = length;
			Hash = hash ? MakeHash ( bytes, start, length ) : 0;
		}

		//public ByteString ( ByteStreamMark mark, int length, bool hash = true )
		//{
		//	Mark = mark;
		//	Length = length;
		//	Hash = hash ? MakeHash ( mark.Buffer, mark.Start, mark.Buffer.Length ) : 0;
		//}

		public ByteString ( string str )
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes ( str );
			Mark = new ByteStreamMark ( bytes, 0 );
			Length = bytes.Length;
			Hash = MakeHash ( bytes, 0, Length );
		}

		public override int GetHashCode ()
		{
			return Hash;
		}

		public static bool operator == ( ByteString a, ByteString b )
		{
			int len = a.Length;
			if ( len != b.Length )
				return false;

			if ( a.Hash != 0 && b.Hash != 0 && a.Hash != b.Hash )
				return false;

			var aa = a.Mark.Buffer;
			var bb = b.Mark.Buffer;
			var ii = a.Mark.Start;
			var jj = b.Mark.Start;

			for ( int i = 0 ; i < len ; i++ ) {
				if ( aa [ ii ] != bb [ jj ] )
					return false;
			}

			return true;
		}

		public static bool operator != ( ByteString one, ByteString two )
		{
			return !(one == two);
		}

		public bool Equals ( ByteString other )
		{
			return this == other;
		}

		public override bool Equals ( object obj )
		{
			if ( obj is ByteString other )
				return this == other;
			return false;
		}

		public byte this [ int index ] {
			get {
				if ( index >= Length )
					throw new ArgumentOutOfRangeException ();
				return Mark.Buffer [ Mark.Start + index ];
			}
		}

		public override string ToString ()
		{
			return System.Text.Encoding.UTF8.GetString ( Mark.Buffer, Mark.Start, Length );
		}

		private static int MakeHash ( byte [] buffer, int next, int length )
		{
			uint kU32Bias = 2166136261;
			uint kU32Magic = 16777619;

			uint ans = kU32Bias;
			for ( ; ; ) {
				if ( --length < 0 )
					break;
				ans ^= buffer [ next ];
				ans *= kU32Magic;
				next++;
			}

			return unchecked((int) ans);
		}
	}
}