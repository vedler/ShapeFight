using UnityEngine;
using System.Collections;

public class WSelectionManager {

    private GameManager gameManager;

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
        weapons = Resources.LoadAll<GameObject>("Weapons");

        //One way of choosing which projectile prefab to choose
        for (int i = 0; i < weapons.Length; ++i)
        {
            if (weapons[i].name == "newRocket")
            {
                mainWeapon = GameObject.Instantiate(weapons[i]);
            } else if (weapons[i].name == "newBullet")
            {
                altWeapon = GameObject.Instantiate(weapons[i]);
            }
        }
    }



    void Start()
    {
    }

}
