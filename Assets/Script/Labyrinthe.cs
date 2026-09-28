using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using JetBrains.Annotations;


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
        FindAllEnemies();

    }

    // Update is called once per frame
    void Update()
    {
        if (gameWon) return;
        Vector3 playerPos = player.transform.position;
        bool playerCaught = false;
        foreach(var agent in agents)
        {
            if(!agent.enabled) continue;
            if ((agent.transform.position - playerPos).sqrMagnitude < detectionRangeSqr)
            {
                playerCaught = true; break;
            }
        }
        //Si capturan al jugador
        if(playerCaught)
        {
            TeleportPlayerToEntrance();
            RelocateAllNPC();
            return;
        }
        //Si el jugador llega al area de salida
        if ((playerPos - exit.position).sqrMagnitude < exitRangeSqr)
        {
            WinGame();
            return;

        }
        //Persecucion
        foreach (var agent in agents)
        {
            if (agent.enabled && !agent.isStopped)
            {
                agent.SetDestination(playerPos);
            }
        }
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

        foreach (var agent in agents)
        {
            agent.isStopped = true;

        }

        winCanvas.SetActive(true);
    }
    // Meotodo para posicionar a lo enemigos
    public void RelocateAllNPC()
    {
        if (triangulation.vertices.Length == 0)
        {
            return;
        }
        foreach (var agent in agents)
        {
            agent.enabled = false;
            agent.transform.position =GetValidRandmPosition(); //Generar posición aleatoria
            agent.enabled = true;
        }
    }
    public Vector3 GetValidRandmPosition()
    {
        Vector3 pos;
        do
        {
            int t= random.Next(0,triangulation.indices.Length/3)*3;

            Vector3 v1= triangulation.vertices[triangulation.indices[t]];
            Vector3 v2 = triangulation.vertices[triangulation.indices[t+1]];
            Vector3 v3 = triangulation.vertices[triangulation.indices[t+2]];
            float r1 = (float)random.NextDouble();
            float r2 = (float)random.NextDouble();
            if(r1+r2>1f)
            {
                r1 = 1f - r1;
                r2=1f-r2;
            }
            pos = v1+r1*(v2-v1)+r2*(v3-v1);
        }
        while ((pos - entrancePos).sqrMagnitude < minDistanceSqr);
        return pos;

    }
        //Encontrar todos los enemigos

        void FindAllEnemies()
        {
            agents.Clear();

            foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
            {
                if (agent.CompareTag("Enemy"))
                {
                    agents.Add(agent);
                }
            }
        }

    }

