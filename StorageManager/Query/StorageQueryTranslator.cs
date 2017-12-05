using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using StorageManager.Enums;
using StorageManager.Exceptions;

namespace StorageManager.Query
{
    public class StorageQueryTranslator : ExpressionVisitor
    {
        protected int? Take { get; private set; }

        protected int? Skip { get; private set; }

        protected List<StorageQueryOrderCriteria> OrderCriteria { get; }


        protected readonly Stack<StorageQueryOperand> VisitedElements = new Stack<StorageQueryOperand>();





        public StorageQueryTranslator()
        {
            OrderCriteria = new List<StorageQueryOrderCriteria>();
        }






        public virtual StorageQueryOperand Translate(Expression expression)
        {
            Visit(expression);
            return VisitedElements.Any() ? VisitedElements.Pop() : null;
        }

        internal QueryResultType ResultType { get; set; }




        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Take")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                Take = (int?) VisitedElements.Pop().Evaluate();
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                Skip = (int?)VisitedElements.Pop().Evaluate();
                return m;
            }
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                var criteria = new StorageQueryOrderCriteria
                {
                    Direction = QueryOrderDirection.Ascending,
                    OrderField = (StorageQueryEntityMember) VisitedElements.Pop()
                };
                if (OrderCriteria.Any())
                    throw new StorageArgumentException("Order criteria already has some terms, use ThenBy");
                OrderCriteria.Add(criteria);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderByDescending")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                var criteria = new StorageQueryOrderCriteria
                {
                    Direction = QueryOrderDirection.Descending,
                    OrderField = (StorageQueryEntityMember)VisitedElements.Pop()
                };
                if (OrderCriteria.Any())
                    throw new StorageArgumentException("Order criteria already has some terms, use ThenByDescending");
                OrderCriteria.Add(criteria);
                return m;
            }


            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenBy")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                var criteria = new StorageQueryOrderCriteria
                {
                    Direction = QueryOrderDirection.Ascending,
                    OrderField = (StorageQueryEntityMember)VisitedElements.Pop()
                };
                if (!OrderCriteria.Any())
                    throw new StorageArgumentException("Order criteria not have terms, use OrderBy");
                OrderCriteria.Add(criteria);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenByDescending")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                var criteria = new StorageQueryOrderCriteria
                {
                    Direction = QueryOrderDirection.Descending,
                    OrderField = (StorageQueryEntityMember)VisitedElements.Pop()
                };
                if (!OrderCriteria.Any())
                    throw new StorageArgumentException("Order criteria not have terms, use OrderByDescending");
                OrderCriteria.Add(criteria);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "FirstOrDefault")
            {
                Take = 1;
                foreach (var arg in m.Arguments)
                    Visit(arg);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "First")
            {
                Take = 1;
                foreach (var arg in m.Arguments)
                    Visit(arg);
                return m;
            }

            if (m.Method.DeclaringType == typeof(string) && m.Method.Name == "IsNullOrWhiteSpace")
            {
                foreach (var arg in m.Arguments)
                    Visit(arg);
                return m;
            }

            try
            {
                ProcessMemberStack(m);
            }
            catch
            {
                throw new StorageArgumentException($"Can not process expression {m}");
            }
            return m;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    Visit(u.Operand);
                    var operand = VisitedElements.Pop();
                    VisitedElements.Push(new StorageComparisonExpression(operand, new StorageQueryConstantMember(false), ExpressionType.Equal ));
                    return u;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    return u.Operand;
                case ExpressionType.Quote:
                    var expressionAccessor = StripQuotes(u);
                    Visit(expressionAccessor);
                    return u;
                default:
                    throw new StorageNotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            Visit(b.Right);

            var right = VisitedElements.Pop();
            var left = VisitedElements.Pop();

            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Not:
                    VisitedElements.Push(new StorageLogicalExpression(left, right, b.NodeType));
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    VisitedElements.Push(new StorageComparisonExpression(left, right, b.NodeType));
                    break;
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(b.NodeType), $"Expression type {b.NodeType} not supported");
            }
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (!(c.Value is IQueryable q))
            {
                VisitedElements.Push(new StorageQueryConstantMember(c.Value));
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Call:
                    ProcessMemberStack(node);
                    break;
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(node.NodeType),$"Expression Type {node.Expression.NodeType} not supported in this point.");
            }

            return node;
        }

        private void ProcessMemberStack(Expression node, Stack<StorageMemberAccessInfo> stack = null)
        {
            stack = stack ?? new Stack<StorageMemberAccessInfo>();

            switch (node)
            {
                case ParameterExpression param:
                    VisitedElements.Push(new StorageQueryEntityMember(stack));
                    break;
                case MemberExpression member:
                    if (member.Expression == null)
                    {
                        if (member.Member is PropertyInfo prop)
                            VisitedElements.Push(new StorageQueryConstantMember(prop.GetValue(null)));
                        else if (member.Member is MethodInfo method)
                            VisitedElements.Push(new StorageQueryConstantMember(method.Invoke(null, new object[0])));
                    }
                    else
                    {
                        stack.Push(new StorageMemberAccessInfo(member.Member));
                        ProcessMemberStack(member.Expression, stack);
                    }
                    break;
                case MethodCallExpression call:
                    List<StorageQueryConstantMember> parameters = new List<StorageQueryConstantMember>();
                    foreach (var argument in call.Arguments)
                    {
                        Visit(argument);
                        var element = VisitedElements.Pop();
                        if (argument.NodeType == ExpressionType.Constant)
                            parameters.Add((StorageQueryConstantMember)element);
                        else
                            throw new InvalidOperationException($"Expression {argument.NodeType} not allowed as ExpressionCall argunent");
                    }
                    if (call.Object == null)
                    {
                        var value = call.Method.Invoke(null,
                            parameters.Select(e => e.Evaluate()).ToArray());
                        VisitedElements.Push(new StorageQueryConstantMember(value));
                    }
                    break;
                case ConstantExpression constant:
                    StorageMemberAccessInfo pathPart = null;
                    object instanceValue = constant.Value;
                    while (stack.Any())
                    {
                        pathPart = stack.Pop();
                        if (pathPart.MemberPath is PropertyInfo prop)
                            instanceValue = prop.GetValue(instanceValue);
                        else if (pathPart.MemberPath is FieldInfo fld)
                            instanceValue = fld.GetValue(instanceValue);
                        else if (pathPart.MemberPath is MethodInfo method)
                            instanceValue = method.Invoke(instanceValue, new object[0]);
                    }
                    VisitedElements.Push(new StorageQueryConstantMember(instanceValue));
                    break;
                default:
                    throw new InvalidOperationException($"Expression type: '{node.NodeType}' not implemented");
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            List<object> arguments = new List<object>();
            foreach (var arg in node.Arguments)
            {
                Visit(arg);
                arguments.Add(VisitedElements.Pop().Evaluate());
            }
            var newObject = Activator.CreateInstance(node.Type, arguments.ToArray());
            return base.Visit(Expression.Constant(newObject));
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return base.VisitCatchBlock(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return base.VisitElementInit(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return base.VisitLabelTarget(node);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return base.VisitSwitchCase(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }

        protected override Expression VisitConditional(System.Linq.Expressions.ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return base.VisitDebugInfo(node);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            return base.VisitDynamic(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            return base.VisitExtension(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            return base.VisitGoto(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            return base.VisitIndex(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return base.VisitInvocation(node);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            return base.VisitLabel(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            return base.VisitLoop(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return base.VisitMemberInit(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return base.VisitNewArray(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return base.VisitRuntimeVariables(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return base.VisitSwitch(node);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            return base.VisitTry(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return base.VisitTypeBinary(node);
        }







        private Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }


    }
}