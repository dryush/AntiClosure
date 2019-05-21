using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AntiClosure.AST;

namespace AntiClosure.ClosureRemover
{


    public class ClosureRemover
    {

        public void RemoveClosures(Node_FunctionDecl root)
        {
            AC_FunctionDeclTree acFunctionDeclTree = new AC_FunctionDeclTree();

            var fillFunctionDeclarationsInfoVisiter = new FillFunctionDeclarationsInfoVisitor(acFunctionDeclTree);
            fillFunctionDeclarationsInfoVisiter.Visit(root);

            var fillFunctionCallsInfoVisiter = new FillFunctionCallsInfoVisitor(acFunctionDeclTree);
            fillFunctionCallsInfoVisiter.Visit(root);

            RemoveClosures(acFunctionDeclTree.FunctionsTable[root]);
            UpFunctions(root, acFunctionDeclTree.FunctionsTable.Values);

        }

        protected void SetFullParamsList(AC_FunctionDeclTree.AC_FunctionDecl func)
        {
            var declaretedVarNames = func.Node_VarDecls.Select(nvd => nvd.Name);
            var funcParamNames = func.Cur_Node_FunctionDecl.Params;
            var notDeclVarNames = func.UsedVarNames
                .Except(declaretedVarNames)
                .Except(funcParamNames);

            if (notDeclVarNames.Count() > 0)
            {
                foreach (var notDeclVarName in notDeclVarNames)
                {
                    func.Cur_Node_FunctionDecl.Params.AddLast(notDeclVarName);

                    foreach (var caller in func.Callers)
                    {
                        caller.UsedVarNames.Add(notDeclVarName);
                    }

                    foreach (var call in func.Calls)
                    {
                        call.Param_Nodes.AddLast(new Node_Expr_ID() { Name = notDeclVarName });
                    }
                }

            }

        }

        protected void RemoveClosures(AC_FunctionDeclTree.AC_FunctionDecl functionDecl)
        {
            foreach (var childFunctionDecl in functionDecl.Child_FuncDecls)
            {
                RemoveClosures(childFunctionDecl);
            }
            SetFullParamsList(functionDecl);
        }

        protected void UpFunctions(Node_FunctionDecl root, IEnumerable<AC_FunctionDeclTree.AC_FunctionDecl> funcs)
        {
            foreach (var func in funcs)
            {
                if (func.Parent != null)
                {
                    var funcStatement = func.Parent.Cur_Node_FunctionDecl.Body.First(st => (st as Node_Statement_FunctionDecl)?.FunctionDecl == func.Cur_Node_FunctionDecl);
                    func.Parent.Cur_Node_FunctionDecl.Body.Remove(funcStatement);
                    root.Body.AddLast(funcStatement);
                }
            }
        }


        protected class FillFunctionDeclarationsInfoVisitor : AstVisitor_Empty
        {
            public AC_FunctionDeclTree AC_FunctionDeclTree { get; protected set; }

            public FillFunctionDeclarationsInfoVisitor(AC_FunctionDeclTree functionDeclTree)
            {
                this.AC_FunctionDeclTree = functionDeclTree;
            }

            public override object Visit(Node_Expr_ID node)
            {
                AC_FunctionDeclTree.CurrentFunc.AddUsedVars(node);

                return base.Visit(node);
            }

            public override object Visit(Node_VarDecl node)
            {
                AC_FunctionDeclTree.CurrentFunc.AddVarDecl(node);
                return base.Visit(node);
            }

            public override object Visit(Node_FunctionDecl node)
            {
                AC_FunctionDeclTree.EnterFunction(node);

                var n = base.Visit(node);

                AC_FunctionDeclTree.LeaveFunction();
                return n;
            }
        }


        protected class FillFunctionCallsInfoVisitor : AstVisitor_Empty
        {
            public AC_FunctionDeclTree AC_FunctionDeclTree { get; protected set; }

            public FillFunctionCallsInfoVisitor(AC_FunctionDeclTree functionDeclTree)
            {
                this.AC_FunctionDeclTree = functionDeclTree;
            }

            LinkedList<AC_FunctionDeclTree.AC_FunctionDecl> funcsWay = new LinkedList<AC_FunctionDeclTree.AC_FunctionDecl>();
            public override object Visit(Node_FunctionDecl node)
            {
                funcsWay.AddLast( AC_FunctionDeclTree.FunctionsTable[node]);

                var n = base.Visit(node);

                funcsWay.RemoveLast();

                return n;
            }

            public override object Visit(Node_Expr_FunctionCall node)
            {
                funcsWay.Last().AddFunctionCall(node);
                var _node = base.Visit(node);

                return _node;
            }
        }


        protected class AC_FunctionDeclTree
        {
            Dictionary<Node_FunctionDecl, AC_FunctionDecl> _FunctionsTable = new Dictionary<Node_FunctionDecl, AC_FunctionDecl>();
            public IReadOnlyDictionary<Node_FunctionDecl, AC_FunctionDecl> FunctionsTable { get => _FunctionsTable; }
            public class AC_FunctionDecl
            {
                /*Узел Объявления Функции*/
                public Node_FunctionDecl Cur_Node_FunctionDecl { get; set; }
                /*Узлы Объявления Переменных */
                protected LinkedList<Node_VarDecl> _Node_VarDecls { get; set; } = new LinkedList<Node_VarDecl>();
                public IEnumerable<Node_VarDecl> Node_VarDecls { get => _Node_VarDecls; }
                /*Узлы Функций, ОБъявленных внутри*/
                protected LinkedList<AC_FunctionDecl> _Child_FuncDecls { get; set; } = new LinkedList<AC_FunctionDecl>();
                public IEnumerable<AC_FunctionDecl> Child_FuncDecls { get => _Child_FuncDecls; }
                /*Узлы вызова Функций внутри*/
                protected LinkedList<Node_Expr_FunctionCall> _FunctionCallsInBody { get; set; } = new LinkedList<Node_Expr_FunctionCall>();
                public IEnumerable<Node_Expr_FunctionCall> FunctionCallsInBody { get => _FunctionCallsInBody; }

                /*Функции, которые вызывали эту функцию*/
                protected LinkedList<AC_FunctionDecl> _Callers { get; set; } = new LinkedList<AC_FunctionDecl>();
                public IEnumerable<AC_FunctionDecl> Callers { get => _Callers; }

                /*Вызовы этой функции*/
                protected LinkedList<Node_Expr_FunctionCall> _Calls { get; set; } = new LinkedList<Node_Expr_FunctionCall>();
                public IEnumerable<Node_Expr_FunctionCall> Calls { get => _Calls; }

                /*Переменные, использованные в функции*/
                protected LinkedList<Node_Expr_ID> _UsedVars { get; set; } = new LinkedList<Node_Expr_ID>();
                public IEnumerable<Node_Expr_ID> UsedVars { get => _UsedVars; }
                public HashSet<string> UsedVarNames { get; } = new HashSet<string>();

                public AC_FunctionDecl Parent { protected set; get; }

                protected HashSet<string> _innerVars = new HashSet<string>();

                public AC_FunctionDecl(Node_FunctionDecl funcDecl)
                {
                    Cur_Node_FunctionDecl = funcDecl;
                }

                public void AddChildDecl(AC_FunctionDecl functionDecl)
                {
                    functionDecl.Parent = this;
                    _Child_FuncDecls.AddLast(functionDecl);
                }

                public void AddChildDecl(Node_FunctionDecl functionDecl)
                {
                    AddChildDecl(new AC_FunctionDecl(functionDecl));
                }

                public void AddVarDecl(Node_VarDecl varDecl)
                {
                    _innerVars.Add(varDecl.Name);
                    _Node_VarDecls.AddLast(varDecl);
                }

                public void AddFunctionCall(Node_Expr_FunctionCall funcCall)
                {
                    this._FunctionCallsInBody.AddLast(funcCall);
                    var funcCallDecl = FindDeclaration(funcCall);

                    if (funcCallDecl == null)
                        throw new NotFoundFunctionDeclarationException(funcCall.Name);

                    funcCallDecl._Calls.AddLast(funcCall);
                    funcCallDecl._Callers.AddLast(this);
                }

                protected AC_FunctionDecl FindDeclaration(Node_Expr_FunctionCall funcCall)
                {
                    return this.Child_FuncDecls.FirstOrDefault(f => f.Cur_Node_FunctionDecl.Name == funcCall.Name) ??
                        this.Parent?.FindDeclaration(funcCall);
                }

                public void AddUsedVars(Node_Expr_ID usedVarNode)
                {
                    this._UsedVars.AddLast(usedVarNode);
                    this.UsedVarNames.Add(usedVarNode.Name);
                }

            }

            public AC_FunctionDecl CurrentFunc { get; set; } = null;
            protected AC_FunctionDecl StartFunc { get; set; } = null;
            public void EnterFunction(Node_FunctionDecl nodeFuncDecl)
            {
                var acFuncDecl = new AC_FunctionDecl(nodeFuncDecl);
                if (CurrentFunc != null)
                    CurrentFunc.AddChildDecl(acFuncDecl);
                if (CurrentFunc == null)
                    StartFunc = acFuncDecl;
                CurrentFunc = acFuncDecl;
                _FunctionsTable[nodeFuncDecl] = CurrentFunc;
            }

            public void LeaveFunction()
            {
                CurrentFunc = CurrentFunc.Parent;
            }

        }

        public class NotFoundFunctionDeclarationException : Exception
        {
            public NotFoundFunctionDeclarationException(string functionName) 
                : base("Can`t find function declaration: " + functionName)
            { }
        }
    }
}
