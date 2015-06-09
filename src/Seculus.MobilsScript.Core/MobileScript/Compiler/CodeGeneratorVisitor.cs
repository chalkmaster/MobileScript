using System;
using System.Collections.Generic;
using System.Globalization;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;
using Seculus.MobileScript.Core.MobileScript.Symbols;
using Seculus.MobileScript.Core.MobileScript.VirtualMachine;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Visita a árvore que representa o programa com o objetivo de gerar o 
    /// código correspondente para a máquina virtual (MobileVM).
    /// </summary>
    public class CodeGeneratorVisitor : INodeVisitor
    {
        #region Fields

        /// <summary>
        /// Instância da máquina virtual à qual será agregado o código gerado.
        /// </summary>
        private readonly Program _program;

        #endregion

        #region Constructors

        public CodeGeneratorVisitor(Program program)
        {
            _program = program;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converte os strings "true" e "false" para os respectivos valores booleanos.
        /// </summary>
        /// <param name="str">String que representa o boolean.</param>
        /// <returns>True, false ou lança exception.</returns>
        private bool ConvertToBool(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new CodeGenerationException("Unexpected null str");

            if (str.Equals("true", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (str.Equals("false", StringComparison.InvariantCultureIgnoreCase)) return false;

            throw new CodeGenerationException("Wrong value for boolean constant");
        }

        /// <summary>
        /// Converte um string para o valor inteiro que ele representa.
        /// </summary>
        /// <param name="str">String que representa o inteiro.</param>
        /// <returns>Valor inteiro</returns>
        private int ConvertToInt(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new CodeGenerationException("Unexpected null str");

            int value;
            if (Int32.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            throw new CodeGenerationException("Wrong value for int constant");
        }

        /// <summary>
        /// Converte um string para o valor float correspondente.
        /// </summary>
        /// <param name="str">String que representa o float.</param>
        /// <returns>Valor float</returns>
        private double ConvertToFloat(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new CodeGenerationException("Unexpected null str");

            double value;
            if (Double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            throw new CodeGenerationException("Wrong value for float constant");
        }

        /// <summary>
        /// Gera o código para uma operação de indexação, 
        /// de forma a deixar o endereço absoluto do elemento indexado no topo da pilha.
        /// </summary>
        /// <param name="indexing">Nó que descreve a operação de indexação.</param>
        /// <param name="finalIndexingOperation">Indica se a operação de indexação é a última na sequência de indexações. (Ex: em x[i][j][k], [k] é a última da sequência).</param>
        /// <returns>Endereço da primeira instrução gerada</returns>
        private int GenerateIndexingCode(IndexingOperation indexing, bool finalIndexingOperation)
        {
            if (indexing == null) throw new CodeGenerationException("Unexpected null IndexingOperation");
            if (indexing.Operand1 == null) throw new CodeGenerationException("Unexpected null operand 1 in IndexingOperation");
            if (indexing.Operand2 == null) throw new CodeGenerationException("Unexpected null operand 2 in IndexingOperation");

            Expression vector = indexing.Operand1;
            Expression index = indexing.Operand2;

            // Esse método coloca o endereço do vetor no topo da pilha!
            int address = GenerateAssignmentLeftSideCode(vector, false);

            // Coloca o valor do indexador no topo da pilha (Ex: valor de "i" em um for).
            index.Accept(this);


            var vectorType = vector.ReturnType as VectorTypeDeclaration;
            if (finalIndexingOperation)
            {
                int vectorSize = (vectorType != null && vectorType.Size.HasValue) ? vectorType.Size.Value : -1;

                // Coloca no topo da pilha o endereço de memória da posição do vetor requisitada!
                _program.AddInstruction(InstructionCode.Idx, new Operand(vectorSize));
            }
            else
            {
                // Coloca no topo da pilha o endereço de memória da posição do vetor requisitada (no caso, o "subvetor").
                _program.AddInstruction(InstructionCode.OffS, new Operand(GetTotalSubvectorsSize(vectorType)));
            }

            return address;
        }

        /// <summary>
        /// Retorna o total de memória ocupada por todas as sub-dimensões desse vetor.
        /// </summary>
        /// <param name="vector">Vetor</param>
        /// <returns>Ex: Para o vetor: matrix[3][2][5], retorna 10 (2 * 5).</returns>
        private int GetTotalSubvectorsSize(VectorTypeDeclaration vector)
        {
            if (vector == null) throw new CodeGenerationException("Unexpected null VectorTypeDeclaration");

            var subVector = vector.ElementType as VectorTypeDeclaration;

            // Base case. Se não for vetor, retorna 1.
            if (subVector == null)
            {
                return 1;
            }

            if (!subVector.Size.HasValue) throw new CodeGenerationException("Unexpected null subVector size.");
            return subVector.Size.Value * GetTotalSubvectorsSize(subVector);
        }

        /// <summary>
        /// Gera o código para o lado esquerdo de uma atribuição, 
        /// de forma a deixar o endereço absoluto da variável no topo da pilha.
        /// </summary>
        /// <param name="leftSide">Nó que descreve o lado esquerdo de uma atribuição.</param>
        /// <param name="finalIndexingOperation">Indica se a operação de indexação é a última na sequência de indexações. (Ex: em x[i][j][k], [k] é a última da sequência).</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        private int GenerateAssignmentLeftSideCode(Expression leftSide, bool finalIndexingOperation)
        {
            int address = -99999; // invalid address

            if (leftSide is Variable)
            {
                address = GenerateCodeToLoadVariableAbsoluteAddress((Variable)leftSide);
            }
            else if (leftSide is IndexingOperation)
            {
                address = GenerateIndexingCode((IndexingOperation)leftSide, finalIndexingOperation);
            }
            else
            {
                throw new CodeGenerationException("Invalid subclass in left side of assignment");
            }

            return address;
        }

        /// <summary>
        /// Gera o código para colocar colocar o endereço absoluto da variável no topo da pilha.
        /// </summary>
        /// <param name="variable">Variável</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        private int GenerateCodeToLoadVariableAbsoluteAddress(Variable variable)
        {
            int address = -99999;

            VariableSymbol variableSymbol = variable.VariableSymbol;
            if (variableSymbol == null) throw new CodeGenerationException("Unexpected null variable symbol in Variable");

            if (variableSymbol is ParameterSymbol && ((ParameterSymbol)variableSymbol).ByRef)
            {
                // Se for um parâmetro passado por referência, o valor dele é na verdade o endereço dele!
                // Nesse caso, basta carregar o valor dele (endereço) e colocar na pilha.
                address = _program.AddInstruction(InstructionCode.LdVar, new Operand(variableSymbol.Address));
            }
            else
            {
                if (variableSymbol.Level == 0)
                {
                    // Se a variável for global, basta fazer o push do seu endereço absoluto (que está em variableSymbol.Address).
                    address = _program.AddInstruction(InstructionCode.GlAddr, new Operand(variableSymbol.Address));
                }
                else
                {
                    // Se a variável for local, basta fazer o push do seu endereço absoluto (o endereço relativo está em variableSymbol.Address).
                    // A instrução LdAddr empilha o endereço absoluto baseado no endereço relativo (passado como operando da instrução).
                    address = _program.AddInstruction(InstructionCode.LdAddr, new Operand(variableSymbol.Address));
                }
            }

            return address;
        }

        /// <summary>
        /// Gera código para uma chamada de função (usada para FunctionCallStatement e FunctionCall)
        /// </summary>
        /// <param name="functionSymbol">Símbolo que descreve a função na tabela de símbolos.</param>
        /// <param name="parameters">Lista dos parâmetros passados para a função.</param>
        private void GenerateFunctionCallCode(FunctionSymbol functionSymbol, IList<Expression> parameters)
        {
            if (functionSymbol == null) throw new CodeGenerationException("Unexpected function symbol in function call");

            bool isVoid = (functionSymbol.Type == TypeDeclaration.Void);

            // Se a função retorna um valor (não é void) e é pré-definida, reserva espaço para o valor retornado.
            if (!isVoid && !(functionSymbol is PreDefinedFunctionSymbol))
            {
                _program.AddInstruction(InstructionCode.IncT, new Operand(1));
            }

            // Empilha os parâmetros
            if (parameters != null)
            {
                if (parameters.Count != functionSymbol.Parameters.Count) throw new CodeGenerationException("Expected parameters length and informed parameters length are different.");
                for (int i = 0; i < functionSymbol.Parameters.Count; i++)
                {
                    ParameterSymbol parameterSymbol = functionSymbol.Parameters[i];
                    Expression parameter = parameters[i];
                    if (parameterSymbol.ByRef)
                    {
                        // O parâmetro é passado por referência. Gera código p/ empilhar seu endereço.
                        GenerateAssignmentLeftSideCode(parameter, true);
                    }
                    else
                    {
                        // O parâmetro é passado por valor. Gera código p/ empilhar o valor da expressão.
                        parameter.Accept(this);
                    }
                }
            }

            // Gera código para a chamada da função.
            if (functionSymbol is PreDefinedFunctionSymbol)
            {
                int code = (int)((PreDefinedFunctionSymbol)functionSymbol).Code;
                _program.AddInstruction(InstructionCode.SysCall, new Operand(code));

                // TODO: Porque não descartamos os parâmetros para funções pre-definidas?
            }
            else
            {
                if (functionSymbol.StartAddress < 0) throw new CodeGenerationException("Undefined start address in FunctionSymbol");
                _program.AddInstruction(InstructionCode.Call, new Operand(functionSymbol.StartAddress));

                // Logo após a chamada da função, geramos código para descartar os parâmetros passados a ela.
                if (parameters != null && parameters.Count > 0)
                {
                    _program.AddInstruction(InstructionCode.IncT, new Operand(-parameters.Count));
                }
            }
        }

        #endregion

        #region Implementation of IVisitor

        /// <summary>
        /// Gera a instrução para empilhar ao valor da constante e retorna o endereço da instrução gerada.
        /// </summary>
        /// <param name="constant">Constant</param>
        /// <returns>Endereço da instrução</returns>
        public object Visit(Constant constant)
        {
            if (constant == null) throw new CodeGenerationException("Unexpected null Constant");
            if (constant.ReturnType == null) throw new CodeGenerationException("Unexpected null Constant type");

            // Endereço da instrução.
            int result = -1;

            if (constant.ReturnType == TypeDeclaration.Bool)
            {
                result = _program.AddInstruction(InstructionCode.BoolConst, new Operand(ConvertToBool(constant.Value)));
            }
            else if (constant.ReturnType == TypeDeclaration.Int)
            {
                result = _program.AddInstruction(InstructionCode.IntConst, new Operand(ConvertToInt(constant.Value)));
            }
            else if (constant.ReturnType == TypeDeclaration.Float)
            {
                result = _program.AddInstruction(InstructionCode.FloatConst, new Operand(ConvertToFloat(constant.Value)));
            }
            else if (constant.ReturnType == TypeDeclaration.String)
            {
                result = _program.AddInstruction(InstructionCode.StrConst, new Operand(constant.Value));
            }
            else
            {
                throw new CodeGenerationException("Wrong constant type");
            }

            return result;
        }

        /// <summary>
        /// Visita uma declaração de variável, e retorna a representação da 
        /// atribuição de um valor inicial à variável (como um objeto Assignment).
        /// </summary>
        /// <param name="variableDeclaration">Declaração de variável.</param>
        /// <returns>
        /// Se a variável tiver valor inicial, 
        /// retorna um objeto Assignment que representa a atribuição do valor inicial da variável. 
        /// Caso contrário, retorna null.
        /// </returns>
        public object Visit(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration == null) throw new CodeGenerationException("Unexpected null VariableDeclaration");

            if (variableDeclaration.InitialValue != null)
            {
                var variable = new Variable(variableDeclaration.Name, variableDeclaration.LexSymbol);
                variable.VariableSymbol = variableDeclaration.VariableSymbol;
                variable.DefineReturnType(variableDeclaration.VariableSymbol.Type);
                return new Assignment(variable, variableDeclaration.InitialValue, variableDeclaration.LexSymbol);
            }

            return null;
        }

        /// <summary>
        /// Visita um objeto VariableDeclarationList e retorna uma lista contendo 
        /// as representações dos valores iniciais das variáveis.
        /// </summary>
        /// <param name="variableDeclarationList">Lista de declaração de variáveis.</param>
        /// <returns>IList[Expression]</returns>
        public object Visit(VariableDeclarationList variableDeclarationList)
        {
            if (variableDeclarationList == null) throw new CodeGenerationException("Unexpected null VariableDeclarationList");

            IList<Expression> initialValues = new List<Expression>();
            foreach (var variablesDeclaration in variableDeclarationList.VariablesDeclarations)
            {
                var assignment = (Assignment)variablesDeclaration.Accept(this);
                if (assignment != null)
                {
                    initialValues.Add(assignment);
                }
            }

            return initialValues;
        }

        public object Visit(TypeDeclaration type)
        {
            // Nada a fazer
            return null;
        }

        public object Visit(VectorTypeDeclaration vectorType)
        {
            // Nada a fazer
            return null;
        }

        /// <summary>
        /// Gera o código para a função.
        /// Além disso, a tabela de símbolos é atualizada com o endereço inicial da função para usos subsequentes.
        /// </summary>
        /// <param name="functionDeclaration">Declaração da função.</param>
        /// <returns>Endereço da instrução inicial da função.</returns>
        public object Visit(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration == null) throw new CodeGenerationException("Unexpected null FunctionDeclaration");
            if (functionDeclaration.FunctionSymbol == null) throw new CodeGenerationException("Unexpected null function symbol in FunctionDeclaration");
            if (functionDeclaration.FunctionSymbol.Scope == null) throw new CodeGenerationException("Unexpected null scope in function symbol");
            if (functionDeclaration.Body == null) throw new CodeGenerationException("Unexpected null body in FunctionDeclaration");

            FunctionSymbol functionSymbol = functionDeclaration.FunctionSymbol;
            Scope functionScope = functionSymbol.Scope;

            // Calcula o endereço dos parâmetros e das variáveis locais dessa função (caso ainda não tenha sido calculado).
            functionScope.CalculateParametersAndVariablesAddressesAndSizes();

            // Geramos a instrução ENTER passando como argumento o tamanho da área de memória que vai ser utilizada pela função.
            functionSymbol.StartAddress = _program.AddInstruction(InstructionCode.Enter, new Operand(functionScope.LocalVariablesSize));

            // Gera o código do corpo da função.
            functionDeclaration.Body.Accept(this);

            // Logo depois de gerar o código do body, adicionamos o return.
            _program.AddInstruction(InstructionCode.Return, new Operand(functionScope.LocalVariablesSize));

            return functionSymbol.StartAddress;
        }

        /// <summary>
        /// Gera código para empilhar o valor de uma variável, supondo que ela é usada numa expressão.
        /// </summary>
        /// <param name="variable">Variável</param>
        /// <returns>Retorna o endereço da primeira instrução gerada.</returns>
        public object Visit(Variable variable)
        {
            if (variable == null) throw new CodeGenerationException("Unexpected null Variable");
            if (variable.VariableSymbol == null) throw new CodeGenerationException("Unexpected null variableSymbol in Variable");

            VariableSymbol variableSymbol = variable.VariableSymbol;

            int loadVarInstructionAddress;
            if (variableSymbol.Level == 0) // variavel global.
            {
                loadVarInstructionAddress = _program.AddInstruction(InstructionCode.LdGlVar, new Operand(variableSymbol.Address));
            }
            else // variavel local.
            {
                loadVarInstructionAddress = _program.AddInstruction(InstructionCode.LdVar, new Operand(variableSymbol.Address));
            }

            // se for parâmetro, temos que carregar o valor da variável (pois foi empilhado apenas seu endereço)
            var parameterSymbol = variableSymbol as ParameterSymbol;
            if (parameterSymbol != null && parameterSymbol.ByRef)
            {
                _program.AddInstruction(InstructionCode.Ldi, new Operand(parameterSymbol.Address));
            }

            return loadVarInstructionAddress;
        }

        /// <summary>
        /// Gera código para menos unário ou not.
        /// </summary>
        /// <param name="unaryOperation">Operação unária.</param>
        /// <returns>Endereço da instrução gerada.</returns>
        public object Visit(UnaryOperation unaryOperation)
        {
            if (unaryOperation == null) throw new CodeGenerationException("Unexpected null UnaryOperation");
            if (unaryOperation.Operand == null) throw new CodeGenerationException("Unexpected null operand in UnaryOperation");
            if (unaryOperation.Operand.ReturnType == null) throw new CodeGenerationException("Unexpected null operand type in UnaryOperation");

            // Visita o operando
            unaryOperation.Operand.Accept(this);

            TypeDeclaration operandType = unaryOperation.Operand.ReturnType;

            switch (unaryOperation.OperationType)
            {
                case RplOperationType.Minus:
                    InstructionCode instructionCode;

                    if (operandType == TypeDeclaration.Int)
                    {
                        instructionCode = InstructionCode.Minus;
                    }
                    else if (operandType == TypeDeclaration.Float)
                    {
                        instructionCode = InstructionCode.FMinus;
                    }
                    else
                    {
                        throw new CodeGenerationException("Invalid operand type for unary minus");
                    }

                    return _program.AddInstruction(instructionCode);

                case RplOperationType.Not:
                    if (operandType == TypeDeclaration.Bool)
                    {
                        return _program.AddInstruction(InstructionCode.Not);
                    }
                    throw new CodeGenerationException("Invalid operand type for unary not");

                default:
                    throw new CodeGenerationException("Invalid unary operation code");
            }
        }

        /// <summary>
        /// Gera código para um operador diádico.
        /// </summary>
        /// <param name="dyadicOperation">Operação diádica</param>
        /// <returns>
        /// Endereço da instrução da operação diádica. 
        /// (no caso é o mesmo endereço do primeiro operador, uma vez que a operação diádica é armazenada na seguinte ordem: OPERANDO1, OPERANDO2, OPERADOR).
        /// </returns>
        public object Visit(DyadicOperation dyadicOperation)
        {
            if (dyadicOperation == null) throw new CodeGenerationException("Unexpected null DyadicOperation");
            if (dyadicOperation.Operand1 == null) throw new CodeGenerationException("Unexpected null operand1 in DyadicOperation");
            if (dyadicOperation.Operand2 == null) throw new CodeGenerationException("Unexpected null operand2 in DyadicOperation");
            if (dyadicOperation.Operand1.ReturnType == null || dyadicOperation.Operand2.ReturnType == null)
            {
                throw new CodeGenerationException("Unexpected null operand type");
            }

            // Endereço da operação diádica. A operação começa no primeiro operando pq ela é armazenada na seguinte ordem: OPERANDO1, OPERANDO2, OPERADOR.
            object address = dyadicOperation.Operand1.Accept(this);
            dyadicOperation.Operand2.Accept(this);

            TypeDeclaration operand1Type = dyadicOperation.Operand1.ReturnType;
            TypeDeclaration operand2Type = dyadicOperation.Operand2.ReturnType;

            // se pelo menos um dos tipos for float, geramos instruções para float.
            bool isFloat = ((operand1Type == TypeDeclaration.Float) || (operand2Type == TypeDeclaration.Float));

            switch (dyadicOperation.OperationType)
            {
                case RplOperationType.Add:
                    if (operand1Type == TypeDeclaration.String && operand2Type == TypeDeclaration.String)
                    {
                        _program.AddInstruction(InstructionCode.Concat);
                    }
                    else if (isFloat)
                    {
                        _program.AddInstruction(InstructionCode.FAdd);
                    }
                    else
                    {
                        _program.AddInstruction(InstructionCode.Add);
                    }
                    break;

                case RplOperationType.Subtract:
                    _program.AddInstruction(isFloat ? InstructionCode.FSub : InstructionCode.Sub);
                    break;

                case RplOperationType.Multiply:
                    _program.AddInstruction(isFloat ? InstructionCode.FMult : InstructionCode.Mult);
                    break;

                case RplOperationType.Divide:
                    _program.AddInstruction(isFloat ? InstructionCode.FDiv : InstructionCode.Div);
                    break;

                case RplOperationType.Module:
                    _program.AddInstruction(isFloat ? InstructionCode.FMod : InstructionCode.Mod);
                    break;

                case RplOperationType.Equal:
                    _program.AddInstruction(InstructionCode.Eq);
                    break;

                case RplOperationType.NotEqual:
                    _program.AddInstruction(InstructionCode.NE);
                    break;

                case RplOperationType.GreaterThan:
                    _program.AddInstruction(InstructionCode.GT);
                    break;

                case RplOperationType.LessThan:
                    _program.AddInstruction(InstructionCode.LT);
                    break;

                case RplOperationType.GreaterOrEqual:
                    _program.AddInstruction(InstructionCode.GE);
                    break;

                case RplOperationType.LessOrEqual:
                    _program.AddInstruction(InstructionCode.LE);
                    break;

                case RplOperationType.And:
                    _program.AddInstruction(InstructionCode.And);
                    break;

                case RplOperationType.Or:
                    _program.AddInstruction(InstructionCode.Or);
                    break;

                default:
                    throw new CodeGenerationException("Invalid dyadic operator");
            }

            return address;
        }

        /// <summary>
        /// Gera código para empilhar uma tupla.
        /// </summary>
        /// <param name="tuple">Tupla</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(TupleConstant tuple)
        {
            if (tuple == null) throw new CodeGenerationException("Unexpected null TupleConstant");
            if (tuple.Elements == null) throw new CodeGenerationException("Unexpected null elements in TupleConstant");
            if (tuple.Elements.Count == 0) throw new CodeGenerationException("Unexpected empty elements list in TupleConstant");

            int address = (int)tuple.Elements[0].Accept(this);
            for (int i = 1; i < tuple.Elements.Count; i++)
            {
                tuple.Elements[i].Accept(this);
            }

            return address;
        }

        /// <summary>
        /// Gera código para um comando return.
        /// </summary>
        /// <param name="returnStatement">Comando de return</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(ReturnStatement returnStatement)
        {
            if (returnStatement == null) throw new CodeGenerationException("Unexpected null ReturnStatement");
            if (returnStatement.FunctionSymbol == null) throw new CodeGenerationException("Unexpected null function symbol in ReturnStatement");

            // Endereço da última instrução inserida na VM.
            int pc = _program.GetProgramCounter();

            if (returnStatement.ReturnType != TypeDeclaration.Void)
            {
                // o tipo de retorno não é void e portanto existe um valor de retorno.
                if (returnStatement.ValueToReturn == null) throw new CodeGenerationException("Unexpected null return expression");
                returnStatement.ValueToReturn.Accept(this);
                int returnValueAddress = returnStatement.FunctionSymbol.GetRelativeReturnValueAddress();
                _program.AddInstruction(InstructionCode.StVar, new Operand(returnValueAddress));
            }
            _program.AddInstruction(InstructionCode.Return, new Operand(returnStatement.FunctionSymbol.Scope.LocalVariablesSize));

            return pc + 1; // Endereço da primeira instrução gerada, pois geramos ao menos uma instrução.
        }

        /// <summary>
        /// Gera código para um comando composto.
        /// </summary>
        /// <param name="compoundStatement">Comando composto</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(CompoundStatement compoundStatement)
        {
            if (compoundStatement == null) throw new CodeGenerationException("Unexpected null CompoundStatement");
            if (compoundStatement.Initializations == null) throw new CodeGenerationException("Unexpected null initializations in CompoundStatement");
            if (compoundStatement.Declarations == null) throw new CodeGenerationException("Unexpected null declarations in CompoundStatement");
            if (compoundStatement.Statements == null) throw new CodeGenerationException("Unexpected null statements in CompoundStatement");

            // Endereço da última instrução inserida na VM.
            int pc = _program.GetProgramCounter();

            bool atLeastOneInstructionGenerated = false;

            // Gera o código das inicializações necessárias para esse comando composto.
            foreach (var initialization in compoundStatement.Initializations)
            {
                initialization.Accept(this);
                atLeastOneInstructionGenerated = true;
            }

            // Gera o código das declarações.
            foreach (var declaration in compoundStatement.Declarations)
            {
                var expressions = (IList<Expression>)declaration.Accept(this);
                foreach (var expression in expressions)
                {
                    expression.Accept(this);
                    atLeastOneInstructionGenerated = true;
                }
            }

            // Gera o código dos comandos
            foreach (var statement in compoundStatement.Statements)
            {
                statement.Accept(this);
                atLeastOneInstructionGenerated = true;
            }

            return pc + (atLeastOneInstructionGenerated ? 1 : 0);
        }

        /// <summary>
        /// Gera código para o comando condicional.
        /// </summary>
        /// <param name="ifStatement">Comando condicional.</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(IfStatement ifStatement)
        {
            if (ifStatement == null) throw new CodeGenerationException("Unexpected null IfStatement");
            if (ifStatement.Condition == null) throw new CodeGenerationException("Unexpected null condition in IfStatement");
            if (ifStatement.ThenPart == null) throw new CodeGenerationException("Unexpected null then part in IfStatement");

            // O valor de retorno é o endereço inicial da condição.
            object address = ifStatement.Condition.Accept(this);
            // Gera a instrução que irá pular para o else (ou para o fim do comando se não tiver else) caso a condição retorne false.
            int jumpFAddress = _program.AddInstruction(InstructionCode.JumpF, new Operand(-99999)); // Endereço temporariamente indefinido.

            // Gera as instruções do then.
            ifStatement.ThenPart.Accept(this);
            // Gera a instruções que irá pular para o fim do comando logo após executar o then.
            int jumpAddress = _program.AddInstruction(InstructionCode.Jump, new Operand(-99999)); // Endereço temporariamente indefinido.

            // Gera as instruções do else.
            if (ifStatement.ElsePart != null)
            {
                ifStatement.ElsePart.Accept(this);
            }

            // Ajusta o JumpF para pular para logo depois do Jump (que é o fim do then).
            _program.FixOperand(jumpFAddress, new Operand(jumpAddress + 1));

            // Ajusta o Jump para pular para logo depois da última instrução gerada até agora (que é o fim do if completo).
            int pc = _program.GetProgramCounter();
            _program.FixOperand(jumpAddress, new Operand(pc + 1));

            return address;
        }

        /// <summary>
        /// Gera código para o comando de repetição.
        /// </summary>
        /// <param name="whileStatement">Comando de repetição</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(WhileStatement whileStatement)
        {
            if (whileStatement == null) throw new CodeGenerationException("Unexpected null WhileStatement");
            if (whileStatement.Condition == null) throw new CodeGenerationException("Unexpected null condition in WhileStatement");

            // Gera o código para a condição.
            int address = (int)whileStatement.Condition.Accept(this);
            int jumpFAddress = _program.AddInstruction(InstructionCode.JumpF, new Operand(-99999)); // Endereço temporariamente indefinido.

            // Gera o código para o corpo do while.
            if (whileStatement.Body != null)
            {
                whileStatement.Body.Accept(this);
            }
            int jumpAddress = _program.AddInstruction(InstructionCode.Jump, new Operand(address)); // Volta pra o início do while.

            // Ajusta o JumpF para redirecionar para o primeiro comando depois do corpo do while caso a condição seja false.
            _program.FixOperand(jumpFAddress, new Operand(jumpAddress + 1));

            return address;
        }

        /// <summary>
        /// Gera código para a operação de chamada de função (numa expressão)
        /// </summary>
        /// <param name="functionCall">Chamada de função</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(FunctionCall functionCall)
        {
            if (functionCall == null) throw new CodeGenerationException("Unexpected null FunctionCall");

            int pc = _program.GetProgramCounter();
            GenerateFunctionCallCode(functionCall.FunctionSymbol, functionCall.Parameters);
            return pc + 1;
        }

        /// <summary>
        /// Gera código para o comando de chamada de função.
        /// </summary>
        /// <param name="functionCallStatement">Chamada de função</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(FunctionCallStatement functionCallStatement)
        {
            if (functionCallStatement == null) throw new CodeGenerationException("Unexpected null FunctionCallStatement");

            int pc = _program.GetProgramCounter();

            // Gera o código para a chamada de função.
            GenerateFunctionCallCode(functionCallStatement.FunctionSymbol, functionCallStatement.Parameters);

            // Como é o comando (não está em uma expressão como por exemplo atribuição), 
            // temos que descartar o valor de retorno se a função não for void.
            if (functionCallStatement.FunctionSymbol.Type != TypeDeclaration.Void)
            {
                _program.AddInstruction(InstructionCode.IncT, new Operand(-1));
            }

            return pc + 1;
        }

        /// <summary>
        /// Gera código para uma atribuição.
        /// </summary>
        /// <param name="assignment">Atribuição</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(Assignment assignment)
        {
            if (assignment == null) throw new CodeGenerationException("Unexpected null Assignment");
            if (assignment.Variable == null) throw new CodeGenerationException("Unexpected null left side in Assignment");
            if (assignment.Value == null) throw new CodeGenerationException("Unexpected null right side in Assignment");
            if (assignment.Variable.ReturnType == null) throw new CodeGenerationException("Unexpected null left side type in Assignment");
            if (assignment.Value.ReturnType == null) throw new CodeGenerationException("Unexpected null right side type in Assignment");

            Expression leftSide = assignment.Variable;
            Expression rightSide = assignment.Value;

            // Gera o código do lado direito da atribuição.
            object address = rightSide.Accept(this);

            // Gera o código do lado esquerdo da atribuição.
            GenerateAssignmentLeftSideCode(leftSide, true);

            // Gera o código da atribuição propriamente dita (setar o valor na variável).
            if (leftSide.ReturnType is VectorTypeDeclaration)
            {
                int vectorTotalSize = leftSide.ReturnType.TotalSize;
                _program.AddInstruction(InstructionCode.StBlock, new Operand(vectorTotalSize));
            }
            else
            {
                _program.AddInstruction(InstructionCode.Sti);
            }

            return address;
        }

        /// <summary>
        /// Gera código para uma indexação.
        /// </summary>
        /// <param name="indexing">Indexação</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(IndexingOperation indexing)
        {
            // Gera código para deixar o endereço da variável indexada no topo da pilha.
            int address = GenerateIndexingCode(indexing, true);

            // Carrega o valor da variável indexada.
            _program.AddInstruction(InstructionCode.Ldi);

            return address;
        }

        /// <summary>
        /// Gera o código correspondente ao programa.
        /// </summary>
        /// <param name="program">Programa</param>
        /// <returns>Endereço da primeira instrução gerada.</returns>
        public object Visit(ProgramDescription program)
        {
            if (program == null) throw new CodeGenerationException("Unexpected null ProgramDescription");
            if (program.Scope == null) throw new CodeGenerationException("Unexpected null scope in ProgramDescription");
            if (program.Declarations == null) throw new CodeGenerationException("Unexpected null declarations in ProgramDescription");
            if (program.Body == null) throw new CodeGenerationException("Unexpected null body in ProgramDescription");

            // Calcula o endereço dos parâmetros e das variáveis locais dessa função (caso ainda não tenha sido calculado).
            program.Scope.CalculateParametersAndVariablesAddressesAndSizes();

            int totalSize = program.Scope.LocalVariablesSize;

            // Gera código para as declarações.
            foreach (var declaration in program.Declarations)
            {
                object obj = declaration.Accept(this);
                if (declaration is VariableDeclarationList)
                {
                    // Associa a inicialização das variáveis ao corpo do do programa.
                    foreach (var expression in (IList<Expression>)obj)
                    {
                        var assignment = expression as Assignment;
                        if (assignment == null) throw new CodeGenerationException("Variable initialization code must be an Assignment");
                        program.Body.Initializations.Add(assignment);
                    }
                }
            }

            // Gera o código para a entrada da função "main".
            int entryPointAddress = _program.AddInstruction(InstructionCode.Enter, new Operand(totalSize));

            // Gera o código do corpo da "função main".
            program.Body.Accept(this);

            // Gera o código para o fim da "função main".
            _program.AddInstruction(InstructionCode.Halt);

            // Retorna o endereço da primeira instrução do programa.
            return entryPointAddress;
        }

        #endregion
    }
}
