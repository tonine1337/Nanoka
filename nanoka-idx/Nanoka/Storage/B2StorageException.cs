using System;
using System.Runtime.Serialization;

namespace Nanoka.Storage
{
    [Serializable]
    public class B2StorageException : Exception
    {
        public B2StorageException() { }
        public B2StorageException(string message) : base(message) { }
        public B2StorageException(string message, Exception inner) : base(message, inner) { }

        protected B2StorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}