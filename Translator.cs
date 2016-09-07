using System;
using System.Collections.Generic;

namespace Joe
{
    internal class Translator
    {
        private ASTStatementList tree;
        public Environment environment;
        public ASTNode CurrentStatement { get; set; }

        List<String> BUILT_IN_FUNCTIONS = new List<String> { "print", "tostring", "input", "pinput", "toint", "len", "memdump" };

        public Translator(ASTStatementList tree)
        {
            this.tree = tree;
            environment = new Environment();
        }

        internal void Translate()
        {
            foreach (ASTNode statement in tree.statementList)
            {
                this.CurrentStatement = statement;
                var r = translate(statement);
            }
        }

        private object translate(ASTNode statement)
        {
            if (statement.GetType() == typeof(ASTDigit))
            {
                return ((ASTDigit)statement).Value;
            }
            if (statement.GetType() == typeof(ASTFloat))
            {
                return ((ASTFloat)statement).Value;
            }
            if (statement.GetType() == typeof(ASTNull))
            {
                return null;
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
            else if (statement.GetType() == typeof(ASTArrayLen))
            {
                return transArrayLen((ASTArrayLen)statement);
            }
            else if (statement.GetType() == typeof(ASTArray))
            {
                return transArray((ASTArray)statement);
            }
            else if (statement.GetType() == typeof(ASTSubscript))
            {
                return transSub((ASTSubscript)statement);
            }
            else
            {
                return null;
            }
        }

        private object transSub(ASTSubscript statement)
        {
            var index = (int)translate(statement.Subscript);
            var ident = translate(statement.Identifier);
            if (ident.GetType() == typeof(string))
            {
                return ident.ToString()[index].ToString();
            }
            else
            {
                return (environment.GetValue(((ASTIdent)statement.Identifier).Value, index));
            }
        }

        private object transArray(ASTArray statement)
        {
            List<object> newArray = new List<object>();
            while (statement != null)
            {
                newArray.Add(translate(statement.Value));
                statement = statement.Array;
            }
            return newArray.ToArray();
        }

        private object transArrayLen(ASTArrayLen statement)
        {
            var length = (int)translate(statement.Length);
            return new object[length];
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
                if (statement.ElseBody != null)
                {
                    return transIfBody((ASTIfBody)statement.ElseBody);
                }
                else return null;
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
                MethodSignature ms = new MethodSignature(statement, environment);        
                ASTFunctionDef function = (ASTFunctionDef)this.environment.GetValue(ms.ToString());
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
                    if (defArgs.PassByReference && defArgs.DefaultValue != null)
                    {
                        throw new Exception("Referenced parameters cannot have a default value.");
                    }
                    if (!defArgs.PassByReference)
                    {     

                        if (arguments.Identifier.GetType() == typeof(ASTArgumentPlaceHolder))
                        {
                            string argName = ((ASTIdent)defArgs.Identifier).Value;
                            var argValue = translate(defArgs.DefaultValue);
                            this.environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                            this.environment.SetValue(argName, argValue);
                        }
                        else
                        {
                            string argName = ((ASTIdent)defArgs.Identifier).Value;
                            var argValue = translate(arguments.Identifier);
                            this.environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                            this.environment.SetValue(argName, argValue);
                        }
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
            else if (identifer.Equals("toint"))
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
            else if (identifer.Equals("memdump"))
            {
                Console.Out.WriteLine("~~~~BEGIN MEM DUMP~~~~");
                this.environment.MemDump();
            }
            else if (identifer.Equals("len"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier);
                if(translatedValue.GetType() == typeof(object[]))
                {
                    return ((object[])translatedValue).Length;
                }
                else if (translatedValue.GetType() == typeof(string))
                {
                    return ((string)translatedValue).Length;
                }
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
            if (statement.Identifier.GetType() == typeof(ASTSubscript))
            {

                var subNode = (ASTSubscript)statement.Identifier;
                int index = (int)translate(subNode.Subscript);
                environment.SetValue(((ASTIdent)subNode.Identifier).Value, translate(statement.Value), index);
            }
            else
            {
                environment.SetValue(((ASTIdent)statement.Identifier).Value, translate(statement.Value));
            }
            return null;
        }

        private object transVarDef(ASTVarDef statement)
        {
            environment.AddValueWithType(((ASTIdent)statement.VarName).Value, ((ASTIdent)statement.VarType).Value);
            environment.SetValue(((ASTIdent)statement.VarName).Value, translate(statement.Value));
            return null;
        }

        private object transFuncDef(ASTFunctionDef statement)
        {
            MethodSignature signature = new MethodSignature(statement);
            environment.AddValueWithType(signature.ToString(), "func");
            environment.SetValue(signature.ToString(), statement);
            return null;
        }

        private object transBinOp(ASTBinOP statement)
        {
            var translatedOp1 = translate(statement.Op1);
            var translatedOp2 = translate(statement.OP2);
            if (translatedOp2.GetType() == typeof(string) && translatedOp1.GetType() == typeof(string))
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
            else 
            {
                if (translatedOp1.GetType() == typeof(int))
                {       
                    if (((ASTOpExp)statement.Operator).Operator.Equals("+"))
                    {
                        return (int)translatedOp1 + Convert.ToInt32(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("-"))
                    {
                        return (int)translatedOp1 - Convert.ToInt32(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("*"))
                    {
                        return (int)translatedOp1 * Convert.ToInt32(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("/"))
                    {
                        return (int)translatedOp1 / Convert.ToInt32(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("^^"))
                    {
                        return (int)Math.Pow((int)translatedOp1, Convert.ToInt32(translatedOp2));
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("%"))
                    {
                        return (int)translatedOp1 % Convert.ToInt32(translatedOp2);
                    }
                    else throw new Exception();
                } else if (translatedOp1.GetType() == typeof(double))
                {
                    if (((ASTOpExp)statement.Operator).Operator.Equals("+"))
                    {
                        return (double)translatedOp1 + Convert.ToDouble(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("-"))
                    {
                        return (double)translatedOp1 - Convert.ToDouble(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("*"))
                    {
                        return (double)translatedOp1 * Convert.ToDouble(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("/"))
                    {
                        return (double)translatedOp1 / Convert.ToDouble(translatedOp2);
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("^^"))
                    {
                        return (double)Math.Pow((double)translatedOp1, Convert.ToDouble(translatedOp2)); 
                    }
                    else if (((ASTOpExp)statement.Operator).Operator.Equals("%"))
                    {
                        return (double)translatedOp1 % Convert.ToDouble(translatedOp2); 
                    }
                    else throw new Exception();
                }
                throw new MismatchedTypesException();
            }
            
                throw new MismatchedTypesException();
            
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