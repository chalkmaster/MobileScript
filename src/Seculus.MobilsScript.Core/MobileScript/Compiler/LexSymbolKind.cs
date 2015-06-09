namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Define os tipos de um símbolo léxico (usado no analisador léxico).
    /// </summary>
    public enum LexSymbolKind
    {
        /// <summary>
        /// {
        /// </summary>
        LeftBrace = 0,

        /// <summary>
        /// }
        /// </summary>
        RightBrace = 1,

        /// <summary>
        /// ;
        /// </summary>
        SemiColon = 2,

        /// <summary>
        /// :
        /// </summary>
        Colon = 3,

        /// <summary>
        /// ,
        /// </summary>
        Comma = 5,

        /// <summary>
        /// (
        /// </summary>
        LeftParentheses = 6,

        /// <summary>
        /// )
        /// </summary>
        RightParentheses = 7,

        /// <summary>
        /// !
        /// </summary>
        NotOperator = 8,

        /// <summary>
        /// =
        /// </summary>
        AssignOperator = 9,

        /// <summary>
        /// &lt;
        /// </summary>
        LessThanOperator = 10,

        /// <summary>
        /// &gt;
        /// </summary>
        GreaterThanOperator = 11,

        /// <summary>
        /// +
        /// </summary>
        AddOperator = 12,

        /// <summary>
        /// -
        /// </summary>
        SubtractOperator = 13,

        /// <summary>
        /// *
        /// </summary>
        MultiplyOperator = 14,

        /// <summary>
        /// /
        /// </summary>
        DivideOperator = 15,

        /// <summary>
        /// %
        /// </summary>
        ModuleOperator = 16,

        /// <summary>
        /// [
        /// </summary>
        LeftSquareBracket = 17,

        /// <summary>
        /// ]
        /// </summary>
        RightSquareBracket = 18,




        /// <summary>
        /// ==
        /// </summary>
        EqualOperator = 20,

        /// <summary>
        /// !=
        /// </summary>
        NotEqualOperator = 21,

        /// <summary>
        /// Less or equal (&lt;=)
        /// </summary>
        LessOrEqualOperator = 22,

        /// <summary>
        /// Greater or equal (&gt;=)
        /// </summary>
        GreaterOrEqualOperator = 23,

        /// <summary>
        /// ||
        /// </summary>
        OrOperator = 24,

        /// <summary>
        /// &&
        /// </summary>
        AndOperator = 25,



        /// <summary>
        /// true
        /// </summary>
        TrueReservedWord = 26,

        /// <summary>
        /// false
        /// </summary>
        FalseReservedWord = 27,

        /// <summary>
        /// int
        /// </summary>
        IntReservedWord = 19,

        /// <summary>
        /// boolean
        /// </summary>
        BooleanReservedWord = 28,

        /// <summary>
        /// void
        /// </summary>
        VoidReservedWord = 29,

        /// <summary>
        /// string
        /// </summary>
        StringReservedWord = 30,

        /// <summary>
        /// float
        /// </summary>
        FloatReservedWord = 35,

        /// <summary>
        /// if
        /// </summary>
        IfReservedWord = 31,

        /// <summary>
        /// else
        /// </summary>
        ElseReservedWord = 32,

        /// <summary>
        /// while
        /// </summary>
        WhileReservedWord = 33,

        /// <summary>
        /// ref
        /// </summary>
        RefReservedWord = 34,

        /// <summary>
        /// return
        /// </summary>
        ReturnReservedWord = 40,





        /// <summary>
        /// Int literal
        /// </summary>
        IntConstant = 41,

        /// <summary>
        /// Float literal
        /// </summary>
        FloatConstant = 42,

        /// <summary>
        /// String literal
        /// </summary>
        StringConstant = 43,




        /// <summary>
        /// Representa um identificador
        /// </summary>
        Identifier = 44,

        /// <summary>
        /// Final do arquivo
        /// </summary>
        EndOfFile = 45,

        /// <summary>
        /// Erro. Utilizado quando um símbolo não esperado é encontrado.
        /// </summary>
        Error = 46,

        /// <summary>
        /// Diretiva de preprocessamento
        /// </summary>
        PreprocessorDirective = 47
    }
}
