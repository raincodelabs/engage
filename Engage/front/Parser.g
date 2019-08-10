[namespace Engage.front]
[class public Parser]

# Spacing
string s ::= (i:Lay/i:comment)+;
string Lay ::= LayNoNL / NL;
# The first three spaces are 0xA0 representations, then it's a space, then a tab
string LayNoNL ::= rei:"[\uFFFD\u00A0  \t]";
string comment ::= "//" re:"[^\r\n]*" ;
string NL ::= ("\r\n" / "\n" / "\r");
