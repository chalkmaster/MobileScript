using System;
using System.Runtime.Serialization;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Exceção durante a compilação
    /// </summary>
    public class CompilationException : ApplicationException
    {
        public CompilationException() { }

        public CompilationException(string message) : base(message) { }

        public CompilationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
