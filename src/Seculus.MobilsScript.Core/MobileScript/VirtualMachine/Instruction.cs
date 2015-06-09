using System.Text;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Representa uma instrução
    /// </summary>
    public struct Instruction
    {
        #region Public Properties

        /// <summary>
        /// Código de operação
        /// </summary>
        public InstructionCode Code { get; private set; }

        /// <summary>
        /// Operando
        /// </summary>
        public Operand Operand { get; internal set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// Nome da operação
        /// </summary>
        private string Name { get { return InstructionsTable.Instance.GetName(Code); } }

        #endregion

        #region Constructors

        public Instruction(InstructionCode code) : this(code, new Operand()) { }

        public Instruction(InstructionCode code, Operand operand) : this()
        {
            Code = code;
            Operand = operand;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converte a instrução para string.
        /// </summary>
        /// <returns>Versão legível da instrução.</returns>
        public override string ToString()
        {
            var instructionStr = new StringBuilder();

            instructionStr.Append(Name);
            if (!Operand.IsNull()) instructionStr.Append(" " + Operand);

            return instructionStr.ToString();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Monta uma instrução a partir da sua representação em string (já separada em partes).
        /// </summary>
        /// <param name="instructionName">Nome da instrução</param>
        /// <param name="operandTypeChar">Caracter que indica o tipo do operando</param>
        /// <param name="operandValue">Valor do operando</param>
        /// <returns>Instrução</returns>
        public static Instruction Assemble(string instructionName, char operandTypeChar, string operandValue)
        {
            return new Instruction(InstructionsTable.Instance.GetCode(instructionName), Operand.Assemble(operandTypeChar, operandValue));
        }

        /// <summary>
        /// Monta uma instrução a partir da sua representação em string (já separada em partes).
        /// </summary>
        /// <param name="instructionName">Nome da instrução</param>
        /// <returns>Instrução</returns>
        public static Instruction Assemble(string instructionName)
        {
            return new Instruction(InstructionsTable.Instance.GetCode(instructionName));
        }

        #endregion
    }
}
