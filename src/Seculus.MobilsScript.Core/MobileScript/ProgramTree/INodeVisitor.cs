using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Expressions.Statements;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    /// <summary>
    /// Define os métodos que devem ser implementados por um Visitor. 
    /// (um método Visit() para cada classe concreta da hierarquia baseada em AbsNode).
    /// </summary>
    public interface INodeVisitor
    {
        object Visit(Constant constant);
        object Visit(VariableDeclaration variableDeclaration);
        object Visit(VariableDeclarationList variableDeclarationList);
        object Visit(TypeDeclaration type);
        object Visit(VectorTypeDeclaration vectorType);
        object Visit(FunctionDeclaration functionDeclaration);
        object Visit(Variable variable);
        object Visit(UnaryOperation unaryOperation);
        object Visit(DyadicOperation dyadicOperation);
        object Visit(TupleConstant tuple);
        object Visit(ReturnStatement returnStatement);
        object Visit(CompoundStatement compoundStatement);
        object Visit(IfStatement ifStatement);
        object Visit(WhileStatement whileStatement);
        object Visit(FunctionCall functionCall);
        object Visit(FunctionCallStatement functionCallStatement);
        object Visit(Assignment assignment);
        object Visit(IndexingOperation indexing);
        object Visit(ProgramDescription program);
    }
}
