using System.Collections.Generic;
using System.Reflection;
using StorageManager.Exceptions;

namespace StorageManager.Query
{
    public class StorageQueryEntityMember : StorageQueryOperand
    {
        private StorageMemberAccessInfo[] StackValueMembers { get; }

        public StorageQueryEntityMember(Stack<StorageMemberAccessInfo> stack)
        {
            MemberPath = string.Empty;
            StackValueMembers = stack.ToArray();
            foreach (var member in StackValueMembers)
            {
                if (string.IsNullOrWhiteSpace(MemberPath))
                    MemberPath += member;
                else
                    MemberPath += "." + member;
            }
        }

        public string MemberPath { get; private set; }
        public override object Evaluate(object source = null)
        {
            if(source == null)
                throw new StorageArgumentException($"Can not evaluate expression {MemberPath} without source object");

            var currentObject = source;
            foreach(var member in StackValueMembers)
            {
                if (member.MemberPath is MethodInfo method)
                    currentObject = method.Invoke(currentObject, member.Parameters);
                else if (member.MemberPath is PropertyInfo property)
                    currentObject = property.GetValue(currentObject);
            }
            return currentObject;
        }

        public override string ToString()
        {
            return MemberPath;
        }
    }
}