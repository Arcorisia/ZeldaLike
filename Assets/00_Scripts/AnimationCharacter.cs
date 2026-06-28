using System;
using UnityEngine;

public class AnimationCharacter : MonoBehaviour
{
   public Animator animator;
   private Rigidbody rb;
   public bool isAttacking = false;


    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        animator.SetBool("Attack", isAttacking);
        if(isAttacking)
        {
            animator.SetBool("Idle", false);
            animator.SetFloat("Speed", 0f);
            return; // Evitar actualizar la velocidad durante el ataque
        }

        Vector3 rbVel = rb.linearVelocity;
        float speed = Mathf.Abs(rbVel.x) + Mathf.Abs(rbVel.z);
        bool isIdle = speed < 0.01f;

        animator.SetBool("Idle", isIdle);
        animator.SetFloat("Speed", speed);
    }
}
