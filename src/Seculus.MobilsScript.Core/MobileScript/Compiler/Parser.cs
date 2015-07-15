using System.Diagnostics;
using System.Collections.Generic;
using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Analisador sintático.
    /// 
    /// Método de análise usado:
    ///   Descendente Recursivo (motivo: não depende de ferramentas externas e é mais que suficiente para este caso)
    /// </summary>
    public class Parser
    {
        #region Fields

        /// <summary>
        /// Último símbolo lido.
        /// </summary>
        private LexSymbol _lastSymbol;


        private static LexSymbolKind[] stStartSymbs = { 
                                                         LexSymbolKind.Identifier, 
                                                         LexSymbolKind.IfReservedWord, 
                                                         LexSymbolKind.WhileReservedWord,
                                                         LexSymbolKind.ReturnReservedWord,
                                                         LexSymbolKind.LeftBrace,
                                                         LexSymbolKind.SemiColon
                                                      };

        /// <summary>
        /// Conjunto de símbolos que podem iniciar um comando
        /// </summary>
        private static ISet<LexSymbolKind> statStartSet = new HashSet<LexSymbolKind>(stStartSymbs);

        private static LexSymbolKind[] prTypeSymbs = {
                                                        LexSymbolKind.IntReservedWord,
                                                        LexSymbolKind.FloatReservedWord,
                                                        LexSymbolKind.BooleanReservedWord,
                                                        LexSymbolKind.StringReservedWord,
                                                        LexSymbolKind.VoidReservedWord
                                                     };


        /// <summary>
        /// Conjunto de símbolos que definem um tipo primitivo
        /// </summary>
        private static ISet<LexSymbolKind> prTypeSymbSet = new HashSet<LexSymbolKind>(prTypeSymbs);


        private static LexSymbolKind[] relOpSymbs = {
                                                    LexSymbolKind.EqualOperator,
                                                    LexSymbolKind.NotEqualOperator,
                                                    LexSymbolKind.LessThanOperator,
                                                    LexSymbolKind.GreaterThanOperator,        
                                                    LexSymbolKind.LessOrEqualOperator,
                                                    LexSymbolKind.GreaterOrEqualOperator
                                                  };

        /// <summary>
        /// Conjunto de símbolos associados às operações relacionais
        /// </summary>
        private static ISet<LexSymbolKind> relOpSymbSet = new HashSet<LexSymbolKind>(relOpSymbs);

        private static LexSymbolKind[] factorStartSymbs = { 
                                                            LexSymbolKind.Identifier,
                                                            LexSymbolKind.SubtractOperator,
                                                            LexSymbolKind.IntConstant,
                                                            LexSymbolKind.FloatConstant,
                                                            LexSymbolKind.TrueReservedWord,
                                                            LexSymbolKind.FalseReservedWord,
                                                            LexSymbolKind.StringConstant,
                                                            LexSymbolKind.LeftParentheses
                                                          };

        /// <summary>
        /// Conjunto de símbolos que podem iniciar um fator (método Factor())
        /// </summary>
        private static ISet<LexSymbolKind> factorStartSymbSet = new HashSet<LexSymbolKind>(factorStartSymbs);

        private static LexSymbolKind[] mulOpSymbs = {
                                                        LexSymbolKind.MultiplyOperator,
                                                        LexSymbolKind.DivideOperator,
                                                        LexSymbolKind.ModuleOperator
                                                    };

        /// <summary>
        /// Conjunto de 'operadores de multiplicação'
        /// </summary>
        private static ISet<LexSymbolKind> mulOpSymbSet = new HashSet<LexSymbolKind>(mulOpSymbs);

        private static LexSymbolKind[] eofSymbs = { LexSymbolKind.EndOfFile };

        public static ISet<LexSymbolKind> eofSymbSet = new HashSet<LexSymbolKind>(eofSymbs);

        private static ISet<LexSymbolKind> emptySet = new HashSet<LexSymbolKind>();

        /// <summary>
        /// Conj. contendo apenas ';' (a ser usado p/ TestNextSymbol)
        /// </summary>
        private static LexSymbolKind[] semicolonSymbs = { LexSymbolKind.SemiColon };
        private static ISet<LexSymbolKind> semicolonSymbSet = new HashSet<LexSymbolKind>(semicolonSymbs);

        /// <summary>
        /// Conj. contendo apenas '(' (a ser usado p/ TestNextSymbol)
        /// </summary>
        private static LexSymbolKind[] leftParSymbs = { LexSymbolKind.LeftParentheses };
        private static ISet<LexSymbolKind> leftParSymbSet = new HashSet<LexSymbolKind>(leftParSymbs);

        /// <summary>
        /// Conj. contendo apenas ')' (a ser usado p/ TestNextSymbol)
        /// </summary>
        private static LexSymbolKind[] rightParSymbs =  {  LexSymbolKind.RightParentheses };
        private static ISet<LexSymbolKind> rightParSymbSet = new HashSet<LexSymbolKind>(rightParSymbs);

        private static LexSymbolKind[] rightBraceSymbs = { LexSymbolKind.RightBrace };
        private static ISet<LexSymbolKind> rightBraceSymbSet = new HashSet<LexSymbolKind>(rightBraceSymbs);

        private static LexSymbolKind[] leftBraceSymbs = { LexSymbolKind.LeftBrace };
        private static ISet<LexSymbolKind> leftBraceSymbSet = new HashSet<LexSymbolKind>(leftBraceSymbs);

        /// <summary>
        /// Limite para o número de erros sintáticos
        /// </summary>
        const int  _errorLimit = 128;

         #endregion

        #region Properties

        /// <summary>
        /// Lista contendo os eventuais erros sintáticos encontrados.
        /// </summary>
        public IList<CompilationError> Errors { get; private set; }

        #endregion

        #region Constructors

        public Parser()
        {
            Errors = new List<CompilationError>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Inicia o analizador sintático.
        /// </summary>
        /// <param name="file">Arquivo de entrada</param>
        public void Init(MobileScriptReader file)
        {
            Lex.Instance.Init(file);
            ReadNextSymbol();
        }

        /// <summary>
        /// Reconhece um programa. 
        /// program => declarations compoundStat
        /// </summary>
        /// <returns>Descrição do programa.</returns>
        public ProgramDescription ParseFullProgram(ISet<LexSymbolKind> followSymbols)
        {
            var program = new ProgramDescription(_lastSymbol);

            // Reconhece as declarações.
            while (IsPrimitiveType(_lastSymbol.Kind))
            {
                Declaration declaration = GetFunctionDeclarationOrVariableDeclarationList(followSymbols);
                if (declaration != null)
                {
                    program.Declarations.Add(declaration);
                }
            }

            // Reconhece o corpo do programa.
            CompoundStatement body = (_lastSymbol.Kind == LexSymbolKind.LeftBrace) ? GetFunctionBody(followSymbols) : null;
            if (body != null)
            {
                program.Body = body;
            }

            // Trata o final do arquivo.
            if ((_lastSymbol != null) && (_lastSymbol.Kind != LexSymbolKind.EndOfFile))
            {
                EmitUnexpectedSymbolError("end of file", _lastSymbol);
            }

            return program;
        }

        /// <summary>
        /// Reconhece uma sequência de definições de funções (ex. biblioteca).
        /// </summary>
        /// <returns>Lista de declaração de funções.</returns>
        public IList<FunctionDeclaration> ParseLibrary(ISet<LexSymbolKind> followSymbs)
        {
            var functionsList = new List<FunctionDeclaration>();

            followSymbs = setUnion(prTypeSymbSet, followSymbs);

            while (IsPrimitiveType(_lastSymbol.Kind))
            {
                TypeDeclaration type = GetPrimitiveType();

                if (_lastSymbol.Kind != LexSymbolKind.Identifier)
                {
                    EmitUnexpectedSymbolError("identifier", _lastSymbol);
                }
                string name = _lastSymbol.Text;

                ReadNextSymbol(); // Pula o 'identificador'

                if (_lastSymbol.Kind == LexSymbolKind.LeftParentheses)
                {
                    FunctionDeclaration function = GetFunctionDeclaration(type, name, followSymbs);
                    functionsList.Add(function);
                }
            }

            // Trata o final do arquivo.
            if ((_lastSymbol != null) && (_lastSymbol.Kind != LexSymbolKind.EndOfFile))
            {
                EmitUnexpectedSymbolError("end of file", _lastSymbol);
            }

            return functionsList;
        }

        /// <summary>
        /// Reconhece uma sequência de declarações (variáveis globais ou funções), relativas ao código associado ao fluxo.
        /// </summary>
        /// <returns>Objeto do tipo ProgramDescription contendo a representação das declarações encontradas.</returns>
        public ProgramDescription ParseGlobalDeclarations(ISet<LexSymbolKind> followSymbols)
        {
            var program = new ProgramDescription(_lastSymbol);

            // Reconhece as declarações.
            while (IsPrimitiveType(_lastSymbol.Kind))
            {
                Declaration declaration = GetFunctionDeclarationOrVariableDeclarationList(followSymbols);
                if (declaration != null)
                {
                    program.Declarations.Add(declaration);
                }
            }

            // Trata o final do arquivo.
            if ((_lastSymbol != null) && (_lastSymbol.Kind != LexSymbolKind.EndOfFile))
            {
                EmitUnexpectedSymbolError("end of file", _lastSymbol);
            }

            return program;
        }

        /// <summary>
        /// Reconhece uma sequência de ações (comandos). 
        /// Ex: Script direto, sem função envolvendo.
        /// </summary>
        /// <returns>Objeto do tipo ProgramDescription contendo a representação dos comandos encontrados.</returns>
        public ProgramDescription ParseStatements(ISet<LexSymbolKind> followSymbols)
        {
            var program = new ProgramDescription(_lastSymbol)
                              {
                                  Body = new CompoundStatement(_lastSymbol)
                              };


            TestNextSymbol(prTypeSymbSet, setUnion(followSymbols, statStartSet), "int, float, boolean, string, void", false);
            // Reconhece declarações
            while (IsPrimitiveType(_lastSymbol.Kind))
            {
                VariableDeclarationList variableList = GetVariableDeclarationList(followSymbols);
                if (variableList != null)
                {
                    program.Body.Declarations.Add(variableList);
                }
                TestNextSymbol(setUnion(prTypeSymbSet, statStartSet), followSymbols, "int, float, boolean, string, void", false);
            }

            // Reconhece os comandos
            while (IsStatementStart(_lastSymbol.Kind))
            {
                Statement statement = GetStatement(followSymbols);
                if (statement != null)
                {
                    program.Body.Statements.Add(statement);
                }
                TestNextSymbol(statStartSet, addToSet(followSymbols, LexSymbolKind.EndOfFile), "identifier, if, while, { or return", false);

            }

            // Trata o final do arquivo.
            if ((_lastSymbol != null) && (_lastSymbol.Kind != LexSymbolKind.EndOfFile))
            {
                EmitUnexpectedSymbolError("end of file", _lastSymbol);
            }

            return program;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Lê o próximo símbolo da entrada
        /// </summary>
        private void ReadNextSymbol()
        {
            _lastSymbol = Lex.Instance.GetNextSymbol();
        }

        /// <summary>
        /// Verifica se o último símbolo é um símbolo esperado:
        /// se igual, le o próximo símbolo e se diferente gera mensagem
        /// </summary>
        /// <param name="expectedSymbolKind">Tipo do símbolo esperado.</param>
        /// <param name="expectedSymbolName">Nome do símbolo esperado.</param>
        private void CheckLastSymbolKindAndReadNext(LexSymbolKind expectedSymbolKind, string expectedSymbolName)
        {
            if (_lastSymbol.Kind != expectedSymbolKind)
            {
                EmitUnexpectedSymbolError(expectedSymbolName, _lastSymbol);
            }
            else
            {
                ReadNextSymbol();
            }
        }

        /// <summary>
        /// Verifica se o último símbolo lido é um dos símbolos esperados. Caso não seja,
        /// gera a mensagem de erro indicada por errorMsg e ignora os símbolos de entrada
        /// até encontrar um símbolo esperado ou um símbolo no conjunto followSymbols.
        /// Este método é a base para o esquema de recuperação de erros sintáticos:
        ///     caso detecte algum erro, procura por um ponto confiável para continuar a análise sintática.
        ///     Deve ser aplicado em pontos específicos da análise sintática. Neste caso, considerando as
        ///     dependências entre os símbolos não terminais da gramática (que na implementação viram métodos
        ///     desta classe), esses pontos estão em GetFactor(), em GetStatement() e derivados, e no método responsável
        ///     pelo tratamento do 'corpo' de uma função -- essa regra é passível de ajustes pontuais em função
        ///     dos resultados obtidos [o processo é 'ad hoc', sorry ].
        /// </summary>
        /// <param name="expectedSymbols"></param>
        /// <param name="followSymbols"></param>
        private bool TestNextSymbol(ISet<LexSymbolKind> expectedSymbols, ISet<LexSymbolKind> followSymbols, string errorMsg, bool readNext)
        {
            if( !expectedSymbols.Contains(_lastSymbol.Kind) ) {
                EmitUnexpectedSymbolError(errorMsg, _lastSymbol, false);
                followSymbols = setUnion(expectedSymbols, followSymbols);
                while (
                        (_lastSymbol != null) &&
                        (_lastSymbol.Kind != LexSymbolKind.EndOfFile) &&
                        ! followSymbols.Contains(_lastSymbol.Kind)
                      )
                {
                    ReadNextSymbol();
                }
                return false;
            }
            if(readNext) ReadNextSymbol();
            return true;
        }



        /// <summary>
        /// Insere uma mensagem de erro na lista.
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        /// <param name="symbol">Símbolo encontrado (diferente do esperado).</param>
        private void EmitError(string message, LexSymbol symbol)
        {
            if( Errors.Count <= _errorLimit ) Errors.Add(new CompilationError(message, CompilationErrorType.SyntaxError, symbol));
            ReadNextSymbol();  
        }

        private void EmitUnexpectedSymbolError(string expected, LexSymbol symbolFound)
        {
            EmitError("Expected [{0}] but found [{1}]".FormatWith(expected, symbolFound.Text), symbolFound);
           
        }

        /// <summary>
        /// Versão da função anterior que não lê o próximo símbolo.
        /// (usada por testNextSymbol())
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="symbolFound"></param>
        /// <param name="dummy"></param>
        private void EmitUnexpectedSymbolError(string expected, LexSymbol symbolFound, bool dummy)
        {
            if (Errors.Count <= _errorLimit) Errors.Add(new CompilationError("Expected [{0}] but found [{1}]".FormatWith(expected, symbolFound.Text), CompilationErrorType.SyntaxError, symbolFound));
        }

        

        /// <summary>
        /// Converte um código de operação criado pelo léxico para o equivalente na árvore de programa (pura burocracia!)
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        private RplOperationType GetRplOperation(LexSymbolKind kind)
        {
            switch (kind)
            {
                case LexSymbolKind.LessThanOperator:
                    return RplOperationType.LessThan;

                case LexSymbolKind.GreaterThanOperator:
                    return RplOperationType.GreaterThan;

                case LexSymbolKind.AddOperator:
                    return RplOperationType.Add;

                case LexSymbolKind.SubtractOperator:
                    return RplOperationType.Subtract;

                case LexSymbolKind.MultiplyOperator:
                    return RplOperationType.Multiply;

                case LexSymbolKind.DivideOperator:
                    return RplOperationType.Divide;

                case LexSymbolKind.ModuleOperator:
                    return RplOperationType.Module;

                case LexSymbolKind.EqualOperator:
                    return RplOperationType.Equal;

                case LexSymbolKind.NotEqualOperator:
                    return RplOperationType.NotEqual;

                case LexSymbolKind.LessOrEqualOperator:
                    return RplOperationType.LessOrEqual;

                case LexSymbolKind.GreaterOrEqualOperator:
                    return RplOperationType.GreaterOrEqual;

                case LexSymbolKind.OrOperator:
                    return RplOperationType.Or;

                case LexSymbolKind.AndOperator:
                    return RplOperationType.And;

                default:
                    throw new ParsingException("Unexpected symbol kind.");
            }
        }

        /// <summary>
        /// Identifica se o tipo de símbolo informado é um tipo referente a um operador relacional. 
        /// ('==', '!=', '&lt;', '&gt;', '&lt;=', '&gt;=').
        /// </summary>
        /// <param name="symbolKind">Tipo do símbolo</param>
        /// <returns>True se for um operador relacional. Caso contrário, false.</returns>
        private bool IsRelationalOperator(LexSymbolKind symbolKind)
        {
            return relOpSymbSet.Contains(symbolKind);
        }


        /// <summary>
        /// Retorna um novo conjunto de símbolos cujo conteúdo é a união dos dois conjuntos passados como parâmetro.
        /// (usado nos métodos de análise sintática)
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private ISet<LexSymbolKind> setUnion(ISet<LexSymbolKind> s1, ISet<LexSymbolKind> s2)
        {
            ISet<LexSymbolKind> res = new HashSet<LexSymbolKind>(s1);
            res.UnionWith(s2);
            return res;
        }

        private ISet<LexSymbolKind> setUnion(ISet<LexSymbolKind> s1, ISet<LexSymbolKind> s2, ISet<LexSymbolKind> s3)
        {
            ISet<LexSymbolKind> res = new HashSet<LexSymbolKind>(s1);
            res.UnionWith(s2);
            res.UnionWith(s3);
            return res;
        }

        private ISet<LexSymbolKind> addToSet(ISet<LexSymbolKind> s, LexSymbolKind symb)
        {
            ISet<LexSymbolKind> res = new HashSet<LexSymbolKind>(s);
            res.Add(symb);
            return res;
        }

        private ISet<LexSymbolKind> addToSet(ISet<LexSymbolKind> s, LexSymbolKind symb1, LexSymbolKind symb2)
        {
            ISet<LexSymbolKind> res = new HashSet<LexSymbolKind>(s);
            res.Add(symb1);
            res.Add(symb2);
            return res;
        }

        private ISet<LexSymbolKind> newSymbolSet(LexSymbolKind symb)
        {
            ISet<LexSymbolKind> res = new HashSet<LexSymbolKind>();
            res.Add(symb);
            return res;
        }

        /// <summary>
        /// Identifica se o tipo de símbolo informado inicia um comando. 
        /// (identifier, 'if', 'while', '{', 'return', ';').
        /// </summary>
        /// <param name="symbolKind">Tipo do símbolo</param>
        /// <returns>True se for um operador relacional. Caso contrário, false.</returns>
        private bool IsStatementStart(LexSymbolKind symbolKind)
        {
            return statStartSet.Contains(symbolKind);          
        }

        /// <summary>
        /// Identifica se o tipo de símbolo informado é o nome de um tipo primitivo. 
        /// (int, float, boolean, string, void).
        /// </summary>
        /// <param name="symbolKind"></param>
        /// <returns></returns>
        private bool IsPrimitiveType(LexSymbolKind symbolKind)
        {
            return prTypeSymbSet.Contains(symbolKind);
        }

        #endregion

        #region Private Methods - Recognizes Declarations

        /// <summary>
        /// Reconhece a declaração dos parâmetros de uma função.
        /// GetParameterDeclaration =>  [ 'ref' ] primType ident { '[' int ']' }
        /// </summary>
        /// <returns>VariableDeclaration</returns>
        private VariableDeclaration GetParameterDeclaration()
        {
            // Verifica se é referência
            bool byRef = false;
            if (_lastSymbol.Kind == LexSymbolKind.RefReservedWord)
            {
                byRef = true;
                ReadNextSymbol();
            }

            // Pega o tipo do parâmetro
            TypeDeclaration type = GetPrimitiveType();

            // Pega o nome da variável
            string name = _lastSymbol.Text;
            LexSymbol symbol = _lastSymbol;
            CheckLastSymbolKindAndReadNext(LexSymbolKind.Identifier, "identifier");

            type = GetCorrectType(type);
            return new VariableDeclaration(name, type, null, byRef, symbol);
        }

        /// <summary>
        /// Reconhece uma lista de parâmetros numa declaração de função. 
        /// parametersList => [ parameterDeclaration { ',' parameterDeclaration } ] ')'
        /// </summary>
        /// <returns></returns>
        private IList<VariableDeclaration> GetParameterDeclarationList()
        {
            var parametersList = new List<VariableDeclaration>();

            while (_lastSymbol.Kind != LexSymbolKind.RightParentheses)
            {
                parametersList.Add(GetParameterDeclaration());
                if ((_lastSymbol.Kind != LexSymbolKind.Comma) && (_lastSymbol.Kind != LexSymbolKind.RightParentheses))
                {
                    EmitUnexpectedSymbolError(", or )", _lastSymbol);
                }

                // Se o próximo símbolo for ',', pula.
                if (_lastSymbol.Kind == LexSymbolKind.Comma)
                {
                    ReadNextSymbol();
                }
            }

            CheckLastSymbolKindAndReadNext(LexSymbolKind.RightParentheses, ")");
            return parametersList;
        }

        /// <summary>
        /// Reconhece uma declaração de variável da qual já conhecemos o tipo "inicial". 
        /// (Digo tipo inicial pois podemos descobrir que na verdade é um vetor). 
        /// GetVariableDeclaration => ident { '[' int ']' } startValue
        /// </summary>
        /// <param name="type">Tipo "inicial" da variável. (Digo tipo inicial pois podemos descobrir que na verdade é um vetor).</param>
        /// <returns>VariableDeclaration</returns>
        private VariableDeclaration GetVariableDeclaration(TypeDeclaration type, ISet<LexSymbolKind> followSymbols)
        {
            if (_lastSymbol.Kind == LexSymbolKind.Identifier)
            {
                string name = _lastSymbol.Text;
                ReadNextSymbol(); // Pula o identifier. Já pegamos o nome dele acima.
                return GetVariableDeclaration(type, name, followSymbols);
            }
            else
            {
                EmitUnexpectedSymbolError("identifier", _lastSymbol);
                ReadNextSymbol(); // Pula o símbolo "defeituoso".
                return null;
            }
        }

        /// <summary>
        /// Reconhece uma declaração de variável (após o reconhecimento do tipo e do nome) 
        /// e depois de se certificar que não é uma declaração de função.
        /// </summary>
        /// <param name="type">Tipo já reconhecido</param>
        /// <param name="name">Nome já reconhecido</param>
        /// <returns>VariableDeclaration</returns>
        private VariableDeclaration GetVariableDeclaration(TypeDeclaration type, string name, ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol = _lastSymbol;

            type = GetCorrectType(type);
            Expression initialValue = GetVariableInitialValue(followSymbols);

            // Valida o caso de array sem tamanho.
            var vectorType = type as VectorTypeDeclaration;
            if (vectorType != null)
            {
                // Se o tamanho for NULL, significa que o cara não colocou tamanho no vetor. 
                // nesse caso é obrigatório que o cara tenha informado um valor inicial.
                if (!vectorType.Size.HasValue)
                {
                    var tupleConstant = initialValue as TupleConstant;
                    if (tupleConstant != null)
                    {
                        vectorType.Size = tupleConstant.Elements.Count;
                    }
                    else
                    {
                        EmitError("Vectors with no fixed size must have an initial value (values between squares).", _lastSymbol);
                    }
                }
            }

            return new VariableDeclaration(name, type, initialValue, symbol);
        }

        /// <summary>
        /// Reconhece uma lista de declarações de variáveis, tendo reconhecido o tipo e o nome 
        /// (e após saber que não se trata de uma declaração de função).
        /// </summary>
        /// <param name="type">Tipo da variável.</param>
        /// <param name="name">Nome da variável.</param>
        /// <returns></returns>
        private VariableDeclarationList GetVariableDeclarationList(TypeDeclaration type, string name, ISet<LexSymbolKind> followSymbols)
        {
            // Cria a lista e adiciona a primeira variável.
            var variableList = new VariableDeclarationList(type, _lastSymbol);

            followSymbols = addToSet(followSymbols, LexSymbolKind.Comma, LexSymbolKind.SemiColon);

            VariableDeclaration variable = GetVariableDeclaration(type, name, followSymbols);
            if (variable != null)
            {
                variableList.Add(variable);
            }

            // O próximo símbolo tem que ser uma vírgula (para lista) ou um ponto-e-vírgula (para uma única declaração).
            if ((_lastSymbol.Kind != LexSymbolKind.Comma) && (_lastSymbol.Kind != LexSymbolKind.SemiColon))
            {
                EmitUnexpectedSymbolError(", or ;", _lastSymbol);
            }

            while ((_lastSymbol.Kind != LexSymbolKind.SemiColon) && (_lastSymbol.Kind != LexSymbolKind.EndOfFile) && (_lastSymbol.Kind != LexSymbolKind.Error))
            {
                // Se for vírgula, pula.
                if (_lastSymbol.Kind == LexSymbolKind.Comma)
                {
                    ReadNextSymbol();
                }

                variable = GetVariableDeclaration(type, followSymbols); // Como é uma lista, o tipo já foi identificado lá atrás.
                if (variable != null)
                {
                    variableList.Add(variable);
                }
            }

            // Se for ';' o comando acabou com sucesso!
            if (_lastSymbol.Kind == LexSymbolKind.SemiColon)
            {
                ReadNextSymbol(); // Pula o ';'
                return variableList;
            }

            EmitUnexpectedSymbolError(";", _lastSymbol);
            return null;
        }

        /// <summary>
        /// Reconhece uma lista de declaração de variáveis (ex int a, b = 1, c[2] = {1,2}; ) 
        /// variableDeclaration => type variableList ';' 
        /// variableList => variableDeclaration { ',' variableDeclaration }
        /// </summary>
        /// <returns></returns>
        private VariableDeclarationList GetVariableDeclarationList(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(IsPrimitiveType(_lastSymbol.Kind), "Expecting a primitive type symbol but got {0}.".FormatWith(_lastSymbol));
            // Pega o tipo da declaração de variável(eis).
            TypeDeclaration type = GetPrimitiveType();

            // O próximo símbolo tem que ser um identificador (nome da variável).
            if (_lastSymbol.Kind != LexSymbolKind.Identifier)
            {
                EmitUnexpectedSymbolError("identifier", _lastSymbol);
            }

            string variableName = _lastSymbol.Text;
            ReadNextSymbol(); // Pula o identificador.

            return GetVariableDeclarationList(type, variableName, followSymbols);
        }

        /// <summary>
        /// Verifica se o tipo da variável é primitivo ou vetor.
        /// </summary>
        /// <param name="maybeArray">Tipo primitivo, que talvez possa ser array (se tiver '[' na frente).</param>
        /// <returns>O tipo correto</returns>
        private TypeDeclaration GetCorrectType(TypeDeclaration maybeArray)
        {
            if (_lastSymbol.Kind != LexSymbolKind.LeftSquareBracket)
            {
                return maybeArray; // é um tipo primitivo
            }

            return GetVectorType(maybeArray);
        }

        /// <summary>
        /// Retorna uma declaração de vetor.
        /// </summary>
        /// <param name="arrayItensType">Tipo dos itens do vetor.</param>
        /// <returns>Declaração de vetor.</returns>
        private VectorTypeDeclaration GetVectorType(TypeDeclaration arrayItensType)
        {
            LexSymbol symbol = _lastSymbol;
            ReadNextSymbol(); // pula o '['

            //int idx = -1;
            //if (_lastSymbol.Kind != LexSymbolKind.IntConstant)
            //{
            //    EmitUnexpectedSymbolError("integer constant", _lastSymbol);
            //}
            //else
            //{
            //    idx = _lastSymbol.GetIntValue();
            //}
            int? idx = null;
            if (_lastSymbol.Kind == LexSymbolKind.IntConstant)
            {
                idx = _lastSymbol.GetIntValue();
                ReadNextSymbol(); // pula o int
            }

            CheckLastSymbolKindAndReadNext(LexSymbolKind.RightSquareBracket, "]");
            return new VectorTypeDeclaration(idx, GetCorrectType(arrayItensType), symbol);
        }

        /// <summary>
        /// Reconhece um tipo primitivo:
        /// GetPrimitiveType => 'int' | 'boolean' | 'string' | 'float' | 'void'
        /// </summary>
        /// <returns>TypeDeclaration</returns>
        private TypeDeclaration GetPrimitiveType()
        {
            LexSymbolKind kind = _lastSymbol.Kind;
            // Guardo o último símbolo porque vou fazer um ReadNextSymbol antes de fazer o return.
            LexSymbol symbol = _lastSymbol;

            ReadNextSymbol(); // Lê o próximo antes de retornar.

            switch (kind)
            {
                case LexSymbolKind.IntReservedWord:
                    return TypeDeclaration.Int;
                case LexSymbolKind.FloatReservedWord:
                    return TypeDeclaration.Float;
                case LexSymbolKind.BooleanReservedWord:
                    return TypeDeclaration.Bool;
                case LexSymbolKind.StringReservedWord:
                    return TypeDeclaration.String;
                case LexSymbolKind.VoidReservedWord:
                    return TypeDeclaration.Void;
                default:
                    return new TypeDeclaration(RplConstants.WrongTypeName, symbol); // WRONG TYPE
            }
        }

        /// <summary>
        /// Reconhece uma declaração de função (após ler o tipo, nome e '(' ). 
        /// functionDeclaration => type name '(' [ parmList ] ')' compoundStat
        /// </summary>
        /// <param name="type">Tipo de retorno da função.</param>
        /// <param name="name">Nome da função.</param>
        /// <returns></returns>
        private FunctionDeclaration GetFunctionDeclaration(TypeDeclaration type, string name, ISet<LexSymbolKind> followSymbs)
        {
            CheckLastSymbolKindAndReadNext(LexSymbolKind.LeftParentheses, "(");

            LexSymbol symbol = _lastSymbol;

            // Reconhece os parâmetros da função (caso existam).
            IList<VariableDeclaration> parameters = GetParameterDeclarationList();

            // Reconhece o corpo da função.
            CompoundStatement body = GetFunctionBody(followSymbs);

            return new FunctionDeclaration(name, type, parameters, body, symbol);
        }

        /// <summary>
        /// Reconhece uma sequência de declarações de variáveis ou de uma função. 
        /// functionDeclarationOrVariableDeclarationList => type identifier '(' functionDeclaration | type identifier variableDeclarationList
        /// </summary>
        /// <returns></returns>
        private Declaration GetFunctionDeclarationOrVariableDeclarationList(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(IsPrimitiveType(_lastSymbol.Kind), "Expecting a primitive type symbol but got {0}.".FormatWith(_lastSymbol));
            // Pega o tipo da função ou variável(eis).
            TypeDeclaration type = GetPrimitiveType();

            if (_lastSymbol.Kind != LexSymbolKind.Identifier)
            {
                EmitUnexpectedSymbolError("identifier", _lastSymbol);
            }

            // Pega o nome da função ou variável (ou primeira variável de uma lista).
            string name = _lastSymbol.Text;

            // Pula o identificador
            ReadNextSymbol();

            // Se tiver um parenteses depois, é uma função.
            if (_lastSymbol.Kind == LexSymbolKind.LeftParentheses)
            {
                return GetFunctionDeclaration(type, name, followSymbols);
            }

            // Caso contrário, é uma declaração de variável.
            return GetVariableDeclarationList(type, name, followSymbols);
        }

        #endregion

        #region Prime Methods - Recognizes Expressions

        /// <summary>
        /// Reconhece uma constante: constante simples ou tupla.
        /// constant => intConst | boolConst | stringConst | floatConst | tupleConst
        /// </summary>
        /// <returns>Constante</returns>
        private Expression GetConstant()
        {
            LexSymbolKind kind = _lastSymbol.Kind;

            // Se não for "{" é porque não é tupla.
            if (kind != LexSymbolKind.LeftBrace)
            {
                return GetSimpleConstant();
            }

            return GetTuple();
        }

        /// <summary>
        /// Reconhece uma tupla.
        /// </summary>
        /// <returns>Tupla ou null caso haja algum símbolo não esperado.</returns>
        private TupleConstant GetTuple()
        {
            LexSymbolKind kind = _lastSymbol.Kind;

            bool hasError = false;
            var tuple = new TupleConstant(new List<Expression>(), _lastSymbol);

            ReadNextSymbol(); // pula '{', que representa o início da tupla.

            while ((kind != LexSymbolKind.RightBrace) && !hasError)
            {
                Constant constant = GetSimpleConstant();
                if (constant != null)
                {
                    tuple.Elements.Add(constant);
                    if (_lastSymbol.Kind == LexSymbolKind.Comma)
                    {
                        ReadNextSymbol(); // pula a vírgula para ler o próximo item da tupla.
                    }
                    kind = _lastSymbol.Kind;
                }
                else
                {
                    hasError = true;
                }
            }

            if (!hasError)
            {
                ReadNextSymbol();
                return tuple;
            }

            EmitUnexpectedSymbolError("constant value or }", _lastSymbol);
            return null;
        }

        /// <summary>
        /// Reconhece uma constante simples (que não é uma tupla).
        /// </summary>
        /// <returns>Constante ou null se o símbolo encontrado não referente a uma constante.</returns>
        private Constant GetSimpleConstant()
        {
            LexSymbolKind kind = _lastSymbol.Kind;
            string txt = _lastSymbol.Text;
            LexSymbol symbol = _lastSymbol;

            ReadNextSymbol(); // Já lê o próximo símbolo antes de retornar a função.

            if (kind == LexSymbolKind.IntConstant)
            {
                return new Constant(txt, TypeDeclaration.Int, symbol);
            }
            if (kind == LexSymbolKind.TrueReservedWord)
            {
                return new Constant(Keywords.Literals.True, TypeDeclaration.Bool, symbol);
            }
            if (kind == LexSymbolKind.FalseReservedWord)
            {
                return new Constant(Keywords.Literals.False, TypeDeclaration.Bool, symbol);
            }
            if (kind == LexSymbolKind.StringConstant)
            {
                return new Constant(txt, TypeDeclaration.String, symbol);
            }
            if (kind == LexSymbolKind.FloatConstant)
            {
                return new Constant(txt, TypeDeclaration.Float, symbol);
            }

            EmitUnexpectedSymbolError("constant value", symbol);
            return null;
        }

        /// <summary>
        /// Reconhece o valor inicial da variável numa declaração.
        /// startValue => [ '=' constant ]
        /// </summary>
        /// <returns></returns>
        private Expression GetVariableInitialValue(ISet<LexSymbolKind> followSymbols)
        {
            if (_lastSymbol.Kind != LexSymbolKind.AssignOperator)
            {
                return null;
            }

            ReadNextSymbol(); // pula o '='

            // Se for '{', temos uma tupla. Nesse caso, lemos a constante.
            if (_lastSymbol.Kind == LexSymbolKind.LeftBrace)
            {
                return GetConstant();
            }

            // Caso contrário, é uma expressão.
            return GetExpression(followSymbols);
        }

        /// <summary>
        /// Reconhece o índice de um vetor.
        /// index => '[' expression ']'
        /// </summary>
        /// <returns></returns>
        private Expression GetIndex(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.LeftSquareBracket);
            ReadNextSymbol(); // Pula '['
            Expression expression = GetFullExpression(this.addToSet(followSymbols,LexSymbolKind.RightSquareBracket));
            CheckLastSymbolKindAndReadNext(LexSymbolKind.RightSquareBracket, "]");
            return expression;
        }

        /// <summary>
        /// Reconhece uma variável num comando.
        /// variable => ident [ index ]
        /// </summary>
        /// <param name="name">Nome da variável (identificador).</param>
        /// <returns>Variable ou Indexing</returns>
        private Expression GetVariable(string name, ISet<LexSymbolKind> followSymbols)
        {
            Expression result = new Variable(name, _lastSymbol);
            followSymbols = addToSet(followSymbols, LexSymbolKind.RightSquareBracket);
            while (_lastSymbol.Kind == LexSymbolKind.LeftSquareBracket)
            {
                result = new IndexingOperation(result, GetIndex(followSymbols), _lastSymbol);
            }
            return result;
        }

        /// <summary>
        /// Reconhece uma chamada de função.
        /// </summary>
        /// <param name="functionName">Nome da função (identificador).</param>
        /// <returns></returns>
        private FunctionCall GetFunctionCallOperation(string functionName, ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.LeftParentheses);

            var functionCall = new FunctionCall(functionName, _lastSymbol);
            ReadNextSymbol(); // Pula '('
            followSymbols = addToSet(followSymbols, LexSymbolKind.RightParentheses, LexSymbolKind.Comma);
            while ((_lastSymbol.Kind != LexSymbolKind.RightParentheses) &&
                   (_lastSymbol.Kind != LexSymbolKind.EndOfFile) &&
                   (_lastSymbol.Kind != LexSymbolKind.Error))
            {
                Expression parameter = GetFullExpression(followSymbols);
                functionCall.Parameters.Add(parameter);
                if (_lastSymbol.Kind == LexSymbolKind.Comma)
                {
                    ReadNextSymbol(); // Se for vírgula, nós pulamos.
                }
            }

            CheckLastSymbolKindAndReadNext(LexSymbolKind.RightParentheses, ")");

            return functionCall;
        }

        /// <summary>
        /// Reconhece uma variável ou chamada de função (após reconhecer um identificador).
        /// variableOrFunctionCall => identifier '(' parametersList ')' | identifier [  '[' expression ']'  ]
        /// </summary>
        /// <param name="name">Nome (identificador).</param>
        /// <returns>Expressão</returns>
        private Expression GetVariableOrFunctionCallOperation(string name, ISet<LexSymbolKind> followSymbols)
        {
            if (_lastSymbol.Kind == LexSymbolKind.LeftParentheses)
            {
                return GetFunctionCallOperation(name, followSymbols);
            }
            return GetVariable(name, followSymbols);
        }

        /// <summary>
        /// Reconhece um 'fator': variável, constante, chamada de função ou uma expressão entre parentesis.
        /// factor  : identifier
        ///         | - factor
        ///         | '(' fullExpression ')'
        ///         | intConstant
        ///         | boolConstant
        ///         | stringConstant
        ///         | functionCall
        /// </summary>
        /// <returns>Expressão</returns>
        private Expression GetFactor(ISet<LexSymbolKind> followSymbols)
        {
 
            TestNextSymbol(factorStartSymbSet, followSymbols, "identifier, '-', '(' or constant", false); // TODO: tirar (Factor)

            LexSymbol symbol = _lastSymbol;

            if (factorStartSymbSet.Contains(symbol.Kind))
            {
                ReadNextSymbol(); // Lê o próximo símbolo e continua a análise sintática

                switch (symbol.Kind)
                {
                    case LexSymbolKind.Identifier:
                        return GetVariableOrFunctionCallOperation(symbol.Text, followSymbols);
                    case LexSymbolKind.SubtractOperator:
                        return new UnaryOperation(RplOperationType.Minus, GetFactor(followSymbols), symbol);
                    case LexSymbolKind.IntConstant:
                        return new Constant(symbol.Text, TypeDeclaration.Int, symbol);
                    case LexSymbolKind.FloatConstant:
                        return new Constant(symbol.Text, TypeDeclaration.Float, symbol);
                    case LexSymbolKind.TrueReservedWord:
                    case LexSymbolKind.FalseReservedWord:
                        return new Constant(symbol.Text, TypeDeclaration.Bool, symbol);
                    case LexSymbolKind.StringConstant:
                        return new Constant(symbol.Text, TypeDeclaration.String, symbol);
                    case LexSymbolKind.LeftParentheses:
                        // ReadNextSymbol(); // pula ')'  <=== erro !!!
                        Expression expression = GetFullExpression(addToSet(followSymbols, LexSymbolKind.RightParentheses));
                        CheckLastSymbolKindAndReadNext(LexSymbolKind.RightParentheses, ")");
                        return expression;
                }

            }
 
            return null;        
        }

        /// <summary>
        /// Reconhece um 'fator' formado p. uma seq. de operações de multiplicação ou divisão ou modulo 
        /// term : factor { {'*' | '/' | '%'}  term }
        /// </summary>
        /// <returns></returns>
        private Expression GetTerm(ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol;
            followSymbols = setUnion(followSymbols, mulOpSymbSet);
            Expression expression = GetFactor(followSymbols);
            while ( mulOpSymbSet.Contains(_lastSymbol.Kind) )
            {
                symbol = _lastSymbol;
                ReadNextSymbol(); // Pula o operador ('*' ou '/' ou '%')
                expression = new DyadicOperation(GetRplOperation(symbol.Kind), expression, GetTerm(followSymbols), symbol);
            }
            return expression;
        }

        /// <summary>
        /// Reconhece um 'termo' (formado por uma seq. de operações de adição)
        /// simpleExpression => term { {'+' | '-'} simpleExpression }
        /// </summary>
        /// <returns></returns>
        private Expression GetSimpleExpression(ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol;
            Expression expression = GetTerm(addToSet(followSymbols, LexSymbolKind.AddOperator, LexSymbolKind.SubtractOperator));
            while ((_lastSymbol.Kind == LexSymbolKind.AddOperator) || _lastSymbol.Kind == LexSymbolKind.SubtractOperator)
            {
                symbol = _lastSymbol;
                ReadNextSymbol(); // Pula o operador ('+' ou '-')
                expression = new DyadicOperation(GetRplOperation(symbol.Kind), expression, GetSimpleExpression(followSymbols), symbol);
            }
            return expression;
        }

        /// <summary>
        /// Reconhece uma 'expressão simples': não inclui os operadores lógicos
        /// GetExpression : simpleExpression [ RelationalOperator simpleExpression ]
        /// </summary>
        /// <returns></returns>
        private Expression GetExpression(ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol;
            Expression expression = GetSimpleExpression(followSymbols);
            if (IsRelationalOperator(_lastSymbol.Kind))
            {
                symbol = _lastSymbol;
                ReadNextSymbol(); // Pula o operador relacional.
                return new DyadicOperation(GetRplOperation(symbol.Kind), expression, GetSimpleExpression(followSymbols), symbol);
            }
            return expression;
        }

        /// <summary>
        /// Reconhece uma expressão 'completa': inclui operadores lógicos e relacionais.
        /// 
        /// fullExpression: 
        ///     ('NOT' fullExpression) | (expression [ ( 'AND' | 'OR' ) fullExpression ])
        /// </summary>
        /// <returns></returns>
        private Expression GetFullExpression(ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol;
            if (_lastSymbol.Kind == LexSymbolKind.NotOperator)
            {
                symbol = _lastSymbol;
                ReadNextSymbol(); // pula o '!'
                return new UnaryOperation(RplOperationType.Not, GetFullExpression(followSymbols), symbol);
            }

            Expression expression = GetExpression(followSymbols);
            if ((_lastSymbol.Kind == LexSymbolKind.AndOperator) || (_lastSymbol.Kind == LexSymbolKind.OrOperator))
            {
                symbol = _lastSymbol;
                ReadNextSymbol(); // Pula o operador ('&&' ou '||')
                return new DyadicOperation(GetRplOperation(symbol.Kind), expression, GetFullExpression(followSymbols), symbol);
            }
            return expression;
        }

        /// <summary>
        /// Reconhece um comando de atribuição. 
        /// assignment => variable '=' expression ';'
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        private Statement GetAssignmentStatement(string variableName, ISet<LexSymbolKind> followSymbols)
        {
            LexSymbol symbol = _lastSymbol;
            followSymbols = addToSet(followSymbols, LexSymbolKind.SemiColon);
            Expression variable = GetVariable(variableName, addToSet(followSymbols, LexSymbolKind.AssignOperator));
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.AssignOperator, "=");
            TestNextSymbol(newSymbolSet(LexSymbolKind.AssignOperator), followSymbols, "=", false);
            if (_lastSymbol.Kind == LexSymbolKind.AssignOperator) ReadNextSymbol();
            Expression assignedValue = GetFullExpression(followSymbols);
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.SemiColon, ";");
            TestNextSymbol(semicolonSymbSet, followSymbols, ";", true);

            return new Assignment(variable, assignedValue, symbol);
        }

        /// <summary>
        /// Reconhece uma chamada de função como um comando. 
        /// functionCallStatement => identifier '(' [ expression { ',' expression } ] ')' ';'
        /// </summary>
        /// <param name="functionName">Nome da função.</param>
        /// <returns></returns>
        private Statement GetFunctionCallStatement(string functionName, ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.LeftParentheses);

            LexSymbol symbol = _lastSymbol;
            ReadNextSymbol(); // Pula o '('

            var functionCallStatement = new FunctionCallStatement(functionName, symbol);
            bool error = false;
            while ((_lastSymbol.Kind != LexSymbolKind.RightParentheses) && (!error))
            {
                Expression parameter = GetFullExpression(addToSet(followSymbols, LexSymbolKind.RightParentheses));
                if (parameter != null)
                {
                    functionCallStatement.Parameters.Add(parameter);
                    if (_lastSymbol.Kind == LexSymbolKind.Comma)
                    {
                        ReadNextSymbol(); // Pula a ','
                    }
                    else if (_lastSymbol.Kind != LexSymbolKind.RightParentheses)
                    {
                        error = true;
                        EmitUnexpectedSymbolError(", or )", _lastSymbol);
                    }
                }
            }

            ReadNextSymbol(); // Pula o ')'
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.SemiColon, ";");
            TestNextSymbol(semicolonSymbSet, followSymbols, ";", true);
            return functionCallStatement;
        }

        /// <summary>
        /// Reconhece um comando de atribuição ou chamada de função (após reconhecer um identificador). 
        /// assignmentOrFunctionCall => identifier '(' call | identifier assignment
        /// </summary>
        /// <returns></returns>
        private Statement GetAssignmentOrFunctionCallStatement(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.Identifier);

            followSymbols = addToSet(followSymbols, LexSymbolKind.SemiColon);
            string identifierName = _lastSymbol.Text;
            ReadNextSymbol(); // Pula o identificador
            if (_lastSymbol.Kind == LexSymbolKind.LeftParentheses)
            {
                return GetFunctionCallStatement(identifierName, followSymbols);
            }
            return GetAssignmentStatement(identifierName, followSymbols);
        }

        /// <summary>
        /// Reconhece o comando condicional (exemplo: if (a > b) f1(); else f2(); )
        /// GetIfStatement => 'if' '(' expression ')' statement [ 'else' statement ']
        /// </summary>
        /// <returns></returns>
        private IfStatement GetIfStatement(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.IfReservedWord);

            LexSymbol symbol = _lastSymbol;
            ReadNextSymbol(); // pula o 'if'

            TestNextSymbol(leftParSymbSet, addToSet(followSymbols, LexSymbolKind.RightParentheses), "(", true);
            ISet<LexSymbolKind> exprFollowSymbols = setUnion(followSymbols, addToSet(statStartSet, LexSymbolKind.RightParentheses, LexSymbolKind.ElseReservedWord));
            Expression condition = GetFullExpression(exprFollowSymbols);
            TestNextSymbol(rightParSymbSet, exprFollowSymbols, "(", true);
            Statement thenStatement = GetStatement(addToSet(followSymbols,LexSymbolKind.ElseReservedWord));
            Statement elseStatement = null;

            if (_lastSymbol.Kind == LexSymbolKind.ElseReservedWord)
            {
                ReadNextSymbol(); // Pula o 'else'
                elseStatement = GetStatement(followSymbols);
            }

            return new IfStatement(condition, thenStatement, elseStatement, symbol);
        }

        /// <summary>
        /// Reconhece o comando 'while' (ex: while (a > b) { a = a-b; i = i+1; } ). 
        /// whileStatement => 'while' '(' expression ')' statement
        /// </summary>
        /// <returns></returns>
        private WhileStatement GetWhileStatement(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.WhileReservedWord);

            LexSymbol symbol = _lastSymbol;
            ReadNextSymbol(); // Pula o 'while'
            followSymbols = setUnion(followSymbols, addToSet(statStartSet, LexSymbolKind.RightParentheses));
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.LeftParentheses, "(");
            TestNextSymbol(leftParSymbSet, followSymbols, "(", true);
            Expression condition = GetFullExpression(followSymbols);
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.RightParentheses, ")");
            TestNextSymbol(rightParSymbSet, followSymbols, ")", true);

            Statement body = GetStatement(followSymbols);

            return new WhileStatement(condition, body, symbol);
        }

        /// <summary>
        /// Reconhece um comando composto. 
        /// compoundStatement => '{'  { statement } '}'
        /// </summary>
        /// <returns></returns>
        private CompoundStatement GetCompoundStatement(ISet<LexSymbolKind> followSymbols)
        {
           //  if (_lastSymbol.Kind != LexSymbolKind.LeftBrace) throw new UnexpectedSymbolException(LexSymbolKind.LeftBrace, _lastSymbol.Kind);
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.LeftBrace);

            var compoundStatement = new CompoundStatement(_lastSymbol);
            ReadNextSymbol(); // Pula o '{'
        
            while (IsStatementStart(_lastSymbol.Kind))
            {
                Statement statement = GetStatement(addToSet(followSymbols, LexSymbolKind.RightBrace));
                if (statement != null)
                {
                    compoundStatement.Statements.Add(statement);
                }
            }
            CheckLastSymbolKindAndReadNext(LexSymbolKind.RightBrace, "}");
            return compoundStatement;
        }

        /// <summary>
        /// Reconhece o comando 'return'. 
        /// 'return' [ expression ]  ';'
        /// </summary>
        /// <returns></returns>
        private ReturnStatement GetReturnStatement(ISet<LexSymbolKind> followSymbols)
        {
            Debug.Assert(_lastSymbol.Kind == LexSymbolKind.ReturnReservedWord);

            LexSymbol symbol = _lastSymbol;
            ReadNextSymbol(); // Pula 'return'

            Expression returnExpression = null;
            if (_lastSymbol.Kind != LexSymbolKind.SemiColon)
            {
                returnExpression = GetFullExpression(addToSet(followSymbols,LexSymbolKind.SemiColon));
            }
            //CheckLastSymbolKindAndReadNext(LexSymbolKind.SemiColon, ";");
            TestNextSymbol(semicolonSymbSet, followSymbols, ";", true);
            
            return new ReturnStatement(returnExpression, symbol);
        }

        /// <summary>
        /// Reconhece um comando da linguagem
        /// statement => assignOrCall | ifStatement | whileStatement | compoundStatement
        /// </summary>
        /// <returns></returns>
        private Statement GetStatement(ISet<LexSymbolKind> followSymbols)
        {
 
            ISet<LexSymbolKind> followSymbolsWithSemicolon = addToSet(followSymbols, LexSymbolKind.SemiColon);
            TestNextSymbol(statStartSet, followSymbolsWithSemicolon, "identifier, if, while, { or return", false);
            switch (_lastSymbol.Kind)
            {
                case LexSymbolKind.Identifier:
                    return GetAssignmentOrFunctionCallStatement(followSymbolsWithSemicolon);
                case LexSymbolKind.IfReservedWord:
                    return GetIfStatement(followSymbols);
                case LexSymbolKind.WhileReservedWord:
                    return GetWhileStatement(followSymbols);
                case LexSymbolKind.LeftBrace:
                    return GetCompoundStatement(followSymbols);
                case LexSymbolKind.ReturnReservedWord:
                    return GetReturnStatement(followSymbolsWithSemicolon);
                default:
                     EmitUnexpectedSymbolError("identifier, if, while, { or return", _lastSymbol);
                    return null;
            }
        }

        /// <summary>
        /// Reconhece o 'corpo' de uma função. 
        /// functionBody => '{' { variableDeclaration } { statement } '}'
        /// </summary>
        /// <returns></returns>
        private CompoundStatement GetFunctionBody(ISet<LexSymbolKind> followSymbols)
        {

            TestNextSymbol(newSymbolSet(LexSymbolKind.LeftBrace), setUnion(followSymbols, statStartSet, prTypeSymbSet), "{", true);

            //CheckLastSymbolKindAndReadNext(LexSymbolKind.LeftBrace, "{"); 

            followSymbols = addToSet(followSymbols,LexSymbolKind.RightBrace);

            var functionBody = new CompoundStatement(_lastSymbol);

            while (IsPrimitiveType(_lastSymbol.Kind))
            {
                VariableDeclarationList variableList = GetVariableDeclarationList(followSymbols);
                if (variableList != null)
                {
                    functionBody.Declarations.Add(variableList);
                }
                TestNextSymbol(setUnion(prTypeSymbSet, statStartSet), followSymbols, "int, float, boolean, string, void", false);
            }

            while (IsStatementStart(_lastSymbol.Kind))
            {
                Statement statement = GetStatement(followSymbols);
                if (statement != null)
                {
                    functionBody.Statements.Add(statement);
                }
                TestNextSymbol(addToSet(statStartSet, LexSymbolKind.RightBrace), followSymbols, "identifier, if, while, { or return", false);
            }

            //CheckLastSymbolKindAndReadNext(LexSymbolKind.RightBrace, "}");
            TestNextSymbol(rightBraceSymbSet, setUnion(prTypeSymbSet, followSymbols), "}", true);
            return functionBody;
        }

        #endregion

        #region support methods
        #endregion
    }
}
