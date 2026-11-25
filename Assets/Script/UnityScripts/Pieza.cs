using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pieza : MonoBehaviour
{
    [SerializeField] private Color colorEntregada = Color.green;

    private Rigidbody cuerpo;
    private Collider colision;
    private Renderer visual;
    private Transform padreInicial;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Color colorInicial;
    private RigidbodyInterpolation interpolacionInicial;

    public bool Sujeta { get; private set; }
    public bool Entregada { get; private set; }
    public bool EnReposo => !Sujeta && (cuerpo.IsSleeping() || cuerpo.linearVelocity.sqrMagnitude < 0.0001f);

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        colision = GetComponent<Collider>();
        visual = GetComponent<Renderer>();

        padreInicial = transform.parent;
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;
        colorInicial = visual.material.color;
        interpolacionInicial = cuerpo.interpolation;
    }

    public void Sujetar(Transform pinza)
    {
        Sujeta = true;
        cuerpo.isKinematic = true;

        cuerpo.interpolation = RigidbodyInterpolation.None;

        colision.enabled = false;
        transform.SetParent(pinza, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Soltar(Transform contenedor)
    {
        Sujeta = false;
        transform.SetParent(contenedor, true);
        colision.enabled = true;
        cuerpo.isKinematic = false;
        cuerpo.interpolation = interpolacionInicial;
        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
    }

    public void MarcarEntregada()
    {
        Entregada = true;
        visual.material.color = colorEntregada;
    }

    public void Reiniciar()
    {
        Sujeta = false;
        Entregada = false;
        cuerpo.isKinematic = true;
        transform.SetParent(padreInicial, false);
        transform.localPosition = posicionInicial;
        transform.localRotation = rotacionInicial;
        colision.enabled = true;
        cuerpo.isKinematic = false;
        cuerpo.interpolation = interpolacionInicial;
        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
        visual.material.color = colorInicial;
    }
}
