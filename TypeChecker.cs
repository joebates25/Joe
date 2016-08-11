using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    class TypeChecker
    {

        ASTStatementList NodeTree;
        Environment typeEnvironment;
        public TypeChecker(ASTStatementList list)
        {
            this.NodeTree = list;
            typeEnvironment = new Environment();
        }

        public void TypeCheck()
        {
            foreach (ASTNode statement in NodeTree.statementList)
            {
                typeCheck(statement);
            }
        }

        public void typeCheck(ASTNode statement)
        {
            if (statement.GetType() == typeof(ASTVarDef))
            {
                ASTVarDef varDef = (ASTVarDef)statement;
                var variableName = ((ASTIdent)varDef.VarName).Value;
                var variableType = ((ASTIdent)varDef.VarType).Value;
                typeEnvironment.AddValue(variableName);
                typeEnvironment.SetValue(variableName, variableType);
            }
            else if (statement.GetType() == typeof(ASTAssignNode))
            {
                ASTAssignNode varAssign = (ASTAssignNode)statement;
                var variableName = ((ASTIdent)varAssign.Identifier).Value;
                if (varAssign.Value.GetType() == typeof(ASTIdent))
                {
                    ASTIdent valueIdent = (ASTIdent)varAssign.Value;
                    String identType = typeEnvironment.GetValue(variableName).ToString();
                    String valueType = typeEnvironment.GetValue(valueIdent.Value).ToString();
                    if (!identType.Equals(valueType))
                    {
                        throw new MismatchedTypesException();   
                    }
                }
                else if (varAssign.Value.GetType() == typeof(ASTDigit))
                {                                                       
                    String identType = typeEnvironment.GetValue(variableName).ToString();
                    String valueType = "int";
                    if (!identType.Equals(valueType))
                    {
                        throw new MismatchedTypesException();
                    }
                }
                else if (varAssign.Value.GetType() == typeof(ASTString))
                {                                                     
                    String identType = typeEnvironment.GetValue(variableName).ToString();
                    String valueType = "string";
                    if (!identType.Equals(valueType))
                    {
                        throw new MismatchedTypesException();
                    }
                }
                }
            }

        }
    }
