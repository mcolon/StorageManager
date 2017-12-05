using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using StorageManager.Exceptions;
using StorageManager.Query;

namespace StorageManager.Extensions
{
    public static class ExpressionExtensions
    {
        public static string PropertyPath(this Expression source)
        {
            if (source == null)
                return null;

            if (source is MemberExpression memberExp)
            {
                var stringPath = memberExp.ToString();
                var firstDot = stringPath.IndexOf('.');
                return firstDot > 0 ? stringPath.Substring(firstDot + 1) : stringPath;
            }

            if (source.GetType().GetGenericTypeDefinition() == typeof(Expression<>))
            {
                var arguments = source.GetType().GetGenericArguments().ElementAt(0);
                if (arguments.GetGenericTypeDefinition() == typeof(Func<,>))
                {
                    var property = source.GetType().GetProperty("Body");
                    return ((Expression)property.GetValue(source)).PropertyPath();
                }
            }
            return source.ToString();
        }

        public static Expression FindEntityExpression<T>(this Expression arg)
        {
            if (arg is BinaryExpression binaryExpression)
            {
                if (binaryExpression.Left.NodeType == ExpressionType.MemberAccess && CheckType<T>((MemberExpression)binaryExpression.Left))
                    return binaryExpression.Left;
                if (binaryExpression.Right.NodeType == ExpressionType.MemberAccess && CheckType<T>((MemberExpression)binaryExpression.Right))
                    return binaryExpression.Right;
            }
            return null;
        }


        public static object InvokeMemberAcces(this MemberExpression expression, object entity)
        {
            object result = entity;

            if (expression.Expression is MemberExpression member)
                result = InvokeMemberAcces(member, entity);


            if (expression.Member is PropertyInfo prop)
            {
                if (result != null)
                    return prop.GetValue(result);

                return null;
            }

            throw new StorageArgumentException($"Cant read value for {expression}");
        }

        public static object GetExpressionFuncValue<T, TOut>(this Expression<Func<T, TOut>> func, T entity)
        {
            var compiled = func.Compile();
            return compiled(entity);
        }

        public static object Evaluate(this Expression toEvaluate) 
        {
            if (toEvaluate is ConstantExpression constant)
                return constant.Value;

            if (toEvaluate is UnaryExpression unaryExpression)
            { 
                if (unaryExpression.Operand is NewExpression newExpression)
                {
                    return Activator.CreateInstance(newExpression.Type, newExpression.Arguments.Select(e => ((ConstantExpression)e).Value).ToArray());
                }
            }

            throw new StorageArgumentException($"Can not evaluate Expression '{toEvaluate}'");
        }

        public static StorageQueryEntityMember ToMemberAccessInfo(this Expression source)
        {
            StorageQueryTranslator _translator = new StorageQueryTranslator();

            var result = _translator.Translate(source);
                if (result is StorageQueryEntityMember memberAcces)
                    return memberAcces;
            throw new StorageArgumentOutOfRangeException(nameof(source), $"Can not build a {nameof(StorageQueryEntityMember)} from the expression '{source}'");
        }
        public static StorageQueryConstantMember ToConstantAccessInfo(this Expression source)
        {
            StorageQueryTranslator _translator = new StorageQueryTranslator();

            var result = _translator.Translate(source);
                if (result is StorageQueryConstantMember constantAcces)
                    return constantAcces;
            throw new StorageArgumentOutOfRangeException(nameof(source), $"Can not build a {nameof(StorageQueryConstantMember)} from the expression '{source}'");
        }

        private static bool CheckType<T>(MemberExpression member)
        {
            var exp = member.Expression;
            do
            {
                if (exp.Type == typeof(T))
                    return true;

                if (exp.NodeType == ExpressionType.MemberAccess)
                    exp = ((MemberExpression)exp).Expression;
            } while (exp != null);
            return false;
        }

    }
}
