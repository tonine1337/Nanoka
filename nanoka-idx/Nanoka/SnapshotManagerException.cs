using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class SnapshotManagerException : Exception
    {
        public SnapshotManagerException() { }
        public SnapshotManagerException(string message) : base(message) { }
        public SnapshotManagerException(string message, Exception inner) : base(message, inner) { }

        protected SnapshotManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}