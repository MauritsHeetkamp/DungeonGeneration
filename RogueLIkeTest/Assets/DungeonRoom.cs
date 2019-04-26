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

    public GameObject[] possibleDoorLocations;

    public void Awake()
    {

    }
    public void Initialize(DungeonCreator owner, GameObject entrance = null)
    {
        print(gameObject + "HAS COLLISION : " + HasCollision());
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
    public virtual IEnumerator SpawnNextRoom(bool yes = false)
    {
        print(" Spawned " + gameObject);
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
                print("OGSPAWN");
                creator.SpawnRandomRoom(gameObject, availableDoors[i].transform, wantedDoorDirection, yes);
            }
        }
        else
        {
            if(creator.openProcesses <= 0)
            {
                StartCoroutine(creator.CheckRoomCount());
            }
        }
    }
    public bool HasCollision()
    {
        Vector3 colliderHalfExtends = transform.GetComponent<BoxCollider>().size;
        colliderHalfExtends.x *= transform.localScale.x;
        colliderHalfExtends.y *= transform.localScale.y;
        colliderHalfExtends.z *= transform.localScale.z;
        colliderHalfExtends /= 2;
        bool returnValue = Physics.CheckBox(transform.position, colliderHalfExtends, transform.rotation);
        if (returnValue)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, colliderHalfExtends, transform.rotation);
            if(creator != null)
            {
                creator.hits = hits;
            }
            foreach(Collider col in hits)
            {
                print(gameObject + " HAS COLLIDED WITH " + col.gameObject);
            }
        }
        return returnValue;
    }
    public enum RoomTypes { Normal, End, Hallway}
}
