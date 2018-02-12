
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
	interface IDump
	{
		void PrettyPrint ( System.IO.TextWriter where, string prolog );
	}

	class ResultOrErrorList<T> where T: IDump
	{
		public T Result { get; }
		public List<CompilationException> Errors { get; }
		public bool IsResult => Result != null;
		public bool HasErrors => Errors != null;

		public ResultOrErrorList ( T result )
		{
			Result = result;
		}

		public ResultOrErrorList ( List<CompilationException> errors )
		{
			Errors = errors;
		}

		public void Dump ( System.IO.TextWriter tw, string prolog = "" )
		{
			if ( Result != null )
			{
				Result.PrettyPrint ( tw, prolog );
			}
			else if ( Errors != null ) 
			{
				foreach ( var err in Errors )
				{
					tw.WriteLine ( err.Message );
				}
			}
		}
	}
}