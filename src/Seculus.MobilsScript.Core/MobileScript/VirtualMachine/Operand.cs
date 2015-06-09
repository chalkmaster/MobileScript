using System;
using System.Globalization;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Representa um operador de uma instrução.
    /// </summary>
    public struct Operand
    {
        #region Constructors

        public Operand(int value) : this()
        {
            Value = value;
            Type = DataType.Int;
        }

        public Operand(string value) : this()
        {
            Value = value ?? String.Empty;
            Type = DataType.String;
        }

        public Operand(char value) : this()
        {
            Value = value;
            Type = DataType.Char;
        }

        public Operand(bool value) : this()
        {
            Value = value;
            Type = DataType.Boolean;
        }

        public Operand(double value) : this()
        {
            Value = value;
            Type = DataType.Float;
        }

        #endregion

        #region Properties

        public DataType Type { get; private set; }

        public object Value { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Informa se esse operando é nulo.
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            return Type == DataType.Null || Value == null;
        }

        public int GetIntValue()
        {
            if (IsNull()) throw new NullOperandException();
            if (Type != DataType.Int) throw new InvalidOperandException();
            return (int) Value;
        }
        public string GetStringValue()
        {
            if (IsNull()) throw new NullOperandException();
            if (Type != DataType.String) throw new InvalidOperandException();
            return (string)Value;
        }
        public char GetCharValue()
        {
            if (IsNull()) throw new NullOperandException();
            if (Type != DataType.Char) throw new InvalidOperandException();
            return (char)Value;
        }
        public bool GetBoolValue()
        {
            if (IsNull()) throw new NullOperandException();
            if (Type != DataType.Boolean) throw new InvalidOperandException();
            return (bool)Value;
        }
        public double GetFloatValue()
        {
            if (IsNull()) throw new NullOperandException();
            if (Type != DataType.Float) throw new InvalidOperandException();
            return (double)Value;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case DataType.Int:
                    return Conventions.IntChar + ((int)Value).ToString(CultureInfo.InvariantCulture);
                case DataType.String:
                    return Conventions.StringChar + ((string)Value);
                case DataType.Char:
                    return Conventions.CharChar + ((char)Value).ToString(CultureInfo.InvariantCulture);
                case DataType.Boolean:
                    return Conventions.BooleanChar + ((bool)Value).ToString(CultureInfo.InvariantCulture).ToLower();
                case DataType.Float:
                    return Conventions.FloatChar + ((double)Value).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Monta um operando a partir da sua representação em string (já separada em partes).
        /// </summary>
        /// <param name="typeChar">Caracter que indica o tipo do operando</param>
        /// <param name="value">Valor do operando</param>
        /// <returns>Operando</returns>
        public static Operand Assemble(char typeChar, string value)
        {
            switch (typeChar)
            {
                case Conventions.BooleanChar:
                    return new Operand(Boolean.Parse(value));
                case Conventions.CharChar:
                    return new Operand(value[0]);
                case Conventions.FloatChar:
                    return new Operand(Double.Parse(value, CultureInfo.InvariantCulture));
                case Conventions.IntChar:
                    return new Operand(Int32.Parse(value, CultureInfo.InvariantCulture));
                case Conventions.StringChar:
                    return new Operand(value);
                default:
                    throw new ArgumentOutOfRangeException("typeChar");
            }
        }

        #endregion
    }
}
