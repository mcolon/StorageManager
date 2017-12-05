using System.Reflection;

namespace StorageManager.Query
{
    public class StorageMemberAccessInfo
    {
        public StorageMemberAccessInfo(MemberInfo memberPath, object[] parameters = null)
        {
            MemberPath = memberPath;
            Parameters = parameters ?? new object[0];
        }

        public MemberInfo MemberPath { get; }
        public object[] Parameters { get; }

        public override string ToString()
        {

            return MemberPath is PropertyInfo 
                ? MemberPath.Name
                : $"{MemberPath.Name}({string.Join(", ", Parameters)})";
        }
    }
}