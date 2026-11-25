using UnityEngine;

public abstract class EntradaBrazo : MonoBehaviour
{
    public Vector2 Eje { get; protected set; }

    private bool pulsacionPendiente;

    // Devuelve true una sola vez por cada pulsación.
    public bool ConsumirPulsacion()
    {
        bool pulsado = pulsacionPendiente;
        pulsacionPendiente = false;
        return pulsado;
    }

    protected void RegistrarPulsacion()
    {
        pulsacionPendiente = true;
    }
}
