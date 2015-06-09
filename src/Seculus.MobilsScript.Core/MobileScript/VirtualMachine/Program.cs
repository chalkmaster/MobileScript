using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using AppPlat.Framework.Infrastructure.ErrorHandling;
using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Representa um programa executável. 
    /// Essa máquina é baseada em RPN (Reverse Polish notation), também conhecido como Postfix Notation, 
    /// que é uma notação matemática onde os operadores vem depois dos operandos (ex: calculadora HP 12C).
    /// </summary>
    public class Program : IDisposable
    {
        #region Constants

        private const int NoAddr = -1;

        #endregion

        #region Fields

        /// <summary>
        /// Pilha (memória de trabalho)
        /// </summary>
        private readonly object[] _stack;

        /// <summary>
        /// Área de código
        /// </summary>
        private readonly Instruction[] _code;

        /// <summary>
        /// Indica se o programa está em execução.
        /// </summary>
        private bool _running;

        /// <summary>
        /// Program Counter (aka Instruction Pointer): Aponta para a próxima instrução a ser executada.
        /// </summary>
        private int _pc;

        /// <summary>
        /// Aponta para o registro de ativação corrente.
        /// </summary>
        private int _base;

        /// <summary>
        /// Aponta para o topo da pilha.
        /// </summary>
        private int _top;

        /// <summary>
        /// Código de erro
        /// </summary>
        private ErrorCode _error;

        /// <summary>
        /// Dispositivo de entrada
        /// </summary>
        private TextReader _stdin;

        /// <summary>
        /// Dispositivo de saída padrão.
        /// </summary>
        private TextWriter _stdout;

        /// <summary>
        /// Dispositivo de saída de erro.
        /// </summary>
        private readonly TextWriter _stderr;

        #endregion

        #region Constructors

        public Program(int stackSize, int codeSize, TextReader input, TextWriter output, TextWriter error)
        {
            _stack = new object[stackSize];
            _code = new Instruction[codeSize];
            Reset();
            _stdin = input;
            _stdout = output;
            _stderr = error;
        }

        public Program(int stackSize, int codeSize, TextReader input)
            : this(stackSize, codeSize, input, Console.Out, Console.Error)
        { }

        public Program(int stackSize, int codeSize)
            : this(stackSize, codeSize, TextReader.Synchronized(new StreamReader(Console.OpenStandardInput(0), Encoding.UTF8)))
        { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Garante que a pilha tem espaço suficiente para alocar a quantidade informada de memória. 
        /// Se essa afirmativa for falsa, lança exceção.
        /// </summary>
        /// <param name="requiredSpace">Espaço necessário</param>
        /// <exception cref="StackException">Se a pilha não tiver espaço para alocar a quantidade de memória necessária.</exception>
        private void EnsureStackHasSpace(int requiredSpace)
        {
            if (!StackHasSpace(requiredSpace))
            {
                _stderr.WriteLine("Stack does not have {0} of available space as required.".FormatWith(requiredSpace));
                throw new StackException();
            }
        }

        /// <summary>
        /// Garante que a pilha tenha ao menos o número de elementos informado.
        /// Se não tiver, lança exceção.
        /// </summary>
        /// <param name="num">Número de elementos</param>
        /// <exception cref="StackException">Se a pilha tiver menos elementos do que informado.</exception>
        private void EnsureStackHasAtLeastNumOfElements(int num)
        {
            if (!StackHasAtLeastNumOfElements(num))
            {
                _stderr.WriteLine("Stack does not have {0} elements as required.".FormatWith(num));
                throw new StackException();
            }
        }

        /// <summary>
        /// Garante que dois operandos tenham o mesmo tipo. 
        /// Se essa afirmativa for false, lança exceção.
        /// </summary>
        /// <param name="op1">Operando 1</param>
        /// <param name="op2">Operando 2</param>
        /// <exception cref="InvalidOperandException">Se os operandos forem de tipos diferentes.</exception>
        private void EnsureOperandsHaveSameType(object op1, object op2)
        {
            if (!OperandsHaveSameType(op1, op2))
            {
                _stderr.WriteLine("Comparing {0} with {1}.".FormatWith(op1.GetType(), op2.GetType()));
                throw new InvalidOperandException();
            }
        }

        /// <summary>
        /// Verifica se o espaço disponível na pilha permite a alocação de um certo tamanho.
        /// </summary>
        /// <param name="requiredSize">Tamanho a ser alocado</param>
        /// <returns>True se tiver espaço. Caso contrário, false.</returns>
        private bool StackHasSpace(int requiredSize)
        {
            return (_top + requiredSize) < _stack.Length;
        }

        /// <summary>
        /// Verifica se a pilha tem ao menos o número de elementos informado.
        /// </summary>
        /// <param name="n">Número de elementos.</param>
        /// <returns>True se a pilha tiver ao menos o número de elementos informado. Caso contrário, false.</returns>
        private bool StackHasAtLeastNumOfElements(int n)
        {
            return ((_top + 1) >= n);
        }

        /// <summary>
        /// Verifica se dois operandos são do mesmo tipo.
        /// </summary>
        /// <param name="op1">Operando 1.</param>
        /// <param name="op2">Operando 2.</param>
        /// <returns>True se os operandos forem do mesmo tipo. Caso contrário, false.</returns>
        private bool OperandsHaveSameType(object op1, object op2)
        {
            return (op1.GetType() == op2.GetType());
        }

        /// <summary>
        /// Desempilha um objeto.
        /// </summary>
        /// <returns>Objeto desempilhado ou NULL se a VM estiver com algum erro.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        private object Pop()
        {
            if (_error != ErrorCode.NoError) return null;
            if (_top < 0) throw new StackException();
            object obj = _stack[_top--];
            if (obj == null) throw new NullOperandException();
            return obj;
        }

        /// <summary>
        /// Desempilha um inteiro.
        /// </summary>
        /// <returns>Inteiro desempilhado.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for inteiro.</exception>
        private int PopInt()
        {
            object obj = Pop();
            if (obj == null || obj.GetType() != typeof(int))
            {
                throw new InvalidOperandException();
            }
            return (int)obj;
        }

        /// <summary>
        /// Desempilha um float.
        /// </summary>
        /// <returns>Float desempilhado.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for float.</exception>
        private double PopFloat()
        {
            object obj = Pop();
            if (obj == null)
            {
                throw new InvalidOperandException();
            }

            Type objType = obj.GetType();
            if (objType != typeof(double) && objType != typeof(int))
            {
                throw new InvalidOperandException();
            }

            if (objType == typeof(int))
            {
                int val = (int) obj;
                return val;
            }
            return (double)obj;
        }

        /// <summary>
        /// Desempilha um booleano.
        /// </summary>
        /// <returns>Booleano desempilhado.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for booleano.</exception>
        private bool PopBool()
        {
            object obj = Pop();
            if (obj == null || obj.GetType() != typeof(bool))
            {
                throw new InvalidOperandException();
            }
            return (bool)obj;
        }

        /// <summary>
        /// Desempilha um char.
        /// </summary>
        /// <returns>Char desempilhado.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for char.</exception>
        private char PopChar()
        {
            object obj = Pop();
            if (obj == null || obj.GetType() != typeof(char))
            {
                throw new InvalidOperandException();
            }
            return (char)obj;
        }

        /// <summary>
        /// Desempilha uma string.
        /// </summary>
        /// <returns>String desempilhada.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for string.</exception>
        private string PopString()
        {
            object obj = Pop();
            if (obj == null || obj.GetType() != typeof(string))
            {
                throw new InvalidOperandException();
            }
            return (string)obj;
        }

        /// <summary>
        /// Desempilha um objeto do tipo IComparable.
        /// </summary>
        /// <returns>IComparable desempilhado.</returns>
        /// <exception cref="StackException">Se não tiver mais objetos na pilha.</exception>
        /// <exception cref="NullOperandException">Se o objeto desempilhado for nulo.</exception>
        /// <exception cref="InvalidOperandException">Se o objeto desempilhado não for do tipo IComparable.</exception>
        private IComparable PopComparable()
        {
            object obj = Pop();
            if (obj == null || !(obj is IComparable))
            {
                throw new InvalidOperandException();
            }
            return (IComparable)obj;
        }

        /// <summary>
        /// Empilha um objeto.
        /// </summary>
        /// <param name="obj">Objeto a ser empilhado.</param>
        private void Push(object obj)
        {
            if (++_top >= _stack.Length)
            {
                throw new StackException();
            }

            _stack[_top] = obj;
        }

        /// <summary>
        /// Inicia a máquina
        /// </summary>
        private void Reset()
        {
            _top = -1; // pilha vazia
            _base = NoAddr;
            _pc = NoAddr; // endereço inválido
            _running = false;
            _error = ErrorCode.NoError;
        }

        /// <summary>
        /// Escreve o status da máquina (registradores) na saída de erro.
        /// </summary>
        private void ShowStatus()
        {
            string strTop = "???";
            string strTopType = "???";
            string strBase = "???";
            string instruction = "???";
            try { strTop = _stack[_top].ToString(); }
            catch (Exception) { }
            try { strTopType = _stack[_top].GetType().FullName; }
            catch (Exception) { }
            try { strBase = _stack[_base].ToString(); }
            catch (Exception) { }
            try { instruction = _code[_pc].ToString(); }
            catch (Exception) { }

            _stderr.Write("status ==>");
            _stderr.Write(" pc:" + _pc + "\t");
            _stderr.Write(" top:" + _top + "\t");
            _stderr.Write(" stack[top]:(" + strTopType + ")" + strTop + "\t");
            _stderr.Write(" base:" + _base + "\t");
            _stderr.Write(" stack[base]:" + strBase + "\t");
            _stderr.Write(" instruction:" + instruction + "\t");
            _stderr.Write(" running:" + _running + "\t");
            _stderr.Write(" ErrorCode:" + _error);
            _stderr.WriteLine();
        }

        ///// <summary>
        ///// Executa uma função pré-definida da linguagem ('system call')
        ///// </summary>
        ///// <param name="functionCode">Código da função.</param>
        ///// <param name="debug">Informa se estamos em DEBUG ou não.</param>
        //private void SysCall(PreDefinedFunctionCode functionCode, bool debug = false)
        //{
        //    int intVal1; // Representa um operando inteiro.
        //    string strVal1; // Representa um operando string.
        //    float floatVal1; // Representa um operando do tipo float.

        //    switch (functionCode)
        //    {
        //        case PreDefinedFunctionCode.Substring:
        //            break;

        //        case PreDefinedFunctionCode.Print:
        //            strVal1 = PopString();
        //            _stdout.Write(strVal1);
        //            if (debug) _stderr.WriteLine("VM:: SYSCALL print(\"{0}\")", strVal1);
        //            break;

        //        case PreDefinedFunctionCode.PrintLn:
        //            _stdout.WriteLine();
        //            if (debug) _stderr.WriteLine("VM:: SYSCALL println()");
        //            break;

        //        case PreDefinedFunctionCode.IntToStr:
        //            intVal1 = PopInt();
        //            strVal1 = intVal1.ToString(CultureInfo.InvariantCulture);
        //            Push(strVal1);
        //            if (debug) _stderr.WriteLine("VM:: SYSCALL intToStr({0})", intVal1);
        //            break;

        //        case PreDefinedFunctionCode.FloatToStr:
        //            floatVal1 = PopFloat();
        //            strVal1 = floatVal1.ToString(CultureInfo.InvariantCulture);
        //            Push(strVal1);
        //            if (debug) _stderr.WriteLine("VM:: SYSCALL floatToStr({0})", floatVal1);
        //            break;

        //        case PreDefinedFunctionCode.BoolToStr:
        //            break;
        //        case PreDefinedFunctionCode.StrToInt:
        //            break;
        //        case PreDefinedFunctionCode.StrToFloat:
        //            break;
        //        case PreDefinedFunctionCode.NewDate:
        //            break;
        //        case PreDefinedFunctionCode.Today:
        //            break;
        //        case PreDefinedFunctionCode.GetDay:
        //            break;
        //        case PreDefinedFunctionCode.GetMonth:
        //            break;
        //        case PreDefinedFunctionCode.GetYear:
        //            break;
        //        case PreDefinedFunctionCode.ParseDate:
        //            break;
        //        case PreDefinedFunctionCode.CallService:
        //            break;
        //        case PreDefinedFunctionCode.GetCurrentActivityName:
        //            break;
        //        case PreDefinedFunctionCode.GetCurrentModelName:
        //            break;
        //        case PreDefinedFunctionCode.StartSynchronization:
        //            break;
        //        case PreDefinedFunctionCode.ShowInfoMessage:
        //            break;
        //        case PreDefinedFunctionCode.ShowErrorMessage:
        //            break;
        //        case PreDefinedFunctionCode.ShowWarningMessage:
        //            break;
        //        case PreDefinedFunctionCode.Find:
        //            break;
        //        case PreDefinedFunctionCode.StartLocationCapture:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException("functionCode");
        //    }
        //}

        /// <summary>
        /// Executa uma função pré-definida da linguagem ('system call')
        /// </summary>
        /// <param name="functionCode">Código da função.</param>
        /// <param name="debug">Informa se estamos em DEBUG ou não.</param>
        private void SysCall(PreDefinedFunctionCode functionCode, bool debug = false)
        {
            int intVal1; // Representa um operando inteiro.
            int intVal2; // Representa um operando inteiro.
            int intVal3; // Representa um operando inteiro.
            int intVal4; // Representa um operando inteiro.
            int intVal5; // Representa um operando inteiro.
            string strVal1; // Representa um operando string.
            string strVal2; // Representa um operando string.
            string strVal3; // Representa um operando string.
            //double floatVal1; // Representa um operando float.
            bool boolVal1; // Representa um operando booleano.

            switch (functionCode)
            {
                case PreDefinedFunctionCode.Print:
                    strVal1 = PopString();
                    _stdout.Write(strVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL print(\"{0}\")", strVal1);
                    break;
                case PreDefinedFunctionCode.PrintLn:
                    _stdout.WriteLine();
                    if (debug) _stderr.WriteLine("VM:: SYSCALL println()");
                    break;



                case PreDefinedFunctionCode.Substring:
                    intVal1 = PopInt(); // endIndex
                    intVal2 = PopInt(); // startIndex (0-based)
                    strVal1 = PopString(); // string
                    strVal2 = strVal1.Substring(intVal2, (intVal1 - intVal2));
                    Push(strVal2);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL substring(\"{0}\", {1}. {2}) returned \"{3}\"", strVal1, intVal2, intVal1, strVal2);
                    break;
                case PreDefinedFunctionCode.StrLen:
                    strVal1 = PopString();
                    intVal1 = strVal1.Length;
                    Push(intVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL strlen(\"{0}\") returned {1}", strVal1, intVal1);
                    break;
                case PreDefinedFunctionCode.CharVal:
                    strVal1 = PopString(); // char
                    intVal1 = (int)strVal1[0];
                    Push(intVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL charVal(\"{0}\") returned {1}", strVal1, intVal1);
                    break;
                case PreDefinedFunctionCode.CharValAt:
                    intVal1 = PopInt(); // index
                    strVal1 = PopString(); // string
                    intVal2 = (int)strVal1[intVal1];
                    Push(intVal2);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL charValAt(\"{0}\", {1}) returned {2}", strVal1, intVal1, intVal2);
                    break;
                case PreDefinedFunctionCode.BuildStringFromChars:
                    intVal1 = PopInt(); // length
                    intVal2 = PopInt(); // array address (because it is passed by reference).

                    StringBuilder strBuilder = new StringBuilder(intVal1);
                    for (int i = 0; i < intVal1; i++)
                    {
                        intVal3 = (int)_stack[intVal2 + i]; // intVal2 is the base address and i is the array indexer.
                        strBuilder.Append((char)intVal3);
                    }
                    Push(strBuilder.ToString());
                    if (debug) _stderr.WriteLine("VM:: SYSCALL buildStringFromChars({0}, {1}) returned {2}", "ARRAY", intVal1, strBuilder);
                    break;




                case PreDefinedFunctionCode.Now:
                    strVal1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Push(strVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL now() returned {0}", strVal1);
                    break;




                case PreDefinedFunctionCode.GetImei:
                    strVal1 = "FAKEIMEI";
                    Push(strVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL getImei() returned \"{0}\"", strVal1);
                    break;
                case PreDefinedFunctionCode.SendEmail:
                    strVal1 = PopString(); // body
                    strVal2 = PopString(); // title
                    strVal3 = PopString(); // to
                    _stderr.WriteLine("to: {0}, title: {1}\n-- body --\n {2}", strVal3, strVal2, strVal1);
                    Push(true);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL sendEmail(\"{0}\", \"{1}\", \"{2}\") returned {3}", strVal3, strVal2, strVal1, true);
                    string body;
                    break;
                case PreDefinedFunctionCode.SendSms:
                    strVal1 = PopString(); // message
                    strVal2 = PopString(); // number
                    if (debug) _stderr.WriteLine("VM:: SYSCALL sendSms(\"{0}\", \"{1}\")", strVal2, strVal1);
                    break;




                case PreDefinedFunctionCode.ShowDialog:
                    strVal1 = PopString(); // texto
                    strVal2 = PopString(); // titulo
                    if (debug) _stderr.WriteLine("VM:: SYSCALL showDialog(\"{0}\", \"{1}\")", strVal2, strVal1);
                    // Aqui a VM deve chamar a função nativa no mobile.
                    break;
                case PreDefinedFunctionCode.ShowSmartReminder:
                    intVal1 = PopInt(); // tempo (milisegundos).
                    strVal1 = PopString(); // texto
                    if (debug) _stderr.WriteLine("VM:: SYSCALL showSmartReminder(\"{0}\", \"{1}\")", strVal1, intVal1);
                    // Aqui a VM deve chamar a função nativa no mobile.
                    break;
                case PreDefinedFunctionCode.ShowToast:
                    strVal1 = PopString(); // mensagem
                    if (debug) _stderr.WriteLine("VM:: SYSCALL ShowNotification(\"{0}\")", strVal1);
                    break;
                case PreDefinedFunctionCode.GetSubscriberId:
                    strVal1 = "SUBSCRIBERID";
                    Push(strVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL getSubscriberId() returned \"{0}\"", strVal1);
                    break;
                case PreDefinedFunctionCode.GetUserId:
                    intVal1 = 1;
                    Push(intVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL getUserId() returned {0}", intVal1);
                    break;
                case PreDefinedFunctionCode.GetVersion:
                    strVal1 = "1.0.0";
                    Push(strVal1);
                    if (debug) _stderr.WriteLine("VM:: SYSCALL getVersion() returned \"{0}\"", strVal1);
                    break;
                case PreDefinedFunctionCode.CallService:
                    break;
                case PreDefinedFunctionCode.GetCurrentModelName:
                    break;
                case PreDefinedFunctionCode.StartSynchronization:
                    break;
                case PreDefinedFunctionCode.StartLocationCapture:
                    break;
                case PreDefinedFunctionCode.GetCurrentActivityName:
                    break;
                case PreDefinedFunctionCode.GetVarValue:
                    break;
                case PreDefinedFunctionCode.GoToRoute:
                    break;
                case PreDefinedFunctionCode.CountTransitions:
                    break;
                case PreDefinedFunctionCode.GetTransition:
                    intVal1 = PopInt(); // sync (boolean) address.
                    intVal2 = PopInt(); // value (string) address.
                    intVal3 = PopInt(); // date/time (string) address.
                    intVal4 = PopInt(); // index
                    strVal1 = PopString(); // activity name.

                    // Aqui a VM deve chamar a função nativa no mobile.
                    intVal5 = 10; // Id da transição.
                    boolVal1 = true; // Se a transição foi synchronizada.
                    strVal2 = "Transition value..."; // Valor da transição.
                    strVal3 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // data/hora da execução.

                    // Seta os valores de retorno no endereço dos parâmetros passados por referência.
                    _stack[intVal1] = boolVal1;
                    _stack[intVal2] = strVal2;
                    _stack[intVal3] = strVal3;
                    Push(intVal5);
                    if (debug)
                    {
                        _stderr.WriteLine("VM:: SYSCALL getTransition(\"{0}\", {1}, \"{2}\", \"{3}\", {4}) returned {5}", 
                            strVal1, intVal4, strVal3, strVal2, boolVal1, intVal5);
                    }
                    break;
                case PreDefinedFunctionCode.GetHeader:
                    break;

                default:
                    throw new ArgumentOutOfRangeException("functionCode");
            }
        }

        #endregion

        #region Public/Internal Methods

        /// <summary>
        /// Agrega uma instrução ao código.
        /// </summary>
        /// <param name="code">Código da instrução.</param>
        /// <param name="operand">Operando da instrução.</param>
        /// <returns>Endereço da instrução no código</returns>
        internal int AddInstruction(InstructionCode code, Operand operand)
        {
            return AddInstruction(new Instruction(code, operand));
        }

        /// <summary>
        /// Agrega uma instrução ao código.
        /// </summary>
        /// <param name="code">Código da instrução.</param>
        /// <returns>Endereço da instrução no código</returns>
        internal int AddInstruction(InstructionCode code)
        {
            return AddInstruction(new Instruction(code));
        }

        /// <summary>
        /// Agrega uma instrução ao código.
        /// </summary>
        /// <param name="instruction">Instrução</param>
        /// <returns>Endereço da instrução no código.</returns>
        internal int AddInstruction(Instruction instruction)
        {
            _code[++_pc] = instruction;
            return _pc;
        }

        /// <summary>
        /// Retorna uma string com o código desse programa.
        /// </summary>
        /// <param name="entryPointAddress">Endereço de início da execução (entry point)</param>
        /// <param name="limit">Endereço do código final a ser escrito</param>
        /// <returns>String com o código do programa.</returns>
        public string CodeToString(int entryPointAddress, int limit)
        {
            var codeString = new StringBuilder();
            codeString.AppendLine("SIZE " + (limit + 1));
            codeString.AppendLine("START " + entryPointAddress);
            for (int i = 0; i <= limit; i++)
            {
                codeString.AppendLine(_code[i].ToString());
            }
            return codeString.ToString();
        }

        /// <summary>
        /// Escreve o código na saída padrão, a partir do endereço zero.
        /// </summary>
        /// <param name="limit">Endereço final do código a ser escrito.</param>
        public void DumpCode(int limit)
        {
            _stderr.WriteLine("SIZE " + (limit + 1));
            for (int i = 0; i <= limit; i++)
            {
                _stderr.WriteLine("\t" + i + "\t" + _code[i]);
            }
            _stderr.Flush();
        }

        /// <summary>
        /// Escreve o conteúdo da pilha na saída padrão.
        /// </summary>
        public void DumpStack()
        {
            _stderr.WriteLine("STACK:");
            for (int i = _top; i >= 0; i--)
            {
                _stderr.WriteLine("\t" + i + "\t" + _stack[i]);
            }
            _stderr.Flush();
        }

        /// <summary>
        /// Inicia a execução a partir de um endereço inicial, passado como parâmetro.
        /// </summary>
        /// <param name="entryPointAddress">Endereço inicial (de entrada)</param>
        /// <param name="debug">Indica depuração (escreve o estado da máquina a cada instrução)</param>
        /// <returns>Código do erro (0 para execução sem erros)</returns>
        public int Run(int entryPointAddress, bool debug)
        {
            /*******************************************************************
             * OBS:
             * Essa máquina é baseada em RPN (Reverse Polish Notation).
             * RPN é uma técnica onde o operador vem depois dos seus operandos (igual a calculadora HP 12c).
             * Ver como funciona a instrução ADD e SUB que você entenderá.
             *******************************************************************/

            int stackSpace = -1; // Representa o tamanho da pilha.
            int addr = -1; // Representa um apontador para alguma instrução.
            Instruction instruction; // Representa a instrução atual.

            object val; // Representa um valor de tipo desconhecido.
            int intVal; // Representa um valor inteiro.
            char chrVal; // Representa um valor char.
            string strVal; // Representa um valor de string.
            double floatVal; // Representa um valor de ponto flutuante.
            bool boolVal; // Representa um valor booleano.

            object opn1; // Representa um operando de tipo desconhecido.
            object opn2; // Representa um operando de tipo desconhecido.
            IComparable compOpn1; // Representa um operando do tipo IComparable.
            IComparable compOpn2; // Representa um operando do tipo IComparable.
            int intOpn1; // Representa um operando inteiro.
            int intOpn2; // Representa um operando inteiro.
            double floatOpn1; // Representa um operando do tipo float.
            double floatOpn2; // Representa um operando do tipo float.
            bool boolOpn1; // Representa um operando booleano.
            bool boolOpn2; // Representa um operando booleano.
            string strOpn1; // Representa um operando string.
            string strOpn2; // Representa um operando string.

            Reset();

            _pc = entryPointAddress;
            _running = true;

            while (_running)
            {
                try
                {
                    instruction = _code[_pc]; // Pega a instrução atual (a ser executada).
                }
                catch
                {
                    _error = ErrorCode.InvalidAddr;
                    _running = false;
                    return (int)_error;
                }

                if (debug) ShowStatus();

                _pc++;

                try
                {
                    switch (instruction.Code)
                    {
                        case InstructionCode.NOp: // 'no operation': não faz nada
                            break;

                        case InstructionCode.Enter: // entrada em função.
                            stackSpace = instruction.Operand.GetIntValue(); // Argumento: tamanho da área para variáveis locais.
                            EnsureStackHasSpace(stackSpace + 1); // Precisa ter espaço para o que a função precisa mais 1 (para fazer o push do endereço base da pilha).
                            Push(_base);
                            _base = _top; // atualiza base
                            _top += stackSpace; // reserva área para variáveis locais
                            break;

                        case InstructionCode.Call: // chamada de função.
                            addr = instruction.Operand.GetIntValue(); // Argumento: endereço inicial da função
                            Push(_pc); // empilha endereço de retorno
                            _pc = addr;
                            break;

                        case InstructionCode.Return: // retorno de função. 
                            stackSpace = instruction.Operand.GetIntValue(); // Argumento: tamanho da área reservada para variáveis locais.
                            EnsureStackHasAtLeastNumOfElements(stackSpace + 3); // Precisa ter pelo menos 3 caras na pilha, fora o a função precisa. Os 3 são: "base", "pc" e "retorno" da função. Se a função por void, tem uma tratamento especial de reservar espaço na pilha e tirar depois.
                            _top -= stackSpace; // libera área de variáveis locais
                            _base = PopInt(); // restaura base
                            _pc = PopInt(); // desempilha endereço de retorno e desvia
                            break;

                        case InstructionCode.Halt:
                            _running = false;
                            break;

                        case InstructionCode.Jump: // desvio incondicional.
                            addr = instruction.Operand.GetIntValue(); // Argumento: endereço de desvio.
                            _pc = addr;
                            break;

                        case InstructionCode.JumpF: // desvio condicional (desvia se falso).
                            addr = instruction.Operand.GetIntValue(); // Argumento: endereço de desvio.
                            boolVal = PopBool();
                            if (!boolVal)
                            {
                                _pc = addr;
                            }
                            break;

                        case InstructionCode.Add: // soma: desempilha dois valores inteiros, e empilha a soma dos mesmos.
                            intOpn1 = PopInt();
                            intOpn2 = PopInt();
                            Push(intOpn2 + intOpn1);
                            break;

                        case InstructionCode.Sub: // subtração (ver ADD)
                            intOpn1 = PopInt();
                            intOpn2 = PopInt();
                            Push(intOpn2 - intOpn1);
                            break;

                        case InstructionCode.Mult: // multiplicação (ver ADD)
                            intOpn1 = PopInt();
                            intOpn2 = PopInt();
                            Push(intOpn2 * intOpn1);
                            break;

                        case InstructionCode.Div: // divisão (ver ADD)
                            intOpn1 = PopInt();
                            intOpn2 = PopInt();
                            Push(intOpn2 / intOpn1);
                            break;

                        case InstructionCode.Minus: // inverte o sinal do inteiro no topo da pilha.
                            intOpn1 = PopInt();
                            Push(-intOpn1);
                            break;

                        case InstructionCode.Mod: // módulo (ver ADD)
                            intOpn1 = PopInt();
                            intOpn2 = PopInt();
                            Push(intOpn2 % intOpn1);
                            break;

                        case InstructionCode.And: // desempilha dois valores booleanos e empilha o 'and' dos dois.
                            boolOpn1 = PopBool();
                            boolOpn2 = PopBool();
                            Push(boolOpn2 && boolOpn1);
                            break;

                        case InstructionCode.Or: // desempilha dois valores booleanos e empilha o 'or' dos dois.
                            boolOpn1 = PopBool();
                            boolOpn2 = PopBool();
                            Push(boolOpn2 || boolOpn1);
                            break;

                        case InstructionCode.XOr: // desempilha dois valores booleanos e empilha o 'xor' dos dois.
                            boolOpn1 = PopBool();
                            boolOpn2 = PopBool();
                            Push(boolOpn2 ^ boolOpn1);
                            break;

                        case InstructionCode.Not: // nega o valor booleano no topo da pilha
                            boolOpn1 = PopBool();
                            Push(!boolOpn1);
                            break;

                        case InstructionCode.Eq: // compara dois valores ( == ) no topo da pilha e empilha o resultado da comparação.
                            opn1 = Pop();
                            opn2 = Pop();
                            EnsureOperandsHaveSameType(opn1, opn2);
                            Push(opn2.Equals(opn1));
                            break;

                        case InstructionCode.NE: // compara dois valores ( != ) no topo da pilha e empilha o resultado da comparação.
                            opn1 = Pop();
                            opn2 = Pop();
                            EnsureOperandsHaveSameType(opn1, opn2);
                            Push(!opn2.Equals(opn1));
                            break;

                        case InstructionCode.GT: // compara dois valores ( > ) no topo da pilha e empilha o resultado da comparação.
                            compOpn1 = PopComparable();
                            compOpn2 = PopComparable();
                            EnsureOperandsHaveSameType(compOpn1, compOpn2);
                            Push(compOpn2.CompareTo(compOpn1) > 0);
                            break;

                        case InstructionCode.GE: // compara dois valores ( >= ) no topo da pilha e empilha o resultado da comparação.
                            compOpn1 = PopComparable();
                            compOpn2 = PopComparable();
                            EnsureOperandsHaveSameType(compOpn1, compOpn2);
                            Push(compOpn2.CompareTo(compOpn1) >= 0);
                            break;

                        case InstructionCode.LT: // compara dois valores ( < ) no topo da pilha e empilha o resultado da comparação.
                            compOpn1 = PopComparable();
                            compOpn2 = PopComparable();
                            EnsureOperandsHaveSameType(compOpn1, compOpn2);
                            Push(compOpn2.CompareTo(compOpn1) < 0);
                            break;

                        case InstructionCode.LE: // compara dois valores ( <= ) no topo da pilha e empilha o resultado da comparação.
                            compOpn1 = PopComparable();
                            compOpn2 = PopComparable();
                            EnsureOperandsHaveSameType(compOpn1, compOpn2);
                            Push(compOpn2.CompareTo(compOpn1) <= 0);
                            break;

                        case InstructionCode.IntConst: // carrega constante inteira.
                            intVal = instruction.Operand.GetIntValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(intVal); // empilha a constante.
                            break;

                        case InstructionCode.CharConst: // carrega constante char.
                            chrVal = instruction.Operand.GetCharValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(chrVal); // empilha a constante.
                            break;

                        case InstructionCode.StrConst: // carrega constante string.
                            strVal = instruction.Operand.GetStringValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(strVal); // empilha a constante.
                            break;

                        case InstructionCode.FloatConst: // carrega constante float.
                            floatVal = instruction.Operand.GetFloatValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(floatVal); // empilha a constante.
                            break;

                        case InstructionCode.BoolConst: // carrega constante booleana.
                            boolVal = instruction.Operand.GetBoolValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(boolVal); // empilha a constante.
                            break;

                        case InstructionCode.IntToStr: // converte um inteiro (no topo da pilha) para string.
                            intVal = PopInt();
                            Push(intVal.ToString(CultureInfo.InvariantCulture));
                            break;

                        case InstructionCode.BoolToStr: // converte um boleano (no topo da pilha) para string.
                            boolVal = PopBool();
                            Push(boolVal.ToString(CultureInfo.InvariantCulture).ToLower());
                            break;

                        case InstructionCode.CharToStr: // converte um caracter (no topo da pilha) para string.
                            chrVal = PopChar();
                            Push(chrVal.ToString(CultureInfo.InvariantCulture));
                            break;

                        case InstructionCode.FloatToStr:// converte um float (no topo da pilha) para string.
                            floatVal = PopFloat();
                            Push(floatVal.ToString(CultureInfo.InvariantCulture));
                            break;

                        case InstructionCode.CharToInt: // converte um caracter (no topo da pilha) para inteiro.
                            chrVal = PopChar();
                            Push(Convert.ToInt32(chrVal));
                            break;

                        case InstructionCode.FloatToInt: // converte um float (no topo da pilha) para inteiro.
                            floatVal = PopFloat();
                            intVal = (int)Math.Round(floatVal);
                            Push(intVal);
                            break;

                        case InstructionCode.IntToFloat: // converte um inteiro (no topo da pilha) para float.
                            intVal = PopInt();
                            floatVal = (double)intVal;
                            Push(floatVal);
                            break;

                        case InstructionCode.Concat:
                            strOpn1 = PopString();
                            strOpn2 = PopString();
                            Push(strOpn2 + strOpn1);
                            break;

                        case InstructionCode.Inc: // incrementa o inteiro no topo da pilha
                            intOpn1 = PopInt();
                            Push(intOpn1 + 1);
                            break;

                        case InstructionCode.Dec: // decrementa o inteiro no topo da pilha
                            intOpn1 = PopInt();
                            Push(intOpn1 - 1);
                            break;

                        case InstructionCode.LdVar: // carrega o valor de uma variável.
                            addr = instruction.Operand.GetIntValue(); // Operando: endereço da variável.
                            val = _stack[_base + addr];
                            Push(val);
                            break;

                        case InstructionCode.StVar: // desempilha valor e atribui a uma variável.
                            addr = instruction.Operand.GetIntValue(); // Operando: endereço da variável.
                            val = Pop();
                            _stack[_base + addr] = val;
                            if (debug)
                            {
                                _stderr.WriteLine();
                                _stderr.WriteLine("VM:: _stack[{0}] = {1}", (_base + addr), val);
                                _stderr.WriteLine();
                            }
                            break;

                        case InstructionCode.LdAddr: // carrega o endereço absoluto de uma varável.
                            addr = instruction.Operand.GetIntValue(); // Operando: endereço relativo da variável.
                            Push(_base + addr);
                            if (debug)
                            {
                                _stderr.WriteLine();
                                _stderr.WriteLine("VM:: Push({0})", (_base + addr));
                                _stderr.WriteLine();
                            }
                            break;

                        case InstructionCode.Sti: // 'indirect store': desempilha endereço e valor e armazena o valor na variável com esse endereço.
                            addr = PopInt();
                            val = Pop();
                            _stack[addr] = val;
                            if (debug)
                            {
                                _stderr.WriteLine("VM:: Absolute address: {0}", addr);
                                _stderr.WriteLine("VM:: Stored value: {0}", val);
                            }
                            break;

                        case InstructionCode.Ldi: // carrega o valor de uma variável cujo endereço está no topo da pilha.
                            addr = PopInt();
                            val = _stack[addr];
                            Push(val);
                            if (debug)
                            {
                                _stderr.WriteLine("VM:: Absolute address: {0}", addr);
                                _stderr.WriteLine("VM:: Loaded value: {0}", val);
                            }
                            break;

                        case InstructionCode.StBlock: // desempilha um 'bloco' (vetor) e armazena numa variável.
                            intOpn1 = instruction.Operand.GetIntValue(); // Operando: tamanho do bloco;
                            intOpn2 = PopInt(); // endereço inicial da variável (do array).

                            // o for tem que ser de trás pra frente por causa do Pop.
                            for (int i = (intOpn1 - 1); i >= 0; --i)
                            {
                                _stack[intOpn2 + i] = Pop();
                            }
                            break;

                        case InstructionCode.LdBlock: // empilha um 'bloco' de variáveis (vetor)
                            intOpn1 = instruction.Operand.GetIntValue(); // Operando: tamanho do bloco;
                            intOpn2 = PopInt(); // endereço inicial da variável
                            for (int i = 0; i < intOpn1; i++)
                            {
                                Push(intOpn2 + i);
                            }
                            break;

                        case InstructionCode.ReadInt: // le inteiro pela entrada padrão e empilha.
                            strVal = _stdin.ReadLine();
                            if (Int32.TryParse(strVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out intVal))
                            {
                                Push(intVal);
                            }
                            else
                            {
                                Push(default(int));
                            }
                            break;

                        case InstructionCode.ReadStr: // le string pela entrada padrão e empilha.
                            strVal = _stdin.ReadLine();
                            Push(strVal);
                            break;

                        case InstructionCode.ReadFloat:
                            strVal = _stdin.ReadLine();
                            if (Double.TryParse(strVal, NumberStyles.Float, CultureInfo.InvariantCulture, out floatVal))
                            {
                                Push(floatVal);
                            }
                            else
                            {
                                Push(default(double));
                            }
                            break;

                        case InstructionCode.IncT: // reserva espaço na pilha.
                            stackSpace = instruction.Operand.GetIntValue(); // Argumento: espaço a ser reservado na pilha.
                            _top += stackSpace;
                            break;

                        case InstructionCode.SysCall: // Chamada às funções pré-definidas da linguagem.
                            intVal = instruction.Operand.GetIntValue(); // Operando: código da função.
                            SysCall((PreDefinedFunctionCode)intVal, debug);
                            break;

                        case InstructionCode.FAdd: // soma float.
                            floatOpn1 = PopFloat();
                            floatOpn2 = PopFloat();
                            Push(floatOpn2 + floatOpn1);
                            break;

                        case InstructionCode.FSub: // sub float.
                            floatOpn1 = PopFloat();
                            floatOpn2 = PopFloat();
                            Push(floatOpn2 - floatOpn1);
                            break;

                        case InstructionCode.FMult: // mult float.
                            floatOpn1 = PopFloat();
                            floatOpn2 = PopFloat();
                            Push(floatOpn2 * floatOpn1);
                            break;

                        case InstructionCode.FDiv: // div float.
                            floatOpn1 = PopFloat();
                            floatOpn2 = PopFloat();
                            Push(floatOpn2 / floatOpn1);
                            break;

                        case InstructionCode.FMinus: // inverte sinal float.
                            floatOpn1 = PopFloat();
                            Push(-floatOpn1);
                            break;

                        case InstructionCode.FMod:// mod float.
                            floatOpn1 = PopFloat();
                            floatOpn2 = PopFloat();
                            Push(floatOpn2 % floatOpn1);
                            break;

                        case InstructionCode.GlAddr: // igual a INTCONST, usando um mnemonico diferente apenas por clareza.
                            intVal = instruction.Operand.GetIntValue(); // Argumento: valor da constante.
                            EnsureStackHasSpace(1);
                            Push(intVal); // empilha a constante.
                            break;

                        case InstructionCode.LdGlVar: // carrega valor de variável global
                            addr = instruction.Operand.GetIntValue(); // Operando: Endereço da variavel.
                            val = _stack[addr];
                            Push(val);
                            break;

                        case InstructionCode.OffS: // calcula o valor do deslocamento relativo a uma 'indexação intermediária'
                            int size = instruction.Operand.GetIntValue(); // Operando: Tamanho do subvetor.
                            int idxExpr = PopInt();
                            int vectorAddr = PopInt();
                            Push(vectorAddr + (idxExpr * size)); // Empilha o endereço da posição do vetor indexada (endereço do "subvetor").
                            break;

                        case InstructionCode.Idx: // Calcula o endereço de uma 'indexação final' e verifica o limite do vetor.
                            int limit = instruction.Operand.GetIntValue(); // Operando: tamanho do vetor (ou -1 se o tamanho for desconhecido).
                            idxExpr = PopInt();
                            vectorAddr = PopInt();
                            int indexValue = vectorAddr + idxExpr; // neste caso, size == 1
                            if (debug) _stderr.WriteLine("VM:: indexValue: {0}".FormatWith(indexValue));
                            if (limit > -1) // Se o limite for -1, significa que não sabemos o tamanho do array. Então deixamos o usuário indexar a vontade.
                            {
                                if (idxExpr >= limit) throw new InvalidIndexException();
                            }
                            Push(indexValue); // Empilha o endereço da posição do vetor indexada.
                            break;

                        default: // nenhuma das anteriores ...
                            _error = ErrorCode.InvalidOpCode;
                            _running = false;
                            break;
                    }
                }
                catch (StackException)
                {
                    _error = ErrorCode.StackError;
                    _running = false;
                }
                catch (NullOperandException)
                {
                    _error = ErrorCode.NullOperand;
                    _running = false;
                }
                catch (InvalidOperandException)
                {
                    _error = ErrorCode.InvalidOperand;
                    _running = false;
                }
                catch (InvalidIndexException)
                {
                    _error = ErrorCode.IndexError;
                    _running = false;
                }
                catch (Exception ex)
                {
                    _error = ErrorCode.UnknownError;
                    _stderr.WriteLine(ExceptionManager.GetExceptionAsString(ex));
                    _running = false;
                }
            }

            return (int)_error;
        }

        /// <summary>
        /// Program Counter (aka Instruction Pointer): Aponta para a instrução a ser executada.
        /// </summary>
        /// <returns></returns>
        public int GetProgramCounter()
        {
            return _pc;
        }

        /// <summary>
        /// Acerta o valor de um operando.
        /// </summary>
        /// <param name="instructionAddress">Endereço da instrução que contém o operando.</param>
        /// <param name="newOperand">Novo operando</param>
        internal void FixOperand(int instructionAddress, Operand newOperand)
        {
            if (instructionAddress > _pc) throw new CodeGenerationException("instructionAddress > _pc");
            _code[instructionAddress].Operand = newOperand;
        }

        /// <summary>
        /// Retorna todas as instruções desse programa.
        /// </summary>
        /// <returns>Lista de instruções.</returns>
        internal ReadOnlyCollection<Instruction> GetInstructions()
        {
            int listSize = _pc + 1;
            var instructions = new List<Instruction>(listSize);
            for (int i = 0; i < listSize; i++)
            {
                instructions.Add(_code[i]);
            }
            return new ReadOnlyCollection<Instruction>(instructions);
        }

        #endregion

        #region Disposable Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stdin.Dispose();
                    _stdout.Dispose();
                }

                _stdin = null;
                _stdout = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
