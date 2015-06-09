using System.Collections.Generic;
using Seculus.MobileScript.Core.Helpers;

namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Table de instruções.
    /// </summary>
    public class InstructionsTable
    {
        #region Singleton

        private static volatile InstructionsTable _instance;
        private static readonly object SingletonSync = new object();
        public static InstructionsTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SingletonSync)
                    {
                        if (_instance == null)
                        {
                            _instance = new InstructionsTable();
                        }
                    }
                }

                return _instance;
            }
        }

        private InstructionsTable()
        {
            FillMaps();
        }

        #endregion

        #region Fields

        private Dictionary<InstructionCode, string> _codesMap = new Dictionary<InstructionCode, string>();
        private Dictionary<string, InstructionCode> _namesMap = new Dictionary<string, InstructionCode>();

        #endregion

        #region Private Methods

        private void FillMaps()
        {
            var instructions = EnumHelper.GetDisplayNameDictionary(typeof (InstructionCode));
            foreach (var instruction in instructions)
            {
                _codesMap.Add((InstructionCode)instruction.Key, instruction.Value);
                _namesMap.Add(instruction.Value, (InstructionCode)instruction.Key);
            }
        }

        #endregion

        #region Public Methods

        public InstructionCode GetCode(string name)
        {
            return _namesMap[name];
        }

        public string GetName(InstructionCode code)
        {
            return _codesMap[code];
        }

        #endregion
    }
}
