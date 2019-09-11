using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class BookManagerException : Exception
    {
        public BookManagerException() { }
        public BookManagerException(string message) : base(message) { }
        public BookManagerException(string message, Exception inner) : base(message, inner) { }

        protected BookManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}