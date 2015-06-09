namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    public enum RplOperationType
    {
        /// <summary>
        /// +
        /// </summary>
        Add = 2,

        /// <summary>
        /// -
        /// </summary>
        Subtract = 3,

        /// <summary>
        /// *
        /// </summary>
        Multiply = 4,

        /// <summary>
        /// /
        /// </summary>
        Divide = 5,

        /// <summary>
        /// %
        /// </summary>
        Module = 6,

        /// <summary>
        /// ==
        /// </summary>
        Equal = 7,

        /// <summary>
        /// !=
        /// </summary>
        NotEqual = 8,

        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThan = 9,

        /// <summary>
        /// &lt;
        /// </summary>
        LessThan = 10,

        /// <summary>
        /// &gt;=
        /// </summary>
        GreaterOrEqual = 11,

        /// <summary>
        /// &lt;=
        /// </summary>
        LessOrEqual = 12,

        /// <summary>
        /// &&
        /// </summary>
        And = 13,

        /// <summary>
        /// ||
        /// </summary>
        Or = 14,

        /// <summary>
        /// ! (unário)
        /// </summary>
        Not = 15,

        /// <summary>
        /// - (unário)
        /// </summary>
        Minus = 16
    }
}
