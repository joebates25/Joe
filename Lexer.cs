using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    class Lexer
    {

        public string Filename { get; set; }

        public int LineNumber = 1;
        public Lexer(string fileName)
        {
            this.Filename = fileName;
        }

        public List<Token> LexFile()
        {
            LineNumber = 1;
            List<Token> tokens = new List<Token>();
            using (StreamReader fileStream = new StreamReader(Filename))
            {

                char currentChar;
                while (!fileStream.EndOfStream)
                {

                    currentChar = (char)fileStream.Peek();
                    if (Char.IsLetter(currentChar))
                    {
                        tokens.Add(lexIdent(fileStream));
                    }
                    else if (Char.IsDigit(currentChar))
                    {
                        tokens.Add(lexDigit(fileStream));
                    }
                    else if (((char)currentChar).Equals('"'))
                    {
                        tokens.Add(lexString(fileStream));
                    }
                    else if (((char)currentChar).Equals('='))
                    {
                        advanceStream(fileStream);
                        if ((char)fileStream.Peek() == '=') {
                            tokens.Add(new Token("==", TokenType.COMPEQUALS, LineNumber));
                            advanceStream(fileStream);
                        }
                        else
                        {
                            tokens.Add(new Token("=", TokenType.EQUALS, LineNumber));
                        }
                    }
                    else if (((char)currentChar).Equals('!'))
                    {
                        advanceStream(fileStream);
                        if ((char)fileStream.Peek() == '=')
                        {
                            tokens.Add(new Token("!=", TokenType.NOTEQUALS, LineNumber));
                            advanceStream(fileStream);
                        }
                        else
                        {
                            throw new CharacterUnknownException("Unknown character found at: " + LineNumber);
                        }
                    }
                    else if (((char)currentChar).Equals(':'))
                    {
                        tokens.Add(new Token(":", TokenType.COLON, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('('))
                    {
                        tokens.Add(new Token("(", TokenType.LPAREN, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals(')'))
                    {
                        tokens.Add(new Token(")", TokenType.RPAREN, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals(';'))
                    {
                        tokens.Add(new Token(";", TokenType.SEMICOLON, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals(','))
                    {
                        tokens.Add(new Token(",", TokenType.COMMA, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('}'))
                    {
                        tokens.Add(new Token("}", TokenType.RBRACK, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('{'))
                    {
                        tokens.Add(new Token("{", TokenType.LBRACK, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('['))
                    {
                        tokens.Add(new Token("]", TokenType.LSBRACK, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals(']'))
                    {
                        tokens.Add(new Token("[", TokenType.RSBRACK, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals(':'))
                    {
                        tokens.Add(new Token(":", TokenType.COLON, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('+'))
                    {
                        tokens.Add(new Token("+", TokenType.PLUS, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('-'))
                    {
                        tokens.Add(new Token("-", TokenType.MINUS, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('*'))
                    {
                         advanceStream(fileStream);
                        if ((char)fileStream.Peek() == '*')
                        {
                            tokens.Add(new Token("**", TokenType.REF, LineNumber));
                            advanceStream(fileStream);
                        }
                        else
                        {
                            tokens.Add(new Token("*", TokenType.MULT, LineNumber));
                            advanceStream(fileStream);
                        }
                    }
                    else if (((char)currentChar).Equals('/'))
                    {
                        tokens.Add(new Token("/", TokenType.DIV, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('%'))
                    {
                        tokens.Add(new Token("%", TokenType.MOD, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('&'))
                    {
                        tokens.Add(new Token("&", TokenType.AND, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('|'))
                    {
                        tokens.Add(new Token("|", TokenType.OR, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('^'))
                    {    
                        advanceStream(fileStream);
                        if ((char)fileStream.Peek() == '^')
                        {
                            tokens.Add(new Token("^^", TokenType.POW, LineNumber));
                            advanceStream(fileStream);
                        }
                        else
                        {
                            tokens.Add(new Token("^", TokenType.XOR, LineNumber));
                            advanceStream(fileStream);
                        }
                    }
                    else if (((char)currentChar).Equals('<'))
                    {
                        tokens.Add(new Token("<", TokenType.LT, LineNumber));
                        advanceStream(fileStream);
                    }
                    else if (((char)currentChar).Equals('>'))
                    {
                        tokens.Add(new Token(">", TokenType.GT, LineNumber));
                        advanceStream(fileStream);
                    }     
                    else if (((char)currentChar).Equals('@'))
                    {
                        var character = '.';
                        advanceStream(fileStream);
                        while (character != '@')
                        {
                            character = (char)advanceStream(fileStream);
                        }                   
                    }
                    else
                    {
                        advanceStream(fileStream);
                    }                  
                }
            }
            return tokens;
        }

        private char advanceStream(StreamReader fileStream)
        {
            char readCharacter = (char)fileStream.Read();
            if(readCharacter == '\n')
            {
                LineNumber++;
            }
            return readCharacter;
        }

        private Token lexString(StreamReader fileStream)
        {
            List<char> buffer = new List<char>();
            advanceStream(fileStream);
            while (!((char)fileStream.Peek()).Equals('"'))
            {
                buffer.Add((char)advanceStream(fileStream));
            }
            advanceStream(fileStream);
            return new Token((new string(buffer.ToArray())), TokenType.STRING, LineNumber);
        }

        private Token lexDigit(StreamReader stream)
        {
            List<char> buffer = new List<char>();
            while (Char.IsDigit((char)stream.Peek()))
            {
                buffer.Add(advanceStream(stream));
            }
            return new Token((new string(buffer.ToArray())), TokenType.INTEGER, LineNumber);
        }

        public Token lexIdent(StreamReader stream)
        {
            List<char> buffer = new List<char>();
            while (Char.IsLetter((char)stream.Peek()))
            {
                buffer.Add((char)advanceStream(stream));
            }
            string value = new string(buffer.ToArray()).ToLower();
            if (value.Equals("let"))
            {
                return new Token("let", TokenType.LET, LineNumber);
            }
            else if (value.Equals("func"))
            {
                return new Token("func", TokenType.FUNC, LineNumber);
            }
            else if (value.Equals("ret"))
            {
                return new Token("ret", TokenType.RET, LineNumber);
            }
            else if (value.Equals("loop"))
            {
                return new Token("loop", TokenType.LOOP, LineNumber);
            }
            else if (value.Equals("begin"))
            {
                return new Token("begin", TokenType.BEGIN, LineNumber);
            }
            else if (value.Equals("end"))
            {
                return new Token("end", TokenType.END, LineNumber);
            }
            else if (value.Equals("if"))
            {
                return new Token("if", TokenType.IF, LineNumber);
            }
            else if (value.Equals("then"))
            {
                return new Token("then", TokenType.THEN, LineNumber);
            }
            else if (value.Equals("else"))
            {
                return new Token("else", TokenType.ELSE, LineNumber);
            }
            else if (value.Equals("true"))
            {
                return new Token(bool.TrueString, TokenType.TRUE, LineNumber);
            }
            else if (value.Equals("false"))
            {
                return new Token(bool.FalseString, TokenType.FALSE, LineNumber);
            }
            else if (value.Equals("and"))
            {
                return new Token("and", TokenType.AND, LineNumber);
            }
            else if (value.Equals("or"))
            {
                return new Token("or", TokenType.OR, LineNumber);
            }
            else if (value.Equals("xor"))
            {
                return new Token("xor", TokenType.XOR, LineNumber);
            }
            else
            {
                return new Token(value, TokenType.IDENTIFIER, LineNumber);
            }

        }
    }


}
