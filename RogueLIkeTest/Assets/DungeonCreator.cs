using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public Collider[] hits;

    public int dungeons;
    [Header("Generation Data")]
    public int minRooms;
    public int maxRooms;
    public int shopRooms;
    public int bossRooms;

    public bool removePreviousDungeon;


    public GameObject[] walls;
    public GameObject[] doors;
    [Header("Room Data")]
    public List<GameObject> startRooms;
    public List<GameObject> leftRooms;
    public List<GameObject> rightRooms;
    public List<GameObject> upRooms;
    public List<GameObject> downRooms;

    public GameObject[] hallways;
    public LayerMask roomLayer;

    public int openProcesses;
    public int roomCount;
    public List<GameObject> endRooms;
    public List<GameObject> entireDungeon;

    [Header("TEST PUPROSES")]
    public GameObject ogRoom;
    public GameObject hitsThisObject;

    public Material endRoomColor;

    public void Awake()
    {

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            GenerateDungeon();
        }
    }
    // Generates the dungeon
    public void GenerateDungeon() 
    {
        dungeons++;
        print(dungeons);
        if (removePreviousDungeon)
        {
            ClearDungeon();
        }
        entireDungeon.Add(Instantiate(startRooms[Random.Range(0, startRooms.Count)]));
        entireDungeon[entireDungeon.Count - 1].GetComponent<DungeonRoom>().Initialize(this);
        StartCoroutine(entireDungeon[entireDungeon.Count - 1].GetComponent<DungeonRoom>().SpawnNextRoom());
    }
    public void ClearDungeon()
    {
        roomCount = 0;
        openProcesses = 0;
        foreach (GameObject dungeonPart in entireDungeon) /// Destroys the current dungeon if removePreviousDungeon is true;
        {
            DestroyImmediate(dungeonPart);
        }
        entireDungeon = new List<GameObject>();
        endRooms = new List<GameObject>();
    }
    public void SetLocation(Transform thisObject, Transform selectedDoor, Transform doorToSnapTo)
    {
        thisObject.position = doorToSnapTo.position - (selectedDoor.position - thisObject.position);
    }
    public void SpawnRandomHallway(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
    {
        if (roomCount <= maxRooms)
        {
            List<GameObject> availableHallways = new List<GameObject>();
            foreach(GameObject hallway in hallways)
            {
                foreach(GameObject door in hallway.GetComponent<DungeonRoom>().availableDoors)
                {
                    if(door.GetComponent<DungeonDoor>().direction == requiredDirection)
                    {
                        availableHallways.Add(hallway);
                        break;
                    }
                }
            }
            SpawnDungeonPart(availableHallways[Random.Range(0, availableHallways.Count)], requiredDirection, thisRoom, doorPoint);
        }
    }
    public void SpawnRandomRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
    {
        if(roomCount <= maxRooms)
        {
            GameObject roomToSpawn = null;
            switch (requiredDirection)
            {
                case DungeonDoor.DoorDirection.Up:
                    roomToSpawn = upRooms[Random.Range(0, upRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Down:
                    roomToSpawn = downRooms[Random.Range(0, downRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Left:
                    roomToSpawn = leftRooms[Random.Range(0, leftRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Right:
                    roomToSpawn = rightRooms[Random.Range(0, rightRooms.Count)];
                    break;

            }
            SpawnDungeonPart(roomToSpawn, requiredDirection, thisRoom, doorPoint);
        }
    }
    public void SpawnDungeonPart(GameObject roomToSpawn, DungeonDoor.DoorDirection requiredDirection, GameObject parentRoom, Transform doorPoint)
    {
        if (roomCount < maxRooms)
        {
            GameObject spawnedRoom = Instantiate(roomToSpawn, Vector3.zero, Quaternion.identity);
            entireDungeon.Add(spawnedRoom);

            List<Transform> availableDoors = new List<Transform>();
            for (int i = 0; i < spawnedRoom.GetComponent<DungeonRoom>().availableDoors.Count; i++)
            {
                GameObject thisDoor = spawnedRoom.GetComponent<DungeonRoom>().availableDoors[i];
                if (thisDoor.GetComponent<DungeonDoor>().direction == requiredDirection)
                {
                    availableDoors.Add(thisDoor.transform);
                }
            }
            Transform selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];

            SetLocation(spawnedRoom.transform, selectedDoor, doorPoint);
            spawnedRoom.transform.SetParent(parentRoom.transform);
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, selectedDoor.gameObject);
            spawnedRoom.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = doorPoint.gameObject;

            CheckPartCollision(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
        }
    }
    public void CheckPartCollision(GameObject roomToCheck, DungeonDoor.DoorDirection entranceDirection)
    {
        if (roomToCheck.GetComponent<DungeonRoom>().HasCollision())
        {
            print("HAD COLL");
            ReplaceRoom(roomToCheck, entranceDirection);
        }
        else
        {
            StartCoroutine(roomToCheck.GetComponent<DungeonRoom>().SpawnNextRoom());
        }
    }
    public IEnumerator CheckRoomCount()
    {
        yield return null;
        if(roomCount < minRooms)
        {
            ProceedGeneration();
        }
        else
        {
            GenerateDungeon();
        }
    }

    public void ProceedGeneration()
    {
        if(endRooms.Count > 0)
        {
            GameObject roomToReplace = endRooms[Random.Range(0, endRooms.Count)];
            Transform backupTransform = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor.transform;
            GameObject backupParent = roomToReplace.transform.parent.gameObject;
            DungeonDoor.DoorDirection backupDirection = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction;
            endRooms.Remove(roomToReplace);
            entireDungeon.Remove(roomToReplace);
            print("DESTROYED " + roomToReplace);
            print("PROCEEDGEN - REPLACING " + roomToReplace);
            DestroyImmediate(roomToReplace);
            SpawnRandomRoom(backupParent, backupTransform, backupDirection);
        }
        else
        {
            GenerateDungeon();
        }
    }
    public void RemoveDoor(GameObject doorToRemove)
    {
        GameObject newWall = Instantiate(walls[Random.Range(0, walls.Length)], doorToRemove.transform.position, doorToRemove.transform.rotation, doorToRemove.transform.parent);
        doorToRemove.GetComponent<DungeonDoor>().ownerRoom.availableDoors.Remove(doorToRemove);
        DestroyImmediate(doorToRemove);
        /*
        print("SPAWNED SMALL END ROOM");
        Vector3 backupPosition = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform.position;
        GameObject backupParent = roomToReplace.transform.parent.gameObject;
        entireDungeon.Remove(roomToReplace);
        if (endRooms.Contains(roomToReplace))
        {
            endRooms.Remove(roomToReplace);
        }
        print("DESTROYED " + roomToReplace);
        DestroyImmediate(roomToReplace);
        List<GameObject> availableEndRooms = new List<GameObject>();
        for(int i = 0; i < smallEndRooms.Count; i++)
        {
            GameObject thisRoom = smallEndRooms[i];

            if(thisRoom.GetComponent<DungeonRoom>().availableDoors[0].GetComponent<DungeonDoor>().direction == entranceDirection)
            {
                availableEndRooms.Add(thisRoom);
            }
        }
        GameObject newRoom = Instantiate(availableEndRooms[Random.Range(0, availableEndRooms.Count)]);
        newRoom.GetComponent<DungeonRoom>().Initialize(this, newRoom.GetComponent<DungeonRoom>().availableDoors[0]);
        newRoom.transform.position = backupPosition - (newRoom.GetComponent<DungeonRoom>().entranceDoor.transform.position - newRoom.transform.position);
        newRoom.transform.SetParent(backupParent.transform);
        newRoom.GetComponent<DungeonRoom>().replaced = true;
        //newRoom.GetComponent<MeshRenderer>().material = endRoomColor;
        entireDungeon.Add(newRoom);*/



    }
    public void ReplaceRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
        DungeonRoom.RoomTypes backupType = roomToReplace.GetComponent<DungeonRoom>().type;
        GameObject backupParentDoor = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor;
        GameObject backupParent = roomToReplace.transform.parent.gameObject;
        entireDungeon.Remove(roomToReplace);
        if (endRooms.Contains(roomToReplace))
        {
            endRooms.Remove(roomToReplace);
        }
        print("DESTROYED " + roomToReplace);
        DestroyImmediate(roomToReplace);

        List<GameObject> availableOptions = new List<GameObject>();
        if(backupType == DungeonRoom.RoomTypes.Hallway)
        {
            foreach (GameObject hallway in hallways)
            {
                foreach (GameObject door in hallway.GetComponent<DungeonRoom>().availableDoors)
                {
                    if (door.GetComponent<DungeonDoor>().direction == entranceDirection)
                    {
                        availableOptions.Add(hallway);
                        break;
                    }
                }
            }
        }
        else
        {
            switch (entranceDirection)
            {
                case DungeonDoor.DoorDirection.Up:
                    availableOptions = upRooms;
                    break;
                case DungeonDoor.DoorDirection.Down:
                    availableOptions = downRooms;
                    break;
                case DungeonDoor.DoorDirection.Left:
                    availableOptions = leftRooms;
                    break;
                case DungeonDoor.DoorDirection.Right:
                    availableOptions = rightRooms;
                    break;
            }
        }
        GameObject finalRoom = null;
        GameObject entranceDoor = null;

        foreach(GameObject option in availableOptions)
        {
            finalRoom = Instantiate(option);
            List<GameObject> availableDoors = new List<GameObject>();
            foreach(GameObject door in finalRoom.GetComponent<DungeonRoom>().availableDoors)
            {
                if(door.GetComponent<DungeonDoor>().direction == entranceDirection)
                {
                    availableDoors.Add(door);
                }
            }
            entranceDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            finalRoom.transform.position = backupParentDoor.transform.position - (entranceDoor.transform.position - finalRoom.transform.position);
            finalRoom.GetComponent<DungeonRoom>().Initialize(this, entranceDoor);
            finalRoom.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = backupParentDoor;
            if (finalRoom.GetComponent<DungeonRoom>().HasCollision())
            {
                if (endRooms.Contains(finalRoom))
                {
                    endRooms.Remove(finalRoom);
                }
                DestroyImmediate(finalRoom);
                finalRoom = null;
                continue;
            }
            else
            {
                //print("THIS ONE DID NOT COLLIDE C:");
                break;
            }
        }
        if (finalRoom != null)
        {
            print("FOUND NEW ROOM");
            entireDungeon.Add(finalRoom);
            finalRoom.transform.SetParent(backupParent.transform);
            finalRoom.GetComponent<DungeonRoom>().replaced = true;
            StartCoroutine(finalRoom.GetComponent<DungeonRoom>().SpawnNextRoom());
        }
        else
        {
            RemoveDoor(backupParentDoor);
            if (openProcesses <= 0)
            {
                StartCoroutine(CheckRoomCount());
            }
        }
    }
}
