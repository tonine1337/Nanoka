using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class UserManagerException : Exception
    {
        public UserManagerException() { }
        public UserManagerException(string message) : base(message) { }
        public UserManagerException(string message, Exception inner) : base(message, inner) { }

        protected UserManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}