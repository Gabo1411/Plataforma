using UnityEngine;

/// <summary>
/// Pinchos estáticos que causan daño al contacto.
/// Al tocar al jugador, le aplican knockback y pierde una vida.
/// Colocar este script directamente en el prefab de los pinchos
/// junto con un Collider configurado como Trigger.
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("Knockback")]
    [Tooltip("Fuerza horizontal del retroceso al tocar los pinchos.")]
    public float knockbackForce = 12f;
    [Tooltip("Fuerza vertical del retroceso (impulso hacia arriba).")]
    public float knockbackUpForce = 6f;

    [Header("Gizmos")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.6f);

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Dirección de knockback: del pincho hacia el jugador (solo horizontal)
            Vector3 knockbackDir = (other.transform.position - transform.position);
            knockbackDir.y = 0f;

            // Si el jugador está justo encima, empujar hacia donde mira
            if (knockbackDir.sqrMagnitude < 0.01f)
                knockbackDir = -other.transform.forward;

            knockbackDir.Normalize();

            Debug.Log("SpikeTrap: ¡El jugador tocó los pinchos!");
            playerHealth.TakeSpikeHit(knockbackDir, knockbackForce, knockbackUpForce);
        }
    }

    // ============================================================
    // GIZMOS - Visualización en el Editor
    // ============================================================

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
            return;
        }

        // Fallback: dibujar usando localScale
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
