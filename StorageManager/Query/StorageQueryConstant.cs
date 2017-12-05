namespace StorageManager.Query
{
    public class StorageQueryConstantMember : StorageQueryOperand
    {
        public StorageQueryConstantMember(object value = null)
        {
            Value = value;
        }

        public object Value { get; }
        public override object Evaluate(object source = null)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}