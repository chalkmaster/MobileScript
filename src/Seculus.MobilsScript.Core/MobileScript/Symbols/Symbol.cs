using Seculus.MobileScript.Core.Extensions;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Representa um símbolo
    /// </summary>
    public abstract class Symbol
    {
        #region Constructors

        protected Symbol(int level, SymbolKind kind) : this(null, level, kind) { }

        protected Symbol(string name, int level, SymbolKind kind)
        {
            if (name == null)
            {
                name = GenerateName();
            }

            Name = name;
            Level = level;
            Kind = kind;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Nome do símbolo
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Nível de "encaixamento" do símbolo no programa. 
        /// 0 - pré-definido.
        /// 1 - global.
        /// 2 - local
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Categoria do símbolo.
        /// </summary>
        public SymbolKind Kind { get; private set; }

        #endregion

        #region Static Members

        private static int _nameCounter = 0;

        private static string GenerateName()
        {
            return "#{0}".FormatWith(_nameCounter++);
        }

        #endregion
    }
}
