using UnityEngine;
using UnityEngine.AI;
public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}

public class Goblin : Enemy
{
    public NavMeshAgent navMeshAgent;

    public Transform player;
    public EnemyState currentState = EnemyState.Idle; 
        

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // Comportamiento de patrulla o espera
                break;
            case EnemyState.Chasing:
                ChasePlayer();
                break;
            case EnemyState.Attacking:
                // Comportamiento de ataque
                break;
        }
       
    }
    void ChasePlayer()
    {
        // Lógica para perseguir al jugador
        navMeshAgent.SetDestination(player.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = EnemyState.Chasing;
            Debug.Log("Goblin is chasing the player!");
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = EnemyState.Idle;
            Debug.Log("Goblin stopped chasing the player.");
        }
    }




}
