using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine.Serialization
{
    public class LegacyProgramSerializer : IProgramSerializer
    {
        #region Constants

        /// <summary>
        /// Delimitador entre uma instrução e outra.
        /// </summary>
        private const string InstructionDelimiter = ":~";

        /// <summary>
        /// Delimitador entre os elementos de uma mesma instrução.
        /// </summary>
        private const string InstructionElementsDelimiter = "::";
        private const char IntIdentifier = 'i';
        private const char FloatIdentifier = 'f';
        private const char StringIdentifier = 's';
        private const char BoolIdentifier = 'b';
        private const char CharIdentifier = 'c';
        private const char NullIdentifier = 'n';

        #endregion

        #region Implementation of IProgramSerializer

        public string Serialize(Program program)
        {
            var output = new StringBuilder();

            ReadOnlyCollection<Instruction> instructions = program.GetInstructions();
            foreach (var instruction in instructions)
            {
                // Serializa o código da instrução.
                output.Append((int)instruction.Code);

                // Serializa o operando (se for diferente de null).
                if (instruction.Operand.Type != DataType.Null)
                {
                    output.Append(InstructionElementsDelimiter);

                    output.Append(GetOperandValueAsString(instruction.Operand));
                    output.Append(InstructionElementsDelimiter);

                    output.Append(GetOperandTypeRepresentation(instruction.Operand.Type));
                }

                // Serializa o delimitador de instruções.
                output.Append(InstructionDelimiter);
            }

            // Remove a última terminação de instrução.
            if (output.Length > InstructionDelimiter.Length)
            {
                output.Length -= InstructionDelimiter.Length;
            }

            return output.ToString();
        }

        public Program Deserialize(string str)
        {
            var program = new Program(CodeCompiler.DefaultStackSize, CodeCompiler.DefaultCodeSize);

            // Pega as instruções.
            string[] instructionsString = str.Split(new[] { InstructionDelimiter }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var instructionString in instructionsString)
            {
                // Tokeniza os elementos de uma instrução.
                string[] elements = instructionString.Trim().Split(new[] { InstructionElementsDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                if (elements.Length == 1)
                {
                    program.AddInstruction(GetInstructionCode(elements[0]));
                }
                else if (elements.Length == 3)
                {
                    program.AddInstruction(GetInstructionCode(elements[0]), GetOperand(elements[2][0], elements[1]));
                }
                else
                {
                    throw new ApplicationException("Invalid serialization!");
                }
            }

            return program;
        }

        #endregion

        #region Private Methods

        private char GetOperandTypeRepresentation(DataType type)
        {
            switch (type)
            {
                case DataType.Null:
                    return NullIdentifier;
                case DataType.Int:
                    return IntIdentifier;
                case DataType.String:
                    return StringIdentifier;
                case DataType.Char:
                    return CharIdentifier;
                case DataType.Boolean:
                    return BoolIdentifier;
                case DataType.Float:
                    return FloatIdentifier;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private InstructionCode GetInstructionCode(string instructionCodeString)
        {
            return (InstructionCode)Int32.Parse(instructionCodeString, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private Operand GetOperand(char operandType, string operandValue)
        {
            if (operandType == IntIdentifier)
            {
                return new Operand(Int32.Parse(operandValue, NumberStyles.Integer, CultureInfo.InvariantCulture));
            }
            if (operandType == FloatIdentifier)
            {
                return new Operand(Single.Parse(operandValue, NumberStyles.Float, CultureInfo.InvariantCulture));
            }
            if (operandType == BoolIdentifier)
            {
                return new Operand(Boolean.Parse(operandValue));
            }
            if (operandType == CharIdentifier)
            {
                return new Operand(operandValue[0]);
            }
            if (operandType == StringIdentifier)
            {
                return new Operand(operandValue);
            }

            throw new ArgumentOutOfRangeException("operandType");
        }

        private string GetOperandValueAsString(Operand operand)
        {
            switch (operand.Type)
            {
                case DataType.Int:
                    return operand.GetIntValue().ToString(CultureInfo.InvariantCulture);
                case DataType.String:
                    return operand.GetStringValue();
                case DataType.Char:
                    return operand.GetCharValue().ToString(CultureInfo.InvariantCulture);
                case DataType.Boolean:
                    return operand.GetBoolValue().ToString(CultureInfo.InvariantCulture);
                case DataType.Float:
                    return operand.GetFloatValue().ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException("operand");
            }
        }

        #endregion
    }
}
