using UnityEngine;

public class AttributesManager : MonoBehaviour
{
    public int health;
    public bool invincibility;
    private Mago mago;
    private Rambo rambo;
    private Runner runner;
    private GameObject canvasObject;
    private AudioManager audioManager;

    private void Start() 
    {
        // Not an optimal way but whatever
        canvasObject = GameObject.Find("Canvas");
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
    }

    public void DealDamage(GameObject target, int damage)
    {   
        if (!invincibility)
        {
            mago = target.GetComponentInParent<Mago>();
            rambo = target.GetComponentInParent<Rambo>();
            runner = target.GetComponentInParent<Runner>();

            if(target.TryGetComponent<AttributesManager>(out var atm))

                // Before taking damage
                if (target.name == "EnemyObject")
                    BeforeTakingDamage();

                atm.TakeDamage(damage);

                if (target.name == "PlayerObject")
                {
                    var player = canvasObject.GetComponentInChildren<PlayerUI>();
                    player.Health_Update(health);
                    audioManager.PlaySFX(audioManager.playerHit);
                }

                if (health <= 0)
                    Death(target);
        }
        
    }

    public void AddHP(int hp)
    {
        health = hp;
        var player = canvasObject.GetComponentInChildren<PlayerUI>();
        player.Health_Update(health);
    }

    private void BeforeTakingDamage()
    {   
        // If attacked when not knowing player
        if (mago != null)
        {
            if (!mago.playerInSightRange && !mago.ambushed)
                mago.Ambushed();
        }
            
        else if (rambo != null)
        {
            if (!rambo.playerInSightRange && !rambo.ambushed)
                rambo.Ambushed();
        }
        else
        {
            if (!runner.playerInSightRange && !runner.ambushed)
                runner.Ambushed();
        }
    }

    private void Death(GameObject target)
    {
        if (target.name == "EnemyObject")
        {
            if (mago != null)
                mago.KillEnemy();

            else if (rambo != null)
                rambo.KillEnemy();
            else
                runner.KillEnemy();
        }
    }
}
