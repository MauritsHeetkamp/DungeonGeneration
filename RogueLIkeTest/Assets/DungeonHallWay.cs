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

                GameObject roomToSpawn = null;
                switch (wantedDoorDirection)
                {
                    case DungeonDoor.DoorDirection.Up:
                        roomToSpawn = creator.upRooms[Random.Range(0, creator.upRooms.Count)];
                        break;

                    case DungeonDoor.DoorDirection.Down:
                        roomToSpawn = creator.downRooms[Random.Range(0, creator.downRooms.Count)];
                        break;

                    case DungeonDoor.DoorDirection.Left:
                        roomToSpawn = creator.leftRooms[Random.Range(0, creator.leftRooms.Count)];
                        break;

                    case DungeonDoor.DoorDirection.Right:
                        roomToSpawn = creator.rightRooms[Random.Range(0, creator.rightRooms.Count)];
                        break;

                }
                creator.SpawnDungeonPart(roomToSpawn, wantedDoorDirection, gameObject, availableDoors[i].transform);
            }
        }
        else
        {
            if (creator.openProcesses <= 0)
            {
                StartCoroutine(creator.CheckRoomCount());
            }
        }
    }
}
