using System.Collections.Generic;

namespace Seculus.MobileScript.Core
{
    /// <summary>
    /// Validador de objetos
    /// </summary>
    /// <typeparam name="T">Tipo do objeto que sofrerá validação.</typeparam>
    public interface IValidator<in T>
    {
        /// <summary>
        /// Valida um objeto. 
        /// </summary>
        /// <param name="entity">Objeto que sofrerá a validação.</param>
        /// <returns>
        /// Caso o objeto possua uma ou mais regras infringidas uma lista contendo mensagens que as descrevam, caso contrário uma lista vazia.
        /// </returns>
        IList<string> ViolatedRules(T entity);

    }
}
