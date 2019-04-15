using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
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
    public LayerMask roomLayer;

    public int openProcesses;
    public List<GameObject> endRooms;
    public List<GameObject> entireDungeon;


    // Generates the dungeon
    public void GenerateDungeon() 
    {
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
    public void SpawnRandomRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection, bool roomContinueGeneration = true)
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
            Vector3 selectedDoorPosition = selectedDoor.localPosition;
            selectedDoorPosition.y = 0;
            Vector3 requiredPosition = doorPoint.position - selectedDoorPosition;
            spawnedRoom.transform.position = requiredPosition;
            spawnedRoom.transform.SetParent(thisRoom.transform);
            doorPoint.gameObject.GetComponent<DungeonDoor>().childRoom = spawnedRoom.transform;
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this, selectedDoor.gameObject);
            if (roomContinueGeneration)
            {
                StartCoroutine(spawnedRoom.GetComponent<DungeonRoom>().SpawnNextRoom());
                if(openProcesses <= 0)
                {
                    CheckAmount();
                }
            }
        }
    }

    public void CheckAmount()
    {
        if(entireDungeon.Count < minRooms)
        {
            //print(endRooms.Count.ToString() + " endrooms found");
            ProceedGeneration();
            return;
        }
        //print("FINISHED GENERATION");
    }
    public void ProceedGeneration() // Makes sure the generation goes on when there aren't enough rooms
    {
        GameObject roomToChange = endRooms[Random.Range(0, endRooms.Count)];
        endRooms.Remove(roomToChange);;
        entireDungeon.Remove(roomToChange);

        SpawnRandomRoom(roomToChange.transform.parent.gameObject, roomToChange.GetComponent<DungeonRoom>().entranceDoor.transform, roomToChange.GetComponent<DungeonRoom>().entranceDoor.GetComponent<DungeonDoor>().direction);
        DestroyImmediate(roomToChange);
    }
}
