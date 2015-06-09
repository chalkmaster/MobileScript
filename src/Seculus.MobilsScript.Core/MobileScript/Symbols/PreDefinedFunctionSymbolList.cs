using System.Collections.Generic;
using System.Collections.ObjectModel;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.VirtualMachine;

namespace Seculus.MobileScript.Core.MobileScript.Symbols
{
    /// <summary>
    /// Lista de funções pre-definidas pela linguagem.
    /// </summary>
    public class PreDefinedFunctionSymbolList : ReadOnlyCollection<PreDefinedFunctionSymbol>
    {
        #region Singleton

        private static volatile PreDefinedFunctionSymbolList _instance;
        private static readonly object Sync = new object();

        public static PreDefinedFunctionSymbolList Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new PreDefinedFunctionSymbolList();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Constructors

        private PreDefinedFunctionSymbolList() : base(GetAllFunctions()) { }

        #endregion

        #region Private Static Methods

        private static IList<PreDefinedFunctionSymbol> GetAllFunctions()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.

            // I/O
            list.AddRange(GetInputOutputFunctionsList());

            // DateTime handling
            list.AddRange(GetDateTimeHandlingFunctionsList());

            // String handling
            list.AddRange(GetStringHandlingFunctionsList());

            // Dvice functions
            list.AddRange(GetDeviceFunctionsList());

            // OSMobile specific functions
            list.AddRange(GetOsMobileSpecificFunctions());

            return list;
        }

        private static IEnumerable<PreDefinedFunctionSymbol> GetInputOutputFunctionsList()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
            PreDefinedFunctionSymbol function = null; // variável auxiliar


            // void print(string txt)
            function = new PreDefinedFunctionSymbol("print", TypeDeclaration.Void, PreDefinedFunctionCode.Print);
            function.AddParameter(new ParameterSymbol("txt", TypeDeclaration.String, false));
            list.Add(function);

            // void println()
            function = new PreDefinedFunctionSymbol("println", TypeDeclaration.Void, PreDefinedFunctionCode.PrintLn);
            list.Add(function);


            return list;
        }

        private static IEnumerable<PreDefinedFunctionSymbol> GetDateTimeHandlingFunctionsList()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
            PreDefinedFunctionSymbol function = null; // variável auxiliar

            // string now()
            function = new PreDefinedFunctionSymbol("now", TypeDeclaration.String, PreDefinedFunctionCode.Now);
            list.Add(function);


            return list;
        }

        private static IEnumerable<PreDefinedFunctionSymbol> GetStringHandlingFunctionsList()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
            PreDefinedFunctionSymbol function = null; // variável auxiliar


            // string substring(string str, int start, int end)
            function = new PreDefinedFunctionSymbol("substring", TypeDeclaration.String, PreDefinedFunctionCode.Substring);
            function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("start", TypeDeclaration.Int, false));
            function.AddParameter(new ParameterSymbol("end", TypeDeclaration.Int, false));
            list.Add(function);

            // int strlen(string str)
            function = new PreDefinedFunctionSymbol("strlen", TypeDeclaration.Int, PreDefinedFunctionCode.StrLen);
            function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
            list.Add(function);

            // int charVal(string chr)
            function = new PreDefinedFunctionSymbol("charVal", TypeDeclaration.Int, PreDefinedFunctionCode.CharVal);
            function.AddParameter(new ParameterSymbol("chr", TypeDeclaration.String, false));
            list.Add(function);

            // int charValAt(string str, int index)
            function = new PreDefinedFunctionSymbol("charValAt", TypeDeclaration.Int, PreDefinedFunctionCode.CharValAt);
            function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("index", TypeDeclaration.Int, false));
            list.Add(function);

            // string buildStringFromChars(int[] charVals, int length)
            function = new PreDefinedFunctionSymbol("buildStringFromChars", TypeDeclaration.String, PreDefinedFunctionCode.BuildStringFromChars);
            function.AddParameter(new ParameterSymbol("charVals", new VectorTypeDeclaration(null, TypeDeclaration.Int, TypeDeclaration.Int.LexSymbol), true));
            function.AddParameter(new ParameterSymbol("length", TypeDeclaration.Int, false));
            list.Add(function);


            return list;
        }
        
        private static IEnumerable<PreDefinedFunctionSymbol> GetDeviceFunctionsList()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
            PreDefinedFunctionSymbol function = null; // variável auxiliar


            // string getImei()
            function = new PreDefinedFunctionSymbol("getImei", TypeDeclaration.String, PreDefinedFunctionCode.GetImei);
            list.Add(function);

            // boolean sendEmail(string to, string title, string body)
            function = new PreDefinedFunctionSymbol("sendEmail", TypeDeclaration.Bool, PreDefinedFunctionCode.SendEmail);
            function.AddParameter(new ParameterSymbol("to", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("title", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("body", TypeDeclaration.String, false));
            list.Add(function);

            // boolean sendSms(string phone, string message)
            function = new PreDefinedFunctionSymbol("sendSms", TypeDeclaration.Bool, PreDefinedFunctionCode.SendSms);
            function.AddParameter(new ParameterSymbol("phone", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, false));
            list.Add(function);


            return list;
        }

        private static IEnumerable<PreDefinedFunctionSymbol> GetOsMobileSpecificFunctions()
        {
            var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
            PreDefinedFunctionSymbol function = null; // variável auxiliar


            /*
             * OUTPUT METHODS 
             */

            // void showDialog(string title, string message)
            function = new PreDefinedFunctionSymbol("showDialog", TypeDeclaration.Void, PreDefinedFunctionCode.ShowDialog);
            function.AddParameter(new ParameterSymbol("title", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, false));
            list.Add(function);

            // void showSmartReminder(string message, int duration)
            function = new PreDefinedFunctionSymbol("showSmartReminder", TypeDeclaration.Void, PreDefinedFunctionCode.ShowSmartReminder);
            function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("duration", TypeDeclaration.Int, false)); // millisenconds
            list.Add(function);

            // void showToast(string message)
            function = new PreDefinedFunctionSymbol("showToast", TypeDeclaration.Void, PreDefinedFunctionCode.ShowToast);
            function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, false));
            list.Add(function);






            /*
             * APP METHODS
             */

            // string getSubscriberId()
            function = new PreDefinedFunctionSymbol("getSubscriberId", TypeDeclaration.String, PreDefinedFunctionCode.GetSubscriberId);
            list.Add(function);

            // int getUserId()
            function = new PreDefinedFunctionSymbol("getUserId", TypeDeclaration.Int, PreDefinedFunctionCode.GetUserId);
            list.Add(function);

            // string getVersion()
            function = new PreDefinedFunctionSymbol("getVersion", TypeDeclaration.String, PreDefinedFunctionCode.GetVersion);
            list.Add(function);

            // boolean callService(string serviceId, string serviceParm, ref string message, ref string route)
            function = new PreDefinedFunctionSymbol("callService", TypeDeclaration.Bool, PreDefinedFunctionCode.CallService);
            function.AddParameter(new ParameterSymbol("serviceId", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("serviceParameter", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("route", TypeDeclaration.String, true));
            list.Add(function);

            // string getCurrentModelName()
            function = new PreDefinedFunctionSymbol("getCurrentModelName", TypeDeclaration.String, PreDefinedFunctionCode.GetCurrentModelName);
            list.Add(function);

            // void startSynchronization()
            function = new PreDefinedFunctionSymbol("startSynchronization", TypeDeclaration.Void, PreDefinedFunctionCode.StartSynchronization);
            list.Add(function);

            // void startLocationCapture()
            function = new PreDefinedFunctionSymbol("startLocationCapture", TypeDeclaration.Void, PreDefinedFunctionCode.StartLocationCapture);
            list.Add(function);








            /*
             * SPECIFIC SERVICE REQUEST (OS) METHODS
             */


            // string getCurrentActivityName()
            function = new PreDefinedFunctionSymbol("getCurrentActivityName", TypeDeclaration.String, PreDefinedFunctionCode.GetCurrentActivityName);
            list.Add(function);

            // string getVarValue(string varName)
            function = new PreDefinedFunctionSymbol("getVarValue", TypeDeclaration.String, PreDefinedFunctionCode.GetVarValue);
            function.AddParameter(new ParameterSymbol("varName", TypeDeclaration.String, false));
            list.Add(function);

            // void setVarValue(string varName, string varValue)
            function = new PreDefinedFunctionSymbol("setVarValue", TypeDeclaration.Void, PreDefinedFunctionCode.SetVarValue);
            function.AddParameter(new ParameterSymbol("varName", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("varValue", TypeDeclaration.String, false));
            list.Add(function);

            // void goToRoute(string routeName)
            function = new PreDefinedFunctionSymbol("goToRoute", TypeDeclaration.Void, PreDefinedFunctionCode.GoToRoute);
            function.AddParameter(new ParameterSymbol("routeName", TypeDeclaration.String, false));
            list.Add(function);

            // int countTransitions(string activityName)
            function = new PreDefinedFunctionSymbol("countTransitions", TypeDeclaration.Int, PreDefinedFunctionCode.CountTransitions);
            function.AddParameter(new ParameterSymbol("activityName", TypeDeclaration.String, false));
            list.Add(function);

            // int getTransition(string activityName, int index, ref string dateTime, ref string value, ref bool sync)
            function = new PreDefinedFunctionSymbol("getTransition", TypeDeclaration.Int, PreDefinedFunctionCode.GetTransition); // retorna o ID da transição
            function.AddParameter(new ParameterSymbol("activityName", TypeDeclaration.String, false));
            function.AddParameter(new ParameterSymbol("index", TypeDeclaration.Int, false));
            function.AddParameter(new ParameterSymbol("dateTime", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("value", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("sync", TypeDeclaration.Bool, true));
            list.Add(function);

            // bool getHeader(ref string code, ref string customer, ref string address, ref string scheduledDateTime, ref string timeToComplete, ref string obs)
            function = new PreDefinedFunctionSymbol("getHeader", TypeDeclaration.Bool, PreDefinedFunctionCode.GetHeader);
            function.AddParameter(new ParameterSymbol("code", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("customer", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("address", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("scheduledDateTime", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("timeToComplete", TypeDeclaration.String, true));
            function.AddParameter(new ParameterSymbol("obs", TypeDeclaration.String, true));
            list.Add(function);


            return list;
        }

        #endregion

        #region Private Static Methods - OLD

        //private static IList<PreDefinedFunctionSymbol> GetAllFunctions()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.

        //    // I/O
        //    list.AddRange(GetInputOutputFunctionsList());

        //    // Type Conversion
        //    list.AddRange(GetTypeConversionFunctionsList());

        //    // DateTime handling
        //    list.AddRange(GetDateTimeHandlingFunctionsList());

        //    // String handling
        //    list.AddRange(GetStringHandlingFunctionsList());

        //    // OSMobile specific functions
        //    list.AddRange(GetOsMobileSpecificFunctions());

        //    return list;
        //}

        //private static IEnumerable<PreDefinedFunctionSymbol> GetInputOutputFunctionsList()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
        //    PreDefinedFunctionSymbol function = null; // variável auxiliar


        //    // void print(string txt)
        //    function = new PreDefinedFunctionSymbol("print", TypeDeclaration.Void, PreDefinedFunctionCode.Print);
        //    function.AddParameter(new ParameterSymbol("txt", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // void println()
        //    function = new PreDefinedFunctionSymbol("println", TypeDeclaration.Void, PreDefinedFunctionCode.PrintLn);
        //    list.Add(function);


        //    return list;
        //}

        //private static IEnumerable<PreDefinedFunctionSymbol> GetTypeConversionFunctionsList()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
        //    PreDefinedFunctionSymbol function = null; // variável auxiliar


        //    // string intToStr(int val)
        //    function = new PreDefinedFunctionSymbol("intToStr", TypeDeclaration.String, PreDefinedFunctionCode.IntToStr);
        //    function.AddParameter(new ParameterSymbol("val", TypeDeclaration.Int, false));
        //    list.Add(function);

        //    // string floatToStr(float val)
        //    function = new PreDefinedFunctionSymbol("floatToStr", TypeDeclaration.String, PreDefinedFunctionCode.FloatToStr);
        //    function.AddParameter(new ParameterSymbol("val", TypeDeclaration.Float, false));
        //    list.Add(function);

        //    // string boolToStr(boolean val)
        //    function = new PreDefinedFunctionSymbol("boolToStr", TypeDeclaration.String, PreDefinedFunctionCode.BoolToStr);
        //    function.AddParameter(new ParameterSymbol("val", TypeDeclaration.Bool, false));
        //    list.Add(function);

        //    // boolean strToInt(string str, ref int result)
        //    function = new PreDefinedFunctionSymbol("strToInt", TypeDeclaration.Bool, PreDefinedFunctionCode.StrToInt);
        //    function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("result", TypeDeclaration.Int, true));
        //    list.Add(function);

        //    // boolean strToFloat(string str, ref float result)
        //    function = new PreDefinedFunctionSymbol("strToFloat", TypeDeclaration.Bool, PreDefinedFunctionCode.StrToFloat);
        //    function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("result", TypeDeclaration.Float, true));
        //    list.Add(function);


        //    return list;
        //}

        //private static IEnumerable<PreDefinedFunctionSymbol> GetDateTimeHandlingFunctionsList()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
        //    PreDefinedFunctionSymbol function = null; // variável auxiliar


        //    // string newDate(int dd, int mm, int yy)
        //    function = new PreDefinedFunctionSymbol("newDate", TypeDeclaration.String, PreDefinedFunctionCode.NewDate);
        //    function.AddParameter(new ParameterSymbol("dd", TypeDeclaration.Int, false));
        //    function.AddParameter(new ParameterSymbol("mm", TypeDeclaration.Int, false));
        //    function.AddParameter(new ParameterSymbol("yy", TypeDeclaration.Int, false));
        //    list.Add(function);

        //    // string today()
        //    function = new PreDefinedFunctionSymbol("today", TypeDeclaration.String, PreDefinedFunctionCode.Today);
        //    list.Add(function);

        //    // int getDay(string date)
        //    function = new PreDefinedFunctionSymbol("getDay", TypeDeclaration.Int, PreDefinedFunctionCode.GetDay);
        //    function.AddParameter(new ParameterSymbol("date", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // int getMonth(string date)
        //    function = new PreDefinedFunctionSymbol("getMonth", TypeDeclaration.Int, PreDefinedFunctionCode.GetMonth);
        //    function.AddParameter(new ParameterSymbol("date", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // int getYear(string date)
        //    function = new PreDefinedFunctionSymbol("getYear", TypeDeclaration.Int, PreDefinedFunctionCode.GetYear);
        //    function.AddParameter(new ParameterSymbol("date", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // boolean parseDate(string date, ref int day, ref int month, ref int year)
        //    function = new PreDefinedFunctionSymbol("parseDate", TypeDeclaration.Bool, PreDefinedFunctionCode.ParseDate);
        //    function.AddParameter(new ParameterSymbol("date", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("day", TypeDeclaration.Int, true));
        //    function.AddParameter(new ParameterSymbol("month", TypeDeclaration.Int, true));
        //    function.AddParameter(new ParameterSymbol("year", TypeDeclaration.Int, true));
        //    list.Add(function);


        //    return list;
        //}

        //private static IEnumerable<PreDefinedFunctionSymbol> GetStringHandlingFunctionsList()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
        //    PreDefinedFunctionSymbol function = null; // variável auxiliar


        //    // string substring(string str, int start, int end)
        //    function = new PreDefinedFunctionSymbol("substring", TypeDeclaration.String, PreDefinedFunctionCode.Substring);
        //    function.AddParameter(new ParameterSymbol("str", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("start", TypeDeclaration.Int, false));
        //    function.AddParameter(new ParameterSymbol("end", TypeDeclaration.Int, false));
        //    list.Add(function);

        //    // int find(string s1, string s2)
        //    function = new PreDefinedFunctionSymbol("find", TypeDeclaration.Int, PreDefinedFunctionCode.Find);
        //    function.AddParameter(new ParameterSymbol("s1", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("s2", TypeDeclaration.String, false));
        //    list.Add(function);
            

        //    return list;
        //}

        //private static IEnumerable<PreDefinedFunctionSymbol> GetOsMobileSpecificFunctions()
        //{
        //    var list = new List<PreDefinedFunctionSymbol>(); // cria a lista.
        //    PreDefinedFunctionSymbol function = null; // variável auxiliar


        //    // boolean callService(string serviceId, string serviceParm, ref string message, ref string route)
        //    function = new PreDefinedFunctionSymbol("callService", TypeDeclaration.Bool, PreDefinedFunctionCode.CallService);
        //    function.AddParameter(new ParameterSymbol("serviceId", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("serviceParameter", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("message", TypeDeclaration.String, true));
        //    function.AddParameter(new ParameterSymbol("route", TypeDeclaration.String, true));
        //    list.Add(function);

        //    // string getCurrentActivityName()
        //    function = new PreDefinedFunctionSymbol("getCurrentActivityName", TypeDeclaration.String, PreDefinedFunctionCode.GetCurrentActivityName);
        //    list.Add(function);

        //    // string getCurrentModelName()
        //    function = new PreDefinedFunctionSymbol("getCurrentModelName", TypeDeclaration.String, PreDefinedFunctionCode.GetCurrentModelName);
        //    list.Add(function);

        //    // void startLocationCapture()
        //    function = new PreDefinedFunctionSymbol("startLocationCapture", TypeDeclaration.Void, PreDefinedFunctionCode.StartLocationCapture);
        //    list.Add(function);

        //    // void startSynchronization()
        //    function = new PreDefinedFunctionSymbol("startSynchronization", TypeDeclaration.Void, PreDefinedFunctionCode.StartSynchronization);
        //    list.Add(function);

        //    // void showInfoMessage(string msg)
        //    function = new PreDefinedFunctionSymbol("showInfoMessage", TypeDeclaration.Void, PreDefinedFunctionCode.ShowInfoMessage);
        //    function.AddParameter(new ParameterSymbol("msg", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // void showWarningMessage(string msg)
        //    function = new PreDefinedFunctionSymbol("showWarningMessage", TypeDeclaration.Void, PreDefinedFunctionCode.ShowWarningMessage);
        //    function.AddParameter(new ParameterSymbol("msg", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // void showErrorMessage(string msg)
        //    function = new PreDefinedFunctionSymbol("showErrorMessage", TypeDeclaration.Void, PreDefinedFunctionCode.ShowErrorMessage);
        //    function.AddParameter(new ParameterSymbol("msg", TypeDeclaration.String, false));
        //    list.Add(function);


        //    // string getVarValue(string varName)
        //    function = new PreDefinedFunctionSymbol("getVarValue", TypeDeclaration.String, PreDefinedFunctionCode.GetVarValue);
        //    function.AddParameter(new ParameterSymbol("varName", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // void setVarValue(string varName, string varValue)
        //    function = new PreDefinedFunctionSymbol("setVarValue", TypeDeclaration.Void, PreDefinedFunctionCode.SetVarValue);
        //    function.AddParameter(new ParameterSymbol("varName", TypeDeclaration.String, false));
        //    function.AddParameter(new ParameterSymbol("varValue", TypeDeclaration.String, false));
        //    list.Add(function);

        //    // void goToRoute(string routeName)
        //    function = new PreDefinedFunctionSymbol("goToRoute", TypeDeclaration.Void, PreDefinedFunctionCode.GoToRoute);
        //    function.AddParameter(new ParameterSymbol("routeName", TypeDeclaration.String, false));
        //    list.Add(function);


        //    return list;
        //}

        #endregion
    }
}
