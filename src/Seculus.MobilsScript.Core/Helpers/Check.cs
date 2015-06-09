using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Seculus.MobileScript.Core.Helpers
{
    /// <summary>
    /// Classe estática que faz verificações.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Classe estática que realiza verificações de argumentos (parâmetros de métodos)
        /// </summary>
        public static class Argument
        {
            /// <summary>
            /// Verifica se uma string tem um tamanho exato.
            /// Se não tiver, lança exceção.
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="expectedLength">Tamanho esperado para a string (parâmetro)</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro não tiver o tamanho exato esperado.</exception>
            [DebuggerStepThrough]
            public static void HasExactLength(string argument, string argumentName, int expectedLength)
            {
                if (argument == null || argument.Length != expectedLength)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "StringArgumentMustHaveExactLength");
                }
            }

            /// <summary>
            /// Verifica se um objeto não é nulo.
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <exception cref="ArgumentException">Se o parâmetro for nulo.</exception>
            [DebuggerStepThrough]
            public static void IsNotNull(object argument, string argumentName)
            {
                if (argument == null)
                {
                    throw new ArgumentException("ArgumentCannotBeNull", argumentName);
                }
            }

            /// <summary>
            /// Verifica se uma string é vazia ou nula. 
            /// Se for lança exceção.
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">nome do parâmetro</param>
            /// <exception cref="ArgumentException">Se o parâmetro realmente for vazio.</exception>
            [DebuggerStepThrough]
            public static void IsNotNullOrEmpty(string argument, string argumentName)
            {
                if (String.IsNullOrEmpty(argument))
                {
                    throw new ArgumentException("ArgumentCannotBeNullOrEmptyString", argumentName);
                }
            }

            /// <summary>
            /// Verifica se um array é vazio ou nulo. 
            /// Se for lança exceção.
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <exception cref="ArgumentException">Se o parâmetro realmente for vazio.</exception>
            [DebuggerStepThrough]
            public static void IsNotNullOrEmpty<T>(T[] argument, string argumentName)
            {
                if (argument == null || argument.Length <= 0)
                {
                    throw new ArgumentException("ArgumentCannotBeNullOrEmptyArray", argumentName);
                }
            }

            /// <summary>
            /// Verifica se uma coleção é vazia ou nula. 
            /// Se for lança exceção.
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">nome do parâmetro</param>
            /// <exception cref="ArgumentException">Se o parâmetro realmente for nulo ou vazio.</exception>
            [DebuggerStepThrough]
            public static void IsNotNullOrEmpty<T>(IList<T> argument, string argumentName)
            {
                if (argument == null || argument.Count <= 0)
                {
                    throw new ArgumentException("ArgumentCannotBeNullOrEmptyCollection", argumentName);
                }
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(byte argument, string argumentName, byte min, byte max)
            {
                IsInRange((long)argument, argumentName, (long)min, (long)max);
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(short argument, string argumentName, short min, short max)
            {
                IsInRange((long)argument, argumentName, (long)min, (long)max);
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(int argument, string argumentName, int min, int max)
            {
                IsInRange((long)argument, argumentName, (long)min, (long)max);
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(long argument, string argumentName, long min, long max)
            {
                if (argument < min || argument > max)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentMustBeInRange");
                }
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(float argument, string argumentName, float min, float max)
            {
                IsInRange((double)argument, argumentName, (double)min, (double)max);
            }

            /// <summary>
            /// Verifica se um numero está compreendido em um intervalo (inclusive, ou seja, os valores de min e max são considerados dentro do intervalo).
            /// </summary>
            /// <param name="argument">Parâmetro</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <param name="min">Valor mínimo permitido</param>
            /// <param name="max">Valor máximo permitido</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o parâmetro estiver fora do intervalo.</exception>
            [DebuggerStepThrough]
            public static void IsInRange(double argument, string argumentName, double min, double max)
            {
                if (argument < min || argument > max)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentMustBeInRange");
                }
            }

            /// <summary>
            /// Verifica se uma não data é antiga (menor que a data atual). Se for antiga, lança exceção.
            /// </summary>
            /// <param name="argument">Data</param>
            /// <param name="argumentName">Nome do parâmetro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se a data for antiga (menor que a data atual).</exception>
            [DebuggerStepThrough]
            public static void IsNotInPast(DateTime argument, string argumentName)
            {
                if (argument < DateTime.Now)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeInPast");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
            {
                if (argument <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegativeOrZero");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(byte argument, string argumentName)
            {
                IsNotNegativeOrZero((long) argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(short argument, string argumentName)
            {
                IsNotNegativeOrZero((long)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(int argument, string argumentName)
            {
                IsNotNegativeOrZero((long)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(long argument, string argumentName)
            {
                if (argument <= 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegativeOrZero");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(float argument, string argumentName)
            {
                IsNotNegativeOrZero((double)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo ou zero. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor ou igual a zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegativeOrZero(double argument, string argumentName)
            {
                if (argument <= 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegativeOrZero");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(TimeSpan argument, string argumentName)
            {
                if (argument < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegative");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(short argument, string argumentName)
            {
                IsNotNegative((long)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(int argument, string argumentName)
            {
                IsNotNegative((long)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(long argument, string argumentName)
            {
                if (argument < 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegative");
                }
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(float argument, string argumentName)
            {
                IsNotNegative((double)argument, argumentName);
            }

            /// <summary>
            /// Verifica se um valor não é negativo. Se for, lança exceção.
            /// </summary>
            /// <param name="argument">Valor</param>
            /// <param name="argumentName">Nome do parâmtro</param>
            /// <exception cref="ArgumentOutOfRangeException">Se o valor for menor do que zero.</exception>
            [DebuggerStepThrough]
            public static void IsNotNegative(double argument, string argumentName)
            {
                if (argument < 0)
                {
                    throw new ArgumentOutOfRangeException(argumentName, "ArgumentCannotBeNegative");
                }
            }

            public static void IsTrue<T>(bool condition) where T : Exception, new()
            {
                if (!condition)
                {
                    throw new T();
                }
            }

            /// <summary>
            /// Verifica se um arquivo existe. Se não exister, lança exceção.
            /// </summary>
            /// <param name="filePath">Parâmetro (caminho do arquivo).</param>
            /// <param name="argumentName">Nome do parâmetro.</param>
            /// <exception cref="ArgumentException">Se o arquivo não existir.</exception>
            [DebuggerStepThrough]
            public static void FileExists(string filePath, string argumentName)
            {
                if (!File.Exists(filePath))
                {
                    throw new ArgumentException("FileDoesNotExists");
                }
            }

            /// <summary>
            /// Verifica se um diretório existe. Se não exister, lança exceção.
            /// </summary>
            /// <param name="directoryPath">Parâmetro (caminho do diretório).</param>
            /// <param name="argumentName">Nome do parâmetro.</param>
            /// <exception cref="ArgumentException">Se o diretório não existir.</exception>
            [DebuggerStepThrough]
            public static void DirectoryExists(string directoryPath, string argumentName)
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw new ArgumentException("DirectoryDoesNotExists");
                }
            }
        }
    }
}
