using UnityEngine;

public class Attack : MonoBehaviour
{
    public float damage = 10f;
    public Collider damageCollider;

    public void PerformAttack()
    {
        Collider[]hits = Physics.OverlapBox(damageCollider.bounds.center, 
        damageCollider.bounds.extents, damageCollider.transform.rotation);
        foreach (Collider hit in hits)        {
            if (hit.gameObject != gameObject) // Evitar dañarse a sí mismo
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    Debug.Log($"{hit.gameObject.name} ha recibido {damage} de daño.");
                }
            }
        }        
    }
    

}
