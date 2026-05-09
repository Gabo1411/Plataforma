using UnityEngine;

/// <summary>
/// Moneda coleccionable con animación de rotación y flotación.
/// Al recogerla, notifica al GameManager.
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Velocidad de rotación en grados por segundo.")]
    public float rotationSpeed = 180f;
    [Tooltip("Amplitud del movimiento de flotación (bob).")]
    public float bobAmplitude = 0.3f;
    [Tooltip("Velocidad del movimiento de flotación.")]
    public float bobFrequency = 2f;

    [Header("Gizmos")]
    public Color gizmoColor = new Color(1f, 0.85f, 0f, 0.8f); // Dorado

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        // Rotación constante (como las monedas de Mario)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Movimiento de flotación (bob) suave
        float newY = _startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar que sea el jugador
        if (other.GetComponent<PlayerHealth>() != null || other.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectCoin();
            }

            Debug.Log("¡Moneda recogida!");

            // Destruir la moneda
            Destroy(gameObject);
        }
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
