using UnityEngine;

public class Recoil : MonoBehaviour
{   
    private Vector3 curentRotation;
    private Vector3 targetRotation;
    public Weapon heldWeapon;

    void Update()
    {   
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, heldWeapon.returnSpeed * Time.deltaTime);
        curentRotation = Vector3.Slerp(curentRotation, targetRotation, heldWeapon.snapiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(curentRotation);
    }

    public void RecoilFire() 
    {   
        if(heldWeapon.scoping) targetRotation += new Vector3(heldWeapon.aimRecoilX, Random.Range(-heldWeapon.aimRecoilY, heldWeapon.aimRecoilY), Random.Range(-heldWeapon.aimRecoilZ, heldWeapon.aimRecoilZ));
        else targetRotation += new Vector3(heldWeapon.recoilX, Random.Range(-heldWeapon.recoilY, heldWeapon.recoilY), Random.Range(-heldWeapon.recoilZ, heldWeapon.recoilZ));
    }

}
