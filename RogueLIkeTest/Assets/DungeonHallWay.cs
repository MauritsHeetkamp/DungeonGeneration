using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonHallWay : DungeonRoom
{
    public override IEnumerator SpawnNextRoom(bool yes = false)
    {
        return base.SpawnNextRoom(yes);
    }
}
