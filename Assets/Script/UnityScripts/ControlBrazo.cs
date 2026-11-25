using UnityEngine;

// Mueve el objetivo IK. Animation Rigging calcula el hombro y el codo.
public class ControlBrazo : MonoBehaviour
{
    private enum Estado
    {
        Libre,
        Bajando,
        Subiendo
    }

    [Header("Referencias")]
    [SerializeField] private EntradaBrazo entrada;
    [SerializeField] private Transform baseGiratoria;
    [SerializeField] private Transform ikObjetivo;
    [SerializeField] private Transform puntoAgarre;
    [SerializeField] private Transform contenedorPiezas;
    [SerializeField] private Transform dedoIzquierdo;
    [SerializeField] private Transform dedoDerecho;

    [Header("Movimiento")]
    [SerializeField] private bool invertirY;
    [SerializeField] private float velocidadGiro = 90f;
    [SerializeField] private float velocidadRadial = 0.12f;
    [SerializeField] private float velocidadVertical = 0.12f;
    [SerializeField] private float anguloMaximo = 170f;
    [SerializeField] private float radioMinimo = 0.06f;
    [SerializeField] private float radioMaximo = 0.18f;
    [SerializeField] private float alturaViaje = 0.09f;

    [Header("Agarre")]
    [SerializeField] private LayerMask capasApoyo;
    [SerializeField] private LayerMask capaPiezas;
    [SerializeField] private float distanciaMunecaPinza = 0.035f;
    [SerializeField] private float radioAgarre = 0.025f;
    [SerializeField] private float mitadPieza = 0.0125f;
    [SerializeField] private float holguraSoltar = 0.002f;
    [SerializeField] private float dedoAbierto = 0.022f;
    [SerializeField] private float dedoCerrado = 0.0145f;

    private Estado estado = Estado.Libre;
    private float angulo;
    private float radio = 0.14f;
    private float altura;
    private float alturaDestino;
    private Pieza piezaSujeta;

    public bool Habilitado { get; set; }
    public bool TienePieza => piezaSujeta != null;
    public bool EnLimite { get; private set; }

    private void Start()
    {
        altura = alturaViaje;
        AplicarPose();
    }

    private void Update()
    {
        if (!Habilitado)
        {
            entrada.ConsumirPulsacion();
            EnLimite = false;
            return;
        }

        switch (estado)
        {
            case Estado.Libre:
                ActualizarMovimientoLibre();
                break;
            case Estado.Bajando:
                ActualizarBajada();
                break;
            case Estado.Subiendo:
                ActualizarSubida();
                break;
        }

        AplicarPose();
    }

    public void Reiniciar()
    {
        piezaSujeta = null;
        estado = Estado.Libre;
        angulo = 0f;
        radio = 0.14f;
        altura = alturaViaje;
        AplicarPose();
    }

    private void ActualizarMovimientoLibre()
    {
        Vector2 eje = entrada.Eje;
        float avance = invertirY ? -eje.y : eje.y;

        float anguloDeseado = angulo + eje.x * velocidadGiro * Time.deltaTime;
        float radioDeseado = radio + avance * velocidadRadial * Time.deltaTime;

        angulo = Mathf.Clamp(anguloDeseado, -anguloMaximo, anguloMaximo);
        radio = Mathf.Clamp(radioDeseado, radioMinimo, radioMaximo);
        EnLimite = !Mathf.Approximately(angulo, anguloDeseado) || !Mathf.Approximately(radio, radioDeseado);

        if (entrada.ConsumirPulsacion())
        {
            IniciarBajada();
        }
    }

    private void IniciarBajada()
    {
        Vector3 origen = ikObjetivo.position;
        if (!Physics.Raycast(origen, Vector3.down, out RaycastHit impacto, 0.5f, capasApoyo))
        {
            return;
        }

        float margen = TienePieza ? mitadPieza + holguraSoltar : 0f;
        Vector3 puntoMuneca = impacto.point + Vector3.up * (distanciaMunecaPinza + margen);
        alturaDestino = Mathf.Min(baseGiratoria.InverseTransformPoint(puntoMuneca).y, alturaViaje);
        estado = Estado.Bajando;
    }

    private void ActualizarBajada()
    {
        altura = Mathf.MoveTowards(altura, alturaDestino, velocidadVertical * Time.deltaTime);
        if (!Mathf.Approximately(altura, alturaDestino))
        {
            return;
        }

        if (TienePieza)
        {
            piezaSujeta.Soltar(contenedorPiezas);
            piezaSujeta = null;
        }
        else
        {
            AgarrarPiezaCercana();
        }

        estado = Estado.Subiendo;
    }

    private void ActualizarSubida()
    {
        altura = Mathf.MoveTowards(altura, alturaViaje, velocidadVertical * Time.deltaTime);
        if (Mathf.Approximately(altura, alturaViaje))
        {
            estado = Estado.Libre;
        }
    }

    private void AgarrarPiezaCercana()
    {
        Vector3 centroPinza = ikObjetivo.position + Vector3.down * distanciaMunecaPinza;
        Collider[] encontrados = Physics.OverlapSphere(centroPinza, radioAgarre, capaPiezas);

        float menorDistancia = float.MaxValue;
        foreach (Collider encontrado in encontrados)
        {
            Pieza pieza = encontrado.GetComponent<Pieza>();
            if (pieza == null || pieza.Entregada)
            {
                continue;
            }

            float distancia = Vector3.Distance(centroPinza, pieza.transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                piezaSujeta = pieza;
            }
        }

        if (piezaSujeta != null)
        {
            piezaSujeta.Sujetar(puntoAgarre);
        }
    }

    private void AplicarPose()
    {
        baseGiratoria.localRotation = Quaternion.Euler(0f, angulo, 0f);
        ikObjetivo.localPosition = new Vector3(0f, altura, radio);

        float separacion = TienePieza ? dedoCerrado : dedoAbierto;
        dedoIzquierdo.localPosition = new Vector3(-separacion, dedoIzquierdo.localPosition.y, 0f);
        dedoDerecho.localPosition = new Vector3(separacion, dedoDerecho.localPosition.y, 0f);
    }
}
