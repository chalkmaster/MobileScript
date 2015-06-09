using System;
using System.Text;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers
{
    public class SourceCodeBuilder
    {
        #region Fields

        /// <summary>
        /// StringBuilder utilizado para construir o código.
        /// </summary>
        private readonly StringBuilder _code = new StringBuilder();

        /// <summary>
        /// Nível do escopo atual (indica o número de tabs que serão usados).
        /// </summary>
        private int _scopeLevel;

        /// <summary>
        /// Inidica se a linha atual está em branco.
        /// </summary>
        private bool _cleanLine = true;

        #endregion

        #region Properties

        public int Length
        {
            get { return _code.Length; }
            set { _code.Length = value; }
        }

        public char this[int index]
        {
            get { return _code[index]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inicia um novo escopo.
        /// </summary>
        public void OpenScope()
        {
            _scopeLevel++;
        }

        /// <summary>
        /// Fecha o escopo atual.
        /// </summary>
        public void CloseScope()
        {
            if (_scopeLevel == 0)
            {
                throw new ApplicationException("Cannot close an unopened scope.");
            }
            _scopeLevel--;
        }

        /// <summary>
        /// Inicia uma nova linha.
        /// </summary>
        public void NewLine()
        {
            _code.AppendLine();
            _cleanLine = true;
        }

        /// <summary>
        /// Insere um código. 
        /// Se o código for inserido em uma nova linha (método NewLine() foi chamado no passo anterior), 
        /// esse comando também insere a tabulação relativa ao nível do escopo atual.
        /// </summary>
        /// <param name="code">Código</param>
        public void Append(string code)
        {
            if (_cleanLine)
            {
                _code.Append(new string('\t', _scopeLevel));
                _cleanLine = false;
            }
            _code.Append(code);
        }

        public override string ToString()
        {
            return _code.ToString();
        }

        public string ToString(int startIndex, int endIndex)
        {
            if (_code.Length == 0)
            {
                return String.Empty;
            }

            if (startIndex < 0) startIndex = 0;
            if (endIndex < 0) endIndex = 0;
            int length = endIndex - startIndex + 1;

            return _code.ToString(startIndex, length);
        }

        #endregion
    }
}
