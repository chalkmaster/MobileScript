namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine.Serialization
{
    /// <summary>
    /// Serializador do programa.
    /// </summary>
    public interface IProgramSerializer
    {
        /// <summary>
        /// Serializa
        /// </summary>
        /// <param name="program">Programa a ser serializado.</param>
        /// <returns>String contendo o programa serializado.</returns>
        string Serialize(Program program);

        /// <summary>
        /// Deserializa o programa.
        /// </summary>
        /// <param name="str">String contento o programa serializado.</param>
        /// <returns>Programa.</returns>
        Program Deserialize(string str);
    }
}
