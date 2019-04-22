using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeons;
    [Header("Generation Data")]
    public int minRooms;
    public int maxRooms;
    public int shopRooms;
    public int bossRooms;

    public bool removePreviousDungeon;

    [Header("Room Data")]
    public List<GameObject> startRooms;
    public List<GameObject> leftRooms;
    public List<GameObject> rightRooms;
    public List<GameObject> upRooms;
    public List<GameObject> downRooms;
    public List<GameObject> smallEndRooms;
    public LayerMask roomLayer;

    public int openProcesses;
    public List<GameObject> endRooms;
    public List<GameObject> entireDungeon;

    [Header("TEST PUPROSES")]
    public GameObject ogRoom;
    public GameObject hitsThisObject;

    public Material endRoomColor;


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
        openProcesses = 0;
        foreach (GameObject dungeonPart in entireDungeon) /// Destroys the current dungeon if removePreviousDungeon is true;
        {
            DestroyImmediate(dungeonPart);
        }
        entireDungeon = new List<GameObject>();
        endRooms = new List<GameObject>();
    }
    public void SpawnRandomRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection, bool yes = false)
    {
        if(entireDungeon.Count < maxRooms)
        {
            GameObject spawnedRoom = null;
            switch (requiredDirection)
            {
                case DungeonDoor.DoorDirection.Up:
                    spawnedRoom = upRooms[Random.Range(0, upRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Down:
                    spawnedRoom = downRooms[Random.Range(0, downRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Left:
                    spawnedRoom = leftRooms[Random.Range(0, leftRooms.Count)];
                    break;

                case DungeonDoor.DoorDirection.Right:
                    spawnedRoom = rightRooms[Random.Range(0, rightRooms.Count)];
                    break;

            }
            spawnedRoom = Instantiate(spawnedRoom, Vector3.zero, Quaternion.identity);
            entireDungeon.Add(spawnedRoom);

            List<Transform> availableDoors = new List<Transform>();
            for(int i = 0; i < spawnedRoom.GetComponent<DungeonRoom>().availableDoors.Count; i++)
            {
                GameObject thisDoor = spawnedRoom.GetComponent<DungeonRoom>().availableDoors[i];
                if(thisDoor.GetComponent<DungeonDoor>().direction == requiredDirection)
                {
                    availableDoors.Add(thisDoor.transform);
                }
            }
            Transform selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            spawnedRoom.GetComponent<DungeonRoom>().parentDoor = doorPoint.gameObject;
            Vector3 selectedDoorPosition = selectedDoor.localPosition;
            selectedDoorPosition.y = 0;
            Vector3 requiredPosition = doorPoint.position - selectedDoorPosition;
            if (yes)
            {
                spawnedRoom.GetComponent<DungeonRoom>().replaced = true;
                //spawnedRoom.GetComponent<MeshRenderer>().material = endRoomColor;
            }
            spawnedRoom.transform.position = requiredPosition;
            spawnedRoom.transform.SetParent(thisRoom.transform);
            doorPoint.gameObject.GetComponent<DungeonDoor>().childRoom = spawnedRoom.transform;
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, selectedDoor.gameObject);
            if (spawnedRoom.GetComponent<DungeonRoom>().HasCollision())
            {
                print(spawnedRoom + " IS THE SPAWNED ROOM");
                ReplaceRoom(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
            }
            else
            {
                StartCoroutine(spawnedRoom.GetComponent<DungeonRoom>().SpawnNextRoom());
                if (openProcesses <= 0)
                {
                    CheckRoomCount();
                }
            }
        }
    }
    public void CheckRoomCount()
    {
        if(entireDungeon.Count < minRooms)
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
            endRooms.Remove(roomToReplace);
            entireDungeon.Remove(roomToReplace);
            roomToReplace.GetComponent<BoxCollider>().enabled = false;
            roomToReplace.GetComponent<MeshRenderer>().material = endRoomColor;
            print("REPLACING " + roomToReplace);
            SpawnRandomRoom(roomToReplace.transform.parent.gameObject, roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform, roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction, true);
            Destroy(roomToReplace);
        }
        else
        {
            GenerateDungeon();
        }
    }
    public void ReplaceWithSmallEndRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
        entireDungeon.Remove(roomToReplace);
        if (endRooms.Contains(roomToReplace))
        {
            print("Yes");
            endRooms.Remove(roomToReplace);
        }
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
        newRoom.transform.position = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform.position - newRoom.GetComponent<DungeonRoom>().entranceDoor.transform.localPosition;
        newRoom.transform.SetParent(roomToReplace.transform.parent);
        newRoom.GetComponent<DungeonRoom>().replaced = true;
        //newRoom.GetComponent<MeshRenderer>().material = endRoomColor;
        entireDungeon.Add(newRoom);
        roomToReplace.GetComponent<BoxCollider>().enabled = false;
        Destroy(roomToReplace);

    }
    public void ReplaceRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
        entireDungeon.Remove(roomToReplace);
        if (endRooms.Contains(roomToReplace))
        {
            endRooms.Remove(roomToReplace);
        }
        List<GameObject> availableOptions = new List<GameObject>();
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
            finalRoom.transform.position = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform.position - entranceDoor.transform.localPosition;
            finalRoom.GetComponent<DungeonRoom>().Initialize(this, entranceDoor);
            if (finalRoom.GetComponent<DungeonRoom>().HasCollision())
            {
                if (endRooms.Contains(finalRoom))
                {
                    endRooms.Remove(finalRoom);
                }
                Destroy(finalRoom);
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
            entireDungeon.Add(finalRoom);
            finalRoom.transform.SetParent(roomToReplace.transform.parent);
            DestroyImmediate(roomToReplace);
            finalRoom.GetComponent<DungeonRoom>().replaced = true;
            StartCoroutine(finalRoom.GetComponent<DungeonRoom>().SpawnNextRoom());
        }
        else
        {
            roomToReplace.GetComponent<MeshRenderer>().material = endRoomColor;
            ogRoom = roomToReplace;
            hitsThisObject = roomToReplace.transform.parent.gameObject;
            print(roomToReplace + " WILL BE DESTROYED");
            print(roomToReplace.transform.parent.gameObject + " IS THE ROOM THAT SHOULD BE REPLACED");
            ReplaceWithSmallEndRoom(roomToReplace.transform.parent.gameObject, roomToReplace.transform.parent.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction);
            Destroy(roomToReplace);
            if (openProcesses <= 0)
            {
                CheckRoomCount();
            }
        }
    }
}
