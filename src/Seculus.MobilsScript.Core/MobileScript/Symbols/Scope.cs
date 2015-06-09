using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Escopo
    /// </summary>
    public class Scope
    {
        #region Fields

        /// <summary>
        /// Mapa com os símbolos locais a este escopo.
        /// </summary>
        private readonly IDictionary<string, Symbol> _symbolsMap;

        /// <summary>
        /// Função atual (associada a este escopo).
        /// </summary>
        private FunctionSymbol _currentFunction;

        /// <summary>
        /// Indica se o endereço dos símbolos está atualizado.
        /// </summary>
        private bool _symbolsAddressesUpToDate;

        /// <summary>
        /// Tamanho da área usada na pilha para as variáveis definidas neste escopo.
        /// </summary>
        private int _localVariablesSize;

        /// <summary>
        /// Tamanho da área de parâmetros (no caso de função)
        /// </summary>
        private int _parametersSize;

        #endregion

        #region Properties

        /// <summary>
        /// Escopo que contém este escopo
        /// </summary>
        public Scope UpperScope { get; private set; }

        /// <summary>
        /// Nível de encaixamento
        /// </summary>
        public int Level { get; private set; }

        public FunctionSymbol CurrentFunction
        {
            get
            {
                if (_currentFunction != null)
                {
                    return _currentFunction;
                }
                if (UpperScope != null)
                {
                    return UpperScope.CurrentFunction;
                }
                return null;
            }
            internal set
            {
                _currentFunction = value;
            }
        }

        /// <summary>
        /// Lista contendo os símbolos associados a este escopo.
        /// </summary>
        public ReadOnlyCollection<Symbol> Symbols
        {
            get
            {
                return new ReadOnlyCollection<Symbol>(_symbolsMap.Values.ToList());
            }
        }

        /// <summary>
        /// Tamanho da área usada na pilha para as variáveis definidas neste escopo.
        /// </summary>
        public int LocalVariablesSize
        {
            get
            {
                if (!_symbolsAddressesUpToDate)
                {
                    throw new CodeGenerationException("Total size not calculated! Call CalculateParametersAndVariablesAddressesAndSizes before!");
                }
                return _localVariablesSize;
            }
        }

        /// <summary>
        /// Tamanho da área de parâmetros (no caso de função).
        /// </summary>
        public int ParametersSize
        {
            get
            {
                if (!_symbolsAddressesUpToDate)
                {
                    throw new CodeGenerationException("Parameters size not calculated! Call CalculateParametersAndVariablesAddressesAndSizes before!");
                }
                return _parametersSize;
            }
        }

        #endregion

        #region Constructors

        public Scope(Scope upperScope)
        {
            UpperScope = upperScope;
            Level = ((UpperScope == null) ? 0 : UpperScope.Level + 1);
            _symbolsMap = new Dictionary<string, Symbol>();
            _currentFunction = null;
            _symbolsAddressesUpToDate = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Insere símbolo na tabela.
        /// </summary>
        /// <param name="symbol">Símbolo a ser inserido</param>
        /// <returns>True se inserção ocorrer com sucesso. False se já existe simbolo com o mesmo nome.</returns>
        public bool AddSymbol(Symbol symbol)
        {
            if (_symbolsMap.ContainsKey(symbol.Name))
            {
                return false;
            }

            _symbolsMap.Add(symbol.Name, symbol);
            _symbolsAddressesUpToDate = false;
            return true;
        }

        /// <summary>
        /// Procura por um símbolo com o nome indicado, devolvendo sua referência ou null caso não exista. 
        /// </summary>
        /// <param name="name">Nome do símbolo</param>
        /// <returns>Símbolo procurado ou null se não encontrado.</returns>
        public Symbol GetSymbol(string name)
        {
            Symbol symbol = null;
            _symbolsMap.TryGetValue(name, out symbol);
            return symbol;
        }

        /// <summary>
        /// Atualiza o endereço das variáveis e dos parâmetros (no caso de método) desse escopo. 
        /// Também atualiza o tamanho total do escopo e dos seus parâmetros.
        /// </summary>
        public void CalculateParametersAndVariablesAddressesAndSizes()
        {
            // Só calcula se ainda não tiver calculado.
            if (!_symbolsAddressesUpToDate)
            {
                _parametersSize = CalculateParametersAddressesAndGetTotalSize();
                _localVariablesSize = CalculateVariableAddressesAndGetTotalSize();
                _symbolsAddressesUpToDate = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calcula o tamanho da área ocupada pelos parâmetros da função associada a este escopo. 
        /// Esse valor é então usado para atualizar o endereço do valor de retorno da função e o endereço de cada um dos parâmetros.
        /// </summary>
        /// <returns>Tamanho da área ocupada pelos parâmetros da função associada a este escopo.</returns>
        private int CalculateParametersAddressesAndGetTotalSize()
        {
            // soma o tamanho dos parâmetros.
            int paramSize = Symbols.OfType<ParameterSymbol>().Sum(parameterSymbol => (parameterSymbol.ByRef) ? 1 : parameterSymbol.Type.TotalSize);

            // calcula o endereço de cada parâmetro.
            int addr = -paramSize - 2; // O -2 representa o _pc e o _base que são empilhados em toda chamada de função.

            foreach (Symbol symbol in Symbols)
            {
                var parameterSymbol = symbol as ParameterSymbol;
                if (parameterSymbol != null)
                {
                    addr += (parameterSymbol.ByRef) ? 1 : parameterSymbol.Type.TotalSize; // Passagem por referência ocupa sempre 1 na memória (endereço da variável).
                    parameterSymbol.Address = addr;
                }
            }

            return paramSize;
        }

        /// <summary>
        /// Calcula o tamanho da área ocupada pelas variáveis locais definidas neste escopo. 
        /// OBS: Ignora os parâmetros pois eles ficam armazenados "fora" da função (isso no nível do código da VM).
        /// </summary>
        /// <returns>Tamanho da área ocupada pelas variáveis locais.</returns>
        private int CalculateVariableAddressesAndGetTotalSize()
        {
            int totalSize = 0;

            foreach (var symbol in Symbols)
            {
                var variableSymbol = symbol as VariableSymbol;
                if (variableSymbol != null && !(variableSymbol is FunctionSymbol) && !(variableSymbol is ParameterSymbol))
                {
                    variableSymbol.Address = totalSize + 1; // a primeira variável local tem endereço 1
                    totalSize += variableSymbol.Type.TotalSize;
                }
            }

            return totalSize;
        }

        #endregion
    }
}
