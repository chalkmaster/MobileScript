using System;
using System.Globalization;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Representa um símbolo léxico da linguagem RPL.
    /// </summary>
    public class LexSymbol
    {
        #region Properties

        /// <summary>
        /// Texto que compõe o símbolo.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Tipo do símbolo.
        /// </summary>
        public LexSymbolKind Kind { get; private set; }

        /// <summary>
        /// Número da linha.
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Número da coluna.
        /// </summary>
        public int Column { get; private set; }

        public string FileName { get; private set; }

        #endregion

        #region Constructors

        public LexSymbol(string text, LexSymbolKind kind, string fileName, int line, int column)
        {
            Text = text;
            Kind = kind;
            Line = line;
            Column = column;
            FileName = fileName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retorna o valor (Text) da constante como inteiro.
        /// </summary>
        /// <returns>Valor da constante como inteiro.</returns>
        public int GetIntValue()
        {
            if (Kind != LexSymbolKind.IntConstant)
            {
                throw new ApplicationException("Symbol is not an integer constant");
            }

            try
            {
                return Int32.Parse(Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Symbol is not an integer constant", ex);
            }
        }

        /// <summary>
        /// Verifica se esse símbolo é um tipo primitivo.
        /// </summary>
        /// <returns>True se for um tipo primitivo. Caso contrário, false.</returns>
        public bool IsPrimitiveType()
        {
            return
                (
                    (Kind == LexSymbolKind.IntReservedWord)     || (Kind == LexSymbolKind.FloatReservedWord)  ||
                    (Kind == LexSymbolKind.BooleanReservedWord) || (Kind == LexSymbolKind.StringReservedWord) ||
                    (Kind == LexSymbolKind.VoidReservedWord)
                );
        }

        /// <summary>
        /// Verifica se esse símbolo é um operador relacional.
        /// </summary>
        /// <returns>True se for um operador relacional. Caso contrário, false.</returns>
        public bool IsRelationalOperator()
        {
            return
                (
                    (Kind == LexSymbolKind.EqualOperator)       || (Kind == LexSymbolKind.NotEqualOperator)       ||
                    (Kind == LexSymbolKind.LessThanOperator)    || (Kind == LexSymbolKind.LessOrEqualOperator)    ||
                    (Kind == LexSymbolKind.GreaterThanOperator) || (Kind == LexSymbolKind.GreaterOrEqualOperator)
                );
        }

        /// <summary>
        /// Verifica se esse símbolo inicia um comando.
        /// </summary>
        /// <returns>True se for um comando. Caso contrário, false.</returns>
        public bool IsStatement()
        {
            return
                (
                    (Kind == LexSymbolKind.Identifier)          || (Kind == LexSymbolKind.IfReservedWord)   ||
                    (Kind == LexSymbolKind.WhileReservedWord)   || (Kind == LexSymbolKind.LeftBrace)        ||
                    (Kind == LexSymbolKind.ReturnReservedWord)  || (Kind == LexSymbolKind.SemiColon)
                );
        }

        #endregion
    }
}
