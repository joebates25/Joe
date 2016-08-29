using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class UnknownDigitException : Exception
    {
        public UnknownDigitException()
        {
        }

        public UnknownDigitException(string message) : base(message)
        {
        }

        public UnknownDigitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownDigitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}