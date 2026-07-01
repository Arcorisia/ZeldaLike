using UnityEngine;

public class ActivarSpriteAlEntrarZona : MonoBehaviour
{
    public GameObject spriteAActivar;
    public string tagQueActiva = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagQueActiva))
        {
            ActivarSprite();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(tagQueActiva))
        {
            ActivarSprite();
        }
    }

    private void ActivarSprite()
    {
        if (spriteAActivar != null)
        {
            spriteAActivar.SetActive(true);
        }
    }
}
