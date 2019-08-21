[namespace Engage]
[class public Parser]
[using Engage.A]

EngSpec ::= i:s?
	"namespace" i:s NS=Id i:s?
	"types" Types=TypeDecl*,(i:s?) i:s?
	"tokens" Tokens=TokenDecl*,(i:s?) i:s?
	"handlers" Handlers=HandlerDecl*,(i:s?) i:s?;

TypeDecl ::= i:s? Names=Id+,("," i:s?) (i:s? "<:" i:s? Super=Id)? ";";

TokenDecl ::= i:s? Names=Lexeme+,("," i:s?) i:s? "::" i:s? Type=Id;

Lexeme ::=
	  LiteralLex := Literal=Quoted Special=$false
	/ NumberLex  := "number"       Special=$true
	/ StringLex  := "string"       Special=$true;

HandlerDecl ::= i:s? LHS=Trigger i:s? "->" i:s? RHS=Reaction (i:s? "where" Context=Assignment+,("," i:s?))?;

Trigger ::= (Terminal=Quoted / "EOF" EOF=$true / NonTerminal=Id) (i:s "upon" i:s Flag=Id)?;

Reaction ::=
	  PushReaction := "push" i:s Name=Id (i:s? "(" Args=Id+,"," ")")?
	/ WrapReaction := "wrap" i:s Name=Id (i:s? "(" Args=Id+,"," ")")?
	/ LiftReaction := "lift" i:s Flag=Id
	/ DropReaction := "drop" i:s Flag=Id
	/ TrimReaction := "trim" i:s Name=Id (i:s? "*" Starred = $true)?
	;

Assignment ::= i:s? LHS=Id i:s? ":=" i:s? RHS=Operation;

Reaction Operation ::=
	  PopAction       := "pop"    i:s Name=Id
	/ PopStarAction   := "pop*"   i:s Name=Id
	/ PopHashAction   := "pop#"   i:s Name=Id
	/ AwaitAction     := "await"  i:s Name=Id (i:s? "with" i:s TmpContext=Id)?
	/ AwaitAction     := "await"  i:s? "(" i:s? Name=Id i:s "upon" i:s ExtraContext=Id i:s? ")" (i:s? "with" i:s TmpContext=Id)?
	/ AwaitStarAction := "await*" i:s Name=Id (i:s? "with" i:s TmpContext=Id)?
	/ TearAction      := "tear"   i:s Name=Id
	;

string Id ::= re:"[a-zA-Z_01-9#]+";
string Quoted ::= "'" re:"[^']+" "'";

# Spacing
string s ::= (i:Lay/i:comment)+;
string Lay ::= LayNoNL / NL;
# The first three spaces are 0xA0 representations, then it's a space, then a tab
string LayNoNL ::= rei:"[\uFFFD\u00A0  \t]";
string comment ::= "%" re:"[^\r\n]*" ;
string NL ::= ("\r\n" / "\n" / "\r");
