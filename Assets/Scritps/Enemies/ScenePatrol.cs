using Vector3 = UnityEngine.Vector3;
using UnityEngine;
using UnityEngine.AI;

public class ScenePatrol : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    
    [Header("Patrolling")]
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    private void FixedUpdate()
    {
        Patrolling();
    }
    
    private void Patrolling()
    {

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 randomPoint = transform.position + new Vector3(randomX, 0f, randomZ);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas))
        {
            // Choose the random point based on the agent's current position and walk point
            Vector3 agentToRandomPoint = hit.position - transform.position;
            randomPoint = hit.position + agentToRandomPoint.normalized * 2f;

            if (NavMesh.SamplePosition(randomPoint, out hit, walkPointRange, NavMesh.AllAreas))
            {
                walkPoint = hit.position;
                walkPointSet = true;
            }
        }
    }
}
