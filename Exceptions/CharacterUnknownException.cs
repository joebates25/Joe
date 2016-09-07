using System;
using System.Runtime.Serialization;

namespace Joe
{
    [Serializable]
    internal class CharacterUnknownException : Exception
    {
        public CharacterUnknownException()
        {
        }

        public CharacterUnknownException(string message) : base(message)
        {
        }

        public CharacterUnknownException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CharacterUnknownException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}