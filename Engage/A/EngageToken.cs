using System;

namespace Engage
{
	public enum EngageToken 
	{
		KW_POP = 0,
		KW_POP_STAR_ = 1,
		KW_POP_HASH_ = 2,
		KW_AWAIT = 3,
		KW_AWAIT_STAR_ = 4,
		KW_TEAR = 5,
		KW_PUSH = 6,
		KW_WRAP = 7,
		KW_LIFT = 8,
		KW_DROP = 9,
		KW_TRIM = 10,
		KW_NUMBER = 11,
		KW_STRING = 12,
		KW_EOF = 13,
		KW_NAMESPACE = 14,
		KW_TYPES = 15,
		KW_TOKENS = 16,
		KW_HANDLERS = 17,
		KW_WHERE = 18,
		KW_UPON = 19,
		KW_WITH = 20,
		COMMA = 21,
		SEMI = 22,
		IS_TYPE = 23,
		SUB_TYPE = 24,
		STAR = 25,
		ASSIGN = 26,
		ARROW = 27,
		LPAREN = 28,
		RPAREN = 29,
		ID = 30,
		QUOTED = 31,
		WS = 32,
		COMMENT = 33,
	}
}

