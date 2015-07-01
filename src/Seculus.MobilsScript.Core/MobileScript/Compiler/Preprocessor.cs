using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AppPlat.Framework.Infrastructure.InversionOfControl;
using AppPlat.Framework.Services.IO;
using AppPlat.Modules.WorkflowEditor.Core.Services;
using Seculus.MobileScript.Core.Extensions;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Preprocessador
    /// </summary>
    public class Preprocessor
    {
        #region Static Fields

        private static readonly Dictionary<string, PreprocessorDirective> Directives = new Dictionary<string, PreprocessorDirective>();

        #endregion

        #region Static Constructors

        static Preprocessor()
        {
            Directives.Add("#include", PreprocessorDirective.Include);
            Directives.Add("#define", PreprocessorDirective.Define);
            Directives.Add("#undef", PreprocessorDirective.Undefine);
            Directives.Add("#ifdef", PreprocessorDirective.Ifdef);
            Directives.Add("#ifndef", PreprocessorDirective.Ifndef);
            Directives.Add("#else", PreprocessorDirective.Else);
            Directives.Add("#endif", PreprocessorDirective.Endif);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Último caracter tratado.
        /// </summary>
        private char _lastChar = ' ';

        /// <summary>
        /// Arquivo de entrada
        /// </summary>
        private readonly MobileScriptReader _file;

        /// <summary>
        /// Stack of open (not yet finished with #endif) ifs in pre-processor.
        /// </summary>
        private readonly Stack<PreprocessorIf> _ifStack = new Stack<PreprocessorIf>(); 

        #endregion

        #region Constructors

        public Preprocessor(MobileScriptReader file)
        {
            _file = file;
            DefinedWords = new HashSet<string>();
            Errors = new List<CompilationError>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Errors found by the preprocessor.
        /// </summary>
        public IList<CompilationError> Errors { get; private set; }

        /// <summary>
        /// Words defines (by the #define directive).
        /// </summary>
        internal HashSet<string> DefinedWords { get; private set; } 

        #endregion

        #region Public Methods

        public void DoPreprocess()
        {
            while (!_file.IsEndOfFile())
            {
                SkipToNextDirective();
                char chr = _lastChar;

                switch (chr)
                {
                    case '#':
                        ProcessDirective();
                        break;
                }
            }
        }

        #endregion

        #region Private Methods

        private void ProcessDirective()
        {
            string directiveName = GetDirectiveName();
            var directiveSymbol = new LexSymbol(directiveName, LexSymbolKind.PreprocessorDirective, _file.GetFileName(),
                                                _file.GetLineNumber(), _file.GetColumnNumber());

            if (Directives.ContainsKey(directiveName))
            {
                PreprocessorDirective directive = Directives[directiveName];

                switch (directive)
                {
                    case PreprocessorDirective.Include:
                        ProcessInclude(directiveSymbol);
                        break;

                    case PreprocessorDirective.Define:
                        ProcessDefine(directiveSymbol);
                        break;

                    case PreprocessorDirective.Undefine:
                        ProcessUndefine(directiveSymbol);
                        break;

                    case PreprocessorDirective.Ifdef:
                        ProcessIfdef(directiveSymbol);
                        break;

                    case PreprocessorDirective.Ifndef:
                        ProcessIfndef(directiveSymbol);
                        break;

                    case PreprocessorDirective.Else:
                        ProcessElse(directiveSymbol);
                        break;

                    case PreprocessorDirective.Endif:
                        ProcessEndif(directiveSymbol);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Errors.Add(new CompilationError("Invalid preprocessor directive.", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessInclude(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            if (_lastChar == '"')
            {
                string url = GetString();
                ProcessIncludeWithUrl(directiveSymbol, url);
            }
            else if (_lastChar == '<')
            {
                string tagContent = GetTag();
                ProcessIncludeWithLibraryName(directiveSymbol, tagContent);
            }
            else
            {
                Errors.Add(new CompilationError("Invalid file to include.", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessIncludeWithUrl(LexSymbol directiveSymbol, string url)
        {
            if (url == null)
            {
                Errors.Add(new CompilationError("Invalid file url to include.", CompilationErrorType.PreprocessorError, directiveSymbol));
                return;
            }

            string fileContent = DownloadFileContent(url, directiveSymbol);
            if (fileContent == null)
            {
                Errors.Add(new CompilationError("Invalid file  to include.", CompilationErrorType.PreprocessorError, directiveSymbol));
                return;
            }

            _file.AddDeviationToANewFile(_file.GetLineNumber(), url, fileContent, 0);
        }

        private void ProcessIncludeWithLibraryName(LexSymbol directiveSymbol, string libraryName)
        {
            if (libraryName == null)
            {
                Errors.Add(new CompilationError("Invalid file name to include.", CompilationErrorType.PreprocessorError, directiveSymbol));
                return;
            }

            string fileContent = GetLibraryContent(libraryName, directiveSymbol);
            if (fileContent == null)
            {
                Errors.Add(new CompilationError("Invalid file to include.", CompilationErrorType.PreprocessorError, directiveSymbol));
                return;
            }

            _file.AddDeviationToANewFile(_file.GetLineNumber(), libraryName, fileContent, 0);
        }

        private void ProcessDefine(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            string wordToDefine = GetIdentifier().Trim(); // Nome que está sendo definido.
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do que está sendo definido.

            if (!String.IsNullOrWhiteSpace(wordToDefine))
            {
                if (!DefinedWords.Contains(wordToDefine))
                {
                    DefinedWords.Add(wordToDefine);
                    _file.AddDeviation(_file.GetLineNumber(), _file.GetLineNumber() + 1); // Jump directive line
                }
                else
                {
                    Errors.Add(new CompilationError("Word '{0}' is already defined.".FormatWith(wordToDefine), CompilationErrorType.PreprocessorError, directiveSymbol));
                }
            }
            else
            {
                Errors.Add(new CompilationError("Invalid word to define.", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessUndefine(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            string wordToUndefine = GetIdentifier().Trim(); // Nome que está sendo undefined
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do que está sendo undefined.

            if (!String.IsNullOrWhiteSpace(wordToUndefine))
            {
                if (DefinedWords.Contains(wordToUndefine))
                {
                    DefinedWords.Remove(wordToUndefine);
                    _file.AddDeviation(_file.GetLineNumber(), _file.GetLineNumber() + 1); // Jump directive line
                }
                else
                {
                    Errors.Add(new CompilationError("Word '{0}' is not defined.".FormatWith(wordToUndefine), CompilationErrorType.PreprocessorError, directiveSymbol));
                }
            }
            else
            {
                Errors.Add(new CompilationError("Invalid word to undefine.", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessIfdef(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            string word = GetIdentifier().Trim(); // Nome para se verificar se está definido.
            int startLineNumber = _file.GetLineNumber();
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do nome possivelmente definido.

            if (!String.IsNullOrEmpty(word))
            {
                bool isDefined = DefinedWords.Contains(word);
                StartIf(directiveSymbol, !isDefined); // Abre um if.
                if (!isDefined)
                {
                    if (SkipToElseOrEndIfDirective())
                    {
                        SkipLine();
                        _file.AddDeviation(startLineNumber, _file.GetLineNumber());
                    }
                    else
                    {
                        Errors.Add(new CompilationError("No #endif directive matching the #ifdef directive", CompilationErrorType.PreprocessorError, directiveSymbol));
                    }
                }
                else
                {
                    // se a palavra estiver realmente definida, pulamos apenas essa linha da diretiva #ifdef.
                    _file.AddDeviation(startLineNumber, startLineNumber + 1);
                }
            }
            else
            {
                Errors.Add(new CompilationError("Invalid word to check for definition", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessIfndef(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            string word = GetIdentifier().Trim(); // Nome para se verificar se está definido ou não.
            int startLineNumber = _file.GetLineNumber();
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do nome possivelmente definido.

            if (!String.IsNullOrEmpty(word))
            {
                bool isDefined = DefinedWords.Contains(word);
                var ifPart = StartIf(directiveSymbol, isDefined); // Abre um if.
                if (ifPart.Jump)
                {
                    if (SkipToElseOrEndIfDirective())
                    {
                        SkipLine();
                        _file.AddDeviation(startLineNumber, _file.GetLineNumber());
                    }
                    else
                    {
                        Errors.Add(new CompilationError("No #endif directive matching the #ifdef directive", CompilationErrorType.PreprocessorError, directiveSymbol));
                    }
                }
                else
                {
                    // se a palavra realmente não estiver definida, pulamos apenas essa linha da diretiva #ifndef.
                    _file.AddDeviation(startLineNumber, startLineNumber + 1);
                }
            }
            else
            {
                Errors.Add(new CompilationError("Invalid word to check for definition", CompilationErrorType.PreprocessorError, directiveSymbol));
            }
        }

        private void ProcessElse(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            int line = _file.GetLineNumber();
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do que está sendo definido.

            PreprocessorIf elsePart = StartElse(directiveSymbol);
            if (elsePart != null)
            {
                if (elsePart.Jump)
                {
                    if (SkipToEndIfDirective())
                    {
                        SkipLine();
                        _file.AddDeviation(line, _file.GetLineNumber());
                    }
                    else
                    {
                        Errors.Add(new CompilationError("No #endif directive matching the #else directive", CompilationErrorType.PreprocessorError, directiveSymbol));
                    }
                }
                else
                {
                    _file.AddDeviation(line, line + 1);
                }
                
            }
        }

        private void ProcessEndif(LexSymbol directiveSymbol)
        {
            SkipLineWhiteSpaces(); // Pula espaços em branco na linha atual.
            int line = _file.GetLineNumber();
            if (!AssertIsEndOfLine()) return; // Asseguramos que não há mais nada depois do #endif

            PreprocessorIf endIf = EndIf(directiveSymbol);
            if (endIf != null)
            {
                _file.AddDeviation(line, line + 1);
            }
        }

        private bool SkipToElseOrEndIfDirective()
        {
            int ifsOpenedAfterThis = 0;

            while (!_file.IsEndOfFile())
            {
                SkipToNextDirective();

                string directiveName = GetDirectiveName();
                PreprocessorDirective directive = Directives[directiveName];
                var directiveSymbol = new LexSymbol(directiveName, LexSymbolKind.PreprocessorDirective, _file.GetFileName(), _file.GetLineNumber(), _file.GetColumnNumber());

                if (directive == PreprocessorDirective.Ifdef || directive == PreprocessorDirective.Ifndef)
                {
                    ifsOpenedAfterThis++;
                }
                else if (directive == PreprocessorDirective.Else)
                {
                    if (ifsOpenedAfterThis == 0)
                    {
                        StartElse(directiveSymbol);
                        return true;
                    }
                }
                else if (directive == PreprocessorDirective.Endif)
                {
                    if (ifsOpenedAfterThis == 0)
                    {
                        EndIf(directiveSymbol);
                        return true;
                    }
                    ifsOpenedAfterThis--;
                }
            }

            return false;
        }

        private bool SkipToEndIfDirective()
        {
            int ifsOpenedAfterThis = 0;

            while (!_file.IsEndOfFile())
            {
                SkipToNextDirective();

                string directiveName = GetDirectiveName();
                PreprocessorDirective directive = Directives[directiveName];
                var directiveSymbol = new LexSymbol(directiveName, LexSymbolKind.PreprocessorDirective, _file.GetFileName(), _file.GetLineNumber(), _file.GetColumnNumber());

                if (directive == PreprocessorDirective.Ifdef || directive == PreprocessorDirective.Ifndef)
                {
                    ifsOpenedAfterThis++;
                }
                else if (directive == PreprocessorDirective.Endif)
                {
                    if (ifsOpenedAfterThis == 0)
                    {
                        EndIf(directiveSymbol);
                        return true;
                    }
                    ifsOpenedAfterThis--;
                }
            }

            return false;
        }

        private string GetDirectiveName()
        {
            var stringBuilder = new StringBuilder();
            do
            {
                stringBuilder.Append(_lastChar);
                ReadChar();
            } while (IsDirectivePart(_lastChar));

            return stringBuilder.ToString();
        }

        private string GetIdentifier()
        {
            var stringBuilder = new StringBuilder();

            while (IsIdentifierPart(_lastChar) && !_file.IsLineEnd() && !_file.IsEndOfFile())
            {
                stringBuilder.Append(_lastChar);
                ReadChar();
            } 

            return stringBuilder.ToString();
        }

        private string GetString()
        {
            var stringBuilder = new StringBuilder();
            ReadChar();
            while (_lastChar != '"')
            {
                if (_lastChar == '\n' || _lastChar == '\u0003')
                {
                    return null;
                }
                stringBuilder.Append(_lastChar);
                ReadChar();
            }
            ReadChar();
            return stringBuilder.ToString();
        }

        private string GetTag()
        {
            var stringBuilder = new StringBuilder();
            ReadChar();
            while (_lastChar != '>')
            {
                if (_lastChar == '\n' || _lastChar == '\u0003')
                {
                    return null;
                }
                stringBuilder.Append(_lastChar);
                ReadChar();
            }
            ReadChar();
            return stringBuilder.ToString();
        }

        private string GetLibraryContent(string includedValue, LexSymbol directiveSymbol)
        {
            try
            {
                var libraryService = IoC.Resolve<MobileScriptLibraryService>();
                var library = libraryService.GetByIncludeName(includedValue);
                if (library != null)
                {
                    // we need to add a couple of lines at the end of the file to allow proper deviation.
                    StringBuilder fileContentBuilder = new StringBuilder();
                    fileContentBuilder.Append(library.Source);
                    fileContentBuilder.AppendLine();
                    fileContentBuilder.AppendLine();
                    return fileContentBuilder.ToString();
                }
                else
                {
                    Errors.Add(new CompilationError("Library '{0}' not found.".FormatWith(includedValue), CompilationErrorType.PreprocessorError, directiveSymbol));
                    return null;
                }
            }
            catch (Exception)
            {
                Errors.Add(new CompilationError("Error while trying to get library: {0}".FormatWith(includedValue), CompilationErrorType.PreprocessorError, directiveSymbol));
                return null;
            }
        }

        private string DownloadFileContent(string url, LexSymbol directiveSymbol)
        {
            try
            {
                // we need to add a couple of lines at the end of the file to allow proper deviation.
                StringBuilder fileContentBuilder = new StringBuilder(FileDownloader.GetContentAsString(url));
                fileContentBuilder.AppendLine();
                fileContentBuilder.AppendLine();
                return fileContentBuilder.ToString();
            }
            catch (Exception)
            {
                Errors.Add(new CompilationError("Error while trying to download file at url: {0}".FormatWith(url), CompilationErrorType.PreprocessorError, directiveSymbol));
                return null;
            }
        }

        private void ReadChar()
        {
            _lastChar = _file.ReadChar();
        }

        private bool IsDirective()
        {
            return _lastChar == '#';
        }

        private void SkipLine()
        {
            _file.ReadLine();
        }

        private void SkipToNextDirective()
        {
            do
            {
                // pula espaços em branco.
                while (Char.IsWhiteSpace(_lastChar)) ReadChar();

                // se não for diretiva, pula essa linha.
                if (!IsDirective()) 
                {
                    SkipLine();
                    ReadChar();
                }

            } while (!IsDirective() && !_file.IsEndOfFile());
        }

        private void SkipLineWhiteSpaces()
        {
            while (Char.IsWhiteSpace(_lastChar) && !_file.IsLineEnd()) ReadChar();
        }

        private bool AssertIsEndOfLine()
        {
            SkipLineWhiteSpaces();

            if (!_file.IsLineEnd() && !_file.IsEndOfFile())
            {
                var symbol = new LexSymbol(_lastChar.ToString(CultureInfo.InvariantCulture), LexSymbolKind.Error,
                                           _file.GetFileName(), _file.GetLineNumber(), _file.GetColumnNumber());
                Errors.Add(new CompilationError("Unexpected symbol found.", CompilationErrorType.PreprocessorError, symbol));
                return false;
            }

            return true;
        }

        private PreprocessorIf StartIf(LexSymbol symbol, bool jump)
        {
            lock (_ifStack)
            {
                PreprocessorIf ifPart = new PreprocessorIf(IfStatus.If, jump);
                _ifStack.Push(ifPart);
                return ifPart;
            }
        }

        private PreprocessorIf StartElse(LexSymbol symbol)
        {
            lock (_ifStack)
            {
                if (_ifStack.Count > 0 && _ifStack.Peek().Status == IfStatus.If)
                {
                    var ifPart = _ifStack.Pop();
                    var elsePart = new PreprocessorIf(IfStatus.Else, !ifPart.Jump);
                    _ifStack.Push(elsePart);
                    return elsePart;
                }
            }

            Errors.Add(new CompilationError("#else directive has no matching #if directive.", CompilationErrorType.PreprocessorError, symbol));
            return null;
        }

        private PreprocessorIf EndIf(LexSymbol symbol)
        {
            lock (_ifStack)
            {
                if (_ifStack.Count > 0)
                {
                    return _ifStack.Pop();
                }
            }

            Errors.Add(new CompilationError("#endif directive has no matching #if directive.", CompilationErrorType.PreprocessorError, symbol));
            return null;
        }

        #endregion

        #region Private Static Methods

        private static bool IsDirectiveStart(char chr)
        {
            return chr == '#';
        }

        private static bool IsDirectivePart(char chr)
        {
            return
                Char.IsLetter(chr) ||
                Char.IsDigit(chr);
        }

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

        #region Inner Types

        public enum PreprocessorDirective
        {
            /// <summary>
            /// Diretiva inválida
            /// </summary>
            Invalid = 0,

            /// <summary>
            /// #include
            /// </summary>
            Include = 1,

            /// <summary>
            /// #define
            /// </summary>
            Define = 2,
            /// <summary>
            /// #undef
            /// </summary>
            Undefine = 3,

            /// <summary>
            /// #ifdef
            /// </summary>
            Ifdef = 4,
            /// <summary>
            /// #ifndef
            /// </summary>
            Ifndef = 5,
            /// <summary>
            /// #else
            /// </summary>
            Else = 6,
            /// <summary>
            /// #endif
            /// </summary>
            Endif = 7
        }

        private enum IfStatus
        {
            If,
            Else
        }
        private class PreprocessorIf
        {
            public PreprocessorIf(IfStatus status, bool jump)
            {
                Status = status;
                Jump = jump;
            }
            public IfStatus Status { get; set; }
            public bool Jump { get; set; }
        }

        #endregion
    }
}
