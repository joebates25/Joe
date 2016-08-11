using System;
using System.Collections.Generic;

namespace Joe
{
    internal class Translator
    {
        private ASTStatementList tree;
        Environment environment;

        List<String> BUILT_IN_FUNCTIONS = new List<String> { "print", "tostring", "input", "pinput", "int" };

        public Translator(ASTStatementList tree)
        {
            this.tree = tree;
            environment = new Environment();
        }

        internal void Translate()
        {
            foreach (ASTNode statement in tree.statementList)
            {
                var r = translate(statement);
            }
        }

        private object translate(ASTNode statement)
        {
            if (statement.GetType() == typeof(ASTDigit))
            {
                return ((ASTDigit)statement).Value;
            }
            else if (statement.GetType() == typeof(ASTIdent))
            {
                return environment.GetValue(((ASTIdent)statement).Value);
            }
            else if (statement.GetType() == typeof(ASTBinOP))
            {
                return transBinOp((ASTBinOP)statement);
            }
            else if (statement.GetType() == typeof(ASTString))
            {
                return ((ASTString)statement).Value;
            }
            else if (statement.GetType() == typeof(ASTBool))
            {
                return ((ASTBool)statement).Value;
            }
            else if (statement.GetType() == typeof(ASTCompOp))
            {
                return transCompOp((ASTCompOp)statement);
            }
            else if (statement.GetType() == typeof(ASTVarDef))
            {
                return transVarDef((ASTVarDef)statement);
            }
            else if (statement.GetType() == typeof(ASTAssignNode))
            {
                return transAssign((ASTAssignNode)statement);
            }
            else if (statement.GetType() == typeof(ASTFunctionDef))
            {
                return transFuncDef((ASTFunctionDef)statement);
            }
            else if (statement.GetType() == typeof(ASTIfThenStatement))
            {
                return transIfElse((ASTIfThenStatement)statement);
            }
            else if (statement.GetType() == typeof(ASTFunctionCall))
            {
                return transFuncCall((ASTFunctionCall)statement);
            }
            else if (statement.GetType() == typeof(ASTReturnCall))
            {
                return transRetCall((ASTReturnCall)statement);
            }
            else if (statement.GetType() == typeof(ASTLoop))
            {
                return transLoop((ASTLoop)statement);
            }
            else
            {
                return 0;
            }
        }

        private object transIfElse(ASTIfThenStatement statement)
        {
            bool expResult = (bool)translate(statement.CompExpression);
            if (expResult)
            {
                return transIfBody((ASTIfBody)statement.ThenBody);
            }
            else
            {
                return transIfBody((ASTIfBody)statement.ElseBody);
            }
        }

        private object transIfBody(ASTIfBody thenBody)
        {
            var ifEnvironment = new Environment();
            ifEnvironment.EnclosingEnvironment = this.environment;
            this.environment = ifEnvironment;
            var result = translate(thenBody.Statement);
            if (result != null)           //TODO: Look at
            {
                return result;
            }
            if (thenBody.FunctionBody != null)
            {
                return transIfBody(thenBody.FunctionBody);
            }
            this.environment = ifEnvironment.EnclosingEnvironment;
            return null;
        }

        private object transLoop(ASTLoop statement)
        {
            bool expResult = (bool)translate(statement.CompExpression);
            object result = null;
            while (expResult)
            {
                var loopEnvironment = new Environment();
                loopEnvironment.EnclosingEnvironment = this.environment;
                this.environment = loopEnvironment;
                result = transLoopBody((ASTLoopBody)statement.StatementList);
                expResult = (bool)translate(statement.CompExpression);
                this.environment = environment.EnclosingEnvironment;
            }
            return result;
        }



        private object transFuncCall(ASTFunctionCall statement)
        {
            if (isBuiltInFunction(statement))
            {
                return transBuiltInFunction(statement);
            }
            else
            {
                ASTFunctionDef function = (ASTFunctionDef)this.environment.GetValue(((ASTIdent)statement.Identifier).Value);
                var functionEnvironment = new Environment();
                functionEnvironment.EnclosingEnvironment = environment;
                this.environment = functionEnvironment;
                /*
                 * Add function arguments to environment
                 */
                ASTArgsList arguments = (ASTArgsList)statement.ArgumentList;
                ASTArgsDefList defArgs = (ASTArgsDefList)function.ArgsList;

                while (true)
                {
                    if (defArgs == null)
                        break;
                    if (!defArgs.PassByReference)
                    {
                        string argName = ((ASTIdent)defArgs.Identifier).Value;
                        var argValue = translate(arguments.Identifier);
                        this.environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                        this.environment.SetValue(argName, argValue);
                    }
                    else
                    {
                        string argName = ((ASTIdent)defArgs.Identifier).Value;
                        this.environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                        this.environment.SetPointer(argName, new PointerEntry { Value = this.environment.EnclosingEnvironment.ScopeID, Key = ((ASTIdent)arguments.Identifier).Value });
                    }
                    defArgs = defArgs.Arguments;
                    arguments = arguments.Argslist;

                }

                var returnResult = transFunctionBody((ASTFunctionBody)function.StatementList);
                this.environment = environment.EnclosingEnvironment;

                return returnResult;
            }
        }

        private object transBuiltInFunction(ASTFunctionCall statement)
        {
            var identifer = ((ASTIdent)statement.Identifier).Value;
            if (identifer.Equals("print"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier);
                Console.Out.WriteLine(translatedValue);

            }
            else if (identifer.Equals("tostring"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier);
                return translatedValue.ToString();
            }
            else if (identifer.Equals("int"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier);
                return int.Parse((string)translatedValue);
            }
            else if (identifer.Equals("input"))
            {
                return Console.ReadLine();
            }
            else if (identifer.Equals("pinput"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier).ToString();
                Console.Out.Write(translatedValue);
                return Console.ReadLine();
            }
            return null;
        }

        private bool isBuiltInFunction(ASTFunctionCall statement)
        {
            return this.BUILT_IN_FUNCTIONS.Contains(((ASTIdent)statement.Identifier).Value);

        }

        private object transFunctionBody(ASTFunctionBody statementList)
        {
            if (statementList.Statement.GetType() == typeof(ASTReturnCall))
            {
                return transRetCall((ASTReturnCall)statementList.Statement);
            }
            else
            {
                var result = translate(statementList.Statement);
                if (result != null)
                {
                    return result;
                }
                if (statementList.FunctionBody != null)
                {
                    return transFunctionBody(statementList.FunctionBody);
                }
                else
                {
                    return null;
                }
            }
        }
        private object transLoopBody(ASTLoopBody statementList)
        {
            if (statementList.Statement.GetType() == typeof(ASTReturnCall))
            {
                return transRetCall((ASTReturnCall)statementList.Statement);
            }
            else
            {
                translate(statementList.Statement);
                if (statementList.FunctionBody != null)
                {
                    return transLoopBody(statementList.FunctionBody);
                }
                else
                {
                    return null;
                }
            }
        }

        private object transRetCall(ASTReturnCall statement)
        {
            return translate(statement.Expression);
        }

        private object transAssign(ASTAssignNode statement)
        {
            environment.SetValue(((ASTIdent)statement.Identifier).Value, translate(statement.Value));
            return 0;
        }

        private object transVarDef(ASTVarDef statement)
        {
            environment.AddValueWithType(((ASTIdent)statement.VarName).Value, ((ASTIdent)statement.VarType).Value);
            environment.SetValue(((ASTIdent)statement.VarName).Value, translate(statement.Value));
            return null;
        }

        private object transFuncDef(ASTFunctionDef statement)
        {
            environment.AddValueWithType(statement.Identifier.Value, "func");
            environment.SetValue(statement.Identifier.Value, statement);
            return null;
        }

        private object transBinOp(ASTBinOP statement)
        {
            var translatedOp1 = translate(statement.Op1);
            var translatedOp2 = translate(statement.OP2);
            if (translatedOp1.GetType() == translatedOp2.GetType())
            {
                if (translatedOp1.GetType() == typeof(int))
                {
                    if (((ASTOpExp)statement.Operator).Operator.Equals("+"))
                    {
                        return (int)translatedOp1 + (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("-"))
                    {
                        return (int)translatedOp1 - (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("*"))
                    {
                        return (int)translatedOp1 * (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("/"))
                    {
                        return (int)translatedOp1 / (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("^^"))
                    {
                        return Math.Pow((int)translatedOp1, (int)translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("%"))
                    {
                        return (int)translatedOp1 % (int)translatedOp2;
                    }
                    else throw new Exception();
                }
                else
                {
                    if (((ASTOpExp)statement.Operator).Operator.Equals("+"))
                    {
                        return translatedOp1.ToString() + translatedOp2.ToString();
                    }
                    else
                    {
                        throw new MismatchedTypesException("Can only add together 2 strings");
                    }
                }
            }
            else
            {
                throw new MismatchedTypesException();
            }
        }

        private bool transCompOp(ASTCompOp statement)
        {
            var translatedOp1 = translate(statement.Op1);
            var translatedOp2 = translate(statement.OP2);
            if (translatedOp1.GetType() == translatedOp2.GetType())
            {
                if (translatedOp1.GetType() == typeof(int) || translatedOp1.GetType() == typeof(bool))
                {
                    if (((ASTOpExp)statement.Operator).Operator.Equals("<"))
                    {
                        return (int)translatedOp1 < (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals(">"))
                    {
                        return (int)translatedOp1 > (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("!="))
                    {
                        return (int)translatedOp1 != (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("=="))
                    {
                        return (int)translatedOp1 == (int)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("&"))
                    {
                        return (bool)translatedOp1 && (bool)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("|"))
                    {
                        return (bool)translatedOp1 || (bool)translatedOp2;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("^"))
                    {
                        return (bool)translatedOp1 ^ (bool)translatedOp2;
                    }
                    else throw new Exception();
                }
                else
                {
                    if (((ASTOpExp)statement.Operator).Operator.Equals("<"))
                    {
                        return ((string)translatedOp1).Length < ((string)translatedOp2).Length;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals(">"))
                    {
                        return ((string)translatedOp1).Length > ((string)translatedOp2).Length;
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("=="))
                    {
                        return ((string)translatedOp1).Equals((string)translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("!="))
                    {
                        return !((string)translatedOp1).Equals((string)translatedOp2);
                    }
                    else throw new Exception();
                }
            }
            else
            {
                throw new MismatchedTypesException();
            }
        }
    }
}