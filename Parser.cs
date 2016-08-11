using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    class Parser
    {
        public TokenStream tokenStream;
        ASTStatementList tree;


        public Parser(TokenStream stream)
        {
            this.tokenStream = stream;
        }

        public ASTNode Parse()
        {
            tree = new ASTStatementList();

            while (tokenStream.HasNextToken())
            {
                tree.statementList.Add(ParseStatement());
            }
            return tree;
        }

        private ASTNode ParseStatement()
        {
            var nextToken = tokenStream.Peek();
            if (nextToken.Type == TokenType.EQUALS)
            {
                ASTNode assignNode = parseAssign();
                return assignNode;
            }
            else if (tokenStream.Current.Type == TokenType.LET)
            {
                ASTNode node = parseVarDef();
                eatSemiColon();
                return node;
            }
            else if (tokenStream.Current.Type == TokenType.FUNC)
            {
                ASTNode node = parseFunctionDef();
                eatSemiColon();
                return node;
            }
            else if (tokenStream.Current.Type == TokenType.IF)
            {
                ASTNode node = parseIfStatement();
                eatSemiColon();
                return node;
            }
            else if (tokenStream.Current.Type == TokenType.LOOP)
            {
                ASTNode node = parseLoop();
                eatSemiColon();
                return node;
            }
            else if (tokenStream.Current.Type == TokenType.RET)
            {
                ASTReturnCall node = parseReturnExpression();
                eatSemiColon();
                return node;
            }
            else //is expression
            {
                var node = parseExpression();
                eatSemiColon();
                return node;
            }
        }

        private ASTNode parseIfStatement()
        {
            tokenStream.NextToken();             //past if
            ASTIfThenStatement node = new ASTIfThenStatement();
            ASTNode compExp = parseExpression();
            node.CompExpression = compExp;
            tokenStream.NextToken(); // past then
            ASTNode thenBody = parseIfBody();
            node.ThenBody = thenBody;       
            if (tokenStream.Current.Type == TokenType.END)
            {
                tokenStream.NextToken();
                return node;
            }
            else
            {
                tokenStream.NextToken();
                ASTNode elseBody = parseIfBody();
                node.ElseBody = elseBody;
                tokenStream.NextToken();
                return node;                
            }

        }
           
        private ASTReturnCall parseReturnExpression()
        {
            tokenStream.NextToken();
            ASTReturnCall retCall = new ASTReturnCall();
            retCall.Expression = parseExpression();
            return retCall;
        }

                            
        private ASTNode parseLoop()
        {
            ASTLoop loopNode = new ASTLoop();
            var current = tokenStream.NextToken();
            ASTNode exp = parseExpression();
            loopNode.CompExpression = exp;
            tokenStream.NextToken();
            loopNode.StatementList = parseLoopBody();
            tokenStream.NextToken();
            return loopNode;  
        }

        private void eatBegin()
        {
            var token = tokenStream.NextToken();
            if (token.Type != TokenType.BEGIN)
            {
                throw new ParseException("Expected begin. Got " + token.Value + ". Ln: " + token.LineNumber);
            }
        }

        private ASTLoopBody parseLoopBody()
        {
            ASTLoopBody list = new ASTLoopBody();
            if (tokenStream.Current.Type == TokenType.END)
            {
                return null;
            }
            else
            {
                list.Statement = ParseStatement();
                list.FunctionBody = parseLoopBody();
                return list;
            }
        }

        private ASTIfBody parseIfBody()
        {
            ASTIfBody list = new ASTIfBody();
            if (tokenStream.Current.Type == TokenType.END || tokenStream.Current.Type == TokenType.ELSE)
            {
                return null;
            }
            else
            {
                list.Statement = ParseStatement();
                list.FunctionBody = parseIfBody();
                return list;
            }
        }

        private ASTNode parseFunctionDef()
        {
            var current = tokenStream.NextToken();
            ASTFunctionDef node = new ASTFunctionDef();
            node.Identifier = new ASTIdent(current.Value);
            tokenStream.NextToken();
            tokenStream.NextToken();
            node.Type = new ASTIdent(tokenStream.Current.Value);
            tokenStream.NextToken();   //currently on (
            node.ArgsList = parseDefArgs();
            tokenStream.NextToken();
            tokenStream.NextToken();
            node.StatementList = parseFunctionBody();
            tokenStream.NextToken();
            return node;

        }

        private ASTFunctionBody parseFunctionBody()
        {
            ASTFunctionBody list = new ASTFunctionBody();
            if (tokenStream.Current.Type == TokenType.RBRACK)
            {
                return null;
            }
            else
            {
                list.Statement = ParseStatement();
                list.FunctionBody = parseFunctionBody();
                return list;
            }
        }

        private ASTArgsDefList parseDefArgs()
        {
            if (tokenStream.NextToken().Type == TokenType.RPAREN)
            {
                return null;
            }
            else
            {
                bool passByRef = false;
                if (tokenStream.Current.Type == TokenType.REF)
                {
                    if (tokenStream.Current.Type == TokenType.REF)
                    {
                        passByRef = true;
                        tokenStream.NextToken();
                    }
                }
                ASTArgsDefList node = new ASTArgsDefList();
                node.PassByReference = passByRef;
                node.Identifier = new ASTIdent(tokenStream.Current.Value);
                tokenStream.NextToken();

                node.Type = new ASTIdent(tokenStream.NextToken().Value);
                tokenStream.NextToken();
                node.Arguments = null;
                if (tokenStream.Current.Type == TokenType.COMMA)
                {
                    node.Arguments = parseDefArgs();
                }
                return node;
            }
        }

        private ASTNode parseAssign()
        {
            ASTAssignNode assignNode = new ASTAssignNode();
            var identToken = tokenStream.Current;
            assignNode.Identifier = new ASTIdent(identToken.Value);
            tokenStream.NextToken();
            tokenStream.NextToken();
            assignNode.Value = parseExpression();
            tokenStream.NextToken();
            return assignNode;
        }

        private ASTNode parseVarDef()
        {
            var varDefNode = new ASTVarDef();
            var identToken = tokenStream.NextToken(); //get ident token
            varDefNode.VarName = new ASTIdent(identToken.Value);
            tokenStream.NextToken(); //move to colon
            var typeToken = tokenStream.NextToken();
            varDefNode.VarType = new ASTIdent(typeToken.Value);
            tokenStream.NextToken(); //move to =
            tokenStream.NextToken(); //move to exp node
            var expNode = parseExpression();
            varDefNode.Value = expNode;
            return varDefNode;
        }

        private ASTNode parseExpression()
        {
            var nextToken = tokenStream.Peek();
            if (nextToken.Type == TokenType.LPAREN)
            {
                return parseFunctionCall();
            }
            else if (nextToken.Type == TokenType.PLUS || nextToken.Type == TokenType.MINUS || nextToken.Type == TokenType.MULT || nextToken.Type == TokenType.DIV || nextToken.Type == TokenType.MOD || nextToken.Type == TokenType.POW)   // is operator 
            {
                var node1 = parseExp();
                var operand = parseOp();
                var node2 = parseExpression();
                return new ASTBinOP { Op1 = node1, OP2 = node2, Operator = operand };
            }
            else if (nextToken.Type == TokenType.LT || nextToken.Type == TokenType.GT || nextToken.Type == TokenType.COMPEQUALS || nextToken.Type == TokenType.NOTEQUALS || nextToken.Type == TokenType.AND || nextToken.Type == TokenType.OR || nextToken.Type == TokenType.XOR)
            {
                var node1 = parseExp();
                var operand = parseOp();
                var node2 = parseExpression();
                return new ASTCompOp { Op1 = node1, OP2 = node2, Operator = operand };
            }
            else
            {
                var node = parseExp();
                return node;
            }
            throw new NotImplementedException();
        }

        private ASTNode parseOp()
        {
            var node = new ASTOpExp();
            if (tokenStream.Current.Type == TokenType.PLUS)
            {
                node.Operator = "+";
            }
            else if (tokenStream.Current.Type == TokenType.MINUS)
            {
                node.Operator = "-";
            }
            else if (tokenStream.Current.Type == TokenType.MULT)
            {
                node.Operator = "*";
            }
            else if (tokenStream.Current.Type == TokenType.DIV)
            {
                node.Operator = "/";
            }
            else if (tokenStream.Current.Type == TokenType.GT)
            {
                node.Operator = ">";
            }
            else if (tokenStream.Current.Type == TokenType.LT)
            {
                node.Operator = "<";
            }
            else if (tokenStream.Current.Type == TokenType.POW)
            {
                node.Operator = "^^";
            }
            else if (tokenStream.Current.Type == TokenType.COMPEQUALS)
            {
                node.Operator = "==";
            }
            else if (tokenStream.Current.Type == TokenType.NOTEQUALS)
            {
                node.Operator = "!=";
            }
            else if (tokenStream.Current.Type == TokenType.AND)
            {
                node.Operator = "&";
            }
            else if (tokenStream.Current.Type == TokenType.MOD)
            {
                node.Operator = "%";
            }
            else if (tokenStream.Current.Type == TokenType.OR)
            {
                node.Operator = "|";
            }
            else if (tokenStream.Current.Type == TokenType.XOR)
            {
                node.Operator = "^";
            }
            tokenStream.NextToken();
            return node;
        }

        private ASTNode parseExp()
        {
            var token = tokenStream.Current;
            switch (token.Type)
            {
                case TokenType.IDENTIFIER:
                    if (tokenStream.Peek().Type != TokenType.LPAREN)
                    {
                        var node = new ASTIdent(token.Value);
                        tokenStream.NextToken();
                        return node;
                    }
                    else
                    {
                        return parseFunctionCall();
                    }
                case TokenType.INTEGER:
                    var node2 = new ASTDigit(Int32.Parse(token.Value));
                    tokenStream.NextToken();
                    return node2;
                case TokenType.STRING:
                    var node3 = new ASTString(token.Value);
                    tokenStream.NextToken();
                    return node3;
                case TokenType.TRUE:
                case TokenType.FALSE:
                    var node4 = new ASTBool(Boolean.Parse(token.Value));
                    tokenStream.NextToken();
                    return node4;
                default:
                    throw new Exception();
            }
        }

        private void eatSemiColon()
        {
            if (this.tokenStream.Current.Type != TokenType.SEMICOLON)
            {
                throw new ParseException("Expected semicolon. Got " + tokenStream.Current.Value);
            }
            tokenStream.NextToken();
        }

        private ASTNode parseFunctionCall()
        {
            var token = tokenStream.Current; //ident
            tokenStream.NextToken();                //on (
            ASTFunctionCall funcCall = new ASTFunctionCall();
            funcCall.Identifier = (new ASTIdent(token.Value));
            funcCall.ArgumentList = parseArgs();
            tokenStream.NextToken();
            return funcCall;

        }

        private ASTNode parseArgs()
        {
            tokenStream.NextToken(); //skip past ( or ,
            if (tokenStream.Current.Type == TokenType.RPAREN)
            {
                return null;
            }
            else
            {
                
                ASTArgsList args = new ASTArgsList();  
                args.Identifier = parseExpression();
                if (tokenStream.Current.Type == TokenType.COMMA)
                {
                    args.Argslist = (ASTArgsList)parseArgs();
                }
                else
                {
                    args.Argslist = null;
                }
                return args;
            }
        }
    }
}
