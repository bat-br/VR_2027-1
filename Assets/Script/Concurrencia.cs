using UnityEngine;//Corutinas
using System.Collections;
using System.Collections.Generic;
using System;//Sincrono
using System.Threading;//Hilos
using System.Threading.Tasks;
using UnityEngine.Animations;//task
public class Concurrencia : MonoBehaviour
{
    [Header("Actva los metodos ")]
    public bool useSincrono = false;
    public bool useThread = false;
    public bool useCorutine = false;
    public bool useTask = false;
    [Header("Cosas a mover ")]
    public Transform sincroneSphere;
    public Transform threadSphere;
    public Transform coroutineSphere;
    public Transform taskSphere;
    public Transform mainCube;

    //Accion a ejecutar en hilo secundario
    private Queue<Action> mainThreadActions = new Queue<Action>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (useSincrono) MoveSincrono();
        if (threadSphere) MoveWithThread();
        if (coroutineSphere) StartCoroutine( MoveWithCoroutine());
        if (taskSphere) MoveWithTask();

    }

    // Update is called once per frame
    void Update()
    {
        //Siempre el giro del cubo de referencia en hilo principal
        mainCube.Rotate(Vector3.up,50*Time.deltaTime);

        //Ejecuta las acciones en el hilo principal
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count>0)
            {
                mainThreadActions.Dequeue().Invoke();
            }

        }
        
    }

    //Metodo sincrono
    public void MoveSincrono()
    {
        for (int i=0; i<=100; i++)
        {
            sincroneSphere.position += Vector3.right * 0.05f;
        }
        Thread.Sleep(50);
    }

    //Metodo con hilo secundario
    private void MoveWithThread()
    {
        new Thread(() =>
        {
           for (int i = 0; i <= 100; i++)
           {
            Thread.Sleep(50);
                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        threadSphere.position += Vector3.right * 0.05f;
                    });
                }
                
           }
            
        }).Start();
    }
    //Metodo asincrono con task
    private async void MoveWithTask()
    {
        await Task.Run(() =>
        {
            for (int i = 0; i<=100; i++ )
               { 
                    Thread.Sleep(50);
                    lock (mainThreadActions)
                    {
                        mainThreadActions.Enqueue(() =>
                        {
                            taskSphere.position += Vector3.right * 0.05f;
                        });
                    }
               }
       });

    }
    //Metodo para Corutina

   private IEnumerator MoveWithCoroutine()
    {
        for (int i = 0; i <= 100; i++)
        {
            coroutineSphere.position += Vector3.right * 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
