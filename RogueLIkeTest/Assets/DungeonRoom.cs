﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DungeonRoom : MonoBehaviour
{
    public DungeonCreator creator;
    public List<GameObject> availableDoors;
    public GameObject entranceDoor;
    public RoomTypes type;

    public void Initialize(DungeonCreator owner, GameObject entrance = null)
    {
        creator = owner;
        if (entrance)
        {
            entranceDoor = entrance;
            availableDoors.Remove(entrance);
        }
        if (type == RoomTypes.End)
        {
            creator.endRooms.Add(gameObject);
        }
    }
    public IEnumerator SpawnNextRoom()
    {
        if(availableDoors.Count > 0)
        {
            print("ADDED PROCESS");
            creator.openProcesses++;
            yield return null;
            creator.openProcesses--;
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
                print("REMOVED PROCESS");
                creator.SpawnRandomRoom(gameObject, availableDoors[i].transform, wantedDoorDirection);
            }
        }
    }
    public bool HasCollision()
    {
        return Physics.CheckBox(transform.position, transform.GetComponent<BoxCollider>().size / 2, transform.rotation, creator.roomLayer);
    }
    public enum RoomTypes { Normal, End}
}