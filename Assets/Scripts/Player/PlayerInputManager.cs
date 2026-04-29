using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public bool JumpInputDown { get; private set; }
    public bool JumpInputUp { get; private set; }
    public bool JumpInputHeld { get; private set; }
    public bool CrouchInput { get; private set; }

    void Update()
    {
        MovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        JumpInputDown = Input.GetButtonDown("Jump");
        JumpInputUp = Input.GetButtonUp("Jump");
        JumpInputHeld = Input.GetButton("Jump");
        
        // Asumimos que Agacharse es con Control, C o algún botón del mando
        CrouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C) || Input.GetButton("Fire3"); 
    }
}
