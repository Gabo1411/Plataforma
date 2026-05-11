using UnityEngine;

/// <summary>
/// Moneda coleccionable con animación de rotación y flotación.
/// Al recogerla, notifica al GameManager y genera partículas doradas.
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

    [Header("Partículas al recoger")]
    [Tooltip("Color principal de las partículas.")]
    public Color particleColor = new Color(1f, 0.85f, 0f, 1f); // Dorado
    [Tooltip("Cantidad de partículas al recoger.")]
    public int particleCount = 20;

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

            // Generar partículas doradas antes de destruir
            SpawnCollectParticles();

            Debug.Log("¡Moneda recogida!");

            // Destruir la moneda
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Crea un sistema de partículas doradas por código.
    /// Se auto-destruye después de terminar la animación.
    /// </summary>
    private void SpawnCollectParticles()
    {
        // Crear un GameObject temporal para las partículas
        GameObject particleObj = new GameObject("CoinParticles");
        particleObj.transform.position = transform.position;

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Detener para configurar antes de reproducir
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // === Módulo Main ===
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = particleColor;
        main.gravityModifier = 1f;
        main.maxParticles = particleCount;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // === Módulo Emission: todas las partículas de golpe (burst) ===
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        // === Módulo Shape: esfera para que salgan en todas las direcciones ===
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        // === Color over Lifetime: desvanecerse al final ===
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(new Color(1f, 1f, 0.5f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // === Size over Lifetime: se hacen más pequeñas ===
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // === Renderer: usar shader que siempre está disponible ===
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = particleColor;
        renderer.material = mat;

        // Reproducir y auto-destruir
        ps.Play();
        Destroy(particleObj, main.duration + main.startLifetime.constantMax + 0.1f);
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
