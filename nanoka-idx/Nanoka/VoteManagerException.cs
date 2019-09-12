using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class VoteManagerException : Exception
    {
        public VoteManagerException() { }
        public VoteManagerException(string message) : base(message) { }
        public VoteManagerException(string message, Exception inner) : base(message, inner) { }

        protected VoteManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}