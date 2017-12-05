using System.Linq.Expressions;
using StorageManager.Enums;
using StorageManager.Exceptions;

namespace StorageManager.Query
{
    public class StorageLogicalExpression : StorageBinaryExpression
    {
        public StorageLogicalExpression(StorageQueryOperand left, StorageQueryOperand right, ExpressionType bNodeType) : base(left, right)
        {
            switch (bNodeType)
            {
                case ExpressionType.AndAlso:
                    Operator = QueryLogicalOperator.And;
                    break;
                case ExpressionType.OrElse:
                    Operator = QueryLogicalOperator.Or;
                    break;
                case ExpressionType.Not:
                    Operator = QueryLogicalOperator.Not;
                    break;
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(bNodeType), $"Operator {bNodeType} not supported");
            }
        }

        public StorageLogicalExpression(StorageQueryOperand left, StorageQueryOperand right, QueryLogicalOperator operatorNode) : base(left, right)
        {
            Operator = operatorNode;
        }

        public QueryLogicalOperator Operator { get; set; }
        public override object Evaluate(object source = null)
        {
            switch (Operator)
            {
                case QueryLogicalOperator.And:
                    return (bool)LeftOperand.Evaluate() && (bool)RightOperand.Evaluate();
                case QueryLogicalOperator.Or:
                    return (bool)LeftOperand.Evaluate() || (bool)RightOperand.Evaluate();
                case QueryLogicalOperator.Not:
                    return !(bool)LeftOperand.Evaluate();
                default:
                    throw new StorageArgumentOutOfRangeException(nameof(Operator), $"Operator {Operator} not supported");
            }
        }

        public override string ToString()
        {
            if (Operator == QueryLogicalOperator.Not)
                return $"{Operator}({LeftOperand.ToString()})";

            return $"({LeftOperand.ToString()}) {Operator} ({RightOperand.ToString()})";
        }
    }
}