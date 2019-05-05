using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonHallWay : DungeonRoom
{
    public override IEnumerator SpawnNextRoom()
    {
        print(" Spawned " + gameObject);
        creator.openProcesses++;
        yield return null;
        creator.openProcesses--;
        if (availableDoors.Count > 0)
        {
            for (int i = 0; i < availableDoors.Count; i++)
            {
                DungeonDoor.DoorDirection wantedDoorDirection = DungeonDoor.DoorDirection.Left;
                switch (availableDoors[i].GetComponent<DungeonDoor>().direction)
                {
                    case DungeonDoor.DoorDirection.Up:
                        wantedDoorDirection = DungeonDoor.DoorDirection.Down;
                        break;

                    case DungeonDoor.DoorDirection.Down:
                        wantedDoorDirection = DungeonDoor.DoorDirection.Up;
                        break;

                    case DungeonDoor.DoorDirection.Left:
                        wantedDoorDirection = DungeonDoor.DoorDirection.Right;
                        break;

                    case DungeonDoor.DoorDirection.Right:
                        wantedDoorDirection = DungeonDoor.DoorDirection.Left;
                        break;
                }

                creator.SpawnDungeonPartAlt(creator.rooms, wantedDoorDirection, gameObject, availableDoors[i].transform);
            }
        }
        else
        {
            if (creator.openProcesses <= 0)
            {
                creator.CheckRoomCount();
            }
        }
    }
}
