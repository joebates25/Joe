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
            Parser p;
            ASTStatementList tree;    
            TokenStream stream;
            Translator t;
            stream = new TokenStream((new Lexer("sample.joe")).LexFile());
            if (args[0].Equals("token"))
            {
                
            }
            if (args[0].Equals("tree"))
            {
                
                p = new Parser(stream);
                tree = (ASTStatementList)p.Parse();
            }
            if (args[0].Equals("translate"))
            {
                
                p = new Parser(stream);
                tree = (ASTStatementList)p.Parse();
                t = new Translator(tree);
                t.Translate();
            }

            Console.WriteLine("Done Processing.................");                                                                                                                            
            Console.ReadLine();             
        }
    }
}
