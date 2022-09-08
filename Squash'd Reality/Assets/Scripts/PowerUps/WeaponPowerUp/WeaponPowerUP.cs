using System;
using UnityEngine;


public class WeaponPowerUP : PowerUP {
    protected Type weaponType;
    protected override void Start()
    {
        base.Start();
        
    }

    public override void triggerEnter(Collider other)
    {
        UIManager uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        if (other.tag == "Player")
        {
            for (int i = 0; i < other.transform.childCount; i++)
            {
                if (other.transform.GetChild(i).tag.Equals("Weapon"))
                {
                    GameObject oldWeapon = other.transform.GetChild(i).gameObject;
                    Destroy(oldWeapon.GetComponent<Weapon>());
                    Weapon newWeapon = (Weapon) oldWeapon.AddComponent(weaponType);
                    oldWeapon.GetComponent<Shoot>().updateWeapon(newWeapon);
                    if (other.gameObject.GetComponent<PlayerMoveset>().hasAuthority)
                    {
                        uiManager.setWeaponImage(weaponType.ToString());
                        uiManager.setWeaponActive(true);
                    }
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}