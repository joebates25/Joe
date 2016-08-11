using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class VariableAlreadyExistsInScopeException : Exception
    {
        public VariableAlreadyExistsInScopeException()
        {
        }

        public VariableAlreadyExistsInScopeException(string message) : base(message)
        {
        }

        public VariableAlreadyExistsInScopeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VariableAlreadyExistsInScopeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}