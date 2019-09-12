using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class ResultException : ApplicationException
    {
        public Result Result { get; }

        public ResultException(Result result) : base(result.Message)
        {
            Result = result;
        }

        protected ResultException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
