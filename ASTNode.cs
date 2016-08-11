using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    public interface ASTNode
    {

    }

    public class ASTStatementList : ASTNode
    {
        public List<ASTNode> statementList { get; set; }

        public ASTStatementList()
        {
            statementList = new List<ASTNode>();
        }
    }

    public class ASTFunctionBody : ASTNode
    {
        public ASTNode Statement { get; set; }

        public ASTFunctionBody FunctionBody { get; set; }
    }

    public class ASTIfBody : ASTNode
    {
        public ASTNode Statement { get; set; }

        public ASTIfBody FunctionBody { get; set; }
    }

    public class ASTLoopBody: ASTNode
    {
        public ASTNode Statement { get; set; }

        public ASTLoopBody FunctionBody { get; set; }
    }

    public class ASTBool: ASTNode
    {
        public ASTBool(Boolean value)
        {
            this.Value = value;
        }
        public Boolean Value { get; set; }
    }

    

    public class ASTIfThenStatement : ASTNode
    {
        public ASTNode CompExpression { get; set; }

        public ASTNode ThenBody { get; set; }

        public ASTNode ElseBody { get; set; }
    }

    public class ASTStatement : ASTNode
    {
        public List<ASTNode> statementNodes { get; set; }
    }

    public class ASTOpExp : ASTNode
    {

        public String Operator { get; set; }
    }

    public class ASTIdent : ASTNode
    {
        public string Value { get; set; }

        public ASTIdent(string value)
        {
            this.Value = value;
        }
    }


    public class ASTVarDef : ASTNode
    {
        public ASTNode VarName { get; set; }

        public ASTNode VarType { get; set; }

        public ASTNode Value { get; set; }
    }

    public class ASTCompOp : ASTNode
    {
        public ASTNode Op1 { get; set; }

        public ASTNode OP2 { get; set; }

        public ASTNode Operator { get; set; }
    }

    public class ASTLoop: ASTNode
    {
        public ASTNode CompExpression { get; set; }

        public ASTNode StatementList { get; set; }
    }

    public class ASTAssignNode : ASTNode
    {
        public ASTNode Identifier { get; set; }

        public ASTNode Value { get; set; }
    }

    public class ASTBinOP : ASTNode
    {
        public ASTNode Op1 { get; set; }

        public ASTNode OP2 { get; set; }

        public ASTNode Operator { get; set; }
    }
                               

    public class ASTArgsDefList : ASTNode
    {
        public ASTNode Identifier { get; set; }

        public ASTNode Type { get; set; }

        public bool PassByReference { get; set; }

        public ASTArgsDefList Arguments { get; set; }
    }



    public class ASTFunctionDef : ASTNode
    {
        public ASTIdent Identifier { get; set; }

        public ASTNode Type { get; set; }

        public ASTArgsDefList ArgsList { get; set; }

        public ASTNode StatementList { get; set; }
    }

    public class ASTArgsList : ASTNode
    {
        public ASTNode Identifier { get; set; }
        public ASTArgsList Argslist { get; set; }
    }

    public class ASTSubscript : ASTNode
    {
        public ASTNode Identifier { get; set; }

        public ASTNode Subscript { get; set; }
    }

    public class ASTFunctionCall : ASTNode
    {
        public ASTNode Identifier { get; set; }

        public ASTNode ArgumentList { get; set; }
    }

    public class ASTReturnCall : ASTNode
    {
        public ASTNode Expression { get; set; }
    }


    public class ASTString : ASTNode
    {
        public ASTString(string value)
        {
            this.Value = value;
        }
        public string Value { get; set; }
    }

    public class ASTDigit : ASTNode
    {
        public ASTDigit(int value)
        {
            this.Value = value;
        }
        public int Value { get; set; }
    }
}
