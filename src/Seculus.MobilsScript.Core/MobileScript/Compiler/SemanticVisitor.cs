using System;
using System.Collections.Generic;
using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.Helpers;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;
using Seculus.MobileScript.Core.MobileScript.Symbols;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Define o objeto responsável pela verificação semântica da 'árvore de programa'. 
    /// </summary>
    public class SemanticVisitor : INodeVisitor
    {
        #region Constructors

        public SemanticVisitor()
            : this(new SymbolsTable(), new List<CompilationError>())
        { }

        public SemanticVisitor(SymbolsTable symbolsTable, IList<CompilationError> errors)
        {
            SymbolsTable = symbolsTable;
            Errors = errors;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Lista contendo os eventuais erros sintáticos encontrados.
        /// </summary>
        public IList<CompilationError> Errors { get; private set; }

        /// <summary>
        /// Tabela de símbolos
        /// </summary>
        public SymbolsTable SymbolsTable { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Insere uma mensagem de erro na lista.
        /// </summary>
        /// <param name="message">Mensagem de erro</param>
        /// <param name="symbol">Símbolo encontrado (diferente do esperado).</param>
        private void EmitError(string message, LexSymbol symbol)
        {
            Errors.Add(new CompilationError(message, CompilationErrorType.SemanticError, symbol));
        }

        /// <summary>
        /// Verifica se o que foi passado como parâmetro pode ser atribuído ao parâmetro da função.
        /// </summary>
        /// <param name="expectedParameter">Parâmetro esperado.</param>
        /// <param name="passedParameter">Parâmetro passado.</param>
        /// <returns></returns>
        private bool IsParameterAssignmentCompatible(ParameterSymbol expectedParameter, Expression passedParameter)
        {
            if (expectedParameter == null) throw new SemanticException("Unexpected null parameter symbol");
            if (passedParameter == null) throw new SemanticException("Unexpected null expression");

            TypeDeclaration expectedParameterType = expectedParameter.Type;
            TypeDeclaration passedParameterType = passedParameter.ReturnType;

            // Handle primitive types
            if (expectedParameterType.IsPrimitive())
            {
                return IsPrimitiveTypeAssignmentCompatible(expectedParameterType, passedParameterType);
            }


            // Handle vector types
            if (expectedParameterType.IsVector())
            {
                // Vectors must be passed by reference.
                if (!expectedParameter.ByRef)
                {
                    EmitError("Arrays parameters must be passed by reference.", expectedParameter.Type.LexSymbol);
                    return false;
                }

                var expectedVector = expectedParameterType as VectorTypeDeclaration;
                var passedVector = passedParameterType as VectorTypeDeclaration;

                if (expectedVector != null && passedVector != null)
                {
                    return IsVectorParameterAssignmentCompatible(expectedVector, passedVector);
                }
            }


            return false;
        }
        private bool IsVectorParameterAssignmentCompatible(VectorTypeDeclaration expectedVector, VectorTypeDeclaration passedVector)
        {
            if (expectedVector == null) throw new SemanticException("Unexpected null expectedVector VectorTypeDeclaration");
            if (passedVector == null) throw new SemanticException("Unexpected null passedVector VectorTypeDeclaration");
            
            TypeDeclaration expectedVectorElementType = expectedVector.ElementType;
            TypeDeclaration passedVectorElementType = passedVector.ElementType;

            while (expectedVectorElementType.IsVector())
            {
                // Se o esperado for vetor, o outro também tem que ser!
                if (!passedVectorElementType.IsVector())
                {
                    return false;
                }

                // Only the first dimension can have empty sizes.
                if (!((VectorTypeDeclaration)expectedVectorElementType).Size.HasValue)
                {
                    EmitError("Only the first dimension of multidimensional array can have ommitted size.", expectedVector.LexSymbol);
                    return false;
                }

                expectedVectorElementType = ((VectorTypeDeclaration) expectedVectorElementType).ElementType;
                passedVectorElementType = ((VectorTypeDeclaration)passedVectorElementType).ElementType;
            }

            return IsAssignmentCompatible(expectedVectorElementType, passedVectorElementType);
        }

        /// <summary>
        /// Verifica se dois tipos type1 e type2 são compatíveis para atribuição na forma type1 = type2. 
        /// Dois tipos são compatíveis para atribuição nos seguintes casos: 
        ///     - type1 == int, type2 = int
        ///     - type1 == float e ( type2 == float ou type1 = int )
        ///     - type1 == String e type2 == String
        /// </summary>
        /// <param name="type1">Tipo 1 - Tipo da variável (ou parâmetro) que está sendo setada. Formato: type1 = type2</param>
        /// <param name="type2">Tipo 2 - Tipo do valor sendo "passado". Formato: type1 = type2</param>
        /// <returns>True se a atribuição for válida. Caso contrário, false.</returns>
        private bool IsAssignmentCompatible(TypeDeclaration type1, TypeDeclaration type2)
        {
            if (type1 == null) throw new SemanticException("Unexpected null type1");
            if (type2 == null) throw new SemanticException("Unexpected null type2");

            // Handle primitive types
            if (type1.IsPrimitive())
            {
                return IsPrimitiveTypeAssignmentCompatible(type1, type2);
            }

            // Handle vector types
            var vectorType1 = type1 as VectorTypeDeclaration;
            var vectorType2 = type2 as VectorTypeDeclaration;
            if (vectorType1 != null && vectorType2 != null)
            {
                return IsVectorAssignCompatible(vectorType1, vectorType2);
            }

            return false;
        }
        private bool IsPrimitiveTypeAssignmentCompatible(TypeDeclaration type1, TypeDeclaration type2)
        {
            if (type1 == TypeDeclaration.Int)
            {
                return type2 == TypeDeclaration.Int;
            }
            if (type1 == TypeDeclaration.Float)
            {
                return (type2 == TypeDeclaration.Float) || (type2 == TypeDeclaration.Int);
            }
            if (type1 == TypeDeclaration.Bool)
            {
                return type2 == TypeDeclaration.Bool;
            }
            if (type1 == TypeDeclaration.String)
            {
                return type2 == TypeDeclaration.String;
            }

            return false;
        }
        private bool IsVectorAssignCompatible(VectorTypeDeclaration vectorType1, VectorTypeDeclaration vectorType2)
        {
            if (vectorType1.Size != vectorType2.Size)
            {
                return false;
            }

            TypeDeclaration elementType1 = vectorType1.ElementType;
            TypeDeclaration elementType2 = vectorType2.ElementType;

            if ((elementType1 == null) || (elementType2 == null))
            {
                throw new CompilationException("Unexpected null vector element type");
            }

            return IsAssignmentCompatible(elementType1, elementType2);
        }

        /// <summary>
        /// Verifica se uma declaração de variável (ou parâmetro) é válida 
        /// (ainda não insere a declaração na tabela).
        /// </summary>
        /// <param name="variableDeclaration">Declaração de variável (ou parâmetro).</param>
        /// <returns>Tipo correto</returns>
        private TypeDeclaration GetValidatedVariableDeclarationType(VariableDeclaration variableDeclaration)
        {
            // verifica se o nome já foi declarado (no escopo)
            if (SymbolsTable.LocalSearch(variableDeclaration.Name) != null)
            {
                EmitError("variable/parameter: name already declared: {0}".FormatWith(variableDeclaration.Name), variableDeclaration.LexSymbol);
                return null;
            }

            if ((bool)variableDeclaration.Type.Accept(this))
            {
                if (variableDeclaration.InitialValue != null)
                {
                    if ((bool)variableDeclaration.InitialValue.Accept(this))
                    {
                        if (!IsAssignmentCompatible(variableDeclaration.Type, variableDeclaration.InitialValue.ReturnType))
                        {
                            EmitError("Incompatible init value type in var declaration name: {0}".FormatWith(variableDeclaration.Name), variableDeclaration.LexSymbol);
                        }
                    }
                }
                return variableDeclaration.Type;
            }
            return TypeDeclaration.Wrong;
        }

        /// <summary>
        /// Verifica se os parâmetros da função são válidos e os adiciona na tabela da símbolos.
        /// </summary>
        /// <param name="functionDeclaration"></param>
        /// <returns></returns>
        private IList<ParameterSymbol> AddFunctionParametersToSymbolsTable(FunctionDeclaration functionDeclaration)
        {
            IList<ParameterSymbol> symbols = new List<ParameterSymbol>();
            foreach (var parameter in functionDeclaration.Parameters)
            {
                TypeDeclaration parameterType = GetValidatedVariableDeclarationType(parameter);
                if (parameterType != null)
                {
                    var parameterSymbol = new ParameterSymbol(parameter.Name, parameterType, parameter.IsRef);
                    SymbolsTable.AddSymbol(parameterSymbol);
                    symbols.Add(parameterSymbol);
                }
            }
            return symbols;
        }

        /// <summary>
        /// Busca o símbolo de uma função a partir do seu nome.
        /// </summary>
        /// <param name="functionName">Nome da função.</param>
        /// <param name="lexSymbol">Símbolo léxico.</param>
        /// <returns>Símbolo da função</returns>
        private FunctionSymbol GetFunctionSymbol(string functionName, LexSymbol lexSymbol)
        {
            if (String.IsNullOrEmpty(functionName)) throw new SemanticException("Unexpected null function name");

            Symbol symbol = SymbolsTable.Search(functionName);
            if (symbol == null)
            {
                EmitError("Undeclared function {0}".FormatWith(functionName), lexSymbol);
            }
            else
            {
                var functionSymbol = symbol as FunctionSymbol;
                if (functionSymbol == null)
                {
                    EmitError("{0} is not a function".FormatWith(functionName), lexSymbol);
                }
                else
                {
                    return functionSymbol;
                }
            }
            return null;
        }
        /// <summary>
        /// Busca o símbolo de uma função a partir do seu nome.
        /// </summary>
        /// <param name="functionCall">Nó da árvore do programa que representa a chamada de função.</param>
        /// <returns>Símbolo da função.</returns>
        private FunctionSymbol GetFunctionSymbol(FunctionCall functionCall)
        {
            return GetFunctionSymbol(functionCall.Name, functionCall.LexSymbol);
        }
        /// <summary>
        /// Busca o símbolo de uma função a partir do seu nome.
        /// </summary>
        /// <param name="functionCallStatement">Nó da árvore do programa que representa o comando de chamada de função.</param>
        /// <returns>Símbolo da função.</returns>
        private FunctionSymbol GetFunctionSymbol(FunctionCallStatement functionCallStatement)
        {
            return GetFunctionSymbol(functionCallStatement.Name, functionCallStatement.LexSymbol);
        }

        /// <summary>
        /// Verifica se os parâmetros passados para a função são compatíveis com os parâmetros esperados pela função.
        /// </summary>
        /// <param name="functionName">Nome da função.</param>
        /// <param name="passedParameters">Parâmetros passados para a função.</param>
        /// <param name="expectedParameters">Parâmetros esperados pela função.</param>
        /// <param name="lexSymbol">Símbolo léxico que representa a chamada de função.</param>
        /// <returns>True se os parâmetros forem compatíveis. Caso contrário, false.</returns>
        private bool ArePassedParametersCompatibleWithExpectedParameters(string functionName, IList<Expression> passedParameters, IList<ParameterSymbol> expectedParameters, LexSymbol lexSymbol)
        {
            if (String.IsNullOrEmpty(functionName)) throw new SemanticException("Unexpected null function name");
            if (passedParameters == null) throw new SemanticException("Unexpected null parameters in FunctionCall");
            if (expectedParameters == null) throw new SemanticException("Unexpected null parameters in FunctionSymbol");

            bool isValid = true;

            // O número de parâmetros passados tem que ser igual ao número de parâmetros esperados.
            if (passedParameters.Count != expectedParameters.Count)
            {
                EmitError("Wrong number of parameters in function call of function: {0}".FormatWith(functionName), lexSymbol);
                isValid = false;
            }
            else
            {
                for (int i = 0; i < passedParameters.Count; i++)
                {
                    Expression passedParameter = passedParameters[i];
                    ParameterSymbol expectedParameter = expectedParameters[i];

                    if (passedParameter == null) throw new SemanticException("Unexpected null expression in passed parameters list");
                    if (expectedParameter == null) throw new SemanticException("Unexpected null parameter symbol in expected parameters list");

                    if (!IsParameterAssignmentCompatible(expectedParameter, passedParameter))
                    {
                        EmitError("Incompatible parameter type. Expected {0}, but received {1}".FormatWith(expectedParameter.Type, passedParameter.ReturnType), lexSymbol);
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
        /// <summary>
        /// Verifica se os parâmetros passados para a função são compatíveis com os parâmetros esperados pela função.
        /// </summary>
        /// <param name="functionCall">Chamada de função.</param>
        /// <param name="functionSymbol">Símbolo que descreve a função.</param>
        /// <returns>True se os parâmetros forem compatíveis. Caso contrário, false.</returns>
        private bool ArePassedParametersCompatibleWithExpectedParameters(FunctionCall functionCall, FunctionSymbol functionSymbol)
        {
            Check.Argument.IsNotNull(functionCall, "functionCall");
            Check.Argument.IsNotNull(functionSymbol, "functionSymbol");

            return ArePassedParametersCompatibleWithExpectedParameters(functionCall.Name, functionCall.Parameters, functionSymbol.Parameters, functionCall.LexSymbol);
        }
        /// <summary>
        /// Verifica se os parâmetros passados para a função são compatíveis com os parâmetros esperados pela função.
        /// </summary>
        /// <param name="functionCallStatement">Chamada de função.</param>
        /// <param name="functionSymbol">Símbolo que descreve a função.</param>
        /// <returns>True se os parâmetros forem compatíveis. Caso contrário, false.</returns>
        private bool ArePassedParametersCompatibleWithExpectedParameters(FunctionCallStatement functionCallStatement, FunctionSymbol functionSymbol)
        {
            Check.Argument.IsNotNull(functionCallStatement, "functionCallStatement");
            Check.Argument.IsNotNull(functionSymbol, "functionSymbol");

            return ArePassedParametersCompatibleWithExpectedParameters(functionCallStatement.Name, functionCallStatement.Parameters, functionSymbol.Parameters, functionCallStatement.LexSymbol);
        }

        private bool IsIndexable(Expression expression)
        {
            if (expression == null) throw new SemanticException("Unexpected null Expression");
            if (expression.ReturnType == null) throw new SemanticException("Unexpected null return type in Expression");

            return (expression is IndexingOperation) || (expression.ReturnType is VectorTypeDeclaration);
        }

        #endregion

        #region Implementation of IVisitor

        public object Visit(Constant constant)
        {
            if (constant == null) throw new SemanticException("Unexpected null constant");

            // nada a fazer, o parser já verificou que o valor é uma constante válida e já associou um tipo.
            return true;
        }

        public object Visit(VariableDeclaration variableDeclaration)
        {
            if (variableDeclaration == null) throw new SemanticException("Unexpected null VariableDeclaration");
            if (String.IsNullOrEmpty(variableDeclaration.Name)) throw new SemanticException("Unexpected null name in VariableDeclaration");

            TypeDeclaration varType = GetValidatedVariableDeclarationType(variableDeclaration);
            if (varType != null)
            {
                var variableSymbol = new VariableSymbol(variableDeclaration.Name, SymbolsTable.GetCurrentLevel(), varType);
                SymbolsTable.AddSymbol(variableSymbol);
                variableDeclaration.VariableSymbol = variableSymbol;
                return true;
            }
            return false;
        }

        public object Visit(VariableDeclarationList variableDeclarationList)
        {
            if (variableDeclarationList == null) throw new SemanticException("Unexpected null VariableDeclarationList");

            bool result = true;
            foreach (var variableDeclaration in variableDeclarationList.VariablesDeclarations)
            {
                result = result && (bool)variableDeclaration.Accept(this);
            }
            return result;
        }

        public object Visit(TypeDeclaration type)
        {
            if (type == null) throw new SemanticException("Unexpected null TypeDeclaration");
            if (type.Name == RplConstants.WrongTypeName)
            {
                EmitError("Invalid type", type.LexSymbol);
                return false;
            }
            return true;
        }

        public object Visit(VectorTypeDeclaration vectorType)
        {
            if (vectorType == null) throw new SemanticException("Unexpected null VectorTypeDeclaration");
            if (vectorType.ElementType == null) throw new SemanticException("Unexpected null ElementType in VectorTypeDeclaration");

            // verifica o tamanho do vetor.
            if (vectorType.Size <= 0 || vectorType.Size > RplConstants.MaxVectorSize)
            {
                EmitError("Invalid vector type size.", vectorType.LexSymbol);
            }

            return vectorType.ElementType.Accept(this);
        }

        public object Visit(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration == null) throw new SemanticException("Unexpected null FunctionDeclaration");
            if (functionDeclaration.Body == null) throw new SemanticException("Unexpected null body in FunctionDeclaration");
            if (String.IsNullOrEmpty(functionDeclaration.Name)) throw new SemanticException("Unexpected null name in FunctionDeclaration");
            if (functionDeclaration.Parameters == null) throw new SemanticException("Unexpected null parameters list in FunctionDeclaration");
            if (functionDeclaration.ReturnType == null) throw new SemanticException("Unexpected null return type in FunctionDeclaration");


            // Verifica se o nome já não foi declarado (pre ou global).
            bool isFunctionNameAvailable = true;
            if (SymbolsTable.Search(functionDeclaration.Name) != null)
            {
                EmitError("Function name already declared: {0}".FormatWith(functionDeclaration.Name), functionDeclaration.LexSymbol);
                isFunctionNameAvailable = false;
            }

            // Criamos o símbolo da função com um tipo de retorno provisório pois precisamos inserí-la na tabela de símbolos nesse momento 
            // (antes de verificar o tipo uma vez que ao inserir uma função na tabela de símbolos, um novo escopo é criado).
            var functionSymbol = new FunctionSymbol(functionDeclaration.Name, TypeDeclaration.Wrong);
            functionDeclaration.FunctionSymbol = functionSymbol;

            // Adiciona a função na tabela de símbolos e automaticamente inicia um novo escopo.
            SymbolsTable.AddSymbol(functionSymbol);


            // Verifica os parâmetros da função.
            functionSymbol.Parameters = AddFunctionParametersToSymbolsTable(functionDeclaration);


            // Verifica o tipo de retorno da função. TODO: verificar se é um tipo primitivo
            bool isReturnTypeValid = (bool)functionDeclaration.ReturnType.Accept(this);
            functionSymbol.Type = isReturnTypeValid ? functionDeclaration.ReturnType : TypeDeclaration.Wrong;

            // Verifica o corpo da função.
            bool isBodyValid = (bool)functionDeclaration.Body.Accept(this);
            functionSymbol.Body = functionDeclaration.Body;

            // Fecha o escopo da função.
            SymbolsTable.CloseCurrentScope();

            return isFunctionNameAvailable && isReturnTypeValid && isBodyValid;
        }

        public object Visit(Variable variable)
        {
            if (variable == null) throw new SemanticException("Unexpected null Variable");
            if (String.IsNullOrEmpty(variable.Name)) throw new SemanticException("Unexpected null name in Variable");

            TypeDeclaration variableType = TypeDeclaration.Wrong;

            bool isVariableDeclared = true;
            Symbol symbol = SymbolsTable.Search(variable.Name);
            if (symbol == null)
            {
                EmitError("Undeclared variable: {0}".FormatWith(variable.Name), variable.LexSymbol);
                isVariableDeclared = false;
            }
            else if (symbol is VariableSymbol)
            {
                var variableSymbol = (VariableSymbol)symbol;
                variable.VariableSymbol = variableSymbol;
                variableType = variableSymbol.Type;
            }
            else
            {
                EmitError("Operand is not a variable: {0}".FormatWith(variable.Name), variable.LexSymbol);
            }

            // Importante: a visita às folhas deve preceder a visita às operações (+,*, etc) [DiadOp e UnaryOp].
            variable.DefineReturnType(variableType);

            return isVariableDeclared;
        }

        public object Visit(UnaryOperation unaryOperation)
        {
            if (unaryOperation == null) throw new SemanticException("Unexpected null UnaryOperation");
            if (unaryOperation.Operand == null) throw new SemanticException("Unexpected null operand in UnaryOperation");
            if (unaryOperation.OperationType != RplOperationType.Minus && unaryOperation.OperationType != RplOperationType.Not) throw new SemanticException("Invalid OperationType in UnaryOperation");

            bool isOperandValid = (bool)unaryOperation.Operand.Accept(this);
            TypeDeclaration operandType = isOperandValid ? unaryOperation.Operand.ReturnType : TypeDeclaration.Wrong;

            if (unaryOperation.OperationType == RplOperationType.Minus)
            {
                if (operandType == TypeDeclaration.Int || operandType == TypeDeclaration.Float) return true;
            }
            else if (unaryOperation.OperationType == RplOperationType.Not)
            {
                if (operandType == TypeDeclaration.Bool) return true;
            }
            else
            {
                EmitError("Invalid unary operator", unaryOperation.LexSymbol);
            }

            return false;
        }

        public object Visit(DyadicOperation dyadicOperation)
        {
            if (dyadicOperation == null) throw new SemanticException("Unexpected null DyadicOperation");
            if (dyadicOperation.Operand1 == null) throw new SemanticException("Unexpected null Operand1 in DyadicOperation");
            if (dyadicOperation.Operand2 == null) throw new SemanticException("Unexpected null Operand2 in DyadicOperation");

            bool isOperand1Valid = (bool)dyadicOperation.Operand1.Accept(this);
            bool isOperand2Valid = (bool)dyadicOperation.Operand2.Accept(this);

            if (!(isOperand1Valid && isOperand2Valid))
            {
                return false;
            }


            RplOperationType operationType = dyadicOperation.OperationType;
            TypeDeclaration operand1Type = dyadicOperation.Operand1.ReturnType;
            TypeDeclaration operand2Type = dyadicOperation.Operand2.ReturnType;

            if (operand1Type == null) throw new SemanticException("Unexpected null Operand1 type in DyadicOperation");
            if (operand2Type == null) throw new SemanticException("Unexpected null Operand2 type in DyadicOperation");

            // se for concatençao de string, tratamos separado continua.
            if (operationType == RplOperationType.Add && operand1Type == TypeDeclaration.String && operand2Type == TypeDeclaration.String)
            {
                return true;
            }

            switch (operationType)
            {
                case RplOperationType.Add:
                case RplOperationType.Subtract:
                case RplOperationType.Multiply:
                case RplOperationType.Divide:
                case RplOperationType.Module:
                    if (operand1Type.IsNumeric() && operand2Type.IsNumeric())
                    {
                        return true;
                    }
                    EmitError(
                        "Invalid operand type in arithmetic operation. Operand1Type: {0}, operand2Type: {1}".FormatWith(
                            operand1Type.Name, operand2Type.Name), dyadicOperation.LexSymbol);
                    return false;

                case RplOperationType.Equal:
                case RplOperationType.NotEqual:
                case RplOperationType.GreaterThan:
                case RplOperationType.LessThan:
                case RplOperationType.GreaterOrEqual:
                case RplOperationType.LessOrEqual:
                    if (operand1Type.IsComparableTo(operand2Type))
                    {
                        return true;
                    }
                    EmitError("Invalid operand type in comparission", dyadicOperation.LexSymbol);
                    return false;

                case RplOperationType.And:
                case RplOperationType.Or:
                    if (operand1Type == TypeDeclaration.Bool && operand2Type == TypeDeclaration.Bool)
                    {
                        return true;
                    }
                    EmitError("Invalid operand in boolean operation", dyadicOperation.LexSymbol);
                    return false;

                default:
                    throw new SemanticException("Unexpected dyadic operation type");
            }
        }

        public object Visit(TupleConstant tuple)
        {
            /**
            * O que deve ser feito:
            *     percorrer cada elemento da tupla, verificar se são todos do mesmo tipo e
            *     definir o tipo de retorno como sendo o tipo dos elementos, criado como VectorTypeSymb.
            */

            if (tuple == null) throw new SemanticException("Unexpected null TupleConstant");
            if (tuple.Elements == null) throw new SemanticException("Unexpected null elements in TupleConstant");

            if (tuple.Elements.Count == 0)
            {
                EmitError("Empty tuple is not allowed", tuple.LexSymbol);
                return false;
            }

            TypeDeclaration elementType = tuple.Elements[0].ReturnType;
            bool areAllElementsOfTheSameType = true;
            bool areAllElementsValid = true;
            foreach (var expression in tuple.Elements)
            {
                if (expression.ReturnType == null) throw new SemanticException("Unexpected null element type");
                areAllElementsOfTheSameType = areAllElementsOfTheSameType && (expression.ReturnType == elementType);
                areAllElementsValid = areAllElementsValid && (bool)expression.Accept(this);
            }

            if (!(areAllElementsOfTheSameType && areAllElementsValid))
            {
                tuple.DefineReturnType(TypeDeclaration.Wrong);
                return false;
            }

            tuple.DefineReturnType(new VectorTypeDeclaration(tuple.Elements.Count, elementType, tuple.LexSymbol));
            return true;
        }

        public object Visit(ReturnStatement returnStatement)
        {
            // Verifica se o tipo de retorno é válido e é o mesmo que o tipo definido para a função.

            if (returnStatement == null) throw new SemanticException("Unexpected null ReturnStatement");

            Expression returnValue = returnStatement.ValueToReturn;
            TypeDeclaration returnType = null;
            bool isReturnValueValid = true; // Começa como valido pois se não retornar nada (for void) é válido!
            if (returnValue != null)
            {
                isReturnValueValid = (bool)returnValue.Accept(this);
                if (isReturnValueValid)
                {
                    returnType = returnValue.ReturnType;
                }
            }

            FunctionSymbol currentFunction = SymbolsTable.GetCurrentFunction();
            if (currentFunction == null)
            {
                EmitError("Invalid return statement (not a function)", returnStatement.LexSymbol);
                return false;
            }
            returnStatement.FunctionSymbol = currentFunction;

            if ((returnType == currentFunction.Type) ||
                (returnType == null && currentFunction.Type == TypeDeclaration.Void) ||
                (returnType == TypeDeclaration.Void && currentFunction.Type == null))
            {
                return true;
            }

            string returnTypeName = (returnType == null) ? "null" : returnType.Name;
            string functionTypeName = (currentFunction.Type == null) ? "null" : currentFunction.Type.Name;
            EmitError("Invalid return value type: {0}. Expected: {1}".FormatWith(returnTypeName, functionTypeName), returnStatement.LexSymbol);
            return false;
        }

        public object Visit(CompoundStatement compoundStatement)
        {
            if (compoundStatement == null) throw new SemanticException("Unexpected null CompoundStatement");
            if (compoundStatement.Declarations == null) throw new SemanticException("Unexpected null declarations list in CompoundStatement");
            if (compoundStatement.Statements == null) throw new SemanticException("Unexpected null statements list in CompoundStatement");

            bool isCompoundStatementValid = true;

            foreach (var declaration in compoundStatement.Declarations)
            {
                if (declaration == null) throw new SemanticException("Unexpected null declaration in declarations list in CompoundStatement");
                isCompoundStatementValid = isCompoundStatementValid && (bool)declaration.Accept(this);
            }

            foreach (var statement in compoundStatement.Statements)
            {
                if (statement == null) throw new SemanticException("Unexpected null statement in statements list in CompoundStatement");
                isCompoundStatementValid = isCompoundStatementValid && (bool)statement.Accept(this);
            }

            return isCompoundStatementValid;
        }

        public object Visit(IfStatement ifStatement)
        {
            if (ifStatement == null) throw new SemanticException("Unexpected null IfStatement");
            if (ifStatement.Condition == null) throw new SemanticException("Unexpected null condition in IfStatement");

            // Valida a condição
            bool isConditionValid = (bool)ifStatement.Condition.Accept(this);
            if (isConditionValid)
            {
                if (ifStatement.Condition.ReturnType != TypeDeclaration.Bool)
                {
                    EmitError("Boolean condition expected", ifStatement.LexSymbol);
                    isConditionValid = false;
                }
            }

            // Valida o then
            bool isThenPartValid = (ifStatement.ThenPart == null || (bool)ifStatement.ThenPart.Accept(this));

            // Valid o else
            bool isElsePartValid = (ifStatement.ElsePart == null || (bool)ifStatement.ElsePart.Accept(this));

            return isConditionValid && isThenPartValid && isElsePartValid;
        }

        public object Visit(WhileStatement whileStatement)
        {
            if (whileStatement == null) throw new SemanticException("Unexpected null WhileStatement");
            if (whileStatement.Condition == null) throw new SemanticException("Unexpected null condition in WhileStatement");

            // Valida a condição
            bool isConditionValid = (bool)whileStatement.Condition.Accept(this);
            if (isConditionValid)
            {
                if (whileStatement.Condition.ReturnType != TypeDeclaration.Bool)
                {
                    EmitError("Boolean condition expected", whileStatement.LexSymbol);
                    isConditionValid = false;
                }
            }

            // Valida o corpo do while
            bool isBodyValid = (whileStatement.Body == null || (bool)whileStatement.Body.Accept(this));

            return isConditionValid && isBodyValid;
        }

        public object Visit(FunctionCall functionCall)
        {
            /*
             * O principal objetivo desse método é validar os parâmetros passados para a função 
             * com os parâmetros esperados pela função.
             */

            if (functionCall == null) throw new SemanticException("Unexpected null FunctionCall");

            FunctionSymbol functionSymbol = GetFunctionSymbol(functionCall);

            // Só continua se a função existir (aka estiver na tabela de símbolos).
            if (functionSymbol != null)
            {
                // Valida (visita) os parâmetros passados para a função.
                bool areParametersValid = true;
                foreach (var parameter in functionCall.Parameters)
                {
                    areParametersValid = areParametersValid && (bool)parameter.Accept(this);
                }

                // Ajusta as informações da chamada de função.
                functionCall.FunctionSymbol = functionSymbol;

                // Verifica se os parâmetros passados são compatíveis com os parâmetros esperados.
                bool areParametersCompatible = ArePassedParametersCompatibleWithExpectedParameters(functionCall, functionSymbol);

                // Verifica se o tipo de retorno da função é void. Se for, está errado! FunctionCall não pode ser VOID, só FunctionCallStatement!
                bool isReturnTypeVoidOrNull = ((functionCall.ReturnType == null) || (functionCall.ReturnType == TypeDeclaration.Void));


                return !isReturnTypeVoidOrNull && areParametersValid && areParametersCompatible;
            }

            return false;
        }

        public object Visit(FunctionCallStatement functionCallStatement)
        {
            /*
             * O principal objetivo desse método é validar os parâmetros passados para a função 
             * com os parâmetros esperados pela função.
             */

            if (functionCallStatement == null) throw new SemanticException("Unexpected null FunctionCallStatement");

            FunctionSymbol functionSymbol = GetFunctionSymbol(functionCallStatement);

            // Só continua se a função existir (aka estiver na tabela de símbolos).
            if (functionSymbol != null)
            {
                // Valida (visita) os parâmetros passados para a função.
                bool areParametersValid = true;
                foreach (var parameter in functionCallStatement.Parameters)
                {
                    areParametersValid = areParametersValid && (bool)parameter.Accept(this);
                }

                // Ajusta as informações da chamada de função.
                functionCallStatement.FunctionSymbol = functionSymbol;

                // Verifica se os parâmetros passados são compatíveis com os parâmetros esperados.
                bool areParametersCompatible = ArePassedParametersCompatibleWithExpectedParameters(functionCallStatement, functionSymbol);

                // Verifica se o tipo de retorno da função é void. Se for, está certo! FunctionCallStatement só pode ser void!
                bool isVoid = (functionCallStatement.ReturnType == TypeDeclaration.Void);

                return isVoid && areParametersValid && areParametersCompatible;
            }

            return false;
        }

        public object Visit(Assignment assignment)
        {
            if (assignment == null) throw new SemanticException("Unexpected null Assignment");
            if (assignment.Variable == null) throw new SemanticException("Unexpected null variable in Assignment");
            if (assignment.Value == null) throw new SemanticException("Unexpected null value in Assignment");

            bool isVariableValid = (bool)assignment.Variable.Accept(this);
            bool isValueValid = (bool)assignment.Value.Accept(this);
            bool isAssignmentCompatible = IsAssignmentCompatible(assignment.Variable.ReturnType, assignment.Value.ReturnType);

            if (!isVariableValid) EmitError("Error in assignment: Invalid variable", assignment.LexSymbol);
            if (!isValueValid) EmitError("Error in assignment: Invalid value", assignment.LexSymbol);
            if (!isAssignmentCompatible) EmitError("Error in assignment: Incompatible types", assignment.LexSymbol);

            return isVariableValid && isValueValid && isAssignmentCompatible;
        }

        public object Visit(IndexingOperation indexing)
        {
            if (indexing == null) throw new SemanticException("Unexpected null IndexingOperation");
            if (indexing.Operand1 == null) throw new SemanticException("Unexpected null operand1 in IndexingOperation");
            if (indexing.Operand2 == null) throw new SemanticException("Unexpected null operand2 in IndexingOperation");

            bool areOperandsValid = ((bool)indexing.Operand1.Accept(this)) && ((bool)indexing.Operand2.Accept(this));

            // Verifica se o segundo argumento (argumento entre []) é inteiro
            if (indexing.Operand2.ReturnType != TypeDeclaration.Int)
            {
                EmitError("Index should be an index expression", indexing.LexSymbol);
                return false;
            }

            // Verifica se o primeiro argumento é indexavel (é um array).
            if (!IsIndexable(indexing.Operand1))
            {
                EmitError("Invalid index operation", indexing.LexSymbol);
                return false;
            }

            return areOperandsValid;
        }

        public object Visit(ProgramDescription program)
        {
            if (program == null) throw new SemanticException("Unexpected null ProgramDescription");
            if (program.Body == null) throw new SemanticException("Unexpected null body in ProgramDescription");
            if (program.Declarations == null) throw new SemanticException("Unexpected null declarations list in ProgramDescription");
            if (SymbolsTable.CurrentScope == null) throw new SemanticException("Unexpected null currentScope in SymbolsTable");

            // Sets the scope of the program.
            program.Scope = SymbolsTable.CurrentScope;

            bool areDeclarationsValid = true;
            foreach (var declaration in program.Declarations)
            {
                areDeclarationsValid = areDeclarationsValid && (bool)declaration.Accept(this);
            }

            bool isBodyValid = (bool)program.Body.Accept(this);

            return areDeclarationsValid && isBodyValid;
        }

        #endregion
    }
}
