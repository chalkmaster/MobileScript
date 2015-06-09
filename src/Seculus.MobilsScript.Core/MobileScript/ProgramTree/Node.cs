using Seculus.MobileScript.Core.MobileScript.Compiler;

namespace Seculus.MobileScript.Core.MobileScript.ProgramTree
{
    /// <summary>
    /// Classe abstrata a partir da qual são derivadas as classes que representam os nós da árvore de programa.
    /// Prevê os principais métodos necessários à verificação semântica.
    /// Um nó pode ser de dois macro-tipos: Expressão ou Declaração.
    /// </summary>
    public abstract class Node
    {
        #region Properties

        /// <summary>
        /// Símbolo léxico que representa esse nó da árvore do programa.
        /// </summary>
        public LexSymbol LexSymbol { get; private set; }

        #endregion

        #region Constructors

        protected Node(LexSymbol symbol)
        {
            LexSymbol = symbol;
        }
        
        #endregion

        #region Abstract Methods

        /// <summary>
        /// Através deste método o objeto 'visitor' solicita a visita a um objeto da árvore de programa.
        /// </summary>
        /// <param name="visitor">Visitor que será o responsável efetivo pela visita.</param>
        /// <returns>Um objeto que é o resultado da visita.</returns>
        public abstract object Accept(INodeVisitor visitor);

        #endregion
    }
}
