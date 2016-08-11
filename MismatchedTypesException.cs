using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class MismatchedTypesException : Exception
    {
        public MismatchedTypesException()
        {
        }

        public MismatchedTypesException(string message) : base(message)
        {
        }

        public MismatchedTypesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MismatchedTypesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}