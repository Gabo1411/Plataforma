using UnityEngine;

/// <summary>
/// Zona de muerte instantánea (lava, vacío, etc.)
/// Colocar como un Trigger Collider grande debajo del nivel.
/// </summary>
public class KillZone : MonoBehaviour
{
    [Header("Gizmos")]
    [Tooltip("Color del gizmo en el editor para visualizar la zona de muerte.")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f); // Rojo semitransparente

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Debug.Log("KillZone: ¡El jugador cayó en la zona de muerte!");
            playerHealth.TakeLavaHit();
        }
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Detectar si tiene un BoxCollider para dibujar un cubo
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            // Dibujar el gizmo alineado con el collider
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
            return;
        }

        // Si no tiene BoxCollider, dibujar un cubo por defecto
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
