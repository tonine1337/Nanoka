using System;
using System.Net;
using System.Runtime.Serialization;

namespace Nanoka.Core.Client
{
    [Serializable]
    public class DatabaseClientException : Exception
    {
        public int? StatusCode { get; }

        public DatabaseClientException() { }
        public DatabaseClientException(string message) : base(message) { }
        public DatabaseClientException(string message, Exception inner) : base(message, inner) { }

        public DatabaseClientException(int status, string message)
            : base($"Database responded with {status} ({(HttpStatusCode) status}): {message ?? "<unknown reason>"}")
        {
            StatusCode = status;
        }

        protected DatabaseClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}