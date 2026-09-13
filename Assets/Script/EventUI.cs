//Establecer la accion que queremos ya sea ir a una pestaña o ejeutar un boton
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;//Mover texto en el entorno
using UnityEngine.SceneManagement;

public class EventUI : MonoBehaviour
{
    public List<GameObject> objects; //Lista de objetos
    public List<string> messages;//Lista de mensajes a mostrar
    public int currentIndex = 0;
    public TextMeshProUGUI textMeshPro;//Componente de texto en un objeto (va cambiar el valor preasignado)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateVisibility();
        UpdateText();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Metodo para el cilco de objetos 
    public void CycleObjects()
    {
        //Incrementa el indice y vuelve al principio si es necesario
        currentIndex = (currentIndex + 1) % objects.Count;
        //Actualizar la visibilidad de los objetos 
        UpdateVisibility();
    }
    public void CycleObjectsBefore()
    {
        currentIndex = currentIndex - 1;// supongamos que es 0 cuando de -1 va entrar al if lo que hacer es mandarlo a el ultmo indice, y seguira restando hasta que se repita el ciclo
        if (currentIndex < 0)
        {
            currentIndex = objects.Count - 1; 
        }
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        for (int i = 0; i<objects.Count;i++)
        {
            //Solo el objeto en el indice actual es visible
            objects[i].SetActive(i==currentIndex);
        }
    }

    //Metodo para ciclo de texto
    public void CycleText()
    {
        //Incrementa el indice y vuelve al principio si es necesario
        currentIndex = (currentIndex + 1) % messages.Count;
        //Actualizar el texto actual
        UpdateText(); 
    }
    public void CycleTextBefore()
    {
        currentIndex = currentIndex - 1;
        if(currentIndex<0)
        {
            currentIndex = messages.Count - 1;
           
        }
        UpdateText();
    }
    private void UpdateText()
    {
        if (messages.Count > 0 && textMeshPro != null)
        {
            textMeshPro.text = messages[currentIndex];
        }
    }

    //Cambiar de escena por nombre

    public void ChangeSceneByName(string sceneName)
    {
        SceneManager. LoadScene(sceneName);
    }
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Salio del juego");
    }
}
