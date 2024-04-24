using Vector3 = UnityEngine.Vector3;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class Mago : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioClip enemyAmbushSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip spottedSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    private Transform playerObject;
    private PlayerMovement player;
    private GameObject projectile;
    private GameObject bloodExplosion;
    private GameObject ammoPack;
    
    [Header("Attacking")]
    public float timeBetweenAttacks;
    public float launchSpeed;
    public bool ability;
    public float abilityProjectileCount;
    public float abilityProjectileDegree;
    private bool alreadyAttacked = false;

    [Header("Ranges")]
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;

    [Header("Patrolling")]
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    [Header("States")]
    public bool playerInSightRange;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private bool playerIsVisible;
    public bool ambushed;
    private bool spotted = false;

    private float projectileRadius;
    private readonly int excludedLayerMask = ~(1 << 9);

    private void Start() 
    {
        // Not an optimal way of loading variables
        playerObject = GameObject.Find("PlayerObject").GetComponent<Transform>();
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();

        projectile = GameObject.Find("MagoProjectile");
        projectileRadius = projectile.GetComponent<SphereCollider>().radius;
        bloodExplosion = GameObject.Find("BloodExplosion");
        ammoPack = GameObject.Find("Ammo1");
    }

    private void FixedUpdate()
    {
        GetRanges();
        StateManager();
    }

    private void StateManager()
    {
        if (!ambushed)
        {
            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if ((playerInSightRange && !playerInAttackRange) || (playerInSightRange && playerInAttackRange && !playerIsVisible)) ChasePlayer();
            if (playerInSightRange && playerInAttackRange && playerIsVisible) AttackPlayer();

        } else {

            ChasePlayer();
            if (playerInAttackRange)
            {
                walkPointSet = false;
                ambushed = false;
            }
        }
    }

    private void GetRanges()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        var castOrigin = transform.position + Vector3.up;
        var castTarget = playerObject.position + Vector3.down - transform.position;

        if (Physics.Raycast(castOrigin, castTarget, out RaycastHit hit, attackRange + 1, excludedLayerMask))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
                playerIsVisible = true;
            else 
                playerIsVisible = false;
        }
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

    private void ChasePlayer()
    {
        agent.SetDestination(player.groundPosition);
        walkPointSet = false;
        agent.isStopped = false;

        if (!spotted)
        {
            if(Random.Range(0, 3) == 1)
            {
                audioSource.PlayOneShot(spottedSound);
            }
            spotted = true;
        }
    }

    public void Ambushed()
    {
        ambushed = true;
        audioSource.PlayOneShot(enemyAmbushSound);
    }

    private void AttackPlayer()
    {   
        agent.isStopped = true;
        transform.LookAt(new Vector3(playerObject.transform.position.x, 0.5f, playerObject.transform.position.z));

        if (!alreadyAttacked)
        {   
            if (ability)
            {
                for (int i = 0; i < abilityProjectileCount; i++)
                {
                    GameObject bulletObj = Instantiate(projectile, transform.position + Vector3.up, Quaternion.identity);
                    Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
                    Vector3 directionToPlayer = (playerObject.transform.position + Vector3.down - transform.position).normalized;

                    float angleOffset = (i - 1) * abilityProjectileDegree; // -45, 0, +45 degrees
                    Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.up);
                    Vector3 rotatedDirection = rotation * directionToPlayer;

                    bulletRig.AddForce(rotatedDirection * launchSpeed, ForceMode.Impulse);
                    Destroy(bulletObj, 2f);
                }
            }
            else
            {   
                GameObject bulletObj = Instantiate(projectile, transform.position + Vector3.up, Quaternion.identity);
                Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
                Vector3 directionToPlayer = (playerObject.transform.position + Vector3.down - transform.position).normalized;
                bulletRig.AddForce(directionToPlayer * launchSpeed, ForceMode.Impulse);
                Destroy(bulletObj, 2f);
            }
                
            alreadyAttacked = true;
            audioSource.PlayOneShot(shootSound);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
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

    public void KillEnemy()
    {
        // Unnesessary creating another temp sound object, should reuse one (i'm lazy)
        GameObject bloodExplosionParticle = Instantiate(bloodExplosion, transform.position, Quaternion.identity);

        GameObject soundObject = new("DeathSoundObject");
        AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();
        tempAudioSource.outputAudioMixerGroup = sfxMixer.FindMatchingGroups("SFX")[0];
        tempAudioSource.PlayOneShot(deathSound);

        if (Random.Range(0, 3) == 1)
        {
            var origin = transform.position + Vector3.up;
            Instantiate(ammoPack, origin, Quaternion.identity);
        }

        Destroy(bloodExplosionParticle, 1f);
        Destroy(soundObject, deathSound.length);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
