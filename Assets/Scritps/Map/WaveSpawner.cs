using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private GameObject magoObject;
    [SerializeField] private GameObject runnerObject;
    [SerializeField] private GameObject ramboObject;

    [Header("Projectiles")]
    [SerializeField] private GameObject magoProjectile;
    [SerializeField] private GameObject ramboProjectile;
    [SerializeField] private GameObject ramboLeadProjectile;

    [Header("Weapons")]
    [SerializeField] private WeaponSwitching weaponSwitching;
    [SerializeField] private GameObject tommyGun;
    [SerializeField] private GameObject ak47;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject mg3;
    [SerializeField] private GameObject weaponHolder;

    [Header("Other")]
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject weaponPopUp;
    [SerializeField] private GameObject secretDoor;
    [SerializeField] private TMP_Text waveInfo;
    [SerializeField] private Settings settings;
    
    private float waveCountdown;
    private NavMeshAgent agent;
    private Mago magoScript;
    private Runner runnerScript;
    private Rambo ramboScript;
    private bool receivedWeapon = true;
    private ProjectileScript projectile;
    private Transform[] spawnPosts;
    private bool waveMusic = false;

    private int lastIndexUsed = 0;
    
    [SerializeField] private enum SpawnState
    {
        Spawning,
        Waiting,
        Counting,
        PickBoost
    }

    [System.Serializable]
    public class Wave
    {
        public int magoCount;
        public int runnerCount;
        public int ramboCount;

        [Header("Mago Settings")]
        public int magoSpeed;
        public int magoDamage;
        public float magoAttackSpeed;
        public bool magoAbility;
        public int magoProjectileCount;
        public int magoProjectileDegree;

        [Header("Runner Settings")]
        public int runnerSpeed;
        public int runnerDamage;
        public float runnerAttackSpeed;
        public bool runnerAbility;
        public int runnerDashSpeedMultiplier;

        [Header("Rambo Settings")]
        public int ramboSpeed;
        public int ramboDamage;
        public float ramboAttackSpeed;
        public int ramboReloadTime;
        public int ramboMaxAmmo;
        public bool ramboAbility;

        [Header("Spawn Rate")]
        public float spawnRate;
    }

    public Wave[] waves;

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private SpawnState state = SpawnState.Counting;
    [SerializeField] private Dictionary<GameObject, int> enemyCounts = new();
    [SerializeField] private float timePlayed;
    [SerializeField] private int pickWave;
    public int nextWave = -1;
    

    private void Start()
    {
        waveCountdown = timeBetweenWaves;
        settings.SetWave(true);
        spawnPosts = GetComponentsInChildren<Transform>();
        pickWave = UnityEngine.Random.Range(5, 11);
        NextWave();
    }

    private void Update()
    {
        timePlayed += Time.deltaTime;

        if (state == SpawnState.Waiting && !EnemyIsAlive())
        {
            waveInfo.text = "Choose new upgrade";
            receivedWeapon = false;
            NextWave();

            if (nextWave == pickWave)
                secretDoor.SetActive(false);
            else
                secretDoor.SetActive(true);
        }

        if (waveCountdown <= 0)
        {
            StartCoroutine(SpawnWave(waves[nextWave]));
            waveCountdown = timeBetweenWaves;
        }
        else if (!EnemyIsAlive() && !abilities.activeSelf)
        {
            state = SpawnState.Counting;
            waveCountdown -= Time.deltaTime;
            waveInfo.text = $"{Mathf.RoundToInt(waveCountdown + 1)}";
        }

        if (!abilities.activeSelf && !waveMusic)
        {
            audioManager.PlayMusic(audioManager.arenaSoundtrack);
            waveMusic = true;
        }
        if (EnemyIsAlive())
            waveInfo.text = $"Wave {nextWave + 1}\nEnemies alive: {EnemyCount()}";
    }

    private bool EnemyIsAlive()
    {
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Enemy").Length; i++)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy")[i] != null)
            {
                return true;
            }
        }
        return false;
    }

    private int EnemyCount()
    {
        int count = 0;

        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Enemy").Length; i++)
            count = i + 1;

        return count;
    }

    private void NextWave()
    {
        state = SpawnState.PickBoost;
        abilities.SetActive(true);
        nextWave++;
        settings.SetWave(true);

        if (nextWave >= waves.Length)
        {
            TimeSpan time = TimeSpan.FromSeconds(timePlayed);

            string formattedTime = string.Format("{0:D2}:{1:D2}", (int)time.TotalMinutes, time.Seconds);

            playerUI.WinScreen(formattedTime);
            nextWave = 0;
        }

        if (!receivedWeapon)
        {
            if (nextWave == 3)
            {
                tommyGun.transform.parent = weaponHolder.transform;
                weaponSwitching.SetWeapons();
                weaponSwitching.Select(1);
                // Show player he got new weapon
                StartCoroutine(WeaponPopUp());
        
            } else if (nextWave == 6)
            {
                ak47.transform.parent = weaponHolder.transform;
                weaponSwitching.SetWeapons();
                weaponSwitching.Select(2);
                StartCoroutine(WeaponPopUp());
                
            } else if (nextWave == 9)
            {
                shotgun.transform.parent = weaponHolder.transform;
                weaponSwitching.SetWeapons();
                weaponSwitching.Select(3);
                StartCoroutine(WeaponPopUp());
                
            } else if (nextWave == 12)
            {
                mg3.transform.parent = weaponHolder.transform;
                weaponSwitching.SetWeapons();
                weaponSwitching.Select(4);
                StartCoroutine(WeaponPopUp());
            } 
            receivedWeapon = true;
        }
    }

    private IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.Spawning;

        // Initialize the enemy counts
        enemyCounts.Add(magoObject, _wave.magoCount);
        enemyCounts.Add(runnerObject, _wave.runnerCount);
        enemyCounts.Add(ramboObject, _wave.ramboCount);

        // Spawn enemies
        foreach (var kvp in enemyCounts)
        {
            GameObject enemyPrefab = kvp.Key;
            
            if (enemyPrefab.name == "Mago")
            {   
                // Set enemy script values
                magoScript = enemyPrefab.GetComponent<Mago>();
                magoScript.ability = _wave.magoAbility;
                magoScript.timeBetweenAttacks = _wave.magoAttackSpeed;
                magoScript.abilityProjectileCount = _wave.magoProjectileCount;
                magoScript.abilityProjectileDegree = _wave.magoProjectileDegree;

                // Set agent values
                agent = enemyPrefab.GetComponent<NavMeshAgent>();
                agent.speed = _wave.magoSpeed;

                // Set projectile damage
                projectile = magoProjectile.GetComponent<ProjectileScript>();
                projectile.damage = _wave.magoDamage;

            } else if (enemyPrefab.name == "Runner") {

                runnerScript = enemyPrefab.GetComponent<Runner>();
                runnerScript.damage = _wave.runnerDamage;
                runnerScript.timeBetweenAttacks = _wave.runnerAttackSpeed;
                runnerScript.ability = _wave.runnerAbility;
                runnerScript.dashSpeedMultiplier = _wave.runnerDashSpeedMultiplier;

                agent = enemyPrefab.GetComponent<NavMeshAgent>();
                agent.speed = _wave.runnerSpeed;
                agent.acceleration = _wave.runnerSpeed * 5;

            } else if (enemyPrefab.name == "Rambo") {

                ramboScript = enemyPrefab.GetComponent<Rambo>();
                ramboScript.timeBetweenAttacks = _wave.ramboAttackSpeed;
                ramboScript.reloadTime = _wave.ramboReloadTime;
                ramboScript.maxAmmo = _wave.ramboMaxAmmo;
                ramboScript.ability = _wave.ramboAbility;

                agent = enemyPrefab.GetComponent<NavMeshAgent>();
                agent.speed = _wave.ramboSpeed;
                agent.acceleration = _wave.ramboSpeed * 8;

                projectile = ramboProjectile.GetComponent<ProjectileScript>();
                projectile.damage = _wave.ramboDamage;

                projectile = ramboLeadProjectile.GetComponent<ProjectileScript>();
                projectile.damage = _wave.ramboDamage;
            }

            int count = kvp.Value;
            for (int i = 0; i < count; i++)
            {
                // Instantiate the enemy at the random position
                SpawnEnemy(enemyPrefab);
                yield return new WaitForSeconds(_wave.spawnRate);
            }
        }
        
        state = SpawnState.Waiting;
        enemyCounts.Clear();
        yield break;
    }

    private void SpawnEnemy(GameObject _enemy)
    {
        // Generate a random position for spawning enemies
        lastIndexUsed++;

        if (lastIndexUsed >= spawnPosts.Length)
            lastIndexUsed = 1;

       Instantiate(_enemy, spawnPosts[lastIndexUsed].transform.position, Quaternion.identity);
    }

    public IEnumerator WeaponPopUp()
    {
        weaponPopUp.SetActive(true);
        yield return new WaitForSeconds(5);
        weaponPopUp.SetActive(false);
    }
}