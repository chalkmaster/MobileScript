using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Seculus.MobileScript.Core.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// "Leitor" de arquivos do serviço
    /// </summary>
    public class MobileScriptReader : IDisposable
    {
        #region Fields

        private readonly Dictionary<MobileScriptFileReader, Dictionary<int, DeviationDestiny>> _deviations = new Dictionary<MobileScriptFileReader, Dictionary<int, DeviationDestiny>>();
        private readonly HashSet<MobileScriptFileReader> _files = new HashSet<MobileScriptFileReader>();
        private readonly MobileScriptFileReader _initialFile;
        private MobileScriptFileReader _currentFile;

        #endregion

        #region Constructors

        public MobileScriptReader(FileStream fileStream, string fileName = "<CurrentFile>")
            : this(new MobileScriptFileReader(new SourceCodeReader(fileStream), fileName))
        { }
        public MobileScriptReader(string sourceCode, string fileName = "<CurrentFile>")
            : this(new MobileScriptFileReader(new SourceCodeReader(new MemoryStream(Encoding.UTF8.GetBytes(sourceCode))), fileName))
        { }
        private MobileScriptReader(MobileScriptFileReader initialFile)
        {
            _files.Add(initialFile);
            _initialFile = initialFile;
            _currentFile = _initialFile;
            ReadLine();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adiciona um desvio para outra linha do mesmo arquivo.
        /// </summary>
        /// <param name="fromLine">Linha de origem</param>
        /// <param name="toLine">Linha de destino</param>
        public void AddDeviation(int fromLine, int toLine)
        {
            if (fromLine > toLine)
            {
                throw new ApplicationException("Cannot go back on the same file.");
            }
            AddDeviation(_currentFile, fromLine, _currentFile, toLine);
        }

        public void AddDeviationToANewFile(int fromLine, string toFileName, string toFileContent, int toFileLine)
        {
            MobileScriptFileReader newFile = AddFile(toFileName, toFileContent);
            MobileScriptFileReader currentFile = _currentFile;
            AddDeviation(currentFile, fromLine, newFile, toFileLine);
            int numOfLines = newFile.GetNumberOfLines();
            AddDeviation(newFile, numOfLines + 1, currentFile, fromLine + 1);
        }

        /// <summary>
        /// Lê o caracter atual e avança o apontador o próximo caracter.
        /// </summary>
        /// <returns>Caracter atual.</returns>
        public char ReadChar()
        {
            char chr = _currentFile.ReadChar();
            int currentLine = _currentFile.LineNumber;

            if (_deviations.ContainsKey(_currentFile) && _deviations[_currentFile].ContainsKey(currentLine))
            {
                ApplyDeviation(_deviations[_currentFile][currentLine]);
                return ReadChar();
            }

            return chr;
        }

        /// <summary>
        /// Lê a linha atual e avança o apontador para o primeiro caracter da próxima linha.
        /// </summary>
        /// <returns>Linha atual completa (inclusive os caracteres já lidos dessa linha, caso haja algum).</returns>
        public string ReadLine()
        {
            string result = _currentFile.ReadLine();

            // We keep jumping while there is a deviation
            while (_deviations.ContainsKey(_currentFile) && _deviations[_currentFile].ContainsKey(_currentFile.LineNumber))
            {
                ApplyDeviation(_deviations[_currentFile][_currentFile.LineNumber]);
            }

            return result;
        }

        /// <summary>
        /// Olha para o próximo caracter da linha atual, sem "andar" com o apontador 
        /// e sem olhar para a próxima linha caso essa linha já tenha acabado.
        /// </summary>
        /// <returns>Próximo char da linha ou '\0' se estivermos no final da linha.</returns>
        public char LookAtLinesNextChar()
        {
            return _currentFile.LookAtLinesNextChar();
        }

        public string GetFileName()
        {
            return _currentFile.FileName;
        }

        public int GetLineNumber()
        {
            return _currentFile.LineNumber;
        }

        public int GetColumnNumber()
        {
            return _currentFile.ColumnNumber;
        }

        /// <summary>
        /// Verifica se é o final da linha.
        /// </summary>
        /// <returns>Verifica se estamos no final da linha.</returns>
        public bool IsLineEnd()
        {
            return _currentFile.IsLineEnd();
        }

        public bool IsEndOfFile()
        {
            return _currentFile.IsEndOfFile();
        }

        public void Reset()
        {
            ChangeFile(_initialFile);
            ChangeLine(0);
            ReadLine();
        }

        public string ReadToEnd()
        {
            var sb = new StringBuilder();
            while (!IsEndOfFile())
            {
                MobileScriptFileReader prevFile = _currentFile;
                int prevLine = _currentFile.LineNumber;
                char chr = ReadChar(); 
                int curLine = _currentFile.LineNumber;

                if (curLine > prevLine || _currentFile != prevFile)
                {
                    sb.AppendLine();
                }
                if (chr != '\u0003') sb.Append(chr);
            }
            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private MobileScriptFileReader AddFile(string fileName, string fileContent)
        {
            var newFile =
                new MobileScriptFileReader(new SourceCodeReader(new MemoryStream(Encoding.UTF8.GetBytes(fileContent))),
                                           fileName);
            _files.Add(newFile);
            return newFile;
        }

        private void AddDeviation(MobileScriptFileReader fromFile, int fromLine, MobileScriptFileReader toFile, int toLine)
        {
            Check.Argument.IsNotNull(fromFile, "fromFile");
            Check.Argument.IsNotNull(toFile, "toFile");

            if (!_deviations.ContainsKey(fromFile))
            {
                _deviations.Add(fromFile, new Dictionary<int, DeviationDestiny>());
            }
            _deviations[fromFile].Add(fromLine, new DeviationDestiny(toFile, toLine));

            // if it's time to deviate, apply it!
            if (fromFile == _currentFile && fromLine == _currentFile.LineNumber)
            {
                ApplyDeviation(_deviations[fromFile][fromLine]);
            }
        }

        private void ApplyDeviation(DeviationDestiny destiny)
        {
            ChangeFile(destiny.File);
            ChangeLine(destiny.Line);
        }

        private void ChangeFile(MobileScriptFileReader newFile)
        {
            _currentFile = newFile;
        }

        private void ChangeLine(int newLine)
        {
            _currentFile.SetPosition(newLine);
        }

        #endregion

        #region Disposable Pattern

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_currentFile != null) _currentFile.Dispose();
            }

            _disposed = true;
        }

        #endregion

        #region Inner Types

        private sealed class MobileScriptFileReader : IDisposable
        {
            #region Fields

            /// <summary>
            /// Input
            /// </summary>
            private readonly SourceCodeReader _input;

            private string _currentLine;

            #endregion

            #region Constructors

            public MobileScriptFileReader(SourceCodeReader input, string fileName)
            {
                _input = input;
                FileName = fileName;
                LineNumber = 0;
                ColumnNumber = 0;
                _currentLine = String.Empty;
            }

            #endregion

            #region Properties

            public string FileName { get; private set; }

            public int LineNumber { get; private set; }

            public int ColumnNumber { get; private set; }

            #endregion

            #region Public Methods

            /// <summary>
            /// Lê o caracter atual e avança o apontador o próximo caracter.
            /// </summary>
            /// <returns>Caracter atual.</returns>
            public char ReadChar()
            {
                if (ColumnNumber >= _currentLine.Length)
                {
                    ReadLine();
                }
                return _currentLine[ColumnNumber++];
            }

            /// <summary>
            /// Lê a linha atual e avança o apontador para o primeiro caracter da próxima linha.
            /// </summary>
            /// <returns>Linha atual completa (inclusive os caracteres já lidos dessa linha, caso haja algum).</returns>
            public string ReadLine()
            {
                string oldLine = _currentLine;

                MoveToNextLine();

                return oldLine;
            }

            /// <summary>
            /// Olha para o próximo caracter da linha atual, sem "andar" com o apontador 
            /// e sem olhar para a próxima linha caso essa linha já tenha acabado.
            /// </summary>
            /// <returns>Próximo char da linha ou '\0' se estivermos no final da linha.</returns>
            public char LookAtLinesNextChar()
            {
                if (ColumnNumber < _currentLine.Length)
                {
                    return _currentLine[ColumnNumber];
                }
                return '\0';
            }

            /// <summary>
            /// Verifica se é o final da linha.
            /// </summary>
            /// <returns>Verifica se estamos no final da linha.</returns>
            public bool IsLineEnd()
            {
                return ColumnNumber >= _currentLine.Length;
            }

            /// <summary>
            /// Seta a linha atual a ser lida no arquivo.
            /// </summary>
            /// <param name="line">Linha atual.</param>
            public void SetPosition(int line)
            {
                // if we are ahead, we need to reset the stream.
                if (line < LineNumber)
                {
                    Reset();
                }

                while (LineNumber < line)
                {
                    ReadLine();
                }
            }

            public bool IsEndOfFile()
            {
                return LookAtLinesNextChar() == '\u0003';
            }

            public int GetNumberOfLines()
            {
                return _input.GetNumberOfLines();
            }

            #endregion

            #region Private Methods

            private void Reset()
            {
                _input.Reset();
                LineNumber = 0;
                ColumnNumber = 0;
                _currentLine = String.Empty;
            }

            private void MoveToNextLine()
            {
                _currentLine = _input.ReadLine();
                if (_currentLine == null)
                {
                    _currentLine = "\u0003\u0003"; // o char 3 significa "end of text".
                }
                else
                {
                    _currentLine += " ";
                }
                ColumnNumber = 0;
                LineNumber++;
            }

            #endregion

            #region Disposable Pattern

            private bool _disposed = false;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            private void Dispose(bool disposing)
            {
                if (_disposed) return;

                if (disposing)
                {
                    if (_input != null) _input.Dispose();
                }

                _disposed = true;
            }

            #endregion
        }

        private class DeviationDestiny
        {
            public DeviationDestiny(MobileScriptFileReader file, int line)
            {
                File = file;
                Line = line;
            }

            public MobileScriptFileReader File { get; set; }
            public int Line { get; set; }
        }

        #endregion
    }
}
