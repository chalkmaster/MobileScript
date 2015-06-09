using System.Runtime.Serialization;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Exceção durante o parsing.
    /// </summary>
    public class ParsingException : CompilationException
    {
        public ParsingException() { }

        public ParsingException(string message) : base(message) { }

        public ParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
