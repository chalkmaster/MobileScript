using System.Runtime.Serialization;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    public class SemanticException : CompilationException
    {
        public SemanticException() { }

        public SemanticException(string message) : base(message) { }

        public SemanticException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
