using System.Collections.Generic;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Tabela de palavras reservadas
    /// </summary>
    public class ReservedWordsTable
    {
        #region Singleton

        private static readonly object SingletonSync = new object();
        private static volatile ReservedWordsTable _instance = null;

        public static ReservedWordsTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SingletonSync)
                    {
                        if (_instance == null)
                        {
                            _instance = new ReservedWordsTable();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constructors

        // Construtor privado porque essa classe é singleton
        private ReservedWordsTable()
        {
            _reservedWords = BuildReservedWordsTable();
        }

        #endregion

        #region Private Static Methods

        private static Dictionary<string, LexSymbolKind> BuildReservedWordsTable()
        {
            return new Dictionary<string, LexSymbolKind>()
                       {
                           { Keywords.Literals.True,    LexSymbolKind.TrueReservedWord },
                           { Keywords.Literals.False,   LexSymbolKind.FalseReservedWord },
                           { Keywords.Types.Int,        LexSymbolKind.IntReservedWord },
                           { Keywords.Types.Boolean,    LexSymbolKind.BooleanReservedWord },
                           { Keywords.Types.Float,      LexSymbolKind.FloatReservedWord },
                           { Keywords.Types.Void,       LexSymbolKind.VoidReservedWord },
                           { Keywords.Types.String,     LexSymbolKind.StringReservedWord },
                           { Keywords.Statements.If,    LexSymbolKind.IfReservedWord },
                           { Keywords.Statements.Else,  LexSymbolKind.ElseReservedWord },
                           { Keywords.Statements.While, LexSymbolKind.WhileReservedWord },
                           { Keywords.Statements.Return, LexSymbolKind.ReturnReservedWord },
                           { Keywords.Modifiers.Ref,    LexSymbolKind.RefReservedWord }
                       };
        }

        #endregion

        #region Fields

        private readonly Dictionary<string, LexSymbolKind> _reservedWords;

        #endregion

        #region Methods

        /// <summary>
        /// Busca por uma palavra reservada e retorna o tipo do seu símbolo. 
        /// Se não existir uma palavra reservada com esse nome, significa que é um identificador, então retorna "LexSymbolKind.Identifier".
        /// </summary>
        /// <param name="maybeReservedWord">Nome da "talvez palavra reservada" a ser procurada na tabela.</param>
        /// <returns>Tipo do símbolo da palavra reservada ou "Identifier"</returns>
        public LexSymbolKind GetSymbolKind(string maybeReservedWord)
        {
            if (_reservedWords.ContainsKey(maybeReservedWord))
            {
                return _reservedWords[maybeReservedWord];
            }

            return LexSymbolKind.Identifier;
        }

        #endregion
    }
}
