using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class UISelection : MonoBehaviour
{
    public static bool gazedAt;
    public float fillTime = 5f;
    public Image radialImage;
    public UnityEvent onFillComplete; //Evento generico cuando se interactua

    private Coroutine fillCorountine; // Proceso concurrente
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gazedAt = false;
        radialImage.fillAmount = 0f;
    }
    public void OnPointerEnter()
    {
        gazedAt = true;
        if ( fillCorountine != null)
        {
            StopCoroutine(fillCorountine);//Detener cualquier corutina anterior 

        }
        fillCorountine = StartCoroutine(FillRadial());
    }
    public void OnPointerExit()
    {
        gazedAt = false;
        if (fillCorountine != null)
        {
            StopCoroutine(fillCorountine);//Detener cualquier corutina anterior 
            fillCorountine = null;
        }
        radialImage.fillAmount = 0f;
    }
    private IEnumerator FillRadial()
    {
        float elapsedTime = 0f;

        while(elapsedTime < fillTime)
        {
            if (!gazedAt)//Si deja de ser observado
            {
                yield break;//Salir de la corutina
            }
            elapsedTime += Time.deltaTime;
            radialImage.fillAmount =Mathf.Clamp01(elapsedTime / fillTime);

            yield return null;
        }
        //Ejecuta el evento onFillComplete cuando se completa el llenado 
        onFillComplete?.Invoke();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
