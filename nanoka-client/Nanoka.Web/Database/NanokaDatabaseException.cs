using System;
using System.Runtime.Serialization;

namespace Nanoka.Web.Database
{
    [Serializable]
    public class NanokaDatabaseException : Exception
    {
        public NanokaDatabaseException() { }
        public NanokaDatabaseException(string message) : base(message) { }
        public NanokaDatabaseException(string message, Exception inner) : base(message, inner) { }

        protected NanokaDatabaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}