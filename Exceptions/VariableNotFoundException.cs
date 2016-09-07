using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class VariableNotFoundException : Exception
    {
        public VariableNotFoundException()
        {
        }

        public VariableNotFoundException(string message) : base(message)
        {

        }

        public VariableNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VariableNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}