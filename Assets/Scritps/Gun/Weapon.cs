using System.Collections;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private Recoil recoilScript;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private GameObject backround;
    [SerializeField] private GameObject slide;
    [SerializeField] private GameObject magazine;
    [SerializeField] private GameObject smokePuff;
    [SerializeField] private GameObject hitParticle;

    [Header("Weapon Settings")]
    public int damage;
    [SerializeField] private float fireRate;
    [SerializeField] private float hitForce;
    public float reloadSpeed;
    public int magSize;
    public int allAmmo;
    public int ammo;
    public bool scoping = false;
    [SerializeField] private bool shootAnimation;
    [SerializeField] private Vector3 scopePos;

    [Header("Recoil")]
    public float recoilX;
    public float recoilY;
    public float recoilZ;
    public float aimRecoilX;
    public float aimRecoilY;
    public float aimRecoilZ;
    public float snapiness;
    public float returnSpeed;

    [Header("Kickbacks")]
    [SerializeField] private float kickbackForce;
    [SerializeField] private float resetSmooth;

    [Header("Animations")]
    [SerializeField] private Vector3 slideEndPosition;
    [SerializeField] private Vector3 magazineReloadPosition;
    [SerializeField] private float slideMoveTime;

    [Header("Audio")]
    [SerializeField] private AudioClip loopEndSound;
    [SerializeField] private AudioClip emptyMagSound;
    [SerializeField] private AudioClip[] shootSound;
    [SerializeField] private AudioClip[] reloadOutSound;
    [SerializeField] private AudioClip[] reloadInSound;
    [SerializeField] private AudioClip[] chokeSound;

    private float timeElapsed = 0;
    private float timeMagDown;
    private float rotationtime;
    private bool holdingButton;
    private bool reloading;
    private bool shooting;
    private bool switchedWeapon = false;
    private enum ReloadType
    {
        Rotate,
        MagazineDrop,
        PutDown
    }
    
    private ReloadType reloadState;
    private float moveUpDown;
    private float nextActionTime;
    private float timeDown;
    private float ammoToReload;
    private Transform _playerCamera;
    public TMP_Text _ammoText;
    private readonly int excludedLayerMask = ~(1 << 9);

    // Code is chaotic and it's also hella unoptimized, since all weapons are squished into one script
    // because i have no idea how to make classes that work simultaneously with weaponManager

    private void OnEnable()
    {
        recoilScript.heldWeapon = this;
        weaponManager.heldWeapon = this;

        reloading = false;
        if (magazine)
        {
            reloadState = ReloadType.MagazineDrop;
            return;
        }

        if (gameObject.name == "Glock" || gameObject.name == "Malorian")
        {
            reloadState = ReloadType.Rotate;
            return;
        }

        if (gameObject.name == "Pump Action Shotgun")
        {
            reloadState = ReloadType.PutDown;
            return;
        }
    }

    private void OnDisable()
    {    

        if (magazine)
        {
            magazine.transform.localPosition = Vector3.zero;
            return;
        }

        if (gameObject.name == "Glock")
        {
            transform.localRotation = Quaternion.identity;
            return;
        }

        if (gameObject.name == "Pump Action Shotgun")
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return;
        }
    }

    private void Update() 
    {
        // For whatever reason you can shoot when clicking while paused, weird.
        if (backround.activeInHierarchy) return;
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, scoping ? scopePos : Vector3.zero, resetSmooth * Time.deltaTime);

        MyInput();
    }

    private void MyInput()
    {   
        scoping = Input.GetMouseButton(1) && !reloading;

        if (Input.GetKeyUp(KeyCode.R) && !reloading && ammo < magSize && allAmmo != 0 && gameObject.activeSelf)
        {
            StartCoroutine(ReloadCooldown());
        }

        if (Input.GetMouseButtonDown(0))
            switchedWeapon = false;

        if (Input.GetMouseButton(0) && !shooting && !reloading && !switchedWeapon)
        {
            if (ammo > 0)
            {
                Shoot();
                holdingButton = true;
                ammo--;
                _ammoText.text = $"{ammo} / {allAmmo}";
                audioManager.PlaySFX(shootSound[Random.Range(0, shootSound.Length)]);

                if (shootAnimation)
                {
                    StartCoroutine(ShootAnimation());
                }
            }
            else
            {
                audioManager.PlaySFX(emptyMagSound);
            }

            StartCoroutine(ShootingCooldown());
        }
        else if (!reloading && holdingButton && loopEndSound != null && (Input.GetMouseButtonUp(0) || ammo < 1))
        {
            holdingButton = false;
            audioManager.PlaySFX(loopEndSound);
        }
    }

    private void Shoot()
    {
        transform.localPosition += new Vector3(kickbackForce, 0, 0);
        recoilScript.RecoilFire();
        muzzleFlash.Play();

        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward, out var hitInfo, 500, excludedLayerMask)) return;

        // If shooting box, move it
        if (hitInfo.transform.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity += _playerCamera.forward * hitForce;

        // If anything that is in whatIsGround layer, play dust particle
        if (hitInfo.transform.gameObject.layer == 6)
        {
            GameObject objectHit = Instantiate(smokePuff, hitInfo.point, Quaternion.identity);
            ParticleSystem objectHitSystem = objectHit.GetComponent<ParticleSystem>();
            Destroy(objectHit, objectHitSystem.main.duration);
        }

        // If player hit enemy, play blood hit particle
        if (hitInfo.transform.gameObject.layer == 10)
        {
            GameObject enemyHit = Instantiate(hitParticle, hitInfo.point, Quaternion.identity);
            ParticleSystem hitParticleSystem = enemyHit.GetComponent<ParticleSystem>();

            // Set particle count based on weapon damage (looks neat!)
            var emission = hitParticleSystem.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, damage));

            Destroy(enemyHit, hitParticleSystem.main.duration);
        }

        if (hitInfo.transform.TryGetComponent<Secret>(out var secret))
            secret.RunSecret();

        if (!hitInfo.transform.TryGetComponent<AttributesManager>(out var atm)) return;

        
        if (reloadState == ReloadType.PutDown)
        {
            int modifiedDamage = damage;

            if (hitInfo.distance >= 30)
                modifiedDamage = 1;
            else if (hitInfo.distance >= 8)
            {
                float distanceNormalised = (hitInfo.distance - 8) / 22;
                modifiedDamage = Mathf.RoundToInt(Mathf.Lerp(modifiedDamage, 1, distanceNormalised));
            }

            atm.DealDamage(atm.gameObject, modifiedDamage);
            audioManager.PlaySFX(audioManager.enemyHit);
            return;
        }

        atm.DealDamage(atm.gameObject, damage);
        audioManager.PlaySFX(audioManager.enemyHit);
    }

    private IEnumerator ShootAnimation()
    {
        if (reloadState == ReloadType.PutDown) audioManager.PlaySFX(chokeSound[Random.Range(0, chokeSound.Length)]);

        timeElapsed = 0;
        while (timeElapsed < slideMoveTime / 2)
        {
            float t = timeElapsed / (slideMoveTime / 2);
            slide.transform.localPosition = Vector3.Lerp(Vector3.zero, slideEndPosition, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        slide.transform.localPosition = slideEndPosition;
        timeElapsed = 0;
        if (ammo == 0) yield break;

        while (timeElapsed < slideMoveTime / 2)
        {
            float t = timeElapsed / (slideMoveTime / 2);
            slide.transform.localPosition = Vector3.Lerp(slideEndPosition, Vector3.zero, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        slide.transform.localPosition = Vector3.zero;
    }

    private IEnumerator ShootingCooldown()
    {
        shooting = true;
        yield return new WaitForSeconds(1f / fireRate);
        shooting = false;
    }

    private IEnumerator ReloadCooldown()
    {   
        reloading = true;
        _ammoText.text = "Reloading";
        rotationtime = 0f;
        timeElapsed = 0f;

        if (reloadOutSound.Length != 0) audioManager.PlaySFX(reloadOutSound[Random.Range(0, reloadOutSound.Length)]);
        
        if (reloadState == ReloadType.MagazineDrop)
            StartCoroutine(DropMagazine());

        else if (reloadState == ReloadType.PutDown)
        {   
            // How long putting down shotgun animation
            moveUpDown = reloadSpeed;
            ammoToReload = allAmmo < magSize - ammo ? allAmmo : magSize - ammo;
            timeDown = ammoToReload * reloadSpeed;

            float modifiedReloadSpeed = moveUpDown * 2 + timeDown;
            
            StartCoroutine(PutDown());

            yield return new WaitForSeconds(modifiedReloadSpeed);
        }
        else if (reloadState == ReloadType.Rotate)
            StartCoroutine(RotationReload());


        if (reloadState != ReloadType.PutDown) yield return new WaitForSeconds(reloadSpeed);

        // Set slide position to default
        if (shootAnimation) slide.transform.localPosition = Vector3.zero;

        if (allAmmo - (magSize - ammo) < 0)
        {
            ammo += allAmmo;
            allAmmo = 0;
        } 
        else {
            allAmmo = allAmmo - (magSize - ammo);
            ammo = magSize;
        }
            
        _ammoText.text = ammo + " / " + allAmmo;
        reloading = false;
        if (reloadState != ReloadType.PutDown)
            if (reloadInSound.Length != 0) audioManager.PlaySFX(reloadInSound[Random.Range(0, reloadInSound.Length)]);
        
    }

    private IEnumerator DropMagazine() 
    {
        timeElapsed = 0;
        timeMagDown = reloadSpeed / 4 * 2;
        while (timeElapsed < reloadSpeed / 4)
        {       
            float t = timeElapsed / (reloadSpeed / 4);
            magazine.transform.localPosition = Vector3.Lerp(Vector3.zero, magazineReloadPosition, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        magazine.transform.localPosition = magazineReloadPosition;
        yield return new WaitForSeconds(timeMagDown);
        timeElapsed = 0;
        while (timeElapsed < reloadSpeed / 4)
        {   
            float t = timeElapsed / (reloadSpeed / 4);
            magazine.transform.localPosition = Vector3.Lerp(magazineReloadPosition, Vector3.zero, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        magazine.transform.localPosition = Vector3.zero;
    }

    private IEnumerator PutDown() 
    {
        timeElapsed = 0f;

        while (timeElapsed < moveUpDown)
        {       
            float t = timeElapsed / moveUpDown;
            gameObject.transform.SetLocalPositionAndRotation(Vector3.Lerp(Vector3.zero, new Vector3(0, -0.2f, 0), t), Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 30), t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0f;

        while (timeElapsed < timeDown)
        {
            if (timeElapsed >= nextActionTime)
            {
                audioManager.PlaySFX(reloadInSound[Random.Range(0, reloadInSound.Length)]);
                nextActionTime += moveUpDown;
            }

            timeElapsed += Time.deltaTime;
            gameObject.transform.SetLocalPositionAndRotation(new Vector3(0, -0.2f, 0), Quaternion.Euler(0, 0, 30));
            yield return null;
        }

        timeElapsed = 0;
        
        while (timeElapsed < moveUpDown)
        {   
            float t = timeElapsed / moveUpDown;
            gameObject.transform.SetLocalPositionAndRotation(Vector3.Lerp(new Vector3(0, -0.2f, 0), Vector3.zero, t), Quaternion.Lerp(Quaternion.Euler(0, 0, 30), Quaternion.identity, t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        nextActionTime = 0f;
        // Return reload speed value to default
        reloadSpeed = moveUpDown;
    }

    private IEnumerator RotationReload() 
    {
        while (rotationtime < reloadSpeed)
        {       
            var spinDelta = -(Mathf.Cos(Mathf.PI * (rotationtime / reloadSpeed)) - 1f) / 2f;
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, spinDelta * 360f));
            rotationtime += Time.deltaTime;
            yield return null;
        }
    }

    public void Switch(Transform weaponHolder, Transform playerCamera, TMP_Text ammoText)
    {   
        transform.parent = weaponHolder;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _playerCamera = playerCamera;
        _ammoText = ammoText;
        _ammoText.text = ammo + " / " + allAmmo;
        
        scoping = false;
        reloading = false;
        shooting = false;
        switchedWeapon = true;

        nextActionTime = 0f;
    }
}
