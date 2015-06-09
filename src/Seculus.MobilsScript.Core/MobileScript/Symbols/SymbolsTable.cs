using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Tabela de símbolos
    /// </summary>
    public class SymbolsTable
    {
        #region Properties

        /// <summary>
        /// Escopo atual.
        /// </summary>
        public Scope CurrentScope { get; set; }

        /// <summary>
        /// Escopo global.
        /// </summary>
        public Scope GlobalScope { get; private set; }

        #endregion

        #region Constructors

        public SymbolsTable()
        {
            GlobalScope = new Scope(null); // Cria o escopo global
            CurrentScope = GlobalScope;   // o escopo atual é o escopo global.
            AddAllPreDefinedFunctions(); // Carrega a tabela de símbolos com todas as funções pré-definidas.
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Insere um símbolo na tabela.
        /// </summary>
        /// <param name="symbol">Símbolo</param>
        /// <returns>True se o símbolo for inserido com sucesso. False se o símbolo já estiver presente na tabela.</returns>
        public bool AddSymbol(Symbol symbol)
        {
            bool added = CurrentScope.AddSymbol(symbol);
            if (added)
            {
                var functionSymbol = symbol as FunctionSymbol;
                if ((functionSymbol != null) && !(functionSymbol is PreDefinedFunctionSymbol))
                {
                    EnterNewScope();
                    CurrentScope.CurrentFunction = functionSymbol;
                    functionSymbol.Scope = CurrentScope; // o escopo recém criado é associado à função sendo inserida na tabela
                }
            }

            return added;
        }

        /// <summary>
        /// Cria um novo escopo e o seta como escopo atual.
        /// </summary>
        public void EnterNewScope()
        {
            CurrentScope = new Scope(CurrentScope);
        }

        /// <summary>
        /// Fecha o escopo atual
        /// </summary>
        public void CloseCurrentScope()
        {
            if (CurrentScope != null)
            {
                CurrentScope = CurrentScope.UpperScope;
            }
        }

        /// <summary>
        /// Devolve o nível de encaixamento corrente.
        /// </summary>
        /// <returns>Nível de encaixamento atual.</returns>
        public int GetCurrentLevel()
        {
            return CurrentScope.Level;
        }

        /// <summary>
        /// Busca por um símbolo a partir do nome, no escopo atual.
        /// </summary>
        /// <param name="name">Nome do símbolo</param>
        /// <returns>Símbolo (ou null se não encontrar).</returns>
        public Symbol LocalSearch(string name)
        {
            return CurrentScope.GetSymbol(name);
        }

        /// <summary>
        /// Busca por um símbolo a partir do nome, a partir do escopo atual.
        /// </summary>
        /// <param name="name">Nome do símbolo.</param>
        /// <returns>Símbolo</returns>
        public Symbol Search(string name)
        {
            Scope scope = CurrentScope;
            while(scope != null)
            {
                Symbol symbol = scope.GetSymbol(name);
                if(symbol != null)
                {
                    return symbol;
                }
                scope = scope.UpperScope;
            }
            return null;
        }

        /// <summary>
        /// Devolve a função associada ao escopo atual (null se não existir uma função).
        /// </summary>
        /// <returns>Função associada ao escopo atual (null se não existir uma função)</returns>
        public FunctionSymbol GetCurrentFunction()
        {
            if (CurrentScope == null) throw new CompilationException("Unexpected null CurrentScope");
            return CurrentScope.CurrentFunction;
        }

        #endregion

        #region Private Methods

        private void AddAllPreDefinedFunctions()
        {
            IList<PreDefinedFunctionSymbol> preDefinedFunctions = PreDefinedFunctionSymbolList.Instance;

            foreach (var preDefinedFunctionSymbol in preDefinedFunctions)
            {
                AddSymbol(preDefinedFunctionSymbol);
            }
        }

        #endregion
    }
}
