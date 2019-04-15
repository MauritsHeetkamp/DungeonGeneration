using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    public DungeonRoom ownerRoom;
    public DoorDirection direction;


    public enum DoorDirection {Left, Down, Right, Up }
}
