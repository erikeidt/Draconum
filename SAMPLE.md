The following are samples from the code generator, which prints the parse tree as well as code generated for a simulated stack processor assembly language.

This generated code sequence highlights several features:
  the short circuit boolean evaluation of && and || operators
  
  Test 1002 shows the nature of if staments falling thru to the then-part during condition testing,
  which means reversing the condition in order to branch on condition false to the else-part

	------ Test: 1002 ------		if ( a || b ) { c = 1; } else { d = 2; }

	if ( a || b ) { c = 1; } else { d = 2; }
	                                        ^
	results-test-1002.txt: Parse End on line 1 (character offset 40)

		+---If-Then-Else Statement
			+---Condition	binop: 'ShortCircutOr'
			|   +---Var: 'a'
			|   +---Var: 'b'
			+---ThenPart	Block Statement
			|   +---Expression Statement
			|       +---binop: 'Assignment'
			|           +---Var: 'c'
			|           +---num: '1'
			+---ElsePart	Block Statement
				+---Expression Statement
					+---binop: 'Assignment'
						+---Var: 'd'
						+---num: '2'

		PUSH	a
		B.TRUE	L1
		PUSH	b
		B.FALSE	L0
	L1:
		PUSH	#1
		POP	c
		JUMP	L2
	L0:
		PUSH	#2
		POP	d
	L2:


and Test 3001 shows the if ... goto nature of the following if statement; here the break target is directly 
branched to by the && operator test within the if-statement.

	------ Test: 3001 ------		for ( i = 0; i < 100; i++ ) { if ( a[i] != 32 && a[i] != 64 ) break; }

	for ( i = 0; i < 100; i++ ) { if ( a[i] != 32 && a[i] != 64 ) break; }
	                                                                      ^
	results-test-3001.txt: Parse End on line 1 (character offset 70)

		+---For Statement
			+---Initialization	binop: 'Assignment'
			|   +---Var: 'i'
			|   +---num: '0'
			+---Condition	binop: 'LessThan'
			|   +---Var: 'i'
			|   +---num: '100'
			+---Increment	unop: 'PostfixIncrement'
			|   +---Var: 'i'
			+---Body	Block Statement
				+---If-Then Statement
					+---Condition	binop: 'ShortCircutAnd'
					|   +---binop: 'NotEqual'
					|   |   +---binop: 'Subscript'
					|   |   |   +---Var: 'a'
					|   |   |   +---Var: 'i'
					|   |   +---num: '32'
					|   +---binop: 'NotEqual'
					|       +---binop: 'Subscript'
					|       |   +---Var: 'a'
					|       |   +---Var: 'i'
					|       +---num: '64'
					+---ThenPart	Break Statement

	#	for statement
		PUSH	#0
		POP	i
	L0:
		PUSH	i
		PUSH	#100
		B.GE	L2
		PUSH	a
		PUSH	i
		Subscript
		PUSH	#32
		B.EQ	L3
		PUSH	a
		PUSH	i
		Subscript
		PUSH	#64
		B.NE	L2
	L3:
	L1:
		PUSH	i
		INC
		POP	i
		JUMP	L0
	L2:

