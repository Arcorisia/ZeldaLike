using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    public TopDownCharacterController characterController;
    public Attack attack;
    public Interact interactuable;

    public bool canUseInput = true;
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canUseInput) return;
        // Input en Update para no perder frames
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        characterController.Move(new Vector2(h, v));
        // Usar KeyDown para asegurar una sola ejecución por pulsación de Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            attack.PerformAttack();
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            interactuable.PerformInteract();
        }
    }
}
