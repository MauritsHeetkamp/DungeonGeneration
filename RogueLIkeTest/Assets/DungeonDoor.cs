using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    public DungeonRoom ownerRoom;
    public GameObject parentDoor;
    public DoorDirection direction;
    public int id;


    public enum DoorDirection {Left, Down, Right, Up }

    public DungeonDoor(GameObject parentedDoor)
    {
        parentDoor = parentedDoor;
    }
}
