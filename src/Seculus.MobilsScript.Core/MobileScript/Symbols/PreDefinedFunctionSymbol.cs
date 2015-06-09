using System.Text;
using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.VirtualMachine;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa uma função pré-definida da linguagem.
    /// </summary>
    public class PreDefinedFunctionSymbol : FunctionSymbol
    {
        #region Constructors

        public PreDefinedFunctionSymbol(string name, TypeDeclaration type, PreDefinedFunctionCode code) : base(name, type)
        {
            Code = code;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Código da função pre-definida na linguagem.
        /// </summary>
        public PreDefinedFunctionCode Code { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Agrega uma parâmetro à função descrita por este objeto
        /// </summary>
        /// <param name="parameter">Parâmetro</param>
        public void AddParameter(ParameterSymbol parameter)
        {
            Parameters.Add(parameter);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append("{0} {1}(".FormatWith(Type.Name, Name));
            foreach (var param in Parameters)
            {
                if (param.ByRef) result.Append("ref ");
                result.Append("{0} {1}".FormatWith(param.Type.Name, param.Name));
                result.Append(", ");
            }
            if (Parameters.Count > 0) result.Length -= 2; // if we have any parameters, we need to remove the last ", "
            result.Append(");");

            return result.ToString();
        }

        #endregion
    }
}
