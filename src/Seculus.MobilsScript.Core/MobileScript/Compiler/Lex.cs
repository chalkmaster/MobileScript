using System;
using System.Text;
using Seculus.MobileScript.Core.Extensions;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Analisador léxico (também chamado de scanner).
    /// </summary>
    public class Lex
    {
        #region Singleton

        private static readonly object SingletonSync = new object();
        private static volatile Lex _instance = null;

        public static Lex Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SingletonSync)
                    {
                        if (_instance == null)
                        {
                            _instance = new Lex();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constructors

        // Construtor privado porque essa classe é singleton
        private Lex() { }

        #endregion

        #region Constants

        /// <summary>
        /// Tamanho máximo de um identificador
        /// </summary>
        private const int MaxIdentifierSize = 30;

        #endregion

        #region Fields

        /// <summary>
        /// Último caracter tratado.
        /// </summary>
        private char _lastChar = ' ';

        /// <summary>
        /// Nome do arquivo do símbolo atual.
        /// </summary>
        private string _fileName = "";

        /// <summary>
        /// Número da linha do símbolo atual.
        /// </summary>
        private int _line = 0;

        /// <summary>
        /// Coluna do símbolo atual.
        /// </summary>
        private int _column = 0;

        private MobileScriptReader _file;

        #endregion

        #region Public Methods

        /// <summary>
        /// Inicia o analizador léxico
        /// </summary>
        /// <param name="file">Arquivo do código fonte.</param>
        public void Init(MobileScriptReader file)
        {
            _file = file;
            _lastChar = ' ';
            _fileName = "";
            _line = 0;
            _column = 0;
        }

        /// <summary>
        /// Lê o próximo símbolo léxico.
        /// </summary>
        /// <returns>O próximo símbolo.</returns>
        public LexSymbol GetNextSymbol()
        {
            Skip();
            _line = _file.GetLineNumber();
            _column = _file.GetColumnNumber();
            _fileName = _file.GetFileName();

            switch (_lastChar)
            {
                // Single char operators
                case '{':
                case '}':
                case ';':
                case ':':
                case '.':
                case ',':
                case '(':
                case ')':
                case '[':
                case ']':
                case '+':
                case '-':
                case '*':
                case '/':
                case '?':
                case '%':
                    return GetSingleCharOperator(_lastChar);

                // Single or double char operators
                case '<':
                    return GetTwoCharsOrOneCharOperator('<', '=', LexSymbolKind.LessOrEqualOperator, LexSymbolKind.LessThanOperator);
                case '>':
                    return GetTwoCharsOrOneCharOperator('>', '=', LexSymbolKind.GreaterOrEqualOperator, LexSymbolKind.GreaterThanOperator);
                case '=':
                    return GetTwoCharsOrOneCharOperator('=', '=', LexSymbolKind.EqualOperator, LexSymbolKind.AssignOperator);
                case '!':
                    return GetTwoCharsOrOneCharOperator('!', '=', LexSymbolKind.NotEqualOperator, LexSymbolKind.NotOperator);
                case '|':
                    return GetTwoCharsOrOneCharOperator('|', '|', LexSymbolKind.OrOperator, LexSymbolKind.Error);
                case '&':
                    return GetTwoCharsOrOneCharOperator('&', '&', LexSymbolKind.AndOperator, LexSymbolKind.Error);

                // String constant
                case '"':
                    return GetStringConstant();

                // End of File
                case '\u0003':
                    return new LexSymbol("_EOF_", LexSymbolKind.EndOfFile, _fileName, _line, _column);

                // Identificador ou Número
                default:
                    if (Char.IsLetter(_lastChar) || _lastChar == '_')
                    {
                        return GetIdentifierOrReservedWord();
                    }
                    if (Char.IsDigit(_lastChar))
                    {
                        return GetNumber();
                    }
                    Console.Error.WriteLine("Lex - Invalid char: '{0}' code:{1}".FormatWith(_lastChar, (int)_lastChar));
                    var errorSymbol = new LexSymbol("{0}".FormatWith(_lastChar), LexSymbolKind.Error, _fileName, _line, _column);
                    ReadChar();
                    return errorSymbol;
            }
        }

        #endregion

        #region Private Methods

        private void ReadChar()
        {
            _lastChar = _file.ReadChar();
        }

        private void SkipLine()
        {
            _file.ReadLine();
        }

        

        /// <summary>
        /// Ignora espaços e comentários
        /// </summary>
        private void Skip()
        {
            do
            {
                while (Char.IsWhiteSpace(_lastChar)) ReadChar();
                if (IsComment()) // comentário (termina no fim da linha)
                {
                    SkipLine();
                    ReadChar();
                }
            } while (Char.IsWhiteSpace(_lastChar) || IsComment());
        }

        private bool IsComment()
        {
            return _lastChar == '/' && _file.LookAtLinesNextChar() == '/';
        }

        /// <summary>
        /// Lê o próximo símbolo léxico.
        /// </summary>
        /// <param name="chr">Char</param>
        /// <returns>Retorna o símbolo.</returns>
        private LexSymbol GetSingleCharOperator(char chr)
        {
            LexSymbolKind kind;
            switch (chr)
            {
                case '{': kind = LexSymbolKind.LeftBrace; break;
                case '}': kind = LexSymbolKind.RightBrace; break;
                case ';': kind = LexSymbolKind.SemiColon; break;
                case ',': kind = LexSymbolKind.Comma; break;
                case '(': kind = LexSymbolKind.LeftParentheses; break;
                case ')': kind = LexSymbolKind.RightParentheses; break;
                case '<': kind = LexSymbolKind.LessThanOperator; break;
                case '>': kind = LexSymbolKind.GreaterThanOperator; break;
                case '=': kind = LexSymbolKind.AssignOperator; break;
                case '+': kind = LexSymbolKind.AddOperator; break;
                case '-': kind = LexSymbolKind.SubtractOperator; break;
                case '*': kind = LexSymbolKind.MultiplyOperator; break;
                case '/': kind = LexSymbolKind.DivideOperator; break;
                case '%': kind = LexSymbolKind.ModuleOperator; break;
                case '[': kind = LexSymbolKind.LeftSquareBracket; break;
                case ']': kind = LexSymbolKind.RightSquareBracket; break;
                default: kind = LexSymbolKind.Error; break;
            }
            ReadChar();
            return new LexSymbol("" + chr, kind, _fileName, _line, _column);
        }

        /// <summary>
        /// Tenta retornar o operador de duas posições. Se for inválido, retorna o de uma posição.
        /// </summary>
        /// <param name="chr1">Primeiro char</param>
        /// <param name="chr2">Segundo char</param>
        /// <param name="kindIfTwoCharsOperator">Tipo se o símbolo for de dois caracteres</param>
        /// <param name="kindIfOneCharOperator">Tipo se o símbolo for de um caracter</param>
        /// <returns>Símbolo</returns>
        private LexSymbol GetTwoCharsOrOneCharOperator(char chr1, char chr2, LexSymbolKind kindIfTwoCharsOperator, LexSymbolKind kindIfOneCharOperator)
        {
            /*
             * The first was already read, it's ch1. We check now if the next char
             * from the input is ch2. If it is, then it's a double-char operator!
             */

            ReadChar();
            if (_lastChar == chr2)
            {
                ReadChar();
                return new LexSymbol("" + chr1 + chr2, kindIfTwoCharsOperator, _fileName, _line, _column);
            }
            else
            {
                return new LexSymbol("" + chr1, kindIfOneCharOperator, _fileName, _line, _column);
            }
        }

        /// <summary>
        /// Lê uma constante do tipo string.
        /// </summary>
        /// <returns>Símbolo</returns>
        private LexSymbol GetStringConstant()
        {
            var stringBuilder = new StringBuilder();
            ReadChar();
            while (_lastChar != '"')
            {
                if (_lastChar == '\n' || _lastChar == '\u0003')
                {
                    return new LexSymbol(stringBuilder.ToString(), LexSymbolKind.Error, _fileName, _line, _column);
                }
                stringBuilder.Append(_lastChar);
                ReadChar();
            }
            ReadChar();
            return new LexSymbol(stringBuilder.ToString(), LexSymbolKind.StringConstant, _fileName, _line, _column);
        }

        /// <summary>
        /// Lê uma constante que representa um número (inteiro ou float).
        /// </summary>
        /// <returns>Símbolo</returns>
        private LexSymbol GetNumber()
        {
            var stringBuilder = new StringBuilder();
            do
            {
                stringBuilder.Append(_lastChar);
                ReadChar();
            } while (Char.IsDigit(_lastChar));
            if (_lastChar == '.')
            {
                stringBuilder.Append(_lastChar);
                ReadChar();
                while (Char.IsDigit(_lastChar))
                {
                    stringBuilder.Append(_lastChar);
                    ReadChar();
                }
                return new LexSymbol(stringBuilder.ToString(), LexSymbolKind.FloatConstant, _fileName, _line, _column);
            }
            return new LexSymbol(stringBuilder.ToString(), LexSymbolKind.IntConstant, _fileName, _line, _column);
        }

        /// <summary>
        /// Lê um identificador ou uma palavra reservada.
        /// </summary>
        /// <returns>Símbolo</returns>
        private LexSymbol GetIdentifierOrReservedWord()
        {
            var stringBuilder = new StringBuilder();
            do
            {
                if (stringBuilder.Length < MaxIdentifierSize)
                {
                    stringBuilder.Append(_lastChar);
                }
                ReadChar();
            } while (IsIdentifierPart(_lastChar));

            string identifierOrReservedWord = stringBuilder.ToString();
            return new LexSymbol(identifierOrReservedWord, ReservedWordsTable.Instance.GetSymbolKind(identifierOrReservedWord), _fileName, _line, _column);
        }

        /// <summary>
        /// Faz parse dos preprocessors.
        /// </summary>
        /// <returns>True ou false</returns>
        private bool ParsePreProcessor()
        {
            return true;
        }

        #endregion

        #region Private Static Methods

        private static bool IsIdentifierStart(char chr)
        {
            return
                Char.IsLetter(chr) ||
                chr == '$' ||
                chr == '_';
        }

        private static bool IsIdentifierPart(char chr)
        {
            return
                IsIdentifierStart(chr) ||
                Char.IsDigit(chr);
        }

        #endregion
    }
}
