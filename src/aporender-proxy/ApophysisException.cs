using System;
using System.Runtime.Serialization;

namespace Apophysis
{
    [Serializable] 
    public class ApophysisException : Exception
    {
        public ApophysisException(IntPtr hModule)
        {
            HModule = hModule;
        }
        public ApophysisException(IntPtr hModule, string message) : base(message)
        {
            HModule = hModule;
        }
        public ApophysisException(IntPtr hModule, string message, Exception innerException) : base(message, innerException) 
        {
            HModule = hModule;
        }

        public IntPtr HModule { get; }

        protected ApophysisException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}