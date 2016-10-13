using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static bool ready;

    public bool isReady()
    {
        return ready;
    }

    private static GameManager instance;

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            // TODO: Replace with custom exception extended from UnityException (so places that check for GameManager being ready can catch that for example)
            return null;
        }

        return instance;
    }

    private InputManager inputManager;

    public InputManager getInputManager()
    {
        if (inputManager == null || !inputManager.isReady())
        {
            throw new UnityException("GameManager's InputManager instance was accessed before Unity started it!");
        }

        return inputManager;
    }

    // Method to check if all managers are ready
    public bool areManagersReady()
    {
        return inputManager != null && inputManager.isReady();
    }

    // Use this for initialization
    void Start()
    {
        // TODO: Make this to not be destroyed on load and instead use a game manager loader as a component, that gets the instance instead (persist this when scene changes)

        // Set the singleton instance
        GameManager.instance = this;

        // Load other managers
        // PS: We don't make them as a game components, because we want them to be only accessible through the GameManager instance


        // Load and initialize InputManager
        this.inputManager = new InputManager(this);

        // TODO: Make sure all manager and game components are loaded (ready) before allowing any execution (through not letting users get the components)

    }

    // Update is called once per frame
    void Update()
    {
        if (!areManagersReady())
        {
            return;
        }

        inputManager.Update();
    }
    
}
