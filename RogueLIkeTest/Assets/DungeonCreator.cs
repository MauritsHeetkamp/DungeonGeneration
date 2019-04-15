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
    public GameObject[] startRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;
    public GameObject[] upRooms;
    public GameObject[] downRooms;

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
        entireDungeon.Add(Instantiate(startRooms[Random.Range(0, startRooms.Length)]));
        entireDungeon[entireDungeon.Count - 1].GetComponent<DungeonRoom>().Initialize(this);
        entireDungeon[entireDungeon.Count - 1].GetComponent<DungeonRoom>().SpawnNextRoom();
        CheckAmount();
    }
    public void ClearDungeon()
    {
        foreach (GameObject dungeonPart in entireDungeon) /// Destroys the current dungeon if removePreviousDungeon is true;
        {
            DestroyImmediate(dungeonPart);
        }
        entireDungeon = new List<GameObject>();
        endRooms = new List<GameObject>();
    }
    public void SpawnBasicRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
    {
        if(entireDungeon.Count < maxRooms)
        {
            GameObject spawnedRoom = null;
            switch (requiredDirection)
            {
                case DungeonDoor.DoorDirection.Up:
                    spawnedRoom = upRooms[Random.Range(0, upRooms.Length)];
                    break;

                case DungeonDoor.DoorDirection.Down:
                    spawnedRoom = downRooms[Random.Range(0, downRooms.Length)];
                    break;

                case DungeonDoor.DoorDirection.Left:
                    spawnedRoom = leftRooms[Random.Range(0, leftRooms.Length)];
                    break;

                case DungeonDoor.DoorDirection.Right:
                    spawnedRoom = rightRooms[Random.Range(0, rightRooms.Length)];
                    break;

            }
            spawnedRoom = Instantiate(spawnedRoom, Vector3.zero, Quaternion.identity);
            entireDungeon.Add(spawnedRoom);
            spawnedRoom.GetComponent<DungeonRoom>().Initialize(this);

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
            spawnedRoom.GetComponent<DungeonRoom>().availableDoors.Remove(selectedDoor.gameObject);
            spawnedRoom.GetComponent<DungeonRoom>().SpawnNextRoom();
        }
    }
    public void CheckAmount()
    {
        if(entireDungeon.Count < minRooms)
        {
            print(endRooms.Count.ToString() + " endrooms found");
            ProceedGeneration();
            return;
        }
        print("FINISHED GENERATION");
    }
    public void ProceedGeneration() // Makes sure the generation goes on when there aren't enough rooms
    {
        GameObject roomToChange = endRooms[Random.Range(0, endRooms.Count)];
        endRooms.Remove(roomToChange);
        entireDungeon.Remove(roomToChange);

        SpawnBasicRoom(roomToChange, roomToChange.GetComponent<DungeonRoom>().allDoors[0].transform, roomToChange.GetComponent<DungeonRoom>().allDoors[0].GetComponent<DungeonDoor>().direction);
        DestroyImmediate(roomToChange);

        CheckAmount();
    }
}
