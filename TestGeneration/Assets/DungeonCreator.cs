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
        openProcesses = 0;
        foreach (GameObject dungeonPart in entireDungeon) /// Destroys the current dungeon if removePreviousDungeon is true;
        {
            DestroyImmediate(dungeonPart);
        }
        entireDungeon = new List<GameObject>();
        endRooms = new List<GameObject>();
    }
    public void SpawnRandomRoom(GameObject thisRoom, Vector3 doorPoint, DungeonDoor.DoorDirection requiredDirection, bool yes = false)
    {
        if(entireDungeon.Count < maxRooms)
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
            GameObject spawnedRoom = Instantiate(roomToSpawn, Vector3.zero, Quaternion.identity);
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
            Vector3 selectedDoorPosition = selectedDoor.localPosition;
            selectedDoorPosition.y = 0;
            Vector3 requiredPosition = doorPoint - selectedDoorPosition;
            if (yes)
            {
                spawnedRoom.GetComponent<DungeonRoom>().previousReplaced = true;
                //spawnedRoom.GetComponent<MeshRenderer>().material = endRoomColor;
            }
            spawnedRoom.transform.position = requiredPosition;
            spawnedRoom.transform.SetParent(thisRoom.transform);
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, selectedDoor.gameObject);
            print("WITH " + Physics.OverlapBox(spawnedRoom.transform.position, spawnedRoom.GetComponent<BoxCollider>().size / 2, spawnedRoom.transform.rotation).Length + " HITS AAA");
            if (spawnedRoom.GetComponent<DungeonRoom>().HasCollision())
            {
                print(spawnedRoom + " IS THE SPAWNED ROOM");
                ReplaceRoom(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
            }
            else
            {
                print("WITH " + Physics.OverlapBox(spawnedRoom.transform.position, spawnedRoom.GetComponent<BoxCollider>().size / 2, spawnedRoom.transform.rotation).Length + " HITS");
                StartCoroutine(spawnedRoom.GetComponent<DungeonRoom>().SpawnNextRoom());
            }
        }
    }
    public IEnumerator CheckRoomCount()
    {
        yield return null;
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
            Vector3 backupPos = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform.position;
            GameObject backupParent = roomToReplace.transform.parent.gameObject;
            DungeonDoor.DoorDirection backupDirection = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction;
            endRooms.Remove(roomToReplace);
            entireDungeon.Remove(roomToReplace);
            print("DESTROYED " + roomToReplace);
            print("PROCEEDGEN - REPLACING " + roomToReplace);
            DestroyImmediate(roomToReplace);
            SpawnRandomRoom(backupParent, backupPos, backupDirection, true);
        }
        else
        {
            GenerateDungeon();
        }
    }
    public void ReplaceWithSmallEndRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
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
        newRoom.transform.position = backupPosition - newRoom.GetComponent<DungeonRoom>().entranceDoor.transform.localPosition;
        newRoom.transform.SetParent(backupParent.transform);
        newRoom.GetComponent<DungeonRoom>().replaced = true;
        //newRoom.GetComponent<MeshRenderer>().material = endRoomColor;
        entireDungeon.Add(newRoom);

    }
    public void ReplaceRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection)
    {
        Vector3 backupPos = roomToReplace.GetComponent<DungeonRoom>().entranceDoor.transform.position;
        GameObject backupParent = roomToReplace.transform.parent.gameObject;
        entireDungeon.Remove(roomToReplace);
        if (endRooms.Contains(roomToReplace))
        {
            endRooms.Remove(roomToReplace);
        }
        print("DESTROYED " + roomToReplace);
        DestroyImmediate(roomToReplace);
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
            finalRoom.transform.position = backupPos - entranceDoor.transform.localPosition;
            finalRoom.GetComponent<DungeonRoom>().Initialize(this, entranceDoor);
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
            ReplaceWithSmallEndRoom(backupParent, backupParent.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction);
            if (openProcesses <= 0)
            {
                StartCoroutine(CheckRoomCount());
            }
        }
    }
}
