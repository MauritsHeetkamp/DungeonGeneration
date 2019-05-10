using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRoom : MonoBehaviour
{
    public int roomDistanceFromStart;
    public GameObject parentRoom;
    public bool replaced;
    public bool previousReplaced;


    public DungeonCreator creator;
    public List<GameObject> availableDoors;
    public GameObject entranceDoor;
    public RoomTypes type;

    public virtual void Initialize(DungeonCreator owner, GameObject parentRoom_ = null, GameObject entrance = null)
    {
        creator = owner;
        if (parentRoom_ != null)
        {
            parentRoom = parentRoom_;
            roomDistanceFromStart = parentRoom.GetComponent<BaseRoom>().roomDistanceFromStart;
            if (type != RoomTypes.Hallway)
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
        creator.entireDungeon.Add(gameObject);
    }

    public virtual IEnumerator SpawnNextRoom()
    {
        if (availableDoors.Count > 0)
        {
            creator.openProcesses += availableDoors.Count;
        }
        else
        {
            creator.openProcesses++;
        }
        yield return null;
        int backupCount = availableDoors.Count;
        if (availableDoors.Count > 0)
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
                SpawnRoom(wantedDoorDirection, availableDoors[i].transform);
            }
        }
        else
        {
            creator.openProcesses--;
            if (creator.openProcesses == 0)
            {
                creator.CheckRoomCount();
            }
        }
    }
    public virtual void OnDestroyed()
    {
        creator.entireDungeon.Remove(gameObject);
    }
    public abstract void SpawnRoom(DungeonDoor.DoorDirection wantedDir, Transform doorPoint);
    public bool HasCollision(bool notIncludeThis = false)
    {
        Vector3 colliderHalfExtends = transform.GetComponent<BoxCollider>().size;
        colliderHalfExtends.x *= transform.lossyScale.x;
        colliderHalfExtends.y *= transform.lossyScale.y;
        colliderHalfExtends.z *= transform.lossyScale.z;
        colliderHalfExtends /= 2;
        bool returnValue = Physics.CheckBox(transform.position, colliderHalfExtends, transform.rotation, creator.roomLayer);
        if (returnValue)
        {
            if (notIncludeThis)
            {
                Collider[] hits = Physics.OverlapBox(transform.position, colliderHalfExtends, transform.rotation, creator.roomLayer);
                if (hits.Length >= 2)
                {
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
    public enum RoomTypes { Normal, End, Hallway, Shop, Event, Boss, Treasure }
}
