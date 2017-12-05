namespace StorageManager.Query
{
    public abstract class StorageBinaryExpression : StorageQueryOperand
    {
        public StorageQueryOperand LeftOperand { get; set; }
        public StorageQueryOperand RightOperand { get; set; }

        protected StorageBinaryExpression(StorageQueryOperand left, StorageQueryOperand right)
        {
            LeftOperand = left;
            RightOperand = right;
        }
    }
}