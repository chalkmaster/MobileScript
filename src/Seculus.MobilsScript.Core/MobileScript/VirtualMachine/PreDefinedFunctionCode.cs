namespace Seculus.MobileScript.Core.MobileScript.VirtualMachine
{
    /// <summary>
    /// Funções pré-definidas da linguagem.
    /// </summary>
    public enum PreDefinedFunctionCode
    {
        /* 
         * STANDARD I/O METHODS 
         */

        /// <summary>
        /// Imprime um string
        /// </summary>
        Print = 1,

        /// <summary>
        /// Muda de linha
        /// </summary>
        PrintLn = 2,








        /*
         * STRING MANIPULATION METHODS
         */

        /// <summary>
        /// Substring
        /// </summary>
        Substring = 0,

        /// <summary>
        /// Returns the length of a given string.
        /// </summary>
        StrLen = 49,

        /// <summary>
        /// Retorna o valor do primeiro caracter da string passada.
        /// </summary>
        CharVal = 59,

        /// <summary>
        /// Returns the value of a char at a given position.
        /// </summary>
        CharValAt = 60,

        /// <summary>
        /// Constroi uma string apartir dos valores dos caracteres.
        /// </summary>
        BuildStringFromChars = 61,








        /* 
         * DATE/TIME METHODS 
         */

        /// <summary>
        /// Retorna uma string contendo a data e a hora (formato yyyy-MM-dd HH:mm:ss) do momento
        /// </summary>
        Now = 50,









        /* 
         * DEVICE METHODS
         */

        /// <summary>
        /// Retorna o IMEI do dispositivo
        /// </summary>
        GetImei = 32,

        /// <summary>
        /// Envia um e-mail: bool sendEmail(string to, string title, string body)
        /// </summary>
        SendEmail = 57,

        /// <summary>
        /// Envia um SMS
        /// </summary>
        SendSms = 58,








        /* 
         * OSMOBILE OUTPUT METHODS 
         */

        /// <summary>
        /// Exibe uma caixa de diálogo (com texto e titulo).
        /// </summary>
        ShowDialog = 51,

        /// <summary>
        /// Exibe um lembrete (texto, tempo em milisegundos).
        /// </summary>
        ShowSmartReminder = 52,

        /// <summary>
        /// Exibe um toast (com texto).
        /// </summary>
        ShowToast = 53,







        /*
         * OSMOBILE APP METHODS
         */

        /// <summary>
        /// Retorna o subscriber id do dispositivo
        /// </summary>
        GetSubscriberId = 29,

        /// <summary>
        /// Retorna o id do usuário do dispositivo
        /// </summary>
        GetUserId = 30,

        /// <summary>
        /// Retorna a versão do software instalado no dispositivo
        /// </summary>
        GetVersion = 31,

        /// <summary>
        /// Chamada a um serviço
        /// </summary>
        CallService = 33,

        /// <summary>
        /// Retorna o nome do fluxo 
        /// </summary>
        GetCurrentModelName = 35,

        /// <summary>
        /// Dispara a sincronização automática.
        /// </summary>
        StartSynchronization = 36,

        /// <summary>
        /// Inicia captura de localização.
        /// </summary>
        StartLocationCapture = 37,








        /* 
         * OSMOBILE SPECIFIC SERVICE REQUEST (OS) METHODS
         */

        /// <summary>
        /// Retorna o nome da atividade atual
        /// </summary>
        GetCurrentActivityName = 34,

        /// <summary>
        /// Retorna o valor de uma variável do fluxo (variável da caixinha do fluxo)
        /// </summary>
        GetVarValue = 46,

        /// <summary>
        /// Atribui o valor de uma variável do fluxo (variável da caixinha do fluxo)
        /// </summary>
        SetVarValue = 47,

        /// <summary>
        /// Vai para uma rota.
        /// </summary>
        GoToRoute = 48,

        /// <summary>
        /// Retorna o número de transições
        /// </summary>
        CountTransitions = 54,

        /// <summary>
        /// Retorna os dados da transição. (A partir do nome da atividade retorna: data/hora da execução, valor e se foi sincronizada ou não).
        /// </summary>
        GetTransition = 55,

        /// <summary>
        /// Retorna os dados do cabeçalho da OS em execução: 
        /// Cliente, Endereço, Data/Hora de agendamento, Prazo, Observação, Campos Adicionais e Código.
        /// </summary>
        GetHeader = 56
    }

    ///// <summary>
    ///// Funções pré-definidas da linguagem.
    ///// </summary>
    //public enum PreDefinedFunctionCode
    //{
    //    /// <summary>
    //    /// Substring
    //    /// </summary>
    //    Substring = 0,

    //    /// <summary>
    //    /// Imprime um string
    //    /// </summary>
    //    Print = 1,

    //    /// <summary>
    //    /// Muda de linha
    //    /// </summary>
    //    PrintLn = 2,





    //    /* TODO - REMOVE THOSE FUNCTION
        
        
        
    //    /// <summary>
    //    /// Define a saída de uma decisão simples
    //    /// </summary>
    //    SetDecision = 3,

    //    /// <summary>
    //    /// Define a saída de uma decisão múltipla
    //    /// </summary>
    //    SetOption = 4,

        
    //     */





    //    /// <summary>
    //    /// Converte inteiro para string
    //    /// </summary>
    //    IntToStr = 5,

    //    /// <summary>
    //    /// Converte float para string
    //    /// </summary>
    //    FloatToStr = 6,

    //    /// <summary>
    //    /// Converte boolean para string
    //    /// </summary>
    //    BoolToStr = 7,

    //    /// <summary>
    //    /// Converte string para inteiro
    //    /// </summary>
    //    StrToInt = 8,

    //    /// <summary>
    //    /// Converte string para float
    //    /// </summary>
    //    StrToFloat = 9,

    //    /// <summary>
    //    /// Constrói uma data a partir de dia,mes e ano (inteiros)
    //    /// </summary>
    //    NewDate = 10,

    //    /// <summary>
    //    /// Retorna uma string contendo a data de hoje
    //    /// </summary>
    //    Today = 11,

    //    /// <summary>
    //    /// Retorna o dia relativo à data passada como parâmetro
    //    /// </summary>
    //    GetDay = 12,

    //    /// <summary>
    //    /// Retorna o mes relativo à data passada como parâmetro
    //    /// </summary>
    //    GetMonth = 13,

    //    /// <summary>
    //    /// Retorna o ano relativo à data passada como parâmetro
    //    /// </summary>
    //    GetYear = 14,

    //    /// <summary>
    //    /// Devolve dia, mês e ano contidos no string passado como parâmetro
    //    /// </summary>
    //    ParseDate = 15,





    //    /* NOT IMPLEMENTING LISTS HANDLING FOR NOW


    //    /// <summary>
    //    /// Cria uma lista
    //    /// </summary>
    //    CreateList = 16,

    //    /// <summary>
    //    /// Associa uma lista a um recurso utilizado pelo processo
    //    /// </summary>
    //    BindList = 17,

    //    /// <summary>
    //    /// "Lê" o conteúdo de uma lista a partir do recurso associado
    //    /// </summary>
    //    GetListContent = 18,

    //    /// <summary>
    //    /// Copia o conteudo da lista para o recurso associado
    //    /// </summary>
    //    WriteListContent = 19,

    //    /// <summary>
    //    /// Devolve o tamanho da lista
    //    /// </summary>
    //    GetListSize = 20,

    //    /// <summary>
    //    /// Devolve o número de colunas por linha de uma lista
    //    /// </summary>
    //    GetListNCols = 21,

    //    /// <summary>
    //    /// Devolve o nome do recurso associado a uma lista
    //    /// </summary>
    //    GetListResourceName = 22,

    //    /// <summary>
    //    /// Retorna o valor de um elemento da lista
    //    /// </summary>
    //    GetListElement = 23,

    //    /// <summary>
    //    /// Define o valor de um elemento da lista
    //    /// </summary>
    //    SetListElement = 24,

    //    /// <summary>
    //    /// Cria uma nova linha na lista (de strings vazios)
    //    /// </summary>
    //    NewLine = 25,

    //    /// <summary>
    //    /// Devolve o índice da última linha de uma lista
    //    /// </summary>
    //    LastLineNo = 26,

    //    /// <summary>
    //    /// Apaga uma linha da lista
    //    /// </summary>
    //    DeleteLine = 27,

         
        
    //    */







    //    /* NOT FEASABLE ON MOBILE PHONES FOR NOW

    //    /// <summary>
    //    /// Retorna o número do dispositivo
    //    /// </summary>
    //    GetPhoneNumber = 28,

         
    //     */




    //    /* NOT AVAILABLE ON FIRST VERSION

        



    //    /// <summary>
    //    /// Retorna o id do dispositivo
    //    /// </summary>
    //    GetPhoneId = 29,

    //    /// <summary>
    //    /// Retorna o id do usuário do dispositivo
    //    /// </summary>
    //    GetUserId = 30,

    //    /// <summary>
    //    /// Retorna a versão do software instalado no dispositivo
    //    /// </summary>
    //    GetVersion = 31,

    //    /// <summary>
    //    /// Retorna o IMEI do dispositivo (TODO: precisa do PHONEID ?)
    //    /// </summary>
    //    GetImei = 32,

    //     */





    //    /// <summary>
    //    /// Chamada a um serviço
    //    /// </summary>
    //    CallService = 33,

    //    /// <summary>
    //    /// Retorna o nome da atividade atual
    //    /// </summary>
    //    GetCurrentActivityName = 34,

    //    /// <summary>
    //    /// Retorna o nome do fluxo 
    //    /// </summary>
    //    GetCurrentModelName = 35,

    //    /// <summary>
    //    /// Dispara a sincronização automática.
    //    /// </summary>
    //    StartSynchronization = 36,



    //    /* REPLACED BY StartLocationCapture (46)




    //    /// <summary>
    //    /// Inicia a captura de informação do GPS
    //    /// </summary>
    //    StartGpsCapture = 37,

    //    /// <summary>
    //    /// Inicia a captura de informação do LBS
    //    /// </summary>
    //    StartLbsCapture = 38,


    //    */

        



    //    /// <summary>
    //    /// Inicia captura de localização.
    //    /// </summary>
    //    StartLocationCapture = 37,


    //    /// <summary>
    //    /// Mostra mensagem de informação
    //    /// </summary>
    //    ShowInfoMessage = 39,

    //    /// <summary>
    //    /// Mostra mensagem de erro
    //    /// </summary>
    //    ShowErrorMessage = 40,

    //    /// <summary>
    //    /// Mostra mensagem de advertência
    //    /// </summary>
    //    ShowWarningMessage = 41,





    //    /* NOT GOOD FUNCTIONS. CAN BE HARMFUL.

    //    /// <summary>
    //    /// Mostra o menu principal (TODO: no meio do andamento do fluxo ?)
    //    /// </summary>
    //    ShowMainMenu = 42,

    //    /// <summary>
    //    /// Mostra o menu principal (TODO: no meio do andamento do fluxo ?)
    //    /// </summary>
    //    ShowOsList = 43,

    //    */






    //    /* NOT IMPLEMENTING LISTS STUFF FOR NOW


    //    /// <summary>
    //    /// Chamada a um serviço baseado numa lista (TODO: não poderia ser um recurso ?)
    //    /// </summary>
    //    ServiceCallWithList = 44,


    //    */





    //    /// <summary>
    //    /// Busca por substring (num string !) ?)
    //    /// </summary>
    //    Find = 45,




    //    /// <summary>
    //    /// Retorna o valor de uma variável do fluxo (variável da caixinha do fluxo)
    //    /// </summary>
    //    GetVarValue = 46,

    //    /// <summary>
    //    /// Atribui o valor de uma variável do fluxo (variável da caixinha do fluxo)
    //    /// </summary>
    //    SetVarValue = 47,

    //    /// <summary>
    //    /// Vai para uma rota.
    //    /// </summary>
    //    GoToRoute = 48
    //}
}
