using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    class Token
    {
        public Token(string value, TokenType type, int lineNumber)
        {
            this.Value = value;
            this.Type = type;
            this.LineNumber = lineNumber;
        }

        public string Value { get; set; }

        public TokenType Type { get; set; }

        public int LineNumber { get; set; }

        public override string ToString()
        {
            return "Token Value: " + Value + "   Token Type: " + Type.ToString() + "  Line Number: " + LineNumber.ToString(); 
        }


    }

    class TokenStream
    {
        public List<Token> tokens;
        public int currentToken = 0;

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Token NextToken()
        {
            currentToken++;
            try
            {
                return tokens[currentToken];
            } catch(Exception e)
            {
                //Console.Out.Write(e.Message);
                return null;
            }
        }

        public Token Peek()
        {
            return tokens[currentToken+1];
        }


        public bool HasNextToken()
        {
            return currentToken < tokens.Count;
        }

        public Token Next
        {
            get
            {
                return tokens[currentToken+1];
            }
        }

        public Token Current
        {
            get
            {
                return tokens[currentToken];
            }
        }
    }



    enum TokenType
    {
        IDENTIFIER,
        STRING,
        INTEGER,
        QUOTE,
        EQUALS,
        LT,
        GT,
        LTE,
        GTE,
        COLON,
        SEMICOLON,
        LET,
        LPAREN,
        RPAREN,
        COMMA,
        FUNC,
        RET,
        LBRACK,
        RBRACK,
        PLUS,
        MINUS,
        STAR,
        DIV,
        MOD,
        COMPEQUALS,
        NOTEQUALS,
        LOOP,
        BEGIN,
        END,
        IF,
        THEN,
        ELSE,
        TRUE,
        FALSE,
        LSBRACK,
        RSBRACK,
        POW,
        AND,
        OR,
        XOR,
        REF,
        NULL,
        ASSIGN,
        SWIG,
        FLOAT
    }
}
