using UnityEngine;

/// <summary>
/// Genera partículas de polvo al caminar/correr y de viento al hacer dash.
/// Se coloca en el mismo GameObject del jugador.
/// Los ParticleSystems se crean automáticamente por código.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerParticles : MonoBehaviour
{
    [Header("Polvo al moverse")]
    [Tooltip("Color del polvo.")]
    public Color dustColor = new Color(0.7f, 0.6f, 0.5f, 0.6f);
    [Tooltip("Velocidad mínima del jugador para emitir polvo.")]
    public float minSpeedForDust = 2f;
    [Tooltip("Partículas por segundo al moverse a máxima velocidad.")]
    public float dustEmissionRate = 15f;

    [Header("Viento del Dash")]
    [Tooltip("Color de las líneas de viento.")]
    public Color dashColor = new Color(0.8f, 0.9f, 1f, 0.7f);
    [Tooltip("Cantidad de partículas del dash.")]
    public int dashParticleCount = 25;

    private PlayerMovement _movement;
    private ParticleSystem _dustPS;
    private ParticleSystem _dashPS;
    private ParticleSystem.EmissionModule _dustEmission;

    void Start()
    {
        _movement = GetComponent<PlayerMovement>();
        CreateDustSystem();
        CreateDashSystem();
    }

    void Update()
    {
        HandleDust();
        HandleDash();
    }

    // ============================================================
    // POLVO AL CAMINAR
    // ============================================================

    private void HandleDust()
    {
        if (_dustPS == null) return;

        // Emitir polvo solo si está en el suelo y moviéndose
        bool shouldEmit = _movement.IsGrounded &&
                          _movement.HorizontalSpeed > minSpeedForDust &&
                          _movement.CanMove;

        // Ajustar emisión proporcionalmente a la velocidad
        if (shouldEmit)
        {
            float speedRatio = _movement.HorizontalSpeed / _movement.maxSpeed;
            _dustEmission.rateOverTime = dustEmissionRate * speedRatio;

            if (!_dustPS.isPlaying)
                _dustPS.Play();
        }
        else
        {
            _dustEmission.rateOverTime = 0f;
        }
    }

    private bool _wasDashing;

    private void HandleDash()
    {
        if (_dashPS == null) return;

        // Disparar burst de partículas al INICIAR el dash
        if (_movement.IsDashing && !_wasDashing)
        {
            // Posicionar detrás del jugador
            _dashPS.transform.position = transform.position;
            _dashPS.transform.rotation = Quaternion.LookRotation(-transform.forward);
            _dashPS.Play();
        }

        _wasDashing = _movement.IsDashing;
    }

    // ============================================================
    // CREACIÓN DE PARTICLE SYSTEMS POR CÓDIGO
    // ============================================================

    private void CreateDustSystem()
    {
        GameObject dustObj = new GameObject("DustParticles");
        dustObj.transform.SetParent(transform);
        // Posicionar en los pies del jugador
        dustObj.transform.localPosition = new Vector3(0f, 0f, 0f);
        dustObj.transform.localRotation = Quaternion.identity;

        _dustPS = dustObj.AddComponent<ParticleSystem>();
        _dustPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main
        var main = _dustPS.main;
        main.duration = 1f;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = dustColor;
        main.gravityModifier = -0.2f; // Sube ligeramente
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 30;

        // Emission
        var emission = _dustPS.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // Controlado dinámicamente
        _dustEmission = emission;

        // Shape: semicírculo en el suelo
        var shape = _dustPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.3f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        // Color over lifetime: desvanecerse
        var colorOverLifetime = _dustPS.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient dustGradient = new Gradient();
        dustGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(dustColor, 0f),
                new GradientColorKey(dustColor, 0.5f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(dustColor.a, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = dustGradient;

        // Size over lifetime: se agrandan un poco y desaparecen
        var sizeOverLifetime = _dustPS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.3f, 1f),
                new Keyframe(1f, 0.3f)
            )
        );

        // Renderer
        var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = dustColor;
        renderer.material = mat;
    }

    private void CreateDashSystem()
    {
        GameObject dashObj = new GameObject("DashParticles");
        dashObj.transform.SetParent(transform);
        dashObj.transform.localPosition = Vector3.zero;
        dashObj.transform.localRotation = Quaternion.identity;

        _dashPS = dashObj.AddComponent<ParticleSystem>();
        _dashPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main
        var main = _dashPS.main;
        main.duration = 0.3f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = dashColor;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = dashParticleCount;

        // Emission: burst al inicio
        var emission = _dashPS.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)dashParticleCount)
        });

        // Shape: cono estrecho (líneas de velocidad)
        var shape = _dashPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 8f;
        shape.radius = 0.15f;

        // Stretch las partículas para que parezcan líneas de viento
        var renderer = dashObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 3f;
        renderer.velocityScale = 0.1f;
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = dashColor;
        renderer.material = mat;

        // Color over lifetime: desvanecerse rápido
        var colorOverLifetime = _dashPS.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient dashGradient = new Gradient();
        dashGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(dashColor, 0.3f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = dashGradient;

        // Size over lifetime
        var sizeOverLifetime = _dashPS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
    }
}
