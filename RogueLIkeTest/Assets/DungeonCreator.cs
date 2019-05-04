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
    public Vector3 GetLocationData(Transform thisObject, Transform selectedDoor, Transform doorToSnapTo)
    {
        return doorToSnapTo.position - (selectedDoor.position - thisObject.position);
    }
    public void SpawnRandomHallway(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
    {
        List<GameObject> availableHallways = new List<GameObject>();
        foreach (GameObject hallway in hallways)
        {
            foreach (GameObject door in hallway.GetComponent<DungeonRoom>().availableDoors)
            {
                if (door.GetComponent<DungeonDoor>().direction == requiredDirection)
                {
                    availableHallways.Add(hallway);
                    break;
                }
            }
        }
        SpawnDungeonPartAlt(availableHallways[Random.Range(0, availableHallways.Count)], requiredDirection, thisRoom, doorPoint);
    }
    public void SpawnRandomRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
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
        SpawnDungeonPartAlt(roomToSpawn, requiredDirection, thisRoom, doorPoint);
    }
    public void SpawnDungeonPart(GameObject roomToSpawn, DungeonDoor.DoorDirection requiredDirection, GameObject parentRoom, Transform doorPoint)
    {
        if (roomCount < maxRooms)
        {
            GameObject spawnedRoom = Instantiate(roomToSpawn);
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

            GetLocationData(spawnedRoom.transform, selectedDoor, doorPoint);
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, parentRoom, selectedDoor.gameObject);
            spawnedRoom.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = doorPoint.gameObject;

            CheckPartCollision(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
        }
        else
        {
            RemoveDoor(doorPoint.gameObject);
        }
    }
    public void SpawnDungeonPartAlt(GameObject roomToSpawn, DungeonDoor.DoorDirection requiredDirection, GameObject parentRoom, Transform doorPoint)
    {
        List<Transform> availableDoors = new List<Transform>();
        for (int i = 0; i < roomToSpawn.GetComponent<DungeonRoom>().availableDoors.Count; i++)
        {
            GameObject thisDoor = roomToSpawn.GetComponent<DungeonRoom>().availableDoors[i];
            if (thisDoor.GetComponent<DungeonDoor>().direction == requiredDirection)
            {
                availableDoors.Add(thisDoor.transform);
            }
        }
        Transform selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
        int selectedDoorId = selectedDoor.GetComponent<DungeonDoor>().id;
        GameObject spawnedRoom = Instantiate(roomToSpawn, GetLocationData(roomToSpawn.transform, selectedDoor, doorPoint), Quaternion.identity);
        for(int i = 0; i < spawnedRoom.GetComponent<DungeonRoom>().availableDoors.Count; i++)
        {
            Transform thisDoor = spawnedRoom.GetComponent<DungeonRoom>().availableDoors[i].transform;
            if(thisDoor.GetComponent<DungeonDoor>().id == selectedDoorId)
            {
                selectedDoor = thisDoor;
                break;
            }
        }

        entireDungeon.Add(spawnedRoom);
        spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, parentRoom, selectedDoor.gameObject);
        spawnedRoom.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = doorPoint.gameObject;
        CheckPartCollision(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
    }
    public void CheckPartCollision(GameObject roomToCheck, DungeonDoor.DoorDirection entranceDirection)
    {
        if (roomToCheck.GetComponent<DungeonRoom>().HasCollision(true))
        {
            print("HAD COLL");
            ReplaceRoom(roomToCheck, entranceDirection);
        }
        else
        {
            print("DID NOT COLL");
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
    }

    public void ProceedGeneration()
    {
        if(endRooms.Count > 0)
        {
            GameObject roomToReplace = endRooms[Random.Range(0, endRooms.Count)];
            Transform backupTransform = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor.transform;
            GameObject backupParent = roomToReplace.GetComponent<DungeonRoom>().parentRoom;
            DungeonDoor.DoorDirection backupDirection = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction;
            endRooms.Remove(roomToReplace);
            entireDungeon.Remove(roomToReplace);
            print("DESTROYED " + roomToReplace);
            print("PROCEEDGEN - REPLACING " + roomToReplace);
            DestroyImmediate(roomToReplace);
            SpawnRandomRoom(backupParent, backupTransform, backupDirection);
        }
    }
    public void RemoveDoor(GameObject doorToRemove)
    {
        GameObject newWall = Instantiate(walls[Random.Range(0, walls.Length)], doorToRemove.transform.position, doorToRemove.transform.rotation, doorToRemove.transform.parent);
        doorToRemove.GetComponent<DungeonDoor>().ownerRoom.availableDoors.Remove(doorToRemove);
        if(doorToRemove.GetComponent<DungeonDoor>().ownerRoom.availableDoors.Count <= 0)
        {
            doorToRemove.GetComponent<DungeonDoor>().ownerRoom.type = DungeonRoom.RoomTypes.End;
            endRooms.Add(doorToRemove.GetComponent<DungeonDoor>().ownerRoom.gameObject);
        }
        DestroyImmediate(doorToRemove);
    }
    public void ReplaceRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
        DungeonRoom.RoomTypes backupType = roomToReplace.GetComponent<DungeonRoom>().type;
        GameObject backupParentDoor = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor;
        GameObject backupParent = roomToReplace.GetComponent<DungeonRoom>().parentRoom;
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
        GameObject selectedDoor = null;
        int selectedDoorId = 0;
        foreach(GameObject option in availableOptions)
        {
            List<GameObject> availableDoors = new List<GameObject>();
            foreach (GameObject door in option.GetComponent<DungeonRoom>().availableDoors)
            {
                if (door.GetComponent<DungeonDoor>().direction == entranceDirection)
                {
                    availableDoors.Add(door);
                }
            }
            selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            selectedDoorId = selectedDoor.GetComponent<DungeonDoor>().id;

            finalRoom = Instantiate(option, GetLocationData(option.transform, selectedDoor.transform, backupParentDoor.transform), Quaternion.identity);

            for(int i = 0; i < finalRoom.GetComponent<DungeonRoom>().availableDoors.Count; i++)
            {
                GameObject thisDoor = finalRoom.GetComponent<DungeonRoom>().availableDoors[i];
                if(thisDoor.GetComponent<DungeonDoor>().id == selectedDoorId)
                {
                    selectedDoor = thisDoor;
                    break;
                }
            }

            finalRoom.GetComponent<DungeonRoom>().Initialize(this, backupParent, selectedDoor);
            finalRoom.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = backupParentDoor;
            if (finalRoom.GetComponent<DungeonRoom>().HasCollision(true))
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
