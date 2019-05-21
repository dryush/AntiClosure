using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiClosure.AST
{
    public abstract class AstVisitor<T> 
    {
        public abstract T Visit(AST.Node_Const node);
        public abstract T Visit(AST.Node_Expr_Add node);
        public abstract T Visit(AST.Node_Expr_Const node);
        public abstract T Visit(AST.Node_Expr_FunctionCall node);
        public abstract T Visit(AST.Node_Expr_ID node);
        public abstract T Visit(AST.Node_FunctionDecl node);
        public abstract T Visit(AST.Node_Statement_Expr node);
        public abstract T Visit(AST.Node_Statement_FunctionDecl node);
        public abstract T Visit(AST.Node_Statement_Return node);
        public abstract T Visit(AST.Node_Statement_VarDecl node);
        public abstract T Visit(AST.Node_VarDecl node);

        public T OnNullReturn { get => default; }

        public T Visit(Node node)
        {
            if (node == null)
                return OnNullReturn;
            return Visit((dynamic)node);
        }
    }
    


    public class AstVisitor_Empty : AST.AstVisitor<object>
    {

        public override object Visit(Node_Const node)
        {
            return null;
        }

        public override object Visit(Node_Expr_Add node)
        {
            Visit(node.Left);
            Visit(node.Right);
            return null;
        }

        public override object Visit(Node_Expr_Const node)
        {
            Visit(node.Node_Const);
            return null;
        }

        public override object Visit(Node_Expr_FunctionCall node)
        {
            foreach (var paramNode in node.Param_Nodes)
                Visit(paramNode);
            return null;
        }

        public override object Visit(Node_Expr_ID node)
        {
            return null;
        }

        public override object Visit(Node_FunctionDecl node)
        {
            foreach (var bodyStmt in node.Body)
                Visit(bodyStmt);
            return null;
        }


        public override object Visit(Node_Statement_Expr node)
        {
            Visit(node.Expr);
            return null;
        }

        public override object Visit(Node_Statement_FunctionDecl node)
        {
            Visit(node.FunctionDecl);
            return null;
        }

        public override object Visit(Node_Statement_Return node)
        {
            Visit(node.Expr);
            return null;
        }

        public override object Visit(Node_Statement_VarDecl node)
        {
            Visit(node.Node_VarDecl);
            return null;
        }

        public override object Visit(Node_VarDecl node)
        {
            Visit(node.Value);
            return null;
        }
    }

}
