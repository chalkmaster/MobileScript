namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Instruções da VM
    /// </summary>
    public enum InstructionCode : short
    {
        /// <summary>
        /// No Operation
        /// </summary>
        [EnumDisplayName("NOP")]
		NOp = 0,

        /// <summary>
        /// Entrada em procedimento ou função
        /// </summary>
        [EnumDisplayName("ENTER")]
		Enter = 1,

        /// <summary>
        /// Chamada de função
        /// </summary>
        [EnumDisplayName("CALL")]
		Call = 2,

        /// <summary>
        /// Retorno de função
        /// </summary>
        [EnumDisplayName("RETURN")]
        Return = 3,

        /// <summary>
        /// Encerra execução
        /// </summary>
        [EnumDisplayName("HALT")]
		Halt = 4,

        /// <summary>
        /// Desvio incondicional
        /// </summary>
        [EnumDisplayName("JUMP")]
		Jump = 5,

        /// <summary>
        /// Desvia se falso
        /// </summary>
        [EnumDisplayName("JUMPF")]
		JumpF = 6,

        /// <summary>
        /// Soma
        /// </summary>
        [EnumDisplayName("ADD")]
		Add = 7,

        /// <summary>
        /// Subtração
        /// </summary>
        [EnumDisplayName("SUB")]
		Sub = 8,

        /// <summary>
        /// Multiplicação
        /// </summary>
        [EnumDisplayName("MULT")]
		Mult = 9,

        /// <summary>
        /// Divisão
        /// </summary>
        [EnumDisplayName("DIV")]
		Div = 10,

        /// <summary>
        /// Menos unário
        /// </summary>
        [EnumDisplayName("MINUS")]
		Minus = 11,

        /// <summary>
        /// Módulo (%)
        /// </summary>
        [EnumDisplayName("MOD")]
		Mod = 12,

        /// <summary>
        /// And lógico
        /// </summary>
        [EnumDisplayName("AND")]
		And = 13,

        /// <summary>
        /// OR
        /// </summary>
        [EnumDisplayName("OR")]
		Or = 14,

        /// <summary>
        /// OR exclusivo
        /// </summary>
        [EnumDisplayName("XOR")]
		XOr = 15,

        /// <summary>
        /// Negação
        /// </summary>
        [EnumDisplayName("NOT")]
		Not = 16,

        /// <summary>
        /// Igual (==)
        /// </summary>
        [EnumDisplayName("EQ")]
		Eq = 17,

        /// <summary>
        /// Diferente (!=)
        /// </summary>
        [EnumDisplayName("NE")]
		NE = 18,

        /// <summary>
        /// Maior que (&gt;)
        /// </summary>
        [EnumDisplayName("GT")]
		GT = 19,

        /// <summary>
        /// Maior ou igual (&gt;=)
        /// </summary>
        [EnumDisplayName("GE")]
		GE = 20,

        /// <summary>
        /// Menor que (&lt;)
        /// </summary>
        [EnumDisplayName("LT")]
		LT = 21,

        /// <summary>
        /// Menor ou igual (&lt;=)
        /// </summary>
        [EnumDisplayName("LE")]
		LE = 22,

        /// <summary>
        /// Constante inteira
        /// </summary>
        [EnumDisplayName("INTCONST")]
		IntConst = 23,

        /// <summary>
        /// Constante do tipo char
        /// </summary>
        [EnumDisplayName("CHRCONST")]
		CharConst = 24,

        /// <summary>
        /// Constante do tipo string
        /// </summary>
        [EnumDisplayName("STRCONST")]
		StrConst = 25,

        /// <summary>
        /// Constante do tipo float
        /// </summary>
        [EnumDisplayName("FLOATCONST")]
		FloatConst = 26,

        /// <summary>
        /// Constante booleana
        /// </summary>
        [EnumDisplayName("BOOLCONST")]
		BoolConst = 27,

        /// <summary>
        /// Converte inteiro para string
        /// </summary>
        [EnumDisplayName("INTTOSTR")]
		IntToStr = 28,

        /// <summary>
        /// Converte booleano para string
        /// </summary>
        [EnumDisplayName("BOOLTOSTR")]
		BoolToStr = 29,

        /// <summary>
        /// Converte char para string
        /// </summary>
        [EnumDisplayName("CHARTOSTR")]
		CharToStr = 30,

        /// <summary>
        /// Converte float para string
        /// </summary>
        [EnumDisplayName("FLOATTOSTR")]
		FloatToStr = 31,

        /// <summary>
        /// Converte char para inteiro
        /// </summary>
        [EnumDisplayName("CHARTOINT")]
		CharToInt = 32,

        /// <summary>
        /// Converte float para inteiro
        /// </summary>
        [EnumDisplayName("FLOATTOINT")]
		FloatToInt = 33,

        /// <summary>
        /// Converte inteiro para float
        /// </summary>
        [EnumDisplayName("INTTOFLOAT")]
		IntToFloat = 34,

        /// <summary>
        /// Concatena strings
        /// </summary>
        [EnumDisplayName("CONCAT")]
		Concat = 35,

        /// <summary>
        /// Incrementa em 1 o valor no topo da pilha
        /// </summary>
        [EnumDisplayName("INC")]
		Inc = 36,

        /// <summary>
        /// Decrementa em 1 o valor no topo da pilha
        /// </summary>
        [EnumDisplayName("DEC")]
		Dec = 37,

        /// <summary>
        /// Carrega variável
        /// </summary>
        [EnumDisplayName("LDVAR")]
		LdVar = 38,

        /// <summary>
        /// Armazena valor na variável que é operando
        /// </summary>
        [EnumDisplayName("STVAR")]
		StVar = 39,

        /// <summary>
        /// Carrega o endereço da variável que é o operando
        /// </summary>
        [EnumDisplayName("LDADDR")]
		LdAddr = 40,

        /// <summary>
        /// Armazena o valor do topo da pilha na variável cujo endereço está abaixo
        /// </summary>
        [EnumDisplayName("STI")]
		Sti = 41,

        /// <summary>
        /// Carrega o valor da variável cujo endereço está no topo da pilha.
        /// </summary>
        [EnumDisplayName("LDI")]
		Ldi = 42,

        /// <summary>
        /// Armazena o bloco de dados (topo: endereço inicial - operando: tamanho do bloco)
        /// </summary>
        [EnumDisplayName("STBLOCK")]
		StBlock = 43,

        /// <summary>
        /// Carrega um bloco de dados (topo: endereço inicial - operando: tamanho do bloco)
        /// </summary>
        [EnumDisplayName("LDBLOCK")]
		LdBlock = 44,


        // NOT USED!!!
        /*
        /// <summary>
        /// Escreve na saída padrão o valor no topo da pilha como string
        /// </summary>
        [EnumDisplayName("PRINT")]
		Print = 45,

        /// <summary>
        /// Escreve "\n" na saída padrão
        /// </summary>
        [EnumDisplayName("PRINTLN")]
		PrintLn = 46,
        */


        /// <summary>
        /// Lê um valor inteiro da entrada padrão e empilha (INÍCIO: posição atual. FIM: final da linha)
        /// </summary>
        [EnumDisplayName("READINT")]
		ReadInt = 47,

        /// <summary>
        /// Lê uma string da entrada padrão e empilha (INÍCIO: posição atual. FIM: final da linha)
        /// </summary>
        [EnumDisplayName("READSTR")]
		ReadStr = 48,

        /// <summary>
        /// Lê um float da entrada padrão e empilha (INÍCIO: posição atual. FIM: final da linha)
        /// </summary>
        [EnumDisplayName("READFLOAT")]
		ReadFloat = 49,

        /// <summary>
        /// Incrementa topo da pilha de n (n = operando)
        /// </summary>
        [EnumDisplayName("INCT")]
		IncT = 50,

        /// <summary>
        /// Chamada a uma função pré-definida na linguagem.
        /// </summary>
        [EnumDisplayName("SYSCALL")]
	    SysCall = 51,

        /// <summary>
        /// Operação com float: soma
        /// </summary>
        [EnumDisplayName("FADD")]
	    FAdd = 52,

        /// <summary>
        /// Operação com float: subtração
        /// </summary>
        [EnumDisplayName("FSUB")]
	    FSub = 53,

        /// <summary>
        /// Operação com float: multiplicação
        /// </summary>
        [EnumDisplayName("FMULT")]
	    FMult = 54,

        /// <summary>
        /// Operação com float: divisão
        /// </summary>
        [EnumDisplayName("FDIV")]
	    FDiv = 55,

        /// <summary>
        /// Operação com float: menos unário
        /// </summary>
        [EnumDisplayName("FMINUS")]
	    FMinus = 56,

        /// <summary>
        /// Operação mod com float: módulo (resto da divisão)
        /// </summary>
        [EnumDisplayName("FMOD")]
        FMod = 62,

        /// <summary>
        /// Carrega endereço de variável global (igual a INTCONST).
        /// </summary>
        [EnumDisplayName("GLADDR")]
	    GlAddr = 57,

        /// <summary>
        /// Carrega o valor de variável global.
        /// </summary>
        [EnumDisplayName("LDGLVAR")]
	    LdGlVar = 59,

        /// <summary>
        /// Calcula o offset de um elemento de um vetor
        /// </summary>
        [EnumDisplayName("OFFS")]
	    OffS = 60,

        /// <summary>
        /// Faz operação de indexação e verifica limite (gera exceção)
        /// </summary>
        [EnumDisplayName("IDX")]
	    Idx = 61
    }
}
