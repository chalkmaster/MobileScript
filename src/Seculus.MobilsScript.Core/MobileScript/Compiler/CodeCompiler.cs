using System.Collections.Generic;
using Seculus.MobileScript.Core.MobileScript.ProgramTree;
using Seculus.MobileScript.Core.MobileScript.ProgramTree.Declarations;
using Seculus.MobileScript.Core.MobileScript.Symbols;
using Seculus.MobileScript.Core.MobileScript.VirtualMachine;

namespace Seculus.MobileScript.Core.MobileScript.Compiler
{
    /// <summary>
    /// Fachada responsável pela compilação.
    /// </summary>
    public class CodeCompiler
    {
        #region Constants

        /// <summary>
        /// Tamanho da área de memória do programa.
        /// </summary>
        public const int DefaultStackSize = 512;

        /// <summary>
        /// Tamanho da área de código do programa.
        /// </summary>
        public const int DefaultCodeSize = 2048;

        /// <summary>
        /// Representa um endereço inválido.
        /// </summary>
        public const int InvalidAddress = -99999;

        #endregion

        #region Fields

        #endregion

        #region Constructors

        public CodeCompiler()
        {
        }

        #endregion

        #region Public Methods

        public CompilerResults CompileFullProgram(MobileScriptReader file, int stackSize, int codeSize)
        {
            ProgramDescription programDescription;
            var errors = new List<CompilationError>();
            var symbolsTable = new SymbolsTable();

            // Pré-processamento
            if (Preprocess(file, ref errors))
            {
                // Análise sintática.
                if (SyntaticAnalysisForFullProgram(file, ref errors, out programDescription))
                {
                    // Análise semântica.
                    if (SemanticAnalysis(programDescription, symbolsTable, ref errors))
                    {
                        // Geração de código.
                        var vm = new Program(stackSize, codeSize);
                        int entryPointAddress = CodeGeneration(programDescription, ref vm);
                        return new CompilerResults(true, vm, errors, entryPointAddress);
                    }
                }
            }

            return new CompilerResults(false, errors);
        }
        public CompilerResults CompileFullProgram(MobileScriptReader file)
        {
            return CompileFullProgram(file, DefaultStackSize, DefaultCodeSize);
        }

        public CompilerResults CompileFullProgram(MobileScriptReader globalDeclarationsFile, MobileScriptReader mainFunctionFile, int stackSize, int codeSize)
        {
            var vm = new Program(stackSize, codeSize);
            var errors = new List<CompilationError>();
            var symbolsTable = new SymbolsTable();

            // Pré-processamento do globals.
            if (Preprocess(globalDeclarationsFile, ref errors))
            {
                // Analise sintática do globals.
                ProgramDescription globalsDescription;
                if (SyntaticAnalysisForGlobalDeclarations(globalDeclarationsFile, ref errors, out globalsDescription))
                {
                    // Pré-processamento do main.
                    if (Preprocess(mainFunctionFile, ref  errors))
                    {
                        // Analise sintática do main.
                        ProgramDescription mainFunctionDescription;
                        if (SyntaticAnalysisForMainFunctionBody(mainFunctionFile, ref errors, out mainFunctionDescription))
                        {
                            // Análise semântica.
                            ProgramDescription programDescription = MixProgramDescriptions(globalsDescription, mainFunctionDescription);
                            if (SemanticAnalysis(programDescription, symbolsTable, ref errors))
                            {
                                // Geração de código
                                int entryPointAddress = CodeGeneration(programDescription, ref vm);
                                return new CompilerResults(true, vm, errors, entryPointAddress);
                            }
                        }
                    }
                }
            }


            return new CompilerResults(false, errors);
        }
        public CompilerResults CompileFullProgram(MobileScriptReader globalDeclarationsFile, MobileScriptReader mainFunctionFile)
        {
            return CompileFullProgram(globalDeclarationsFile, mainFunctionFile, DefaultStackSize, DefaultCodeSize);
        }

        public CompilerResults CompileLibrary(MobileScriptReader library, int stackSize, int codeSize)
        {
            ProgramDescription programDescription;
            var errors = new List<CompilationError>();
            var symbolsTable = new SymbolsTable();

            // Pré-processamento
            if (Preprocess(library, ref errors))
            {
                // Análise sintática.
                if (SyntaticAnalysisForLibrary(library, ref errors, out programDescription))
                {
                    // Análise semântica.
                    if (SemanticAnalysis(programDescription, symbolsTable, ref errors))
                    {
                        // Geração de código.
                        var vm = new Program(stackSize, codeSize);
                        int entryPointAddress = CodeGeneration(programDescription, ref vm);
                        return new CompilerResults(true, vm, errors, entryPointAddress);
                    }
                }
            }

            return new CompilerResults(false, errors);
        }
        public CompilerResults CompileLibrary(MobileScriptReader library)
        {
            return CompileLibrary(library, DefaultStackSize, DefaultCodeSize);
        }

        public CompilerResults CompileFullProgramWithMultipleEntryPoints(MobileScriptReader globalDeclarations, MobileScriptReader[] mainFunctions, int stackSize, int codeSize, out int globalsEntryPoint)
        {
            var vm = new Program(stackSize, codeSize);
            var errors = new List<CompilationError>();
            var symbolsTable = new SymbolsTable();
            globalsEntryPoint = -1;

            CompilerResults result = null;

            // GLOBALS
            // Pré-processamento do globals.
            if (Preprocess(globalDeclarations, ref errors))
            {
                // Analise sintática do globals.
                ProgramDescription globalsDescription;
                if (SyntaticAnalysisForGlobalDeclarations(globalDeclarations, ref errors, out globalsDescription))
                {
                    // Análise semântica.
                    if (SemanticAnalysis(globalsDescription, symbolsTable, ref errors))
                    {
                        // Geração de código
                        globalsEntryPoint = CodeGeneration(globalsDescription, ref vm);
                        result = new CompilerResults(true, vm, errors);
                    }
                }
            }


            // MAIN
            if (result != null)
            {
                foreach (var mainFunction in mainFunctions)
                {
                    bool succeeded = false;
                    // Pré-processamento do main.
                    if (Preprocess(mainFunction, ref  errors))
                    {
                        // Analise sintática do main.
                        ProgramDescription mainFunctionDescription;
                        if (SyntaticAnalysisForMainFunctionBody(mainFunction, ref errors, out mainFunctionDescription))
                        {
                            // Análise semântica do main.
                            if (SemanticAnalysis(mainFunctionDescription, symbolsTable, ref errors))
                            {
                                // Geração de código
                                int entryPointAddress = CodeGeneration(mainFunctionDescription, ref vm);
                                result.EntryPointAddresses.Add(entryPointAddress);
                                succeeded = true;
                            }
                        }
                    }

                    if (!succeeded)
                    {
                        result.Succeeded = false;
                        break;
                    }
                }
            }


            if (result == null) result = new CompilerResults(false, errors);
            result.Errors = errors;

            return result;
        }
        public CompilerResults CompileFullProgramWithMultipleEntryPoints(MobileScriptReader globalDeclarations, MobileScriptReader[] mainFunctions, out int globalsEntryPoint)
        {
            return CompileFullProgramWithMultipleEntryPoints(globalDeclarations, mainFunctions, DefaultStackSize, DefaultCodeSize, out globalsEntryPoint);
        }

        #endregion

        #region Private Methods

        private bool Preprocess(MobileScriptReader file, ref List<CompilationError> errors)
        {
            int numErrors = errors.Count;

            var preprocessor = new Preprocessor(file);
            preprocessor.DoPreprocess();
            errors.AddRange(preprocessor.Errors);

            file.Reset();

            return errors.Count == numErrors; // true se não ocorreram erros.
        }

        private bool SyntaticAnalysisForFullProgram(MobileScriptReader file, ref List<CompilationError> errors, out ProgramDescription programDescription)
        {
            int numErrors = errors.Count;

            var parser = new Parser();
            parser.Init(file);
            programDescription = parser.ParseFullProgram();
            errors.AddRange(parser.Errors);

            return errors.Count == numErrors; // Se não ocorreram erros.
        }
        private bool SyntaticAnalysisForGlobalDeclarations(MobileScriptReader file, ref List<CompilationError> errors, out ProgramDescription programDescription)
        {
            int numErrors = errors.Count;

            var parser = new Parser();
            parser.Init(file);
            programDescription = parser.ParseGlobalDeclarations();
            errors.AddRange(parser.Errors);

            return errors.Count == numErrors; // Se não ocorreram erros.
        }
        private bool SyntaticAnalysisForMainFunctionBody(MobileScriptReader file, ref List<CompilationError> errors, out ProgramDescription programDescription)
        {
            int numErrors = errors.Count;

            var parser = new Parser();
            parser.Init(file);
            programDescription = parser.ParseStatements();
            errors.AddRange(parser.Errors);

            return errors.Count == numErrors; // Se não ocorreram erros.
        }
        private bool SyntaticAnalysisForLibrary(MobileScriptReader file, ref List<CompilationError> errors, out ProgramDescription programDescription)
        {
            int numErrors = errors.Count;

            var parser = new Parser();
            parser.Init(file);
            IList<FunctionDeclaration> functions = parser.ParseLibrary();
            errors.AddRange(parser.Errors);

            if (functions.Count > 0)
            {
                programDescription = new ProgramDescription(functions[0].LexSymbol);
                foreach (var functionDeclaration in functions)
                {
                    programDescription.Declarations.Add(functionDeclaration);
                }
            }
            else
            {
                programDescription = new ProgramDescription(new LexSymbol("EoF", LexSymbolKind.EndOfFile, file.GetFileName(), 0, 0));
            }

            return errors.Count == numErrors; // Se não ocorreram erros.
        }

        private bool SemanticAnalysis(Node node, SymbolsTable symbolsTable, ref List<CompilationError> errors)
        {
            int numErrors = errors.Count;

            var semanticVisitor = new SemanticVisitor(symbolsTable, errors);
            node.Accept(semanticVisitor);

            return errors.Count == numErrors;
        }

        private int CodeGeneration(Node node, ref Program vm)
        {
            var codeGenVisitor = new CodeGeneratorVisitor(vm);
            return (int)node.Accept(codeGenVisitor);
        }

        private ProgramDescription MixProgramDescriptions(ProgramDescription globalsDescription, ProgramDescription mainDescription)
        {
            var finalDescription = new ProgramDescription(globalsDescription.LexSymbol);

            // Set declarations
            foreach (var declaration in globalsDescription.Declarations)
            {
                finalDescription.Declarations.Add(declaration);
            }

            // Set body
            finalDescription.Body = mainDescription.Body;

            // Set scope
            finalDescription.Scope = mainDescription.Scope;

            return finalDescription;
        }

        #endregion
    }
}
