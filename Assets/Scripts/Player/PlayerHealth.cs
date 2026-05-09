using UnityEngine;
using System.Collections;

/// <summary>
/// Maneja la salud del jugador con dos tipos de daño:
/// - Pinchos: knockback + invencibilidad temporal + pierde vida.
/// - Lava/KillZone: teleporta al inicio + invencibilidad + pierde vida.
/// Al quedarse sin vidas, el GameManager reinicia el nivel.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Invencibilidad")]
    [Tooltip("Segundos de invencibilidad tras recibir daño.")]
    public float invincibilityDuration = 2f;
    [Tooltip("Velocidad del parpadeo durante la invencibilidad.")]
    public float blinkRate = 0.15f;

    private CharacterController _controller;
    private PlayerMovement _movement;
    private bool _isInvincible;
    private Vector3 _initialPosition;
    private Renderer _playerRenderer;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _movement = GetComponent<PlayerMovement>();
        _playerRenderer = GetComponentInChildren<Renderer>();

        // Guardar posición inicial para reset por lava
        _initialPosition = transform.position;
    }

    /// <summary>
    /// Llamado por SpikeTrap: knockback + invencibilidad + pierde vida.
    /// Si se queda sin vidas, el GameManager reinicia el nivel.
    /// </summary>
    public void TakeSpikeHit(Vector3 knockbackDirection, float knockbackForce, float knockbackUpForce)
    {
        if (_isInvincible) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        bool hasLivesLeft = GameManager.Instance.LoseLife();

        if (hasLivesLeft)
        {
            // Aplicar knockback a través de PlayerMovement
            if (_movement != null)
            {
                _movement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackUpForce);
            }

            // Iniciar invencibilidad con parpadeo visual
            StartCoroutine(InvincibilitySequence());
        }
        else
        {
            // Sin vidas: desactivar al jugador, GameManager maneja el game over
            if (_movement != null)
                _movement.CanMove = false;
        }
    }

    /// <summary>
    /// Llamado por KillZone (lava/vacío): teleporta al inicio + pierde vida.
    /// Si se queda sin vidas, el GameManager reinicia el nivel.
    /// </summary>
    public void TakeLavaHit()
    {
        if (_isInvincible) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        bool hasLivesLeft = GameManager.Instance.LoseLife();

        if (hasLivesLeft)
        {
            StartCoroutine(LavaResetSequence());
        }
        else
        {
            if (_movement != null)
                _movement.CanMove = false;
        }
    }

    private IEnumerator LavaResetSequence()
    {
        // Desactivar movimiento durante el reset
        if (_movement != null)
            _movement.CanMove = false;

        // Breve pausa antes de teletransportar
        yield return new WaitForSeconds(0.3f);

        // Teletransportar al inicio (CharacterController debe desactivarse para mover por transform)
        _controller.enabled = false;
        transform.position = _initialPosition;
        _controller.enabled = true;

        // Limpiar velocidad y reactivar movimiento
        if (_movement != null)
        {
            _movement.ResetVelocity();
            _movement.CanMove = true;
        }

        // Invencibilidad post-reset para no morir inmediatamente
        StartCoroutine(InvincibilitySequence());
    }

    private IEnumerator InvincibilitySequence()
    {
        _isInvincible = true;

        // Parpadeo visual durante la invencibilidad
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (_playerRenderer != null)
                _playerRenderer.enabled = !_playerRenderer.enabled;

            yield return new WaitForSeconds(blinkRate);
            elapsed += blinkRate;
        }

        // Asegurar que el renderer quede visible al terminar
        if (_playerRenderer != null)
            _playerRenderer.enabled = true;

        _isInvincible = false;
    }
}
