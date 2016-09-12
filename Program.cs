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
            String content = "";
            Parser p;
            ASTStatementList tree;
            TokenStream stream;
            Translator Translator;
            if (content.Equals(""))
            {
                stream = new TokenStream((new Lexer()).LexFile("sample.joe"));
            }
            else
            {
                stream = new TokenStream((new Lexer()).LexString(content));
            }

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
                stream = new TokenStream((new Lexer()).LexFile("sample.joe"));
                p = new Parser(stream);
                tree = (ASTStatementList)p.Parse();
                Translator = new Translator(tree);
                Translator.Translate();
            }
            Console.WriteLine("Done Processing.................");
            Console.ReadLine();
        }
    }
}
