using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Controla si el jugador puede moverse. Usado por PlayerHealth durante respawn/muerte.
    /// </summary>
    public bool CanMove { get; set; } = true;

    // Propiedades de solo lectura para otros sistemas (partículas, audio, etc.)
    public bool IsGrounded => _controller != null && _controller.isGrounded;
    public bool IsDashing => _isDashing;
    public float HorizontalSpeed => _controller != null ?
        new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude : 0f;

    [Header("Movement")]
    public float maxSpeed = 8f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float rotationSpeed = 15f;

    [Header("Jumping")]
    public float maxJumpHeight = 3.5f;
    public float minJumpHeight = 1f;
    public float timeToJumpApex = 0.4f;
    
    [Header("Dash")]
    [Tooltip("Velocidad del dash.")]
    public float dashSpeed = 20f;
    [Tooltip("Duración del dash en segundos.")]
    public float dashDuration = 0.25f;
    [Tooltip("Tiempo de espera entre dashes en segundos.")]
    public float dashCooldown = 1f;

    [Header("Knockback")]
    [Tooltip("Duración del estado de knockback donde el jugador no puede moverse.")]
    public float knockbackDuration = 0.3f;

    [Header("Physics")]
    public float gravityMultiplier = 1.5f; // Para caer más rápido que al subir

    private CharacterController _controller;
    private PlayerInputManager _input;
    private Transform _mainCamera;

    private Vector3 _velocity;
    private float _gravity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    
    // Dash state
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector3 _dashDirection;

    // Knockback state
    private bool _isKnockedBack;
    private float _knockbackTimer;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputManager>();
        
        if (Camera.main != null)
            _mainCamera = Camera.main.transform;
        else
            Debug.LogWarning("PlayerMovement: No se encontró la cámara principal (tag 'MainCamera').");

        // Fórmulas físicas para saltos consistentes basadas en la altura deseada y tiempo al vértice
        _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
    }

    void Update()
    {
        HandleGravity();

        if (_isKnockedBack)
        {
            // Durante el knockback, no se acepta input pero la gravedad sigue actuando
            _knockbackTimer -= Time.deltaTime;
            if (_knockbackTimer <= 0f)
                _isKnockedBack = false;
        }
        else if (CanMove)
        {
            HandleDash();

            if (!_isDashing)
            {
                HandleMovement();
                HandleJumping();
            }
        }
        else
        {
            // Si no puede moverse, frenar la velocidad horizontal
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
        
        // Mover el personaje utilizando la velocidad acumulada
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        Vector2 input = _input.MovementInput;

        // Calcular dirección relativa a la cámara
        Vector3 targetDirection = Vector3.zero;
        if (_mainCamera != null)
        {
            Vector3 camForward = _mainCamera.forward;
            Vector3 camRight = _mainCamera.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            targetDirection = camForward * input.y + camRight * input.x;
        }

        if (_controller.isGrounded)
        {
            if (targetDirection.magnitude > 0.1f)
            {
                // Acelerar
                Vector3 currentHorizontalVel = new Vector3(_velocity.x, 0, _velocity.z);
                Vector3 newHorizontalVel = Vector3.Lerp(currentHorizontalVel, targetDirection * maxSpeed, Time.deltaTime * acceleration);
                _velocity.x = newHorizontalVel.x;
                _velocity.z = newHorizontalVel.z;

                // Rotar hacia la dirección de movimiento
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // Desacelerar (fricción)
                Vector3 currentHorizontalVel = new Vector3(_velocity.x, 0, _velocity.z);
                Vector3 newHorizontalVel = Vector3.Lerp(currentHorizontalVel, Vector3.zero, Time.deltaTime * deceleration);
                _velocity.x = newHorizontalVel.x;
                _velocity.z = newHorizontalVel.z;
            }
        }
        else
        {
            // Movimiento en el aire (Air Control)
            if (targetDirection.magnitude > 0.1f)
            {
                Vector3 currentHorizontalVel = new Vector3(_velocity.x, 0, _velocity.z);
                // Control en el aire reducido (30% de aceleración normal)
                Vector3 newHorizontalVel = Vector3.Lerp(currentHorizontalVel, targetDirection * maxSpeed, Time.deltaTime * (acceleration * 0.3f));
                _velocity.x = newHorizontalVel.x;
                _velocity.z = newHorizontalVel.z;

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (rotationSpeed * 0.5f));
            }
        }
    }

    private void HandleDash()
    {
        // Cooldown del dash
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        // Iniciar dash con el botón de agacharse (Control/C)
        if (_input.CrouchInput && !_isDashing && _dashCooldownTimer <= 0f)
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            // Dirección del dash: hacia donde mira el jugador
            _dashDirection = transform.forward;

            Debug.Log("¡Dash!");
        }

        // Ejecutar dash
        if (_isDashing)
        {
            _velocity.x = _dashDirection.x * dashSpeed;
            _velocity.z = _dashDirection.z * dashSpeed;
            // Mantener la Y sin cambios para que la gravedad siga actuando

            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }
        }
    }

    private void HandleJumping()
    {
        if (_controller.isGrounded)
        {
            if (_input.JumpInputDown)
            {
                // Salto Normal
                _velocity.y = _maxJumpVelocity;
            }
        }
        else
        {
            // Variable jump height: Si soltamos el botón antes de llegar al punto más alto
            if (_input.JumpInputUp && _velocity.y > _minJumpVelocity)
            {
                _velocity.y = _minJumpVelocity;
            }
        }
    }

    private void HandleGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Pequeña fuerza constante para pegarse al suelo en bajadas
        }

        // Multiplicador de gravedad al caer para un game feel menos "flotante"
        float gravityMultiplierThisFrame = (_velocity.y < 0) ? gravityMultiplier : 1f;
        _velocity.y += _gravity * gravityMultiplierThisFrame * Time.deltaTime;
    }

    // ============================================================
    // KNOCKBACK - Llamado por PlayerHealth
    // ============================================================

    /// <summary>
    /// Aplica un impulso de knockback. El jugador pierde control temporalmente.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force, float upForce)
    {
        _velocity = direction * force;
        _velocity.y = upForce;
        _isKnockedBack = true;
        _knockbackTimer = knockbackDuration;
        _isDashing = false; // Cancelar dash si estaba en uno
    }

    /// <summary>
    /// Resetea toda la velocidad a cero. Usado al teletransportar al jugador.
    /// </summary>
    public void ResetVelocity()
    {
        _velocity = Vector3.zero;
        _isKnockedBack = false;
        _knockbackTimer = 0f;
        _isDashing = false;
    }

    // ============================================================
    // DETECCIÓN DE PLATAFORMAS - CharacterController no genera
    // OnCollisionEnter en otros objetos, así que se detecta desde aquí.
    // ============================================================

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Si pisamos una plataforma que cae (estamos encima, no chocando de lado)
        if (hit.normal.y > 0.5f)
        {
            FallingPlatform platform = hit.collider.GetComponent<FallingPlatform>();
            if (platform != null)
            {
                platform.TriggerFall();
            }
        }
    }
}
