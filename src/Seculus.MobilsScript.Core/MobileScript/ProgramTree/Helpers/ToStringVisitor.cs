using Seculus.MobileScript.Core.Extensions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree.Helpers
{
    /// <summary>
    /// Percorre a árvore do programa e gera uma string contendo o código fonte (engenharia reversa).
    /// </summary>
    public class ToStringVisitor : INodeVisitor
    {
        #region Fields

        private readonly SourceCodeBuilder _codeBuilder = new SourceCodeBuilder();

        #endregion

        #region Private Methods

        private string GetOperationTypeAsCodeString(RplOperationType operationType)
        {
            switch (operationType)
            {
                case RplOperationType.Add:
                    return "+";
                case RplOperationType.Subtract:
                    return "-";
                case RplOperationType.Multiply:
                    return "*";
                case RplOperationType.Divide:
                    return "/";
                case RplOperationType.Module:
                    return "%";
                case RplOperationType.Equal:
                    return "==";
                case RplOperationType.NotEqual:
                    return "!=";
                case RplOperationType.GreaterThan:
                    return ">";
                case RplOperationType.LessThan:
                    return "<";
                case RplOperationType.GreaterOrEqual:
                    return ">=";
                case RplOperationType.LessOrEqual:
                    return "<=";
                case RplOperationType.And:
                    return "&&";
                case RplOperationType.Or:
                    return "||";
                case RplOperationType.Not:
                    return "!";
                case RplOperationType.Minus:
                    return "-";
                default:
                    return "???";
            }
        }

        private void GenerateFunctionHeaderCode(FunctionDeclaration functionDeclaration)
        {
            GenerateCode(functionDeclaration.ReturnType, true);
            _codeBuilder.Append(" ");
            _codeBuilder.Append(functionDeclaration.Name);

            _codeBuilder.Append("(");
            if (functionDeclaration.Parameters != null && functionDeclaration.Parameters.Count > 0)
            {
                foreach (var parameter in functionDeclaration.Parameters)
                {
                    GenerateCode(parameter, true, true, true, true);
                    _codeBuilder.Append(", ");
                }

                if (_codeBuilder.Length >= 2 && _codeBuilder[_codeBuilder.Length - 1] == ' ' && _codeBuilder[_codeBuilder.Length - 2] == ',')
                {
                    _codeBuilder.Length -= 2;
                }
            }
            _codeBuilder.Append(")");
        }
        private void GenerateFunctionBodyCode(FunctionDeclaration functionDeclaration)
        {
            _codeBuilder.Append(" {");
            _codeBuilder.NewLine();
            _codeBuilder.OpenScope();
            functionDeclaration.Body.Accept(this);
            _codeBuilder.CloseScope();
            _codeBuilder.Append("}");
            _codeBuilder.NewLine();
        }

        private void GenerateCode(Constant constant)
        {
            if (constant.ReturnType == TypeDeclaration.String)
            {
                _codeBuilder.Append("\"{0}\"".FormatWith(constant.Value));
            }
            else
            {
                _codeBuilder.Append(constant.Value);
            }
        }
        private void GenerateCode(VariableDeclaration variableDeclaration, bool printRef, bool printType, bool printName, bool printInitialValue)
        {
            if (printRef)
            {
                if (variableDeclaration.IsRef)
                {
                    _codeBuilder.Append("ref ");
                }
            }

            if (printType)
            {
                variableDeclaration.Type.Accept(this);
                _codeBuilder.Append(" ");
            }

            if (printName)
            {
                _codeBuilder.Append(variableDeclaration.Name);
                var vectorType = variableDeclaration.Type as VectorTypeDeclaration;
                while (vectorType != null)
                {
                    _codeBuilder.Append(vectorType.Size >= 0 ? "[{0}]".FormatWith(vectorType.Size) : "[]");
                    vectorType = vectorType.ElementType as VectorTypeDeclaration;
                }
                _codeBuilder.Append(" ");
            }

            if (printInitialValue)
            {
                if (variableDeclaration.InitialValue != null)
                {
                    _codeBuilder.Append("= ");
                    variableDeclaration.InitialValue.Accept(this);
                    _codeBuilder.Append(" ");
                }
            }

            // Remove o último espaço em branco.
            if ((_codeBuilder.Length > 0) && (_codeBuilder[_codeBuilder.Length - 1] == ' '))
            {
                _codeBuilder.Length--;
            }
        }
        private void GenerateCode(VariableDeclarationList variableDeclarationList)
        {
            GenerateCode(variableDeclarationList.Type, false);
            foreach (var variableDeclaration in variableDeclarationList.VariablesDeclarations)
            {
                _codeBuilder.Append(" ");
                GenerateCode(variableDeclaration, false, false, true, true);
                _codeBuilder.Append(",");
            }

            if ((_codeBuilder.Length > 0) && (_codeBuilder[_codeBuilder.Length - 1] == ','))
            {
                _codeBuilder.Length--;
            }
            _codeBuilder.Append(";");
            _codeBuilder.NewLine();
        }
        private void GenerateCode(TypeDeclaration typeDeclaration, bool printSquareBracketIfItsArray)
        {
            _codeBuilder.Append(typeDeclaration.Name);
        }
        private void GenerateCode(VectorTypeDeclaration typeDeclaration, bool printSquareBracketIfItsArray)
        {
            GenerateCode(typeDeclaration.ElementType, true);
            if (printSquareBracketIfItsArray)
            {
                _codeBuilder.Append("[]");
            }
        }
        private void GenerateCode(FunctionDeclaration functionDeclaration)
        {
            GenerateFunctionHeaderCode(functionDeclaration);
            GenerateFunctionBodyCode(functionDeclaration);
            _codeBuilder.NewLine();
        }
        private void GenerateCode(Variable variable)
        {
            _codeBuilder.Append(variable.Name);
        }
        private void GenerateCode(Expression expression, bool wrapWithParentheses)
        {
            if (wrapWithParentheses) _codeBuilder.Append("(");
            expression.Accept(this);
            if (wrapWithParentheses) _codeBuilder.Append(")");
        }
        private void GenerateCode(UnaryOperation unaryOperation)
        {
            // "Operator Operand2"
            _codeBuilder.Append(GetOperationTypeAsCodeString(unaryOperation.OperationType));
            GenerateCode(unaryOperation.Operand, unaryOperation.Operand is DyadicOperation);
        }
        private void GenerateCode(DyadicOperation dyadicOperation)
        {
            // Operand1 Operator Operand2
            GenerateCode(dyadicOperation.Operand1, dyadicOperation.Operand1 is DyadicOperation);
            _codeBuilder.Append(" ");
            _codeBuilder.Append(GetOperationTypeAsCodeString(dyadicOperation.OperationType));
            _codeBuilder.Append(" ");
            GenerateCode(dyadicOperation.Operand2, dyadicOperation.Operand2 is DyadicOperation);
        }
        private void GenerateCode(TupleConstant tuple)
        {
            _codeBuilder.Append("{");

            foreach (var expression in tuple.Elements)
            {
                _codeBuilder.Append(" ");
                expression.Accept(this);
                _codeBuilder.Append(",");
            }

            // Remove a última vírgula.
            if ((_codeBuilder.Length > 0) && (_codeBuilder[_codeBuilder.Length - 1] == ','))
            {
                _codeBuilder.Length--;
            }

            _codeBuilder.Append(" }");
        }
        private void GenerateCode(ReturnStatement returnStatement)
        {
            _codeBuilder.Append("return ");
            returnStatement.ValueToReturn.Accept(this);
            _codeBuilder.Append(";");
            _codeBuilder.NewLine();
        }
        private void GenerateCode(CompoundStatement compoundStatement)
        {
            foreach (var declaration in compoundStatement.Declarations)
            {
                declaration.Accept(this);
            }

            foreach (var statement in compoundStatement.Statements)
            {
                statement.Accept(this);
            }
        }
        private void GenerateCode(IfStatement ifStatement)
        {
            _codeBuilder.Append("if (");
            ifStatement.Condition.Accept(this);
            _codeBuilder.Append(") ");

            _codeBuilder.Append("{");
            _codeBuilder.OpenScope();
            _codeBuilder.NewLine();
            ifStatement.ThenPart.Accept(this);
            _codeBuilder.CloseScope();
            _codeBuilder.Append("}");

            if (ifStatement.ElsePart != null)
            {
                _codeBuilder.Append(" else {");
                _codeBuilder.OpenScope();
                _codeBuilder.NewLine();
                ifStatement.ElsePart.Accept(this);
                _codeBuilder.CloseScope();
                _codeBuilder.Append("}");
            }

            _codeBuilder.NewLine();
        }
        private void GenerateCode(WhileStatement whileStatement)
        {
            _codeBuilder.Append("while (");
            whileStatement.Condition.Accept(this);
            _codeBuilder.Append(") ");

            _codeBuilder.Append("{");
            _codeBuilder.OpenScope();
            _codeBuilder.NewLine();
            whileStatement.Body.Accept(this);
            _codeBuilder.CloseScope();
            _codeBuilder.Append("}");

            _codeBuilder.NewLine();
        }
        private void GenerateCode(FunctionCall functionCall)
        {
            _codeBuilder.Append(functionCall.Name);

            _codeBuilder.Append("(");
            foreach (var parameterExpression in functionCall.Parameters)
            {
                parameterExpression.Accept(this);
                _codeBuilder.Append(", ");
            }
            if (_codeBuilder.Length >= 2 && _codeBuilder[_codeBuilder.Length - 1] == ' ' && _codeBuilder[_codeBuilder.Length - 2] == ',')
            {
                _codeBuilder.Length -= 2;
            }
            _codeBuilder.Append(")");
        }
        private void GenerateCode(FunctionCallStatement functionCallStatement)
        {
            _codeBuilder.Append(functionCallStatement.Name);

            _codeBuilder.Append("(");
            foreach (var parameterExpression in functionCallStatement.Parameters)
            {
                parameterExpression.Accept(this);
                _codeBuilder.Append(", ");
            }
            if (_codeBuilder.Length >= 2 && _codeBuilder[_codeBuilder.Length - 1] == ' ' && _codeBuilder[_codeBuilder.Length - 2] == ',')
            {
                _codeBuilder.Length -= 2;
            }
            _codeBuilder.Append(");");

            _codeBuilder.NewLine();
        }
        private void GenerateCode(Assignment assignment)
        {
            assignment.Variable.Accept(this);
            _codeBuilder.Append(" = ");
            assignment.Value.Accept(this);
            _codeBuilder.Append(";");
            _codeBuilder.NewLine();
        }
        private void GenerateCode(IndexingOperation indexing)
        {
            indexing.Operand1.Accept(this);
            _codeBuilder.Append("[");
            indexing.Operand2.Accept(this);
            _codeBuilder.Append("]");
        }
        private void GenerateCode(ProgramDescription program)
        {
            foreach (var declaration in program.Declarations)
            {
                declaration.Accept(this);
            }

            if (program.Body != null)
            {
                _codeBuilder.Append("{");
                _codeBuilder.OpenScope();
                _codeBuilder.NewLine();

                program.Body.Accept(this);

                _codeBuilder.CloseScope();
                _codeBuilder.Append("}");
                _codeBuilder.NewLine();
            }
        }

        #endregion

        #region Implementation of IVisitor

        public object Visit(Constant constant)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(constant);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(VariableDeclaration variableDeclaration)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(variableDeclaration, true, true, true, true);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(VariableDeclarationList variableDeclarationList)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(variableDeclarationList);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(TypeDeclaration type)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            // Por default não imprimimos '[]' na frente do tipo pois isso é impresso em frente ao nome da variável (característica da linguagem).
            // O '[]' só é impresso na frente do tipo no caso de retorno de funções.
            GenerateCode(type, false);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(VectorTypeDeclaration vectorType)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            // Por default não imprimimos '[]' na frente do tipo pois isso é impresso em frente ao nome da variável (característica da linguagem).
            // O '[]' só é impresso na frente do tipo no caso de retorno de funções.
            GenerateCode(vectorType, false);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(FunctionDeclaration functionDeclaration)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            // Por default não imprimimos '[]' na frente do tipo pois isso é impresso em frente ao nome da variável (característica da linguagem).
            // O '[]' só é impresso na frente do tipo no caso de retorno de funções.
            GenerateCode(functionDeclaration);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(Variable variable)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(variable);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(UnaryOperation unaryOperation)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(unaryOperation);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(DyadicOperation dyadicOperation)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(dyadicOperation);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(TupleConstant tuple)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(tuple);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(ReturnStatement returnStatement)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(returnStatement);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(CompoundStatement compoundStatement)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(compoundStatement);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(IfStatement ifStatement)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(ifStatement);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(WhileStatement whileStatement)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(whileStatement);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(FunctionCall functionCall)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(functionCall);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(FunctionCallStatement functionCallStatement)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(functionCallStatement);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(Assignment assignment)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(assignment);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(IndexingOperation indexing)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(indexing);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        public object Visit(ProgramDescription program)
        {
            // Keep track of where we started
            int startIndex = _codeBuilder.Length - 1;

            GenerateCode(program);

            // we return just the part generated by this method.
            return _codeBuilder.ToString(startIndex, _codeBuilder.Length - 1);
        }

        #endregion
    }
}
