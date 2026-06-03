using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public TopDownCharacterController characterController;
    public Attack attack;
    public Interact interactuable;
    

    // Update is called once per frame
    void Update()
    {
        // Input en Update para no perder frames
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        characterController.Move(new Vector2(h, v));
        if(Input.GetButtonDown("Jump"))
        {
            attack.PerformAttack();
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            interactuable.PerformInteract();
        }
    }
}
