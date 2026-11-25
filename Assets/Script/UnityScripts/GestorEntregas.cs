using TMPro;
using UnityEngine;

public class GestorEntregas : MonoBehaviour
{
    [SerializeField] private Pieza[] piezas;
    [SerializeField] private ZonaEntrega zona;
    [SerializeField] private ControlBrazo brazo;
    [SerializeField] private TMP_Text textoMarcador;

    private int entregadas;

    public bool Completado => entregadas == piezas.Length;

    private void Start()
    {
        ActualizarTexto();
    }

    private void Update()
    {
        if (!zona.Disponible)
        {
            return;
        }

        foreach (Pieza pieza in piezas)
        {
            if (!pieza.Entregada && pieza.EnReposo && zona.Contiene(pieza.transform.position))
            {
                pieza.MarcarEntregada();
                entregadas++;
                ActualizarTexto();
            }
        }
    }

    // Se conecta al evento OnClick del botón Reiniciar.
    public void Reiniciar()
    {
        foreach (Pieza pieza in piezas)
        {
            pieza.Reiniciar();
        }

        entregadas = 0;
        brazo.Reiniciar();
        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        textoMarcador.text = Completado
            ? "¡Completado!"
            : $"Entregadas: {entregadas}/{piezas.Length}";
    }
}
