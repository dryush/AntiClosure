using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using AntiClosure.AST;

namespace AntiClosure.CST
{

  

    public class CstToAstVisitor : LikeJavaScriptBaseVisitor<Node>
    {
        public override Node VisitCompileUnit([NotNull] LikeJavaScriptParser.CompileUnitContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var bodyNodes = context.stmt_list_opt()?.stmt_list()?.stmt()?.Select(stmt => (Visit(stmt))) ?? new List<Node>();
            var bodyStmtNodes = bodyNodes.Where(n => n != null).Select(n => n as Node_Statement);

            return new Node_FunctionDecl()
            {
                Body = new LinkedList<Node_Statement>(bodyStmtNodes),
                Name = Node_FunctionDecl.MAIN
            };
        }

        public override Node VisitConst([NotNull] LikeJavaScriptParser.ConstContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Const
            {
                Value = int.Parse(context.INT().ToString())
            };
        }
        
        public override Node VisitExpr_add([NotNull] LikeJavaScriptParser.Expr_addContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Expr_Add()
            {
                Left = Visit(context.expr()[0]) as Node_Expr,
                Right = Visit(context.expr()[1]) as Node_Expr,
            };
        }

        public override Node VisitExpr_const([NotNull] LikeJavaScriptParser.Expr_constContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Expr_Const() { Node_Const = Visit(context.@const()) as Node_Const };
        }

        public override Node VisitExpr_function_call([NotNull] LikeJavaScriptParser.Expr_function_callContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var exprs = context.function_call().expr_list_opt()?.expr_list();
            var paramNodes = new LinkedList<Node_Expr>();
            while (exprs != null) {
                paramNodes.AddLast(Visit(exprs.expr()) as Node_Expr);
                exprs = exprs.expr_list();
            }
            return new Node_Expr_FunctionCall()
            {
                Name = context.function_call().Identifier().ToString() ,
                Param_Nodes = paramNodes
            };
        }

        public override Node VisitExpr_id([NotNull] LikeJavaScriptParser.Expr_idContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Expr_ID { Name = context.Identifier().ToString() }; 
        }

        public override Node VisitFunction_call([NotNull] LikeJavaScriptParser.Function_callContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var exprs = context.expr_list_opt()?.expr_list();
            var paramNodes = new LinkedList<Node_Expr>();
            while (exprs != null)
            {
                paramNodes.AddLast(Visit(exprs.expr()) as Node_Expr);
                exprs = exprs.expr_list();
            }

            return new Node_Expr_FunctionCall()
            {
                Name = context.Identifier().ToString(),
                Param_Nodes = new LinkedList<Node_Expr>(paramNodes)
            };
        }

        public override Node VisitFunction_decl([NotNull] LikeJavaScriptParser.Function_declContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var bodyNodes = context.stmt_list_opt()?.stmt_list()?.stmt()?.Select(stmt => (Visit(stmt))) ?? new Node[0];
            var bodyStmtNodes = bodyNodes.Where(n => n != null).Select(n => n as Node_Statement);
            var paramNodeNames = context.function_decl_params_opt()?.function_decl_params()?.Identifier()?.Select( i => i.ToString()) ?? new string[0];
            return new Node_FunctionDecl()
            {
                Body = new LinkedList<Node_Statement>( bodyStmtNodes),
                Params = new LinkedList<string>( paramNodeNames),
                Name = context.Identifier()?.ToString() ?? "",
            };
        }
        
        public override Node VisitStmt_expr([NotNull] LikeJavaScriptParser.Stmt_exprContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Statement_Expr()
            {
                Expr = Visit(context.expr_stmt().expr()) as Node_Expr
            };
        }

        public override Node VisitStmt_func_decl([NotNull] LikeJavaScriptParser.Stmt_func_declContext context)
        {
            if (context.exception != null)
                throw context.exception;

            return new Node_Statement_FunctionDecl()
            {
                FunctionDecl = Visit(context.function_decl()) as Node_FunctionDecl
            };
        }

        public override Node VisitStmt_return([NotNull] LikeJavaScriptParser.Stmt_returnContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var expr = context.return_stmt()?.expr() != null ? Visit(context.return_stmt().expr()) : null;
            return new Node_Statement_Return()
            {
                Expr = expr as Node_Expr
            };
        }

        public override Node VisitStmt_var_decl([NotNull] LikeJavaScriptParser.Stmt_var_declContext context)
        {
            if (context.exception != null)
                throw context.exception;

            var val = context.var_decl_stmt().expr() != null ? Visit(context.var_decl_stmt().expr()) : null;
            return new Node_Statement_VarDecl()
            {
                Node_VarDecl = new Node_VarDecl()
                {
                    Name = context.var_decl_stmt().Identifier().ToString(),
                    Value = val as Node_Expr,
                }
            };
        }
        
        

    }
}
    
