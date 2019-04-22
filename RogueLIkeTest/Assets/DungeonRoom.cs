using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DungeonRoom : MonoBehaviour
{
    public GameObject parentDoor;
    public bool replaced;
    public bool previousReplaced;


    public DungeonCreator creator;
    public List<GameObject> availableDoors;
    public GameObject entranceDoor;
    public RoomTypes type;


    public void Awake()
    {
        print(" Spawned " + gameObject);
    }
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
    public IEnumerator SpawnNextRoom(bool yes = false)
    {
        creator.openProcesses++;
        yield return null;
        creator.openProcesses--;
        if(availableDoors.Count > 0)
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
                creator.SpawnRandomRoom(gameObject, availableDoors[i].transform, wantedDoorDirection, yes);
            }
        }
        else
        {
            if(creator.openProcesses <= 0)
            {
                creator.CheckRoomCount();
            }
        }
    }
    public bool HasCollision()
    {
        bool returnValue = Physics.CheckBox(transform.position, transform.GetComponent<BoxCollider>().size / 2, transform.rotation, creator.roomLayer);
        if (returnValue)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, transform.GetComponent<BoxCollider>().size / 2, transform.rotation, creator.roomLayer);
            creator.hits = hits;
            foreach(Collider col in hits)
            {
                print(gameObject + " HAS COLLIDED WITH " + col.gameObject);
            }
        }
        return returnValue;
    }
    public enum RoomTypes { Normal, End}
}
