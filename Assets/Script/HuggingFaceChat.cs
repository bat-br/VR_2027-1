using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class HuggingFaceChat : MonoBehaviour
{

    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text chatText;
    public Button enviarBoton;

    [Header("Animacion")]
    public Animator animator;

    [Header("HugginFace Config")]
    [TextArea] public string apiKey;

    private const string URL = "https://router.huggingface.co/v1/chat/completions";
    // Cambiado a un modelo gratuito
    private const string MODELO = "mistralai/Mistral-7B-Instruct-v0.2";

    private const string PERSONALIDAD =
        "Eres Unity-Chan, una asistente virtual tsundere" +
        "Habla con dulzura pero con orgullo, usas expresiones como trikitraketelas" +
        "Eres alegre, energica y un poco orgullosa.Tus respuestas son cortas, menos de 10 palabras" +
        "Ademas de responder analiza tu emocion y responde SOLO em este formato JSON exacto" +
        "{respuesta:texto que diras,emocion:feliz o enojada o hablar}" +
        "No agregues nada fuera del JSON";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enviarBoton.onClick.AddListener(EnviarMensaje);//Evento a lanzar
    }

    public void EnviarMensaje()
    {
        string mensaje = inputField.text;

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        inputField.text = "";

        StartCoroutine(EnviarMensaje(mensaje));

    }

    IEnumerator EnviarMensaje(string mensaje)
    {
        //Constuir el JSON de forma segura JsonUtility
        var requesData = new HFRequest
        {
            model = MODELO,
            max_tokens = 1024,
            messages = new HFMessage[]
            {
                new HFMessage{role = "system", content = PERSONALIDAD },
                new HFMessage{role="user",content=mensaje}
            }
        };
        string body = JsonUtility.ToJson(requesData);

        var request = new UnityWebRequest(URL,"POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()      
        };

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error HTTP: "+ request.error);
            Debug.LogError("Body enviado: " + body);
            chatText.text = "Error al concetar con HugginFace";
            yield break;
        }
        //Procesar la respuesta
        ProceasarRespuesta(request.downloadHandler.text);

    }
    private void ProceasarRespuesta(string rawReponse)
    {
        try
        {
            var hf = JsonUtility.FromJson<HFRespose>(rawReponse);
            string json = hf.choices[0].message.content;

            var data= JsonUtility.FromJson<RespuestaAI>(json);
            chatText.text = data.respuesta;
            //Ejecutar animacion
            EjecutarAnimacion(data.emocion);
        }
        catch
        {
            chatText.text = "Unity-chan tuvo un error";
        }
    }
    public void EjecutarAnimacion(string emocion)
    {
        if (animator == null) return;
        switch(emocion.ToLower())
        {
            case "feliz":animator.SetTrigger("Feliz"); break;
            case "enojada": animator.SetTrigger("Enojada"); break;
            case "hablar": animator.SetTrigger("Hablar"); break;
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    //Request
    [System.Serializable]
    private class HFRequest
    {
        public string model;
        public int max_tokens;
        public HFMessage[] messages;
    }
    [System.Serializable]
    private class HFMessage
    {
        public string role;
        public string content;
    }
    //Playload del modelo
    [System.Serializable]
    private class RespuestaAI
    {
        public string respuesta;
        public string emocion;
    }
    //Response
    [System.Serializable] private class HFRespose
    {
        public Choice[] choices;

    }
    [System.Serializable] private class Choice
    {
        public HFMessage message;
    }
}

