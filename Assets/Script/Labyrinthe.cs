using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


public class Labyrinthe : MonoBehaviour
{
    //Objetos en la escena
    public GameObject player;
    public Transform entrance;
    public Transform exit;
    public GameObject winCanvas;

    //Variables de configuracion
    public float detectionRange = 1f;
    public float exitRange = 1f;
    public float minDistanceFromEntrance = 3f;

    //Agentes de IA
    List<NavMeshAgent> agents = new();

    NavMeshTriangulation triangulation;
    Vector3 entrancePos;

    //Variables para el algoritmo
    float detectionRangeSqr;
    float exitRangeSqr;
    float minDistanceSqr;

    bool gameWon = false;

    //Concurrencia
    System.Random random = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entrancePos = entrance.position;
        triangulation = NavMesh.CalculateTriangulation();

        detectionRangeSqr = detectionRange * detectionRange;
        exitRangeSqr = exitRange * exitRange;
        minDistanceSqr = minDistanceFromEntrance * minDistanceFromEntrance;

        winCanvas.SetActive(false);

        //Encontrar a los enemigos 


    }

    // Update is called once per frame
    void Update()
    {

    }

    //Metodo para regresar a la entrada
    void TeleportPlayerToEntrance()
    {
        var cc = player.GetComponent<NavMeshAgent>();
        if (cc != null)
        {
            cc.enabled = false;

        }
        player.transform.position = entrancePos;
        if (cc != null)
        {
            cc.enabled = true;

        }
        Debug.Log("Teleport a: " + entrancePos);


    }
    //Objeto canvas de ganar
    void WinGame()
    {
        gameWon = true;

        foreach(var agent in agents)
        {
            agent.isStopped = true;

        }

        winCanvas.SetActive(true);
    }

    //Encontrar todos los enemigos

    void FindAllEnemies()
    {
        agents.Clear();
        foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
        {
            if(agent.CompareTag("Enemy"))
            {
                agents.Add(agent);
            }
        }
    }

}
