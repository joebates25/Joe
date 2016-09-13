using System;
using System.Collections.Generic;

namespace Joe
{
    internal class Translator
    {
        private ASTStatementList StatementTree;
        public Environment Environment;
        public ASTNode CurrentStatement { get; set; }

        List<String> BUILT_IN_FUNCTIONS = new List<String> { "print", "tostring", "input", "pinput", "toint", "len", "memdump", "set", "inc", "dec" };
        List<String> BUILT_IN_TYPES = new List<string> { "int", "float", "string", "func", "arr", "null" };

        public Translator(ASTStatementList tree)
        {
            this.StatementTree = tree;
            Environment = new Environment();
        }

        internal void Translate()
        {
            foreach (ASTNode statement in StatementTree.statementList)
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
                return Environment.GetValue(((ASTIdent)statement).Value);
            }
            else if (statement.GetType() == typeof(ASTBinOP))
            {
                return transBinOp((ASTBinOP)statement);
            }
            else if (statement.GetType() == typeof(ASTUnaryOp))
            {
                return transUnaryOp((ASTUnaryOp)statement);
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
            else if (statement.GetType() == typeof(ASTComposite))
            {
                return transComposite((ASTComposite)statement);
            }
            else if (statement.GetType() == typeof(ASTClassDef))
            {
                return transClassDef((ASTClassDef)statement);
            }
            else
            {
                return null;
            }
        }

        private object transUnaryOp(ASTUnaryOp statement)
        {
            var op = (ASTOpExp)statement.Operator;
            if (op.Operator.Equals("++"))
            {
                var translatedValue = translate(statement.Identifier);
                if (translatedValue.GetType() == typeof(int))
                {
                    return (int)translatedValue + 1;
                }
                else if (translatedValue.GetType() == typeof(float))
                {
                    return (int)translatedValue + 1;
                }
                else
                {
                    throw new InvalidTypeException();
                }
            }
            else if (op.Operator.Equals("--"))
            {
                var translatedValue = translate(statement.Identifier);
                if (translatedValue.GetType() == typeof(int))
                {
                    return (int)translatedValue - 1;
                }
                else if (translatedValue.GetType() == typeof(float))
                {
                    return (float)translatedValue - 1;
                }
                else
                {
                    throw new InvalidTypeException();
                }
            }
            else if (op.Operator.Equals("-"))
            {
                var translatedValue = translate(statement.Identifier);
                if (translatedValue.GetType() == typeof(int))
                {
                    return (int)translatedValue * -1;
                }
                else if (translatedValue.GetType() == typeof(float))
                {
                    return (float)translatedValue * -1;
                }
                else
                {
                    throw new InvalidTypeException();
                }
            }        

            else
                throw new NotImplementedException();
        }

        private object transComposite(ASTNode statement)
        {
            ObjectEntry firstObject = (ObjectEntry)translate(((ASTComposite)statement).LeftOp);
            Environment newEnvironment = new Environment();
            newEnvironment.EnclosingEnvironment = this.Environment;
            this.Environment = newEnvironment;
            foreach (var key in firstObject.Keys)
            {
                Entry entry = (Entry)firstObject[key];
                this.Environment.AddValueWithType(key, entry.Type);
                this.Environment.SetValue(key, entry.Value);
            }
            var secondValue = translate(((ASTComposite)statement).RightOp);
            Guid enclosingScope = this.Environment.EnclosingEnvironment.ScopeID;
            var newEntry = new ObjectEntry(firstObject.Type);
            foreach (var nextKey in firstObject.Keys)
            {
                newEntry[nextKey] = new Entry { Type = ((Entry)firstObject[nextKey]).Type, Value = this.Environment.GetValue(nextKey) };
            }
            this.Environment.SetValue(firstObject.Type, newEntry);
            this.Environment = this.Environment.EnclosingEnvironment;
            return secondValue;
        }

        private object transClassDef(ASTClassDef statement)
        {
            this.Environment.AddValueWithType(statement.Identifier.Value, "class");
            this.Environment.SetValue(statement.Identifier.Value, statement);
            return 0;
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
                return (Environment.GetValue(((ASTIdent)statement.Identifier).Value, index));
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
            ifEnvironment.EnclosingEnvironment = this.Environment;
            this.Environment = ifEnvironment;
            var result = translate(thenBody.Statement);
            if (result != null)           //TODO: Look at
            {
                return result;
            }
            if (thenBody.FunctionBody != null)
            {
                return transIfBody(thenBody.FunctionBody);
            }
            this.Environment = ifEnvironment.EnclosingEnvironment;
            return null;
        }

        private object transLoop(ASTLoop statement)
        {
            bool expResult = (bool)translate(statement.CompExpression);
            object result = null;
            while (expResult)
            {
                var loopEnvironment = new Environment();
                loopEnvironment.EnclosingEnvironment = this.Environment;
                this.Environment = loopEnvironment;
                result = transLoopBody((ASTLoopBody)statement.StatementList);
                expResult = (bool)translate(statement.CompExpression);
                this.Environment = Environment.EnclosingEnvironment;
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
                MethodSignature ms = new MethodSignature(statement, Environment);
                ASTFunctionDef function = (ASTFunctionDef)this.Environment.GetValue(ms.ToString());
                var functionEnvironment = new Environment();
                functionEnvironment.EnclosingEnvironment = Environment;
                this.Environment = functionEnvironment;
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
                            this.Environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                            this.Environment.SetValue(argName, argValue);
                        }
                        else
                        {
                            string argName = ((ASTIdent)defArgs.Identifier).Value;
                            var argValue = translate(arguments.Identifier);
                            this.Environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                            this.Environment.SetValue(argName, argValue);
                        }
                    }
                    else
                    {
                        string argName = ((ASTIdent)defArgs.Identifier).Value;
                        this.Environment.AddValueWithType(argName, ((ASTIdent)defArgs.Type).Value);
                        this.Environment.SetValue(argName, new PointerEntry { Value = this.Environment.EnclosingEnvironment.ScopeID, Key = ((ASTIdent)arguments.Identifier).Value });
                    }
                    defArgs = defArgs.Arguments;
                    arguments = arguments.Argslist;

                }

                var returnResult = transFunctionBody((ASTFunctionBody)function.StatementList);
                this.Environment = Environment.EnclosingEnvironment;

                return returnResult;
            }
        }

        private object transClassMethod(ASTNode node)
        {
            //first load new environment and provide object values
            //then run method, as normal, unload values
            return null;
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
                this.Environment.MemDump();
            }
            else if (identifer.Equals("len"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var translatedValue = translate(defArgs.Identifier);
                if (translatedValue.GetType() == typeof(object[]))
                {
                    return ((object[])translatedValue).Length;
                }
                else if (translatedValue.GetType() == typeof(string))
                {
                    return ((string)translatedValue).Length;
                }
                return Console.ReadLine();
            }
            else if (identifer.Equals("set"))
            {
                ASTArgsList defArgs = (ASTArgsList)statement.ArgumentList;
                var variableIDent = ((ASTIdent)defArgs.Identifier).Value;
                ObjectEntry objectIdent = (ObjectEntry)translate(defArgs.Identifier);
                defArgs = defArgs.Argslist;
                var propertyIdent = ((ASTIdent)defArgs.Identifier).Value;
                defArgs = defArgs.Argslist;
                var valueIdent = translate(defArgs.Identifier);
                objectIdent[propertyIdent] = new Entry { Value = valueIdent, Type = "NEEDTYPE" };
                this.Environment.SetValue(variableIDent, objectIdent);
                return null;

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
                Environment.SetValue(((ASTIdent)subNode.Identifier).Value, translate(statement.Value), index);
            }
            else if (statement.Identifier.GetType() == typeof(ASTComposite))
            {
                Environment.SetValue(transComposite(statement.Identifier).ToString(), translate(statement.Value));
            }
            else
            {
                Environment.SetValue(((ASTIdent)statement.Identifier).Value, translate(statement.Value));
            }
            return null;
        }

        private object transVarDef(ASTVarDef statement)
        {
            if (statement.Value.GetType() == typeof(ASTObjectDec))
            {
                return transObjectDec(statement);
            }
            else
            {
                Environment.AddValueWithType(((ASTIdent)statement.VarName).Value, ((ASTIdent)statement.VarType).Value);
                Environment.SetValue(((ASTIdent)statement.VarName).Value, translate(statement.Value));
                return null;
            }
        }

        private object transObjectDec(ASTVarDef statement)
        {
            var objectName = ((ASTIdent)statement.VarName).Value;
            var className = ((ASTObjectDec)statement.Value).ClassName.Value;
            var superclass = "";
            ASTClassDef classDec = (ASTClassDef)this.Environment.GetValue(className);
            /*
             * Add class members to object
             */
            ObjectEntry objectEntry = new ObjectEntry(objectName);
            AddClassMembersToObject(objectEntry, classDec);

            //Add object to environment
            this.Environment.AddValueWithType(objectName, className);
            this.Environment.SetValue(objectName, objectEntry);
            return null;
        }

        private ObjectEntry AddClassMembersToObject(ObjectEntry objectEntry, ASTClassDef classDec)
        {
            if (classDec.SuperClass != null)
            {
                objectEntry = AddClassMembersToObject(objectEntry, (ASTClassDef)this.Environment.GetValue(((ASTIdent)classDec.SuperClass).Value)); 
            }
            ASTClassDefList statementList = classDec.Items;
            while (statementList != null && statementList.Statement != null)
            {
                if (statementList.Statement.GetType() == typeof(ASTFunctionDef))
                {
                    var functionDef = (ASTFunctionDef)statementList.Statement;
                    MethodSignature ms = new MethodSignature(functionDef);
                    var fullName = ms.ToString();
                    if (objectEntry.ContainsKey(fullName))
                    {
                        objectEntry[fullName] = new Entry { Value = functionDef, Type = ((ASTIdent)functionDef.Type).Value };
                    }
                    else
                    {
                        objectEntry.Add(fullName, new Entry { Value = functionDef, Type = ((ASTIdent)functionDef.Type).Value });
                    }

                }
                else if (statementList.Statement.GetType() == typeof(ASTVarDef))
                {
                    ASTVarDef varDef = (ASTVarDef)statementList.Statement;
                    var fullName = ((ASTIdent)varDef.VarName).Value;
                    if (objectEntry.ContainsKey(fullName))
                    {
                        objectEntry[fullName] = new Entry { Value = translate(varDef.Value), Type = ((ASTIdent)varDef.VarType).Value };
                    }
                    else
                    {
                        objectEntry.Add(fullName, new Entry { Value = translate(varDef.Value), Type = ((ASTIdent)varDef.VarType).Value });
                    }
                }
                statementList = statementList.List;
            }

            return objectEntry;
        }

        private object transFuncDef(ASTFunctionDef statement)
        {
            MethodSignature signature = new MethodSignature(statement);
            Environment.AddValueWithType(signature.ToString(), "func");
            Environment.SetValue(signature.ToString(), statement);
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
                }
                else if (translatedOp1.GetType() == typeof(double))
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
                else if (translatedOp1.GetType() == typeof(string))
                {
                    //String casting
                    return translatedOp1.ToString() + translatedOp2.ToString();
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