using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IPlayerState
{
    void enter();
    void exit();

    // If the command forces the player off the ground, make the check code be run
    bool FixedUpdate(Dictionary<PMovementStateHandler.ECommandType, Queue<ICommand>> commandCache);

    // Check if the state wants to make a switch
    bool check();

    // Get the next state to be switched to
    IPlayerState getNextState();
}