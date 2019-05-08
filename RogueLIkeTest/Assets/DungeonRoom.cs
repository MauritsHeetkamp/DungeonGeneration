using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DungeonRoom : MonoBehaviour
{
    public int roomDistanceFromStart;
    public GameObject parentRoom;
    public bool replaced;
    public bool previousReplaced;


    public DungeonCreator creator;
    public List<GameObject> availableDoors;
    public GameObject entranceDoor;
    public RoomTypes type;

    public GameObject[] possibleDoorLocations;

    public void Awake()
    {

    }
    public void Initialize(DungeonCreator owner, GameObject parentRoom_ = null, GameObject entrance = null)
    {
        creator = owner;
        if(parentRoom_ != null)
        {
            parentRoom = parentRoom_;
            roomDistanceFromStart = parentRoom.GetComponent<DungeonRoom>().roomDistanceFromStart;
            if(type != RoomTypes.Hallway)
            {
                roomDistanceFromStart++;
            }
        }
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
    public virtual IEnumerator SpawnNextRoom()
    {
        creator.roomCount++;
        if(availableDoors.Count > 0)
        {
            creator.openProcesses += availableDoors.Count;
        }
        else
        {
            creator.openProcesses++;
        }
        yield return null;
        int backupCount = availableDoors.Count;
        if(availableDoors.Count > 0)
        {
            for (int i = backupCount - 1; i >= 0; i--)
            {
                creator.openProcesses--;
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
                creator.SpawnDungeonPartAlt(creator.hallways, wantedDoorDirection, gameObject, availableDoors[i].transform);
            }
        }
        else
        {
            creator.openProcesses--;
            if(creator.openProcesses == 0)
            {
                creator.CheckRoomCount();
            }
        }
    }
    public bool HasCollision(bool notIncludeThis = false)
    {
        Vector3 colliderHalfExtends = transform.GetComponent<BoxCollider>().size;
        colliderHalfExtends.x *= transform.lossyScale.x;
        colliderHalfExtends.y *= transform.lossyScale.y;
        colliderHalfExtends.z *= transform.lossyScale.z;
        colliderHalfExtends /= 2;
        bool returnValue = Physics.CheckBox(transform.position, colliderHalfExtends, transform.rotation);
        if (returnValue)
        {
            if (notIncludeThis)
            {
                Collider[] hits = Physics.OverlapBox(transform.position, colliderHalfExtends, transform.rotation);
                if(hits.Length >= 2)
                {
                    foreach (Collider col in hits)
                    {
                        print(gameObject + " HAS COLLIDED WITH " + col.gameObject);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
    public enum RoomTypes { Normal, End, Hallway, Shop, Event}
}
