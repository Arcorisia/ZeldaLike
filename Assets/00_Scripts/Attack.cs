using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float damage = 10f;
    public Collider damageCollider;
    AnimationCharacter animationCharacter;
    void Start()
    {
        animationCharacter = GetComponent<AnimationCharacter>();
    }

    // Tiempo de seguridad en segundos si no se puede obtener la duración real del clip de animación.
    // Ajusta este valor solo si el nombre del clip no coincide o no se detecta correctamente.
    public float attackCooldown = 1.2f;
    public string attackAnimationName = "Attack";

    public void PerformAttack()
    {
        if (animationCharacter.isAttacking) return; // Evitar ataques consecutivos 
        animationCharacter.isAttacking = true;
        animationCharacter.animator.SetBool("Attack", animationCharacter.isAttacking);
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
        // Determinar la duración real de la animación si está disponible
        float animDuration = 0f;
        var ac = animationCharacter.animator;
        if (ac != null && ac.runtimeAnimatorController != null)
        {
            var clips = ac.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == attackAnimationName)
                {
                    animDuration = clip.length / Mathf.Max(0.0001f, ac.speed);
                    break;
                }
            }
        }

        float waitTime = animDuration > 0f ? animDuration : attackCooldown;
        StartCoroutine(WaitForAnimationFinish(waitTime));
    }
    IEnumerator WaitForAnimationFinish(float fallback)
    {
        Animator ac = animationCharacter.animator;
        if (ac == null)
        {
            yield return new WaitForSeconds(fallback);
            EndAttack();
            yield break;
        }

        float timeout = Mathf.Max(fallback, 1.5f);
        float timer = 0f;

        // Esperar a que el Animator entre al estado de ataque
        while (timer < timeout)
        {
            var state = ac.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(attackAnimationName))
                break;
            timer += Time.deltaTime;
            yield return null;
        }

        // Si no entró en el estado, usar fallback
        var current = ac.GetCurrentAnimatorStateInfo(0);
        if (!current.IsName(attackAnimationName))
        {
            yield return new WaitForSeconds(fallback);
            EndAttack();
            yield break;
        }

        // Esperar hasta que el estado termine (normalizedTime >= 1) o salga
        while (true)
        {
            var s = ac.GetCurrentAnimatorStateInfo(0);
            if (!s.IsName(attackAnimationName)) break;
            if (s.normalizedTime >= 1f && !ac.IsInTransition(0)) break;
            yield return null;
        }

        EndAttack();
        yield break;
    }
    public void EndAttack()
    {
        animationCharacter.isAttacking = false;
        animationCharacter.animator.SetBool("Attack", animationCharacter.isAttacking);
    }   
}
