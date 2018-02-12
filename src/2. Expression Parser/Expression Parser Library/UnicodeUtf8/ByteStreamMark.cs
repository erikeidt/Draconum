
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

using System.Collections.Generic;

namespace com.erikeidt.Draconum
{
	public struct ByteStreamMark
	{
		public readonly byte [] Buffer;
		public readonly int Start;

		public ByteStreamMark ( byte [] buffer, int start )
		{
			Buffer = buffer;
			Start = start;
		}

		public ByteString OfLength ( int length )
		{
			return new ByteString ( Buffer, Start, length );
		}

		public List<byte> MakeList ( int length )
		{
			var ans = new List<byte> ( length + 10 );
			for ( int i = 0 ; i < length ; i++ )
				ans.Add ( Buffer [ Start + i ] );
			return ans;
		}
	}
}