using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TMP_Text displayText;
    public Firestoreinicialize cardAccess;
    void Start()
    {
        cardAccess = GameObject.FindGameObjectWithTag("BD").GetComponent<Firestoreinicialize>();
    }

    public void OnTargetFound(Transform imageTargetTransform)
    {
        string cardName = imageTargetTransform.name;
        Debug.Log("Carta encontrada: " + cardName);

        displayText = imageTargetTransform.Find("Text").GetComponent<TextMeshPro>();
        if (displayText != null)
        {
            cardAccess.FetchCardDataFromFireStore(cardName, displayText);
        }
        else
        {
            Debug.Log("Error carta no encontrada");
        }
    }
    public void OnTargetLost(Transform imageTargetTransform)
    {
        displayText = imageTargetTransform.Find("Text").GetComponent<TextMeshPro>();
        if (displayText != null)
        {
            displayText.text = "Buscando carta";
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
