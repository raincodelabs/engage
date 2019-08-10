[namespace Engage.front]
[class public Parser]
[using Engage.mid]

EngSpec ::= i:s?
	"types" Types=TypeDecl*,(i:s?) i:s?
	"tokens" Tokens=TokenDecl*,(i:s?) i:s?
	"handlers" Handlers=HandlerDecl*,(i:s?) i:s?;

TypeDecl ::= i:s? Names=Id+,("," i:s?) (i:s? "<:" i:s? Super=Id)? ";";

TokenDecl ::= i:s? Names=Lexeme+,("," i:s?) i:s? "::" i:s? Type=Id;

Lexeme ::=
	  LiteralLex := Literal=Quoted Special=$false
	/ NumberLex  := "number"       Special=$true
	/ StringLex  := "string"       Special=$true;

HandlerDecl ::= i:s? LHS=Trigger i:s? "->" i:s? RHS=Reaction;

Trigger ::= (Literal=Quoted / "EOF" EOF=$true) ("given" Flag=Id)?;

Reaction ::=
	  PushReaction := "push" i:s Name=Id (i:s? "(" Args=Id+,"," ")")?
	/ LiftReaction := "lift" i:s Flag=Id
	/ DropReaction := "lift" i:s Flag=Id
	;

string Id ::= re:"[a-zA-Z_01-9]+";
string Quoted ::= "'" re:"[^']+" "'";

# Spacing
string s ::= (i:Lay/i:comment)+;
string Lay ::= LayNoNL / NL;
# The first three spaces are 0xA0 representations, then it's a space, then a tab
string LayNoNL ::= rei:"[\uFFFD\u00A0  \t]";
string comment ::= "%" re:"[^\r\n]*" ;
string NL ::= ("\r\n" / "\n" / "\r");
