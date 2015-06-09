using System.ComponentModel;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    public enum ErrorCode : short
    {
        [Description("No error")]
        NoError = 0,

        [Description("Invalid address")]
		InvalidAddr = -1,

        [Description("Invalid operand")]
		InvalidOperand = -2,

        [Description("Invalid instruction code")]
		InvalidOpCode = -3,

        [Description("Invalid index")]
		IndexError = -4,

        [Description("Stack error")]
		StackError = -5,

        [Description("Integer overflow")]
		IntOvfl = -6,

        [Description("Unknown error")]
		UnknownError = -7,

        [Description("Null operand")]
		NullOperand = -8
    }
}
