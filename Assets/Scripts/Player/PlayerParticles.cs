using UnityEngine;

/// <summary>
/// Genera partículas de polvo al caminar/correr y una estela (trail)
/// tipo Blink de Tracer al hacer dash.
/// Se coloca en el mismo GameObject del jugador.
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

    [Header("Estela del Dash (estilo Tracer)")]
    [Tooltip("Color de la estela al inicio.")]
    public Color trailStartColor = new Color(0.4f, 0.7f, 1f, 0.9f);
    [Tooltip("Color de la estela al final.")]
    public Color trailEndColor = new Color(0.6f, 0.85f, 1f, 0f);
    [Tooltip("Ancho de la estela.")]
    public float trailWidth = 0.5f;
    [Tooltip("Tiempo en segundos que tarda la estela en desvanecerse.")]
    public float trailFadeTime = 0.3f;

    private PlayerMovement _movement;
    private ParticleSystem _dustPS;
    private ParticleSystem.EmissionModule _dustEmission;
    private TrailRenderer _dashTrail;
    private bool _wasDashing;

    void Start()
    {
        _movement = GetComponent<PlayerMovement>();
        CreateDustSystem();
        CreateDashTrail();
    }

    void Update()
    {
        HandleDust();
        HandleDashTrail();
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

    // ============================================================
    // ESTELA DEL DASH (TRAIL RENDERER)
    // ============================================================

    private void HandleDashTrail()
    {
        if (_dashTrail == null) return;

        if (_movement.IsDashing)
        {
            // Activar la estela mientras dura el dash
            _dashTrail.emitting = true;
        }
        else if (_wasDashing && !_movement.IsDashing)
        {
            // Al terminar el dash, dejar de emitir (la estela existente se desvanece sola)
            _dashTrail.emitting = false;
        }

        _wasDashing = _movement.IsDashing;
    }

    // ============================================================
    // CREACIÓN POR CÓDIGO
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

    private void CreateDashTrail()
    {
        // Crear hijo para el TrailRenderer (posicionado en el centro del jugador)
        GameObject trailObj = new GameObject("DashTrail");
        trailObj.transform.SetParent(transform);
        trailObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        _dashTrail = trailObj.AddComponent<TrailRenderer>();

        // Duración: cuánto tarda en desaparecer la estela
        _dashTrail.time = trailFadeTime;

        // Ancho: empieza ancho y se adelgaza (como el blink de Tracer)
        _dashTrail.widthMultiplier = trailWidth;
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(0.5f, 0.6f);
        widthCurve.AddKey(1f, 0f);
        _dashTrail.widthCurve = widthCurve;

        // Color: gradiente de color intenso → transparente
        Gradient trailGradient = new Gradient();
        trailGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(trailStartColor, 0f),
                new GradientColorKey(trailEndColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(trailStartColor.a, 0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        _dashTrail.colorGradient = trailGradient;

        // Material
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = Color.white; // El color viene del gradient
        _dashTrail.material = trailMat;

        // Suavizado
        _dashTrail.minVertexDistance = 0.05f;
        _dashTrail.numCornerVertices = 4;
        _dashTrail.numCapVertices = 4;

        // Empieza desactivado
        _dashTrail.emitting = false;
    }
}
