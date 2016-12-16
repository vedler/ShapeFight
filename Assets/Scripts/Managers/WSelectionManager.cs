using UnityEngine;
using System.Collections;

public class WSelectionManager {

    private GameManager gameManager;

    public string mainWeaponName;
    public string altWeaponName;

    public GameObject mainWeapon;
    public GameObject altWeapon;
    public GameObject[] weapons; // push prefabs

    //Should actually use prefab names not numbers, so it would be possible to find the correct
    //prefab from weapons[] using a for loop (see example in initialization method below
    void SwitchWeapons(int main, int alt)
    {
        mainWeapon = weapons[main];
        altWeapon = weapons[alt];
    }

    void SwitchMainWeapon(int main)
    {
        mainWeapon = weapons[main];
    }

    void SwitchAltWeapon(int alt)
    {
        altWeapon = weapons[alt];
    }

    public GameObject getMainWeapon()
    {
        return mainWeapon;
    }

    public GameObject getAltWeapon()
    {
        return altWeapon;
    }

    // Use this for initialization
    public WSelectionManager(GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;
        //weapons = Resources.LoadAll<GameObject>("Photon Unity Networking/Resources/Weapons");

        mainWeaponName = "Weapons/networkRocket";
        altWeaponName = "Weapons/networkBullet";

        //mainWeapon = PhotonNetwork.Instantiate("Weapons/networkRocket", new Vector3(0, 0, 0), new Quaternion(), 0);
        //altWeapon = PhotonNetwork.Instantiate("Weapons/networkBullet", new Vector3(0, 0, 0), new Quaternion(), 0);

        //One way of choosing which projectile prefab to choose
        /*for (int i = 0; i < weapons.Length; ++i)
        {
            if (weapons[i].name == "networkRocket")
            {
                //mainWeapon = GameObject.Instantiate(weapons[i]);
                mainWeapon = PhotonNetwork.Instantiate(weapons[i].name, new Vector3(0,0,0), new Quaternion(), 0);
            }
            else if (weapons[i].name == "networkBullet")
            {
                altWeapon = PhotonNetwork.Instantiate(weapons[i].name, new Vector3(0,0,0), new Quaternion(), 0);
            }
        }*/
    }



    void Start()
    {
    }

}
