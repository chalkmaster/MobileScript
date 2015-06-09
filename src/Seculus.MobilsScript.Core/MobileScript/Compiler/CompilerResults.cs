using System.Collections.Generic;
using System.Linq;
using Seculus.MobileScript.Core.MobileScript.VirtualMachine;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Representa o resultado de uma compilação.
    /// </summary>
    public class CompilerResults
    {
        #region Fields

        private IList<CompilationError> _errors;

        private readonly List<int> _entryPointAddresses = new List<int>();

        #endregion

        #region Constructors

        internal CompilerResults(bool succeeded, Program program, IList<CompilationError> errors, List<int> entryPointAddresses)
        {
            Succeeded = succeeded;
            Program = program;
            _errors = errors ?? new List<CompilationError>(0);
            _entryPointAddresses = entryPointAddresses ?? new List<int>();
        }

        internal CompilerResults(bool succeeded, Program program, IList<CompilationError> errors, int entryPointAddresses)
            : this(succeeded, program, errors, new List<int>() { entryPointAddresses })
        { }

        internal CompilerResults(bool succeeded, IList<CompilationError> errors)
            : this(succeeded, null, errors, null)
        { }

        internal CompilerResults(bool succeeded, Program program, IList<CompilationError> errors)
            : this(succeeded, program, errors, null)
        { }

        #endregion

        #region Properties

        /// <summary>
        /// Informa se a compilação ocorreu com sucesso.
        /// </summary>
        public bool Succeeded { get; internal set; }

        /// <summary>
        /// Programa compilado
        /// </summary>
        public Program Program { get; private set; }

        /// <summary>
        /// Erros de compilação
        /// </summary>
        public IList<CompilationError> Errors
        {
            get { return _errors; }
            set { _errors = value; }
        }

        public List<int> EntryPointAddresses
        {
            get { return _entryPointAddresses; }
        }

        /// <summary>
        /// Primeiro endereço de entrada do programa 
        /// (pode ser null se não tiver nenhum endereço de entrada, como no caso de bibliotecas por exemplo).
        /// </summary>
        public int? EntryPointAddress
        {
            get
            {
                if (_entryPointAddresses.Count == 0)
                {
                    return null;
                }
                return _entryPointAddresses.First();
            }
        }

        #endregion
    }
}
