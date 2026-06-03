using UnityEngine;

public class Chest : MonoBehaviour, Interactuable
{
    public void Interact()
    {
        Debug.Log("Has abierto el cofre!");
        // Aquí puedes agregar lógica para dar al jugador un objeto, abrir una animación, etc.
    }
}
