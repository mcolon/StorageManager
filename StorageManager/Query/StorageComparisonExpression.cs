using System;
using System.Linq.Expressions;
using StorageManager.Enums;
using StorageManager.Exceptions;

namespace StorageManager.Query
{
    public class StorageComparisonExpression : StorageBinaryExpression
    {
        public StorageComparisonExpression(StorageQueryOperand left, StorageQueryOperand right, ExpressionType bNodeType) : base(left, right)
        {
            if (left is StorageQueryEntityMember && right is StorageQueryEntityMember)
            {
                var bleft = left;
                throw new InvalidOperationException("The operands of a Comparsion cant no be both EntityAccess");
            }

            if (left is StorageQueryConstantMember && right is StorageQueryConstantMember)
            {
                var bleft = left;
                throw new InvalidOperationException("The operands of a Comparsion cant no be both Constant");
            }

            switch (bNodeType)
            {
                case ExpressionType.Equal:Operator = QueryComparisonOperator.Equal;
                    break;
                case ExpressionType.NotEqual:
                    Operator = QueryComparisonOperator.NotEqual;
                    break;
                case ExpressionType.GreaterThan:
                    Operator = QueryComparisonOperator.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Operator = QueryComparisonOperator.GreaterThanOrEqual;
                    break;
                case ExpressionType.LessThan:
                    Operator = QueryComparisonOperator.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    Operator = QueryComparisonOperator.LessThanOrEqual;
                    break;
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(bNodeType), $"Operator {bNodeType} not supported");
            }
        }

        public StorageQueryEntityMember EntityMember => LeftOperand as StorageQueryEntityMember ?? RightOperand as StorageQueryEntityMember;
        public StorageQueryConstantMember ConstantMember => LeftOperand as StorageQueryConstantMember ?? RightOperand as StorageQueryConstantMember;
        public QueryComparisonOperator Operator { get; set; }

        public override object Evaluate(object source = null)
        {
            var left = LeftOperand.Evaluate();
            var right = RightOperand.Evaluate();

            switch (Operator)
            {
                case QueryComparisonOperator.Equal:
                    return (dynamic)left == (dynamic)right;
                case QueryComparisonOperator.NotEqual:
                    return (dynamic)left != (dynamic)right;
                case QueryComparisonOperator.GreaterThan:
                    return (dynamic)left > (dynamic)right;
                case QueryComparisonOperator.GreaterThanOrEqual:
                    return (dynamic)left >= (dynamic)right;
                case QueryComparisonOperator.LessThan:
                    return (dynamic)left < (dynamic)right;
                case QueryComparisonOperator.LessThanOrEqual:
                    return (dynamic)left <= (dynamic)right;
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(Operator), $"Operator {Operator} not supported");
            }
        }

        public override string ToString()
        {
            return $"{LeftOperand.ToString()} {Operator} {RightOperand.ToString()}";
        }
    }
}