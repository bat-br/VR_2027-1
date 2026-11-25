using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using System.Security.Cryptography.X509Certificates;

public class BDFireBase : MonoBehaviour
{
    public DatabaseReference reference;

    [SerializeField]
    TMP_InputField textoNombre;
    [SerializeField]
    TMP_InputField textoEdad;
    [SerializeField]
    TMP_InputField textoActualizar;
    public bool registroBooleano = true;

    private void Awake()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Booleano(bool toggleB)
    {
        registroBooleano = toggleB;
    }

    //Registro
public void Registro()
    {
        //Generar una clave unica para el registro

        string key = reference.Child("Nombre").Push().Key;
        reference.Child("Nombre").Child(key).SetValueAsync(textoNombre.text);

        //Clave unica para datos individuales de forma manual
        string key2= reference.Child("Nombre").Push().Key;
        reference.Child("Datos").Child(key2).Child("Texto").SetValueAsync("Registro de texto");
        reference.Child("Datos").SetValueAsync(int.Parse(textoEdad.text));
        reference.Child("Datos").SetValueAsync(registroBooleano);

        //Datos de escuela
        Debug.Log("Dato escuela actual: UNAM");
        reference.Child("Escuela").SetValueAsync("UNAM");
        Debug.Log("Dato año actual: 2025");
        reference.Child("Año").SetValueAsync(2025);

        //Registro de Usuario tipo Usuario
        UsuarioF usuario = new UsuarioF("Carlos", "carlitos@gmail.com");
        string json = JsonUtility.ToJson(usuario);
        reference.Child("Usuario").SetRawJsonValueAsync(json);
    }

    public void Actualizar()
    {
        //Datos actualizar escuela
        Debug.Log("Dato escuela actual: POLI");
        reference.Child("Escuela").SetValueAsync("POLI");
        Debug.Log("Dato año actualizado: 2026");
        reference.Child("Año").SetValueAsync(2026);

    }

    public void LoadFromBD()
    {
        reference.Child("Año").GetValueAsync().ContinueWithOnMainThread(TaskExtension =>
        {
            if(TaskExtension.IsFaulted)
            {
                Debug.LogError("Error al obtener la base de datos"+TaskExtension.Exception);
            }
            else if (TaskExtension.IsCompleted)
            {
                DataSnapshot snapshot = TaskExtension.Result;
                string value = snapshot.Value.ToString();
                Debug.Log($"Tipo de valor obtenido {snapshot.Value.GetType()} y el contenido {value}");

            }
            else
            {
                Debug.Log("Registro no encontrado");
            }
        });
        //Carga de valores anidados
        reference.Child("Nombre").GetValueAsync().ContinueWithOnMainThread(TaskExtension =>
        {
            if (TaskExtension.IsFaulted)
            {
                Debug.LogError("Error al obtener la base de datos: " + TaskExtension.Exception);

            }
            else if(TaskExtension.IsCompleted)
            {
                DataSnapshot snapshot = TaskExtension.Result;
                //Recorrer todos los hios de Nombre y obtener valores
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    string value2 = childSnapshot.Value.ToString();
                    Debug.Log($"Tipo de valor {snapshot.Value.GetType()} y el valor {value2}");
                }
            }
            else
            {
                Debug.Log("Registr no encontrado");
            }
        }
        );
        //Carga tipo JSON
        reference.Child("Usuario").GetValueAsync().ContinueWithOnMainThread(TaskExtension =>
        {
            if (TaskExtension.IsFaulted)
            {
                Debug.LogError("Error al obtener la base de datos: " + TaskExtension.Exception);

            }
            else if (TaskExtension.IsCompleted)
            {
                DataSnapshot snapshot3 = TaskExtension.Result;
                //Convirtiendo el JSON a diccionario
                Dictionary<string, object> userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(snapshot3.GetRawJsonValue());
                Debug.Log(userData.GetType());
                string nombre = ((string)userData["UserName"]);
                string email = ((string)userData["Email"]);
                Debug.Log($"Nombre de usuario: {nombre}, correo:{email}");

            }
            else
            {
                Debug.Log("Registr no encontrado");
            }
        }
       );
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
public class UsuarioF
    {
    public string UserName;
    public string Email;

    public UsuarioF(string userName, string email)
    {
        UserName = userName;
        Email = email;
    }
}

