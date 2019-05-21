using System;
using System.Collections.Generic;
using System.Text;

namespace AntiClosure.AST
{
    public abstract class Node
    {
        
    }
    

    public class Node_Const : Node
    {
        public int Value { get; set; }
    }

    public abstract class Node_Expr : Node { }

    public class Node_Expr_FunctionCall : Node_Expr
    {
        public string Name { get; set; }
        public LinkedList<Node_Expr> Param_Nodes { get; set; } = new LinkedList<Node_Expr>();
    }


    public class Node_Expr_Const : Node_Expr
    {
        public Node_Const Node_Const { get; set; }
        public int? Value { get => Node_Const?.Value; }
    }

    public class Node_Expr_ID : Node_Expr
    {
        public string Name { get; set; }
    }
    
    public class Node_Expr_Add : Node_Expr
    {
        public Node_Expr Left { get; set; }
        public Node_Expr Right { get; set; }
    }
 
    public class Node_Statement : Node
    {

    }

    public class Node_Statement_Expr : Node_Statement
    {
        public Node_Expr Expr { get; set; }
    }

    public class Node_Statement_Return : Node_Statement
    {
        public Node_Expr Expr { get; set; }
    }

    public class Node_Statement_VarDecl : Node_Statement
    {
        public Node_VarDecl Node_VarDecl { get; set; }
    }


    public class Node_Statement_FunctionDecl : Node_Statement
    {
        public Node_FunctionDecl FunctionDecl { get; set; }
    }
    

    public class Node_FunctionDecl : Node
    {
        public LinkedList<string> Params { get; set; } = new LinkedList<string>();
        public LinkedList<Node_Statement> Body { get; set; } = new LinkedList<Node_Statement>();
        public string Name { get; set; }
        public const string MAIN = "-main-";
    }

    public class Node_VarDecl : Node
    {
        public string Name { get; set; }
        public Node_Expr Value { get; set; }
    }
}
