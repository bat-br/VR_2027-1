using TMPro;
using UnityEngine;
using Vuforia;

// Va en el Multi Target. Detiene el juego cuando no hay una pose confiable.
[RequireComponent(typeof(ObserverBehaviour))]
public class EstadoTracking : MonoBehaviour
{
    [SerializeField] private ControlBrazo brazo;
    [SerializeField] private TMP_Text textoAviso;

    private ObserverBehaviour observador;

    public bool Disponible { get; private set; }

    private void Awake()
    {
        observador = GetComponent<ObserverBehaviour>();
        observador.OnTargetStatusChanged += AlCambiarEstado;
        Aplicar(false);
    }

    private void OnDestroy()
    {
        if (observador != null)
        {
            observador.OnTargetStatusChanged -= AlCambiarEstado;
        }

        Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    private void AlCambiarEstado(ObserverBehaviour comportamiento, TargetStatus estado)
    {
        bool disponible = estado.Status == Status.TRACKED || estado.Status == Status.EXTENDED_TRACKED;
        Aplicar(disponible);
    }

    private void Aplicar(bool disponible)
    {
        Disponible = disponible;
        brazo.Habilitado = disponible;
        textoAviso.gameObject.SetActive(!disponible);
        Physics.simulationMode = disponible ? SimulationMode.FixedUpdate : SimulationMode.Script;
    }
}
