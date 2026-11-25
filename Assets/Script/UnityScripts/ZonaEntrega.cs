using UnityEngine;
using Vuforia;

// Va como hijo del Image Target
public class ZonaEntrega : MonoBehaviour
{
    [SerializeField] private ObserverBehaviour targetZona;
    [SerializeField] private Vector3 tamano = new Vector3(0.09f, 0.12f, 0.09f);

    public bool Disponible
    {
        get
        {
            Status estado = targetZona.TargetStatus.Status;
            return estado == Status.TRACKED || estado == Status.EXTENDED_TRACKED;
        }
    }

    public bool Contiene(Vector3 posicionMundo)
    {
        // Ignora la escala del Image Target para que el tamaño siempre esté en metros.
        Vector3 local = Quaternion.Inverse(transform.rotation) * (posicionMundo - transform.position);
        return Mathf.Abs(local.x) <= tamano.x / 2f
            && Mathf.Abs(local.y) <= tamano.y / 2f
            && Mathf.Abs(local.z) <= tamano.z / 2f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, tamano);
    }
}
