using System.Runtime.Serialization;
using Seculus.MobileScript.Core.Extensions;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    public class UnexpectedSymbolException : ParsingException
    {
        public UnexpectedSymbolException() 
        { }

        public UnexpectedSymbolException(string message) : base(message)
        { }

        public UnexpectedSymbolException(LexSymbolKind expectedSymbolKind, LexSymbolKind actualSymbolKind) 
            : base("Unexpected symbol. Found {0} but was expecting {1}".FormatWith(actualSymbolKind, expectedSymbolKind))
        { }

        public UnexpectedSymbolException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { }
    }
}
