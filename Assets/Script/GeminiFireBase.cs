using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.AppCheck;
using Firebase.AI;
using TMPro;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class GeminiFireBase : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text chatText;
    public Button enviarButton;

    [Header("Configuracion AI")]
    public string modelName = "gemini-3.8-flash";

    [Header("App check")]
    [Tooltip("Debug token creado en Firebase")]
    public string appCheckDebugToken = "";
    [Tooltip("Activalo si en la consla la protección cointra repeticion esta activa")]
    public bool usarTokenDeUsoLimitado = true;

    [Header("Animacion")]
    public Animator unityChanAnimator;

    private GenerativeModel model;
    private string historial = "";

    //Prompr del sistema personalidad + formato de respuesta

    private const string personalidad =
        "Eres Unity-Chan, una asistente virtual tsundere" +
        "Habla con dulzura pero con orgullo, usas expresiones como trikitraketelas" +
        "Eres alegre, energica y un poco orgullosa.Tus respuestas son cortas, menos de 10 palabras" +
        "Ademas de responder analiza tu emocion y responde SOLO em este formato JSON exacto" +
        "{\"respuesta\":\"texto que diras\",\"emocion\":\"feliz\"}" +
        "donde emocion es exactamente una de :feliz, enojada, hablar."+
        "No agregues nada fuera del JSON";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        enviarButton.interactable = false;
        chatText.text = "Inicializando Unity-Chan....";

        var depStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if(depStatus != DependencyStatus.Available)
        {
            chatText.text = $"Error inicialiazando Firebase: {depStatus}";
                return;
        }
        if(string.IsNullOrWhiteSpace(appCheckDebugToken))
        {
            Debug.Log("Falta el token de AppCheck");
            return;
        }
        DebugAppCheckProviderFactory.Instance.SetDebugToken(appCheckDebugToken.Trim());
        FirebaseAppCheck.SetAppCheckProviderFactory(DebugAppCheckProviderFactory.Instance);

        //Gemini con token
        var ai = FirebaseAI.GetInstance(
            FirebaseAI.Backend.GoogleAI(),
            useLimitedUseAppCheckTokens:usarTokenDeUsoLimitado
            );
        model =ai.GetGenerativeModel(modelName: modelName);
        chatText.text = "Unity-chan esta lista";
        enviarButton.interactable = true;
    }

    public void OnEnviarButtonClicked()
    {
        string mensaje = inputField.text;
        if (string.IsNullOrWhiteSpace(mensaje)) return;
        inputField.text = "";
        inputField.ActivateInputField();
        EnviarMensaje(mensaje);
    }

    public async void EnviarMensaje(string mensajeUsuario)
    {
        enviarButton.interactable = false;
        chatText.text = "Unity_chan esta pensandp . . .";

        string prompt = $"{personalidad}\nHistorial:\n{historial}Usuario{mensajeUsuario}\nUnity-chan:";

        try
        {
            var respuesta = await model.GenerateContentAsync(prompt);
            historial += $"Usuario: {mensajeUsuario}";
            //Procesar la respuesta 
            ProcesarRespuesta(respuesta.Text ?? "");

        }
        catch(System.Exception e) 
        {
            Debug.LogError("Error al llamar a Gemini " + e);
        }
        finally
        {
            enviarButton.interactable = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ProcesarRespuesta(string respuesta)
    {
        Debug.Log("Respuesta de Gemini:"+respuesta);
        var match = Regex.Match(respuesta, "\\{.*\\}", RegexOptions.Singleline);

        if(match.Success)
        {
            try
            {
                var parsed = JsonUtility.FromJson<RespuestaAI>(match.Value);
                if(!string.IsNullOrEmpty(parsed.respuesta))
                {
                    chatText.text = parsed.respuesta;
                    EjecutarAnimacion(string.IsNullOrEmpty(parsed.emocion)? "hablar" : parsed.emocion);
                    historial += $"Unity-chan: {parsed.respuesta}";
                    return;
                }

            }
            catch
            {
                Debug.LogError("Error al deconstruir la respuesta Json");
            }
        }
        //No vino en JSON: mostrar el texto tal cual
        chatText.text = respuesta.Trim();
        EjecutarAnimacion("hablar");
        historial += $"Unity-chan: {respuesta.Trim()}\n";
    }
    public void EjecutarAnimacion(string emocion)
    {
        if (unityChanAnimator == null) return;
        switch(emocion.Trim().ToLower())
        {
            case "feliz":unityChanAnimator.SetTrigger("Feliz");break;
            case "enojada": unityChanAnimator.SetTrigger("Enojada"); break;
            default: unityChanAnimator.SetTrigger("Hablar"); break;

        }
    }
    [System.Serializable]
    private class RespuestaAI
    {
        public string respuesta;
        public string emocion;
    }
}
