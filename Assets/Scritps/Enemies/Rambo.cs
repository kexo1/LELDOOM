using Vector3 = UnityEngine.Vector3;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;

public class Rambo : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixer sfxMixer;
    [SerializeField] private AudioClip enemyAmbushSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip spottedSound;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;
    private Transform playerPos;
    private GameObject projectile;
    private GameObject projectileLead;
    private Camera playerCamera;
    private PlayerMovement player;
    private GameObject bloodExplosion;
    private GameObject ammoPack;
    private Rigidbody playerRb;

    [Header("Patrolling")]
    [SerializeField] private Vector3 walkPoint;
    [SerializeField] private bool walkPointSet;
    [SerializeField] private float walkPointRange;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    public float reloadTime;
    public int maxAmmo;
    public bool ability;
    private float ammo;
    private float leftRight = 1f;
    private bool switchDirection = true;
    private bool alreadyAttacked = false;

    [Header("States")]
    public bool playerInSightRange;
    public bool ambushed;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private bool playerIsVisible;
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange;
    private bool spotted = false;

    private float projectileRadius;
    private readonly int excludedLayerMask = ~(1 << 9);
    
    private void Start() 
    {
        GameObject playerObj = GameObject.Find("Player");
        playerRb = playerObj.GetComponent<Rigidbody>();
        player = playerObj.GetComponent<PlayerMovement>();

        playerPos = GameObject.Find("PlayerObject").GetComponent<Transform>();
        projectile = GameObject.Find("RamboProjectile");
        projectileLead = GameObject.Find("RamboLeadProjectile");
        playerCamera = GameObject.Find("Camera").GetComponent<Camera>();
        projectileRadius = projectile.GetComponent<SphereCollider>().radius;
        bloodExplosion = GameObject.Find("BloodExplosion");
        ammoPack = GameObject.Find("Ammo1");
        ammo = maxAmmo;
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
        var castTarget = playerPos.position + Vector3.down - transform.position;

        if (Physics.Raycast(castOrigin, castTarget, out RaycastHit hit, attackRange + 1, excludedLayerMask))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                playerIsVisible = true;

                Vector3 directionToEnemy = transform.position - playerCamera.transform.position;
                float angle = Vector3.Angle(playerCamera.transform.forward, directionToEnemy);

                if (angle <= playerCamera.fieldOfView * 0.75f && playerInSightRange)
                    ZigZag();
                else if (angle > playerCamera.fieldOfView * 0.75f)
                    agent.SetDestination(transform.position);
            }
            else 
                playerIsVisible = false;
        }

    }

    private void ZigZag()
    {
        if (!switchDirection)
        {
            Vector3 toPlayer = playerPos.position - transform.position;

            Vector3 zigzagDirection = Vector3.Cross(toPlayer, Vector3.up).normalized * leftRight;

            float distanceToPlayer = toPlayer.magnitude;
            float desiredShootDistance = attackRange * 0.75f;

            if (distanceToPlayer > desiredShootDistance && distanceToPlayer <= attackRange)
            {   
                // Choose closer distance so that player wouldn't escape
                float newDistance = Mathf.Clamp(distanceToPlayer - 1f, desiredShootDistance, attackRange);
                Vector3 closerDirection = toPlayer.normalized;
                agent.destination = closerDirection * newDistance + transform.position + zigzagDirection * 5f;
            }
            else
                agent.destination = transform.position + zigzagDirection * 5f;

        } else
        {
            leftRight *= -1;
            StartCoroutine(SwitchCooldown());
        }
    }

    private IEnumerator SwitchCooldown()
    {
        switchDirection = false;
        yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        switchDirection = true;
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
        transform.LookAt(new Vector3(playerPos.transform.position.x, 0.5f, playerPos.transform.position.z));

        if (!alreadyAttacked)
        {   
            GameObject bulletObj;
            Rigidbody bulletRig;
            Vector3 aimDirection;

            // Here it's calculated if player has been going in one direction too long, then predict player movement
            if (player.playerPrediction && ability)
            {
                bulletObj = Instantiate(projectileLead, transform.position + Vector3.up, Quaternion.identity);
                bulletRig = bulletObj.GetComponent<Rigidbody>();

                Vector3 predictedPos = playerPos.position + Vector3.down + playerRb.velocity * (Vector3.Distance(transform.position, playerPos.position) / 32f);
                aimDirection = (predictedPos - transform.position).normalized;
            }
            else
            {   
                bulletObj = Instantiate(projectile, transform.position + Vector3.up, Quaternion.identity);
                bulletRig = bulletObj.GetComponent<Rigidbody>();

                aimDirection = (playerPos.position + Vector3.down - transform.position).normalized;
            }
                
            bulletRig.AddForce(aimDirection * 32f, ForceMode.Impulse);

            Destroy(bulletObj, 2f);

            audioSource.PlayOneShot(shootSound);
            ammo -= 1;
            float delay;
            
            if (ammo == 0)
            {
                delay = reloadTime;
                ammo = maxAmmo;
            }
            else
                delay = timeBetweenAttacks;

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), delay);
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}