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

handlerDecl : trigger '->' reaction ('where' assignment (',' assignment)*)?;
trigger : (T=QUOTED | Eof='EOF' | NT=ID) ('upon' Flag=ID)?;
reaction
    : Command='push' name ('(' ID (',' ID)* ')')?
    | Command='wrap' name ('(' ID (',' ID)* ')')?
    | Command='lift' ID
    | Command='drop' ID
    | Command='trim' ID (Star='*')?
    | Command='pass'
    ; 
name : ID;
assignment : ID ':=' operation;
operation
    : Command='pop' Name=ID
    | Command='pop*' Name=ID
    | Command='pop#' Name=ID
    | Command='await'
        (
            (Name=ID)
        |
            ('(' Name=ID 'upon' ExtraContext=ID ')')
        )
        ('with' LocalContext=ID)?
    | Command='await*' Name=ID ('with' LocalContext=ID)?
    | Command='tear' Name=ID 
    ;

WHITESPACE : (' ' | '\t' | ' ' | '\r' | '\n' | COMMENT)+ -> skip ;
NEWLINE    : ('\r'? '\n' | '\r')+ ;
COMMENT    : '%' ~[\n\r]+;