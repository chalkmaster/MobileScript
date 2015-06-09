using System;
using System.IO;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    public sealed class SourceCodeReader : MarshalByRefObject, IDisposable
    {
        #region Fields

        private readonly Stream _stream;
        private TextReader _textReader;
        private int _lines = -1;

        #endregion

        #region Constructors

        public SourceCodeReader(Stream stream)
        {
            _stream = stream;
            _stream.Position = 0;
            _textReader = CreateTextReader();
        }

        #endregion

        #region Public Methods

        public string ReadLine()
        {
            return _textReader.ReadLine();
        }

        public void Reset()
        {
            Reset(0);
        }

        public int GetNumberOfLines()
        {
            if (_lines == -1)
            {
                long initialPosition = _stream.Position;
                int lines = 0;
                Reset();
                while (_textReader.ReadLine() != null)
                {
                    lines++;
                }
                _lines = lines;
                Reset(initialPosition);
            }
            return _lines;
        }

        #endregion

        #region Private Methods

        private TextReader CreateTextReader()
        {
            return TextReader.Synchronized(new StreamReader(_stream));
        }

        public void Reset(long initialPosition)
        {
            if (!_stream.CanSeek)
            {
                throw new IOException("Underlying stream does not support seeking.");
            }

            _stream.Position = initialPosition;
            _textReader = CreateTextReader();
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
                if (_textReader != null) _textReader.Dispose();
                if (_stream != null) _stream.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
