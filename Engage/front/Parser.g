[namespace Engage.front]
[class public Parser]
[using Engage.mid]

EngSpec ::= i:s?
	"types" Types=TypeDecl*,(i:s*)
	"tokens" Tokens=TokenDecl*,(i:s*)
	"handlers" Handlers=HandlerDecl*,(i:s*) i:s?;

TypeDecl ::= Names=Id+,"," ("<:" i:s? Super=Id)?;

TokenDecl ::= Names=Lexeme+,"," "::" i:s? Type=Id;

Lexeme ::=
	  LiteralLex := Literal=Quoted  Special=$false
	/ NumberLex  := "number" Special=$true
	/ StringLex  := "string" Special=$true;

HandlerDecl ::= LHS=Trigger "->" RHS=Reaction;

Trigger ::= (Literal=Quoted / "EOF" EOF=$true) ("given" Flag=Id)?;

Reaction ::=
	  PushReaction := "push" Name=Id ("(" Args=Id+,"," ")")?
	/ LiftReaction := "lift" Flag=Id
	/ DropReaction := "lift" Flag=Id
	;

string Id ::= re:"[a-zA-Z_01-9]+";
string Quoted ::= "'" re:"[\\w]+" "'";

# Spacing
string s ::= (i:Lay/i:comment)+;
string Lay ::= LayNoNL / NL;
# The first three spaces are 0xA0 representations, then it's a space, then a tab
string LayNoNL ::= rei:"[\uFFFD\u00A0  \t]";
string comment ::= "%" re:"[^\r\n]*" ;
string NL ::= ("\r\n" / "\n" / "\r");
