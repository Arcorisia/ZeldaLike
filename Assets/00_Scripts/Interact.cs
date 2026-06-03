using UnityEngine;

public class Interact : MonoBehaviour
{
    public Collider interactCollider;

    public void PerformInteract()
    {
        Collider[] hits = Physics.OverlapBox(interactCollider.bounds.center, 
        interactCollider.bounds.extents, interactCollider.transform.rotation);
        foreach (Collider hit in hits)        {
            if (hit.gameObject != gameObject) // Evitar interactuar consigo mismo
            {
                Interactuable interactuable = hit.GetComponent<Interactuable>();
                if (interactuable != null)
                {
                    interactuable.Interact();
                    Debug.Log($"Has interactuado con {hit.gameObject.name}.");
                }
            }
        }        
    }
    
}
