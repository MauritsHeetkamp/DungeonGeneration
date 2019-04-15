using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DungeonRoom : MonoBehaviour
{
    public DungeonCreator creator;
    public List<GameObject> availableDoors;
    public List<GameObject> allDoors;
    public RoomTypes type;

    public void Initialize(DungeonCreator owner)
    {
        creator = owner;
        if (type == RoomTypes.End)
        {
            creator.endRooms.Add(gameObject);
        }
    }
    public void SpawnNextRoom()
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
            creator.SpawnBasicRoom(gameObject, availableDoors[i].transform, wantedDoorDirection);
        }
    }

    public enum RoomTypes { Normal, End}
}
