
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    private GameObject impactParticlePrefab;
    public int damage;
    public WhichProjectile projectile;
    public enum WhichProjectile
    {
        mago,
        rambo
    }

    private void Start() 
    {
        if (projectile == WhichProjectile.mago)
            impactParticlePrefab = GameObject.Find("MagoProjectileImpact");
        else if (projectile == WhichProjectile.rambo)
            impactParticlePrefab = GameObject.Find("RamboProjectileImpact");
    }

    private void OnTriggerEnter(Collider other)
    {   
        if(other.gameObject.name == "EnemyObject" || other.gameObject.CompareTag("Pickables") || other.gameObject.CompareTag("Projectile"))
            return;
        else if (other.gameObject.name == "PlayerObject" && other.TryGetComponent<AttributesManager>(out var atm))
            atm.DealDamage(atm.gameObject, damage);

        if (impactParticlePrefab != null)
        {
            GameObject impactParticleObj = Instantiate(impactParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem impactParticle = impactParticleObj.GetComponent<ParticleSystem>();
            Destroy(impactParticleObj, impactParticle.main.duration);
            Destroy(gameObject);
        }
    }
}