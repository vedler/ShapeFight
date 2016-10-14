using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ICommand
{
    /*
        NOTE: Currently these commands dont serve much purpose, but we can attach a time to them to replay these commands back
            i.e. for a instant replay of your death, a full replay of a match etc.

        Remember: If we ever want to use any kind of non-deterministic behaviour and also support any kind of replays, 
            we need to save those randomness calculations with a timestamp/ordered list too.
            For example just a queue of all of the random numbers we have calculated throughout the game.

    */

    void execute(bool[] inputFlags);
    void execute(bool[] inputFlags, object data);
    ICommand clone();
}