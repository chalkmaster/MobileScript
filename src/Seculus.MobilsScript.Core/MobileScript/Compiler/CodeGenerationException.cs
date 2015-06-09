using System.Runtime.Serialization;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    public class CodeGenerationException : CompilationException
    {
        public CodeGenerationException() { }

        public CodeGenerationException(string message) : base(message) { }

        public CodeGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
