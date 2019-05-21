using AntiClosure.AST;
using AntiClosure.CST;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.IO;
using System.Text;
using AntiClosure.ClosureRemover;
using Antlr4.Runtime.Misc;

namespace AntiClosure
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile;
            string outputFile;
            if ( args.Length < 2)
            {
                Console.WriteLine(@"Note:");
                Console.WriteLine(@" You can use console args to set input and output file");
                Console.WriteLine(@" Example: ");
                Console.WriteLine(@" AntiClosure input.txt output.txt");
                Console.WriteLine("");
                Console.WriteLine("Please, enter input file path:");
                inputFile = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("Please, enter output file path:");
                outputFile = Console.ReadLine();

            }
            else
            {
                inputFile = args[0];
                outputFile = args[1];
            }

            FileStream inputStream = null;
            try
            {
                inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException exp)
            {
                Console.Error.WriteLine("Can`t find input file: " + exp.Message);
                return;
            }
            catch (DirectoryNotFoundException exp)
            {
                Console.Error.WriteLine("Can`t find input file directory: " + exp.Message);
                return;
            }

            StreamReader inputStreamReader = new StreamReader(inputStream);

            //MemoryStream outputStream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            //StreamWriter outputStreamWriter = new StreamWriter(outputStream);
            //StreamWriter outputStreamWriter = new StreamWriter(Console.OpenStandardOutput());
            //outputStreamWriter.AutoFlush = true;
            //Console.SetOut(outputStreamWriter);
            FileStream outputStream = null;
            try
            {
                outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            }
            catch (FileNotFoundException exp)
            {
                Console.Error.WriteLine("Can`t find ouput file: " + exp.Message);
                return;
            }
            catch (DirectoryNotFoundException exp)
            {
                Console.Error.WriteLine("Can`t find ouput file directory: " + exp.Message);
                return;
            }
            StreamWriter outputStreamWriter = new StreamWriter(outputStream, Encoding.Default);
            

            try {
                RemoveClosures(inputStreamReader, outputStreamWriter);
            }
            catch (ParseCancellationException exp)
            {
                Console.Error.Write(exp.Message);
            }
            catch (ClosureRemover.ClosureRemover.NotFoundFunctionDeclarationException exp)
            {
                Console.Error.Write(exp.Message);
            }
            return;

        }

        static void RemoveClosures(StreamReader codeReader, StreamWriter codeWriter)
        {
            AntlrInputStream inputStream = new AntlrInputStream(codeReader);
            LikeJavaScriptLexer lexer = new LikeJavaScriptLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            LikeJavaScriptParser parser = new LikeJavaScriptParser(commonTokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ThrowingErrorListener.INSTANCE);

            var astBuilder = new CstToAstVisitor();
            var ast = astBuilder.Visit(parser.compileUnit()) as Node_FunctionDecl;
            var closureRemover = new ClosureRemover.ClosureRemover();
            closureRemover.RemoveClosures(ast);

            var printer = new AstVisitor_Print();
            printer.Print(codeWriter, ast);
        }
    }

    public class ThrowingErrorListener : BaseErrorListener
    {
        public static ThrowingErrorListener INSTANCE = new ThrowingErrorListener();
        

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new ParseCancellationException("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}