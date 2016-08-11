using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joe
{
    class Program
    {
        static void Main(string[] args)
        {
            
            TokenStream stream = new TokenStream((new Lexer("sample.joe")).LexFile());
            Parser p = new Parser(stream);
            ASTStatementList tree = (ASTStatementList)p.Parse();
            (new Translator(tree)).Translate();
            //(new Translator((ASTStatementList)new Parser(new TokenStream((new Lexer("sample.joe")).LexFile())).Parse())).Translate();
            Console.ReadLine();     

            
        }
    }
}
