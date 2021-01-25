grammar Engage;

engSpec :
    'namespace' ID
    'types' typeDecl*
    'tokens' tokenDecl*
    'handlers' handlerDecl*;

ID : [a-zA-Z_01-9#]+;
typeDecl : ID (',' ID)* ('<:' superType)? ';' ;
superType : ID;

tokenDecl : lexeme (',' lexeme)* '::' ID;
lexeme : Q=QUOTED | N='number' | S='string';
QUOTED : '\'' ~[']+ '\'';

handlerDecl : trigger '->' reaction (Adv=('where' | 'while') assignment (',' assignment)*)?;
trigger : (T=QUOTED | Bof='BOF' | Eof='EOF' | NT=ID) ('upon' Flag=ID)?;
reaction
    : Command='push' name ('(' ID (',' ID)* ')')?
    | Command='wrap' name ('(' ID (',' ID)* ')')?
    | Command='lift' flag
    | Command='drop' flag
    | Command='trim' name (Star='*')?
    | Command='pass'
    | Command='dump' name?
    ; 
name : ID;
flag : ID;
assignment : (ID ':=')? operation;
operation
    : Command='pop' name
    | Command='pop*' name
    | Command='await'
        (
            name
        |
            ('(' name 'upon' ExtraContext=ID ')')
        )
        ('with' LocalContext=ID)?
    | Command='await*' name ('with' LocalContext=ID)?
    | Command='tear' name 
    | Command='dump' name?
    ;

WHITESPACE : (' ' | '\t' | ' ' | '\r' | '\n' | COMMENT)+ -> skip ;
NEWLINE    : ('\r'? '\n' | '\r')+ ;
COMMENT    : '%' ~[\n\r]+;