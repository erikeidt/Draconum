# Draconum
Experiments in Compiler Technology

This project seeks to explore practical and efficient compiler technology & techiques.

The projects herein are designed to be small, straightforward, simple, and readable.

For starters, it contains a high performance non-recursive hand-written expression parser 
that is capable of the rich operators of standard C, including unary, binary operators,
function calls, array references, etc...  The expression parser project focuses just on
parsing expressions, without concern for parsing statements, declartions, or generating code.

The code generation project focuses on generating simple code from an abstract syntax tree,
though borrows the tatement parser, which borrows the expression parser, which borrows the
scanner.

I developed this initial expression parser technique in the 70's.  A few years later I put it 
together a simple scanner. I've been using this approach to parsing & scanning for decades!

The heart of the expression parser (ParseExpressionToStack) takes under 200 lines of code
for all the operators in C.  

Its approach is a two-state two-stack model.  The first state is the 
Unary State, and the second is the Binary State.  The two stacks are operators and operands.
When parsing is completed, the result is the last tree operand remaining on the operand stack.
As I mentioned, it uses a non-recursive approach to parsing expressions.  Typically, it sees
a token (or characters) and dispatches it (them) directly. 

(By contrast, recursive decent parsers (even when recursion is flattened) examine and 
re-examing the same tokens repeatedly.  This is particularly at issue when the language in 
question has a large range of operator precedences.  The deeper the precedences the more 
re-examining of each token.)

The scanner is super simple: it doesn't assemble tokens but rather only variables & constants 
like string literals and numbers.  We leave operator token parsing to the parser, which it does
in context.

However simple, the scanner does pass over comments, and deal with C#-style conditional compilation
of #if name #elif #else.  (Note C#-style, but not C-style general purpose macros and such.
Complex expressions are not evaluated in #if or #elif and #define is not supported, but it could
do the equivalent of -Dname on a command line.)

Recusive decent, on the other hand, is very appropriate at the statement level.  Statements,
for example in a C language, virtually all have the same precedence, so repeated re-examination
of tokens is minimal as a result.

Current Status of this project:

* Simple Scanner
* Rich Expression Parsing
* Control-flow Statement parsing
* Sample Code Generation

Future endeavors
* Function & Type Declarations
* Scoping Symbol Table
* Instruction Set Architecture
* Simulator/Debugger

Some of the value of this content is in being able 
* to correlate small code sequences in C-like language (e.g. if statements, loop statements; && and || operators) and how they relate to assembly language
* to explore parsing statements and expression using simple and efficient hand written parser
* to explore how to generate code for various language constructs

Draconum is ancient Greek for "of Dragons"!

No license is provided for this content, which is under copyright.
To see what that means, see the "For Users" section of https://choosealicense.com/no-permission/
