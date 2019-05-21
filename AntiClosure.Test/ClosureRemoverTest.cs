using AntiClosure.AST;
using AntiClosure.CST;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AntiClosure.Test
{

    public class ClosureRemoverTest
    {

        protected static Node_FunctionDecl ExecuteClosureRemover(string code)
        {

            AntlrInputStream inputStream = new AntlrInputStream(code);
            LikeJavaScriptLexer lexer = new LikeJavaScriptLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            LikeJavaScriptParser parser = new LikeJavaScriptParser(commonTokenStream);

            var astBuilder = new CstToAstVisitor();
            var ast = astBuilder.Visit(parser.compileUnit()) as Node_FunctionDecl;
            var closureRemover = new ClosureRemover.ClosureRemover();
            closureRemover.RemoveClosures(ast);

            return ast;
        }

        protected static bool InnerFunctionsExist(Node_FunctionDecl root)
        {
            bool isExist = root.Body
                .Where(stmt => stmt is Node_Statement_FunctionDecl)
                .Any(stmt =>
                    (stmt as Node_Statement_FunctionDecl).FunctionDecl.Body.Any(innerStmt =>
                        innerStmt is Node_Statement_Expr
                    )
                );
            return isExist;
        }
        
        protected static bool FunctionsDeclarated(Node_FunctionDecl root, IEnumerable<Node_FunctionDecl> functions)
        {
            Dictionary<string, Node_FunctionDecl> funcs = functions.ToDictionary(f => f.Name);
            var rootFuncs = root.Body.Select(stmt => (stmt as Node_Statement_FunctionDecl)?.FunctionDecl).Where(fstmt => fstmt != null);
            return rootFuncs.Count() == functions.Count() &&
                rootFuncs.All(rf =>
               {
                   if (funcs.TryGetValue(rf.Name, out var f))
                   {
                       return f.Params.SequenceEqual(f.Params);
                   }
                   else return false;
               });
        }

        [Fact]
        public void DefaultTest()
        {
            //arrange
            string code = @"
function foo(a) {
  var b = 42;
  function bar(c) {
    return a + b + c;
  }
  return bar(24);
};
";
            //act
            var ast = ExecuteClosureRemover(code);
            //asserts
            Assert.False(InnerFunctionsExist(ast));
        }

        [Theory]
        [MemberData(nameof(CodeExamples))]
        public void HandTests( string code)
        {
            Console.Error.WriteLine("Example: ");
            Console.WriteLine(code);
            Console.WriteLine("Result: ");

            StreamWriter outputStreamWriter = new StreamWriter(Console.OpenStandardOutput());
            outputStreamWriter.AutoFlush = true;
            Console.SetOut(outputStreamWriter);

            var printer = new AstVisitor_Print();
            printer.Print(outputStreamWriter, ExecuteClosureRemover(code));
        }

        public static IEnumerable<object[]> CodeExamples()
        {
            yield return new object[] { @"
function foo(a) {
  var b = 42;
  function bar(c) {
    return a + b + c;
  }
  return bar(24);
};" };
            yield return new object[] { @"
var a;
function f1() {
	function f2(){
		function f3(){
			function f4(){
				function f5(){
					return a;
				}
				return f5();
			}
			retrun f4()
		}
		return f3();
	}
	return f2();
}
f1();" };
            yield return new object[] { @"
var a;
function f1() {
	var b
	function f2(){
		var c
		function f3(){
			var d
			function f4(){
				var e
				function f5(){
					return a + b + c + d + e;
				}
				return f5();
			}
			retrun f4()
		}
		return f3();
	}
	return f2();
}
f1();" };
            yield return new object[] { @"
var a = 2;
function f(){
	var a = 3;
}
f();" };
        }
    }
}
