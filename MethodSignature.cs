using System.Collections.Generic;

namespace Joe
{
    internal class MethodSignature
    {

        

        public string[] ParameterTypes { get; set; }
        

        public MethodSignature(ASTFunctionDef function)
        {
            List<string> argTypes = new List<string>();
            var argsDefList = (ASTArgsDefList)function.ArgsList;
            while(argsDefList != null)
            {
                string passByRef = "";
                if (argsDefList.PassByReference)
                {
                    passByRef = "";
                }
                argTypes.Add(passByRef + ((ASTIdent)argsDefList.Type).Value.ToString());
                argsDefList = argsDefList.Arguments;
            }
            this.ParameterTypes = argTypes.ToArray();
        }

        public MethodSignature(ASTFunctionCall function, Environment e)
        {
            List<string> argTypes = new List<string>();
            var argsList = (ASTArgsList)function.ArgumentList;
            while (argsList != null)
            {
                if (argsList.Identifier.GetType() == typeof(ASTIdent))
                {
                    argTypes.Add(e.GetEntry(((ASTIdent)argsList.Identifier).Value).Type);
                } else if (argsList.Identifier.GetType() == typeof(ASTDigit))
                {
                    argTypes.Add("int");
                }
                else if (argsList.Identifier.GetType() == typeof(ASTString))
                {
                    argTypes.Add("string");
                }
                else if (argsList.Identifier.GetType() == typeof(ASTArray) || argsList.Identifier.GetType() == typeof(ASTArrayLen))
                {
                    argTypes.Add("arr");
                }
                argsList = argsList.Argslist;
            }
            this.ParameterTypes = argTypes.ToArray();
        }

        public bool Equals(MethodSignature ms)
        {
            if (this.ParameterTypes.Length == ms.ParameterTypes.Length)
            {
                for(int i = 0; i < this.ParameterTypes.Length; i++)
                {
                    if (!this.ParameterTypes.Equals(ms.ParameterTypes[i]))
                    {
                        return false;
                    }
                }
            }
            else
                return false;
            return true;
        }
    }
}