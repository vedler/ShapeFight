using UnityEngine;
using System.Collections;

public class WSelectionManager{

    private GameManager gameManager;

    private GameObject mainWeapon;
    private GameObject altWeapon;
    private GameObject[] weapons; // push prefabs
    private string mainWeaponName;
    private string altWeaponName;

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

    public string getMainWeaponName()
    {
        return mainWeaponName;
    }

    public string getAltWeaponName()
    {
        return altWeaponName;
    }

    // Use this for initialization
    public WSelectionManager(GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;
        weapons = Resources.LoadAll<GameObject>("Weapons");

        //One way of choosing which projectile prefab to choose
        for (int i = 0; i < weapons.Length; ++i)
        {
            if (weapons[i].name == "newPellet")
            {
                mainWeapon = GameObject.Instantiate(weapons[i]);
                mainWeaponName = weapons[i].name;
            } else if (weapons[i].name == "newGrenade")
            {
                altWeapon = GameObject.Instantiate(weapons[i]);
                altWeaponName = weapons[i].name;
            }
        }
    }



    void Start()
    {
    }

}
