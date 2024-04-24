using Vector3 = UnityEngine.Vector3;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;

public class Runner : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioClip enemyAmbushSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip spottedSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    private Transform playerObject;
    private PlayerMovement player;
    private GameObject bloodExplosion;
    private Camera playerCamera;
    private AttributesManager playerHealth;
    private GameObject ammoPack;
    
    [Header("Patrolling")]
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    [Header("Attacking")]
    public int damage;
    public float timeBetweenAttacks;
    public bool ability;
    public float dashSpeedMultiplier;
    private bool alreadyAttacked;
    private float dashCooldown;
    private float leftRight = 1f;
    
    [Header("States")]
    public bool playerInSightRange;
    public bool ambushed;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    [SerializeField] private bool playerIsVisible;
    private bool spotted = false;

    private float agentSpeed;
    private bool coolDownSwitch = true;
    private readonly int excludedLayerMask = ~(1 << 9);

    private void Start() 
    {
        playerObject = GameObject.Find("PlayerObject").GetComponent<Transform>();
        playerHealth = playerObject.GetComponent<AttributesManager>();
        playerCamera = GameObject.Find("Camera").GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
        bloodExplosion = GameObject.Find("BloodExplosion");
        ammoPack = GameObject.Find("Ammo1");
        agentSpeed = agent.speed;
    }

    private void FixedUpdate()
    {
        dashCooldown += Time.deltaTime;

        GetRanges();
        StateManager();

        if (coolDownSwitch == true)
            StartCoroutine(DashCooldown());
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
        playerInAttackRange = Physics.CheckSphere(transform.position + Vector3.up, attackRange, whatIsPlayer);

        var castOrigin = transform.position + Vector3.up;
        var castTarget = playerObject.position + Vector3.down - transform.position;

        if (Physics.Raycast(castOrigin, castTarget, out RaycastHit hit, attackRange + 1, excludedLayerMask))
        { 
            if (hit.collider.gameObject.CompareTag("Player"))
                playerIsVisible = true;
            else 
                playerIsVisible = false;
        }

        Vector3 directionToEnemy = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToEnemy);

        if (ability && angle <= playerCamera.fieldOfView * 0.75f && playerInSightRange)
            Dash();
    }

    private void Dash()
    {

        if (dashCooldown <= 0.3f)
        {
            
            Vector3 toPlayer = playerObject.position - transform.position;
            
            float distanceToPlayer = toPlayer.magnitude;
            float desiredDistance = attackRange;

            if (distanceToPlayer > desiredDistance && distanceToPlayer <= sightRange)
                agent.speed = agentSpeed * dashSpeedMultiplier;
        }
        else
            agent.speed = agentSpeed;
    }

    private IEnumerator DashCooldown()
    {
        leftRight *= -1;
        agent.speed = agentSpeed;
        coolDownSwitch = false;

        yield return new WaitForSeconds(Random.Range(1.5f, 3f));

        dashCooldown = 0f;
        coolDownSwitch = true;
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
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            playerHealth.DealDamage(playerHealth.gameObject, damage);
            audioSource.PlayOneShot(shootSound);
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
        Gizmos.DrawWireSphere(transform.position + Vector3.up, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
