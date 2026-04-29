using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 8f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float rotationSpeed = 15f;

    [Header("Jumping")]
    public float maxJumpHeight = 3.5f;
    public float minJumpHeight = 1f;
    public float timeToJumpApex = 0.4f;
    
    [Header("Long Jump (Dash)")]
    public float longJumpForwardForce = 15f;
    public float longJumpHeight = 2f;

    [Header("Physics")]
    public float gravityMultiplier = 1.5f; // Para caer más rápido que al subir

    private CharacterController _controller;
    private PlayerInputManager _input;
    private Transform _mainCamera;

    private Vector3 _velocity;
    private float _gravity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    
    private bool _isLongJumping;

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
        HandleMovement();
        HandleJumping();
        
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
            if (!_isLongJumping) 
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
        }
        else
        {
            // Movimiento en el aire (Air Control)
            if (targetDirection.magnitude > 0.1f && !_isLongJumping)
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

    private void HandleJumping()
    {
        if (_controller.isGrounded)
        {
            _isLongJumping = false;

            if (_input.JumpInputDown)
            {
                // Condición para Salto Largo: Agachado + Moviéndose con cierta velocidad
                Vector3 horizontalSpeed = new Vector3(_velocity.x, 0, _velocity.z);
                if (_input.CrouchInput && horizontalSpeed.magnitude > 2f)
                {
                    // Long Jump (Dash hacia adelante y arriba)
                    _velocity.y = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * longJumpHeight);
                    Vector3 jumpDirection = transform.forward; 
                    _velocity.x = jumpDirection.x * longJumpForwardForce;
                    _velocity.z = jumpDirection.z * longJumpForwardForce;
                    _isLongJumping = true;
                }
                else
                {
                    // Salto Normal
                    _velocity.y = _maxJumpVelocity;
                }
            }
        }
        else
        {
            // Variable jump height: Si soltamos el botón antes de llegar al punto más alto
            if (_input.JumpInputUp && _velocity.y > _minJumpVelocity && !_isLongJumping)
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
        float gravityMultiplierThisFrame = (_velocity.y < 0 && !_isLongJumping) ? gravityMultiplier : 1f;
        _velocity.y += _gravity * gravityMultiplierThisFrame * Time.deltaTime;
    }
}
