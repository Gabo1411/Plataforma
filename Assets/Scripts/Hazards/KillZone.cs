using UnityEngine;

/// <summary>
/// Zona de muerte instantánea (lava, vacío, etc.)
/// Colocar como un Trigger Collider grande debajo del nivel.
/// </summary>
public class KillZone : MonoBehaviour
{
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
}
