using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class ScopeNotFoundException : Exception
    {
        public ScopeNotFoundException()
        {
        }

        public ScopeNotFoundException(string message) : base(message)
        {
        }

        public ScopeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScopeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}