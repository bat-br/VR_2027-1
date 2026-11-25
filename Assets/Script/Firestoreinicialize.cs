using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Xml.Serialization;

public class Firestoreinicialize : MonoBehaviour
{
    [SerializeField] private TMP_InputField cardNameInputField;
    private FirebaseFirestore firestore;


    private void Awake()
    {
        DontDestroyOnLoad(this);
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public  void TextoAR()
    {
        SceneManager.LoadScene("AR");
    }

    public void FetchAndStoreCardData()
    {
        string cardName = cardNameInputField.text;
        StartCoroutine(GetCardData(cardName));
    }
    private IEnumerator GetCardData(string cardName)
    {
        string url = $"https://db.ygoprodeck.com/api/v7/cardinfo.php?name={UnityWebRequest.EscapeURL(cardName)}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ProcessCardData(json);
        }
        else
        {
            Debug.LogError("Erro fetchin card data: " + request.error);
        }

    }
    private void ProcessCardData(string json)
    {
        //Parse JSON data
        var cardData = JsonUtility.FromJson<CardDataResponse>(json);
        if(cardData.data.Length>0)
        {
            var card= cardData.data[0];
            string cardType = card.type;
            string collection = DatermineCollection(cardType);
            string cardName = card.name;

            //crea los docuentos en la referencia de firestore
            DocumentReference docRef = firestore.Collection("Cartas").Document(collection).Collection(cardName).Document("Datos");
            //Preparar datos y guardar en firestore

            var cardInfo = new Dictionary<string, object> 
            {
                {"ATK",card.atk },
                {"DEF",card.def },
                {"Desc",card.desc },
                {"Level",card.level }
            };
            docRef.SetAsync(cardInfo).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Datos de {cardName} almacenados correctamente");
                }
                else
                {
                    Debug.LogError("Error guardando datos: " + task.Exception);
                }
            });
        } 
        else
        {
            Debug.Log("Carta no encontrada");
        }
    }

    private string DatermineCollection(string cardtype)
    {
        if(cardtype.Contains("Monster"))
            return "Monstruo";
        if (cardtype.Contains("Spell"))
            return "Magia";
        if (cardtype.Contains("Trap"))
            return "Trampa";
        else return "Otros";
    }

    public void FetchCardDataFromFireStore(string cardName, TMP_Text textMesh)
    {
        FetchCardDataFromFireStoreAsync(cardName, textMesh);
    }
    public async void FetchCardDataFromFireStoreAsync(string cardName, TMP_Text textMesh)
    {
        textMesh.text = "Buscando datoss...";

        List<string> collections = new List<string>() { "Monstruo", "Magia", "Trampa", "Otros" };
        List<Task<DocumentSnapshot>> task = new List<Task<DocumentSnapshot>>();

        //Iniciar toda las consultas
        foreach(string collection in collections)
        {
            DocumentReference docRef = firestore.Collection("Cartas").Document(collection).Collection(cardName).Document("Datos");

            task.Add(docRef.GetSnapshotAsync());
        }

        try
        {
            //esperar a que todas las consultas terminen
            DocumentSnapshot[] snapshots = await Task.WhenAll(task);

            //Buscar la primera que tenga datos
            foreach (var snapshot in snapshots)
            {
                if(snapshot.Exists)
                {
                    Dictionary < string , object > cardData = snapshot.ToDictionary();
                    textMesh.text = $"Nombre: {cardName}\n"+
                         $"ATK: {cardData["ATK"]}\n" +
                         $"DEF: {cardData["DEF"]}\n"+
                         $"Descripcion: {cardData["Desc"]}\n"+
                         $"Nivel: {cardData["Level"]}\n";
                    return;
                }
            }
            textMesh.text = "carta no encontrada en ninguna coleccion";
        }
        catch(Exception e)
        {
            Debug.LogError("Error al buscar la carta: "+ e.Message);
            textMesh.text = "Error al buscar la carta";
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
//Clase para mapear la respuesta JSON

[System.Serializable]
public class CardDataResponse
{
    public CardData[] data;
}

[System.Serializable]
public class CardData
{
    public string name;
    public string type;
    public string desc;
    public int atk;
    public int def;
    public int level;
}
