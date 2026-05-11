using UnityEngine;
using System.Collections;

/// <summary>
/// Plataforma que tiembla y cae cuando el jugador la pisa.
/// Después de un tiempo, vuelve a su posición original.
/// 
/// USO: Agregar este componente SOLO a las plataformas que quieres que se caigan.
/// Las plataformas sin este script se quedan estáticas normalmente.
/// 
/// DETECCIÓN: PlayerMovement llama a TriggerFall() cuando el CharacterController
/// pisa esta plataforma (vía OnControllerColliderHit).
/// </summary>
public class FallingPlatform : MonoBehaviour
{
    [Header("Temblor")]
    [Tooltip("Duración del temblor antes de caer (segundos).")]
    public float shakeDelay = 1f;
    [Tooltip("Intensidad del temblor.")]
    public float shakeIntensity = 0.1f;
    [Tooltip("Velocidad del temblor.")]
    public float shakeSpeed = 50f;

    [Header("Caída")]
    [Tooltip("Velocidad a la que cae la plataforma.")]
    public float fallSpeed = 12f;
    [Tooltip("Segundos que tarda en desaparecer tras empezar a caer.")]
    public float fallDuration = 1.5f;

    [Header("Reaparición")]
    [Tooltip("Segundos hasta que la plataforma vuelve a aparecer.")]
    public float respawnTime = 4f;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _initialShakeIntensity;
    private bool _isTriggered;
    private Collider _collider;
    private Renderer[] _renderers;

    void Awake()
    {
        _initialShakeIntensity = shakeIntensity;
    }

    void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _collider = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Llamado por PlayerMovement cuando el jugador pisa esta plataforma.
    /// Inicia la secuencia de temblor → caída → reaparición.
    /// </summary>
    public void TriggerFall()
    {
        if (_isTriggered) return;

        _isTriggered = true;
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        // === FASE 1: TEMBLOR ===
        float currentIntensity = _initialShakeIntensity;
        float shakeElapsed = 0f;

        while (shakeElapsed < shakeDelay)
        {
            // Temblor lateral usando seno para un movimiento natural
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * currentIntensity;
            float offsetZ = Mathf.Cos(Time.time * shakeSpeed * 0.8f) * currentIntensity * 0.5f;
            transform.position = _originalPosition + new Vector3(offsetX, 0f, offsetZ);

            shakeElapsed += Time.deltaTime;

            // Aumentar la intensidad progresivamente (más urgencia)
            currentIntensity = Mathf.Lerp(_initialShakeIntensity, _initialShakeIntensity * 3f, shakeElapsed / shakeDelay);

            yield return null;
        }

        // Volver a la posición exacta antes de caer
        transform.position = _originalPosition;

        // === FASE 2: CAÍDA ===
        // Desactivar el collider para que el jugador no se quede pegado
        if (_collider != null)
            _collider.enabled = false;

        float fallElapsed = 0f;
        Vector3 fallStartPos = _originalPosition;

        while (fallElapsed < fallDuration)
        {
            // Caer con aceleración (simular gravedad)
            float fallDistance = 0.5f * fallSpeed * fallElapsed * fallElapsed;
            transform.position = fallStartPos + Vector3.down * fallDistance;

            // Rotar ligeramente mientras cae para efecto visual
            transform.Rotate(Vector3.forward, 30f * Time.deltaTime, Space.World);

            fallElapsed += Time.deltaTime;
            yield return null;
        }

        // === FASE 3: DESAPARECER ===
        SetVisible(false);

        // === FASE 4: ESPERAR Y REAPARECER ===
        yield return new WaitForSeconds(respawnTime);

        // Restaurar posición, rotación y estado
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        if (_collider != null)
            _collider.enabled = true;

        SetVisible(true);
        _isTriggered = false;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in _renderers)
        {
            r.enabled = visible;
        }
    }
}
