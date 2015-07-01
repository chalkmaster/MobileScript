using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Seculus.MobileScript.Core.MobileScript.Compiler;


namespace Seculus.MobileScript
{
    class Program
    {
        static void openFile(String filename)
        {
            try
            {
                FileStream stream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("==>> arquivo "+ filename+" aberto");
                MobileScriptReader msr = new MobileScriptReader(stream);
                CodeCompiler msc = new CodeCompiler();
                CompilerResults res = msc.CompileFullProgram(msr);
                var success = res.Succeeded;
                Console.WriteLine("succeed:" + success.ToString());
                foreach (var err in res.Errors)
                {
                    Console.WriteLine("line:"+err.Line+":"+err.Column+"=>"+err.Message);
                }

           
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static string[] fNames = {
                                    "teste0_3.ms",
                                    "teste0_4.ms",
                                    "teste0_5.ms",
                                    "teste0_6.ms",
                                    "teste0_7.ms",
                                    "teste0_8.ms",
                                    "teste0_9.ms",
                                    "teste0_10.ms",
                                    "teste0_2.ms",
                                    "teste0_11.ms",
                                    "teste0_12.ms",
                                    "teste0_13.ms",
                                    "teste0_14.ms",
                                    "teste0_15.ms",
                                    "teste0_16.ms",
                                    "teste0_17.ms",
                                    "teste0_18.ms",
                                    "teste0_19.ms",
                                    "teste0_20.ms",
                                    "teste0_21.ms",
                                    "teste0mod_2.ms", 
                                    "teste0mod_3.ms",
                                    "teste0_22.ms",
                                    "arrays.ms",
                                    "arrays_1.ms", 
                                    "strings.ms",
                                    "strings_1.ms",
                                    "parsing_1.ms",
                                    "math_1.ms",
                                    "geoLocation_1.ms",
                                    "geoLocation_1a.ms"
                                 };

        static void Main(string[] args)
        {
            String path = @"c:\Klais\Prime\exemplosMS\";
            String filename = fNames[30];
            Console.WriteLine("Inicio -- path:" + filename);
            openFile(path + filename);
            Console.ReadLine();
        }
    }
}
