using System;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public AttributesManager attributesManager;
    [SerializeField] private PlayerScript playerScript;
    private GameObject parent;
    private AudioManager audioManager;
    
    private void Start() 
    {
        parent = transform.parent.gameObject;
        attributesManager = GameObject.Find("PlayerObject").GetComponent<AttributesManager>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {   
            if (attributesManager.health != playerScript.maxHealth)
            {
                attributesManager.AddHP(Math.Min(attributesManager.health + 50, playerScript.maxHealth));
                audioManager.PlaySFX(audioManager.healthPickupSound);
                Destroy(parent);
            }
        }
    }
}
