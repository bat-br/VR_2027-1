using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Threading;
using Unity.VisualScripting;
using System.IO;




public class FlightThreadNoSinc : MonoBehaviour
{
    //Variables de clase 
    public float speed = 50f;
    public float rotationSpeed = 100f;
    public Transform cameraTransform;
    public Vector2 movementInput;

    //Control de iteraciones
    public int turbulenceIterations = 1000000;

    //Lista de vectore de posicion calculados
    private List<Vector3> turbulenceForces = new List<Vector3>();

    //Variables para manipular el hilo secundario
    private Thread turbulenceThread;
    private bool isTurbulenceRunning = false;
    private bool stopTurbulenceThread = false;
    private float captureTime;

    //Banderas de control sobre lectura
    public bool read = false;
    string filepath;



    //Metodo para leer la entrada de teclado
    public void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filepath = Application.dataPath + "/TurbulenceData.txt";
        Debug.Log("Ruta al archivo: " + filepath);

    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("No hay camara asignada");
            return;
        }
        //Tiempo trascurrido 
        captureTime = Time.time;

        //Proceso de consumo de recursos
        if (!isTurbulenceRunning)
        {
            isTurbulenceRunning = true;
            stopTurbulenceThread = false;

            turbulenceThread = new Thread(() => SimulateTurbulence(captureTime));
            turbulenceThread.Start();
        }


        //Mover la nave linealmente
        Vector3 moveDirection = cameraTransform.forward * movementInput.y * speed * Time.deltaTime;

        this.transform.position += moveDirection;

        //Mover la nave en rotacion
        float yaw = movementInput.x * rotationSpeed * Time.deltaTime;
        this.transform.Rotate(0, yaw, 0);

        //Meotod para la lectura del archivo
        TryReadFile();

    }
    //Metodo para simular turbulencia
    public void SimulateTurbulence(float time)
    {
        turbulenceForces.Clear();
        //Repeticiones
        for (int i = 0; i < turbulenceIterations; i++)
        {
            //Verificando si se debe detener el hilo
            if (stopTurbulenceThread)
            {
                break;
            }
            Vector3 force = new Vector3
                (
                    Mathf.PerlinNoise(i * 0.001f, time) * 2 - 1,
                    Mathf.PerlinNoise(i * 0.002f, time) * 2 - 1,
                    Mathf.PerlinNoise(i * 0.003f, time) * 2 - 1
                );

            turbulenceForces.Add(force);
        }

        //Señal en consola de inicio de hilo
        Debug.Log("Iniciando simulaciones de turbulencia");

        //Escritura del archivo

        using (StreamWriter writer = new StreamWriter(filepath, false))
        {
            foreach (var force in turbulenceForces)
            {
                writer.WriteLine(force.ToString());
            }
            writer.Flush();
        }

        Debug.Log("Archivo escrito");

            //Simulacion completa
            isTurbulenceRunning = false;
    }
    public void TryReadFile()
    {
        try
        {
            string content = File.ReadAllText(filepath);
            Debug.Log("Archivo leido: " + content);
        }
        catch(IOException ex) 
        {
            Debug.LogError("Error en acceso al archivo"+ ex.Message);
        }
    }
    private void OnDestroy()
    {
        //Indicador el cierre del hilo secundario
        stopTurbulenceThread = true;

        //Verificar si el hilo existe y se este ejecuantando
        if (turbulenceThread != null && turbulenceThread.IsAlive)
        {
            //Unir al hio principal y cerra ejecucion
            turbulenceThread.Join();
        }
    }
}
