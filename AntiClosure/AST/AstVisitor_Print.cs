using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace AntiClosure.AST
{
    public class AstVisitor_Print : AstVisitor_Empty
    {
        int deep = 0;
        string Tabs => new string(' ', deep * 2);
        StreamWriter _writer = null;
        public void Print(StreamWriter writer, Node root)
        {
            using (_writer = writer)
            {
                Visit(root);
            }
        }

        public override object Visit(Node_Const node)
        {
            _writer.Write(node.Value);
            return null;
        }

        public override object Visit(Node_Expr_Add node)
        {
            Visit(node.Left);
            _writer.Write(" + ");
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
            _writer.Write(node.Name);
            _writer.Write("(");
            foreach( var paramNode in node.Param_Nodes.SkipLast(1))
            {
                Visit(paramNode);
                _writer.Write(", ");
            }
            if( node.Param_Nodes.Count > 0)
                Visit(node.Param_Nodes.Last());
            _writer.Write(")");

            return null;
        }
        public override object Visit(Node_Expr_ID node) {
            _writer.Write(node.Name);
            return null;
        }
        public override object Visit(Node_FunctionDecl node)
        {
            bool isMain = node.Name == Node_FunctionDecl.MAIN;
            if (!isMain)
            {
                _writer.Write("\n" + Tabs + "function " + node.Name + "(");
                foreach (var param in node.Params.SkipLast(1))
                {
                    _writer.Write(param);
                    _writer.Write(", ");
                }

                if (node.Params.Count > 0)
                    _writer.Write(node.Params.Last());
                _writer.Write(")");

                _writer.Write("{\n");
                deep++;
            }

            foreach ( var stmt in node.Body)
            {
                Visit(stmt);
            }

            if(!isMain)
            {
                deep--;

                _writer.Write( Tabs + "}\n\n");

            }
            return null;
        }
        public override object Visit(Node_Statement_Expr node)
        {
            _writer.Write(Tabs);
            Visit(node.Expr);
            _writer.Write(";\n");
            return null;
        }
        public override object Visit(Node_Statement_FunctionDecl node)
        {
            Visit(node.FunctionDecl);
            return null;
        }

        public override object Visit(Node_Statement_Return node)
        {
            _writer.Write(Tabs + "return ");
            Visit(node.Expr);
            _writer.Write(";\n");
            return null;
        }
        public override object Visit(Node_Statement_VarDecl node)
        {
            _writer.Write(Tabs);
            Visit(node.Node_VarDecl);
            _writer.Write(";\n");
            return null;
        }
        public override object Visit(Node_VarDecl node)
        {
            _writer.Write("var " + node.Name);
            if (node.Value != null)
            {
                _writer.Write(" = ");
                Visit(node.Value);
            }
            return null;
        }
    }
}
