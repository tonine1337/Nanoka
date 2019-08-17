using System;
using System.Runtime.Serialization;

namespace Nanoka
{
    [Serializable]
    public class ImageProcessorException : Exception
    {
        public ImageProcessorException() { }
        public ImageProcessorException(string message) : base(message) { }
        public ImageProcessorException(string message, Exception inner) : base(message, inner) { }

        protected ImageProcessorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}