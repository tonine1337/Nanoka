using System;
using System.Runtime.Serialization;

namespace Nanoka.Core.Client
{
    [Serializable]
    public class DatabaseClientException : Exception
    {
        public DatabaseClientException() { }
        public DatabaseClientException(string message) : base(message) { }
        public DatabaseClientException(string message, Exception inner) : base(message, inner) { }

        public DatabaseClientException(int status, string message)
            : base($"Database responded with {status}: {message ?? "<no reason>"}") { }

        protected DatabaseClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}