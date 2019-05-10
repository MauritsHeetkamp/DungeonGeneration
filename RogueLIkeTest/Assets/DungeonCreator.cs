using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public DistanceStack distanceRoomStack;
    public GameObject startRoom;

    public int dungeons;
    [Header("Generation Data")]
    public int minRoomCount;
    public int maxRoomCount;
    public int minShopRoomCount;
    public int minTreasureRoomCount;
    public int minBossRoomCount, minBossRoomDistance;
    public int maxEventRoomCount, maxEventRoomPerFloor;

    public bool removePreviousDungeon;


    public GameObject[] walls;
    public GameObject[] doors;
    [Header("Room Data")]
    public List<GameObject> startRooms;
    public List<GameObject> rooms;

    public List<GameObject> hallways;
    public List<GameObject> bossRooms;
    public List<GameObject> shopRooms;
    public List<GameObject> treasureRooms;
    public List<GameObject> eventRooms;
    public LayerMask roomLayer;

    public int openProcesses;
    public int roomCount;
    public int shopCount;
    public int bossCount;
    public int treasureCount;
    public static int eventRoomCount;
    public int eventRoomsThisFloor;


    [Header("Spawn Odds")]
    [Range(0, 100)]
    public int shopChance;
    [Range(0, 100)]
    public int eventRoomChance;
    [Range(0, 100)]
    public int bossRoomChance;
    [Range(0, 100)]
    public int treasureRoomChance;

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
        startRoom = Instantiate(startRooms[Random.Range(0, startRooms.Count)]);
        startRoom.GetComponent<BaseRoom>().Initialize(this);
        StartCoroutine(startRoom.GetComponent<BaseRoom>().SpawnNextRoom());
    }
    public void ClearDungeon()
    {
        eventRoomsThisFloor = 0;
        eventRoomCount = 0;
        roomCount = 0;
        openProcesses = 0;
        shopCount = 0;
        bossCount = 0;
        treasureCount = 0;
        print(entireDungeon.Count);
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
        SpawnDungeonPartAlt(hallways, requiredDirection, thisRoom, doorPoint, BaseRoom.RoomTypes.Hallway);
    }
    public void SpawnRandomRoom(GameObject thisRoom, Transform doorPoint, DungeonDoor.DoorDirection requiredDirection)
    {
        SpawnDungeonPartAlt(rooms, requiredDirection, thisRoom, doorPoint, BaseRoom.RoomTypes.Normal);
    }
    public void SpawnDungeonPartAlt(List<GameObject> allRooms, DungeonDoor.DoorDirection requiredDirection, GameObject parentRoom, Transform doorPoint, BaseRoom.RoomTypes roomType)
    {
        if(roomCount < maxRoomCount)
        {
            if(roomType == BaseRoom.RoomTypes.Normal)
            {
                List<GameObject> shouldReplace = ReplaceWithSpecialRoom(parentRoom);

                if (shouldReplace != null)
                {
                    allRooms = shouldReplace;
                }
            }

            List<GameObject> possibleRooms = new List<GameObject>();

            foreach (GameObject room in allRooms)
            {
                foreach (GameObject door in room.GetComponent<BaseRoom>().availableDoors)
                {
                    if (door.GetComponent<DungeonDoor>().direction == requiredDirection)
                    {
                        possibleRooms.Add(room);
                        break;
                    }
                }
            }

            GameObject roomToSpawn = possibleRooms[Random.Range(0, possibleRooms.Count)];
            List<Transform> availableDoors = new List<Transform>();
            for (int i = 0; i < roomToSpawn.GetComponent<BaseRoom>().availableDoors.Count; i++)
            {
                GameObject thisDoor = roomToSpawn.GetComponent<BaseRoom>().availableDoors[i];
                if (thisDoor.GetComponent<DungeonDoor>().direction == requiredDirection)
                {
                    availableDoors.Add(thisDoor.transform);
                }
            }
            Transform selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            int selectedDoorId = selectedDoor.GetComponent<DungeonDoor>().id;
            GameObject spawnedRoom = Instantiate(roomToSpawn, GetLocationData(roomToSpawn.transform, selectedDoor, doorPoint), Quaternion.identity);
            for (int i = 0; i < spawnedRoom.GetComponent<BaseRoom>().availableDoors.Count; i++)
            {
                Transform thisDoor = spawnedRoom.GetComponent<BaseRoom>().availableDoors[i].transform;
                if (thisDoor.GetComponent<DungeonDoor>().id == selectedDoorId)
                {
                    selectedDoor = thisDoor;
                    break;
                }
            }

            spawnedRoom.GetComponent<BaseRoom>().Initialize(this, parentRoom, selectedDoor.gameObject);
            spawnedRoom.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = doorPoint.gameObject;
            CheckPartCollision(spawnedRoom, selectedDoor.GetComponent<DungeonDoor>().direction);
        }
        else
        {
            RemoveDoor(doorPoint.gameObject);
            if(openProcesses == 0)
            {
                CheckRoomCount();
            }
        }
    }
    public void CheckPartCollision(GameObject roomToCheck, DungeonDoor.DoorDirection entranceDirection)
    {
        if (roomToCheck.GetComponent<BaseRoom>().HasCollision(true))
        {
            print("HAD COLL");
            ReplaceRoom(roomToCheck, entranceDirection);
        }
        else
        {
            print("DID NOT COLL");
            StartCoroutine(roomToCheck.GetComponent<BaseRoom>().SpawnNextRoom());
        }
    }
    public void CheckRoomCount()
    {
        if(roomCount < minRoomCount)
        {
            ProceedGeneration();
        }
        else
        {
            print("MKAYYYY");
            if(bossCount < minBossRoomCount)
            {
                ForcedReplaceRoom(endRooms, minBossRoomCount - bossCount, bossRooms, minBossRoomDistance);
            }
            if(shopCount < minShopRoomCount)
            {
                ForcedReplaceRoom(endRooms, minShopRoomCount - shopCount, shopRooms);
            }
            if (treasureCount < minTreasureRoomCount)
            {
                ForcedReplaceRoom(endRooms, minTreasureRoomCount - treasureCount, treasureRooms);
            }
            if (bossCount < minBossRoomCount || shopCount < minShopRoomCount || treasureCount < minTreasureRoomCount)
            {
                StartCoroutine(Test());
            }
            else
            {
                print("DONE");
            }
        }
    }
    public IEnumerator Test()
    {
        yield return null;
        GenerateDungeon();
    }
    public void ProceedGeneration()
    {
        if(endRooms.Count > 0)
        {
            GameObject roomToReplace = endRooms[Random.Range(0, endRooms.Count)];
            Transform backupTransform = roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor.transform;
            GameObject backupParent = roomToReplace.GetComponent<BaseRoom>().parentRoom;
            DungeonDoor.DoorDirection backupDirection = roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().direction;;
            print("DESTROYED " + roomToReplace);
            print("PROCEEDGEN - REPLACING " + roomToReplace);
            roomToReplace.GetComponent<BaseRoom>().OnDestroyed();
            DestroyImmediate(roomToReplace);
            SpawnRandomRoom(backupParent, backupTransform, backupDirection);
        }
    }
    public void RemoveDoor(GameObject doorToRemove)
    {
        if (doorToRemove.transform.parent.GetComponent<BaseRoom>().type == BaseRoom.RoomTypes.Hallway)
        {
            GameObject hallway = doorToRemove.transform.parent.gameObject;
            doorToRemove = hallway.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor;
            hallway.GetComponent<BaseRoom>().OnDestroyed();
            DestroyImmediate(hallway);
        }
        GameObject newWall = Instantiate(walls[Random.Range(0, walls.Length)], doorToRemove.transform.position, doorToRemove.transform.rotation, doorToRemove.transform.parent);
        doorToRemove.GetComponent<DungeonDoor>().ownerRoom.availableDoors.Remove(doorToRemove);
        if(doorToRemove.GetComponent<DungeonDoor>().ownerRoom.availableDoors.Count <= 0)
        {
            doorToRemove.GetComponent<DungeonDoor>().ownerRoom.type = BaseRoom.RoomTypes.End;
            endRooms.Add(doorToRemove.GetComponent<DungeonDoor>().ownerRoom.gameObject);
        }
        DestroyImmediate(doorToRemove);
    }
    public void ReplaceRoom(GameObject roomToReplace, DungeonDoor.DoorDirection entranceDirection, List<GameObject> staticOptions = null)
    {
        BaseRoom.RoomTypes backupType = roomToReplace.GetComponent<BaseRoom>().type;
        GameObject backupParentDoor = roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor;
        GameObject backupParent = roomToReplace.GetComponent<BaseRoom>().parentRoom;
        print("DESTROYED " + roomToReplace);
        roomToReplace.GetComponent<BaseRoom>().OnDestroyed();
        DestroyImmediate(roomToReplace);

        List<GameObject> possibleAvailableOptions = new List<GameObject>();
        if(staticOptions != null)
        {
            possibleAvailableOptions = staticOptions;
        }
        else
        {
            if (backupType == BaseRoom.RoomTypes.Hallway)
            {
                possibleAvailableOptions = hallways;
            }
            else
            {
                possibleAvailableOptions = rooms;
            }
        }

        List<GameObject> availableOptions = new List<GameObject>();
        foreach(GameObject option in possibleAvailableOptions)
        {
            foreach(GameObject door in option.GetComponent<BaseRoom>().availableDoors)
            {
                if(door.GetComponent<DungeonDoor>().direction == entranceDirection)
                {
                    availableOptions.Add(option);
                    break;
                }
            }
        }

        GameObject finalRoom = null;
        GameObject selectedDoor = null;
        int selectedDoorId = 0;
        foreach(GameObject option in availableOptions)
        {
            List<GameObject> availableDoors = new List<GameObject>();
            foreach (GameObject door in option.GetComponent<BaseRoom>().availableDoors)
            {
                if (door.GetComponent<DungeonDoor>().direction == entranceDirection)
                {
                    availableDoors.Add(door);
                }
            }
            selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
            selectedDoorId = selectedDoor.GetComponent<DungeonDoor>().id;

            finalRoom = Instantiate(option, GetLocationData(option.transform, selectedDoor.transform, backupParentDoor.transform), Quaternion.identity);

            for(int i = 0; i < finalRoom.GetComponent<BaseRoom>().availableDoors.Count; i++)
            {
                GameObject thisDoor = finalRoom.GetComponent<BaseRoom>().availableDoors[i];
                if(thisDoor.GetComponent<DungeonDoor>().id == selectedDoorId)
                {
                    selectedDoor = thisDoor;
                    break;
                }
            }

            finalRoom.GetComponent<BaseRoom>().Initialize(this, backupParent, selectedDoor);
            finalRoom.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = backupParentDoor;
            if (finalRoom.GetComponent<BaseRoom>().HasCollision(true))
            {
                finalRoom.GetComponent<BaseRoom>().OnDestroyed();
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
            finalRoom.GetComponent<BaseRoom>().replaced = true;
            StartCoroutine(finalRoom.GetComponent<BaseRoom>().SpawnNextRoom());
        }
        else
        {
            RemoveDoor(backupParentDoor);
            if (openProcesses <= 0)
            {
                CheckRoomCount();
            }
        }
    }

    public void ForcedReplaceRoom(List<GameObject> roomsToReplace, int requiredTimes, List<GameObject> staticOptions = null, int minDistanceFromStart = 0)
    {
        bool replaced = false;
        for(int counter = 0; counter < requiredTimes; counter++)
        {
            foreach (GameObject roomToReplace in roomsToReplace)
            {
                if (roomToReplace.GetComponent<BaseRoom>().roomDistanceFromStart >= minDistanceFromStart)
                {
                    print("HIIII");
                    roomToReplace.SetActive(false);
                    DungeonDoor.DoorDirection entranceDirection = roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().direction;

                    List<GameObject> possibleAvailableOptions = new List<GameObject>();
                    if (staticOptions != null)
                    {
                        possibleAvailableOptions = staticOptions;
                    }
                    else
                    {
                        if (roomToReplace.GetComponent<BaseRoom>().type == BaseRoom.RoomTypes.Hallway)
                        {
                            possibleAvailableOptions = hallways;
                        }
                        else
                        {
                            possibleAvailableOptions = rooms;
                        }
                    }

                    List<GameObject> availableOptions = new List<GameObject>();
                    foreach (GameObject option in possibleAvailableOptions)
                    {
                        foreach (GameObject door in option.GetComponent<BaseRoom>().availableDoors)
                        {
                            if (door.GetComponent<DungeonDoor>().direction == entranceDirection)
                            {
                                availableOptions.Add(option);
                                break;
                            }
                        }
                    }

                    GameObject finalRoom = null;
                    GameObject selectedDoor = null;
                    int selectedDoorId = 0;
                    print(availableOptions.Count);
                    foreach (GameObject option in availableOptions)
                    {
                        List<GameObject> availableDoors = new List<GameObject>();
                        foreach (GameObject door in option.GetComponent<BaseRoom>().availableDoors)
                        {
                            if (door.GetComponent<DungeonDoor>().direction == entranceDirection)
                            {
                                availableDoors.Add(door);
                            }
                        }
                        selectedDoor = availableDoors[Random.Range(0, availableDoors.Count)];
                        selectedDoorId = selectedDoor.GetComponent<DungeonDoor>().id;

                        finalRoom = Instantiate(option, GetLocationData(option.transform, selectedDoor.transform, roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor.transform), Quaternion.identity);

                        for (int i = 0; i < finalRoom.GetComponent<BaseRoom>().availableDoors.Count; i++)
                        {
                            GameObject thisDoor = finalRoom.GetComponent<BaseRoom>().availableDoors[i];
                            if (thisDoor.GetComponent<DungeonDoor>().id == selectedDoorId)
                            {
                                selectedDoor = thisDoor;
                                break;
                            }
                        }

                        finalRoom.GetComponent<BaseRoom>().Initialize(this, roomToReplace.GetComponent<BaseRoom>().parentRoom, selectedDoor);
                        finalRoom.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor = roomToReplace.GetComponent<BaseRoom>().entranceDoor.GetComponent<DungeonDoor>().parentDoor;
                        if (finalRoom.GetComponent<BaseRoom>().HasCollision(true))
                        {
                            finalRoom.GetComponent<BaseRoom>().OnDestroyed();
                            DestroyImmediate(finalRoom);
                            finalRoom = null;
                            continue;
                        }
                        else
                        {
                            roomsToReplace.Remove(roomToReplace);
                            roomToReplace.GetComponent<BaseRoom>().OnDestroyed();
                            DestroyImmediate(roomToReplace);
                            //print("THIS ONE DID NOT COLLIDE C:");
                            break;
                        }
                    }
                    if (finalRoom != null)
                    {
                        print("FOUND NEW ROOM");
                        finalRoom.GetComponent<BaseRoom>().replaced = true;
                        //StartCoroutine(finalRoom.GetComponent<BaseRoom>().SpawnNextRoom());
                        replaced = true;
                        break;
                    }
                    else
                    {
                        roomToReplace.SetActive(true);
                    }
                }
            }
            if (!replaced)
            {
                return;
            }
        }
    }

    public List<GameObject> ReplaceWithSpecialRoom(GameObject parentRoom_)
    {
        int randomNum = Random.Range(1, 101);
        if(bossCount < minBossRoomCount && parentRoom_.GetComponent<BaseRoom>().roomDistanceFromStart >= minBossRoomDistance && randomNum <= bossRoomChance)
        {
            return bossRooms;
        }
        randomNum = Random.Range(1, 101);
        if (randomNum <= shopChance && shopCount < minShopRoomCount)
        {
            return shopRooms;
        }
        randomNum = Random.Range(1, 101);
        if (randomNum <= treasureRoomChance && treasureCount < minTreasureRoomCount)
        {
            return treasureRooms;
        }
        randomNum = Random.Range(1, 101);
        if (eventRoomsThisFloor < maxEventRoomPerFloor && randomNum <= eventRoomChance && eventRoomCount < maxEventRoomCount)
        {
            return eventRooms;
        }
        return null;
    }
    [System.Serializable]
    public struct DistanceStack
    {
        public DistanceObject[] stack;
        public void AddRoom(GameObject objectToCheck, float distanceFromStart)
        {
            DistanceObject thisDistanceObject = new DistanceObject(objectToCheck, distanceFromStart);
            for(int i = stack.Length - 1; i >= 0; i--)
            {
                if(stack[i].thisObject == null)
                {
                    stack[i] = thisDistanceObject;
                    return;
                }
                else
                {
                    if(thisDistanceObject.distanceFromReferencePoint > stack[i].distanceFromReferencePoint)
                    {
                        stack[0] = thisDistanceObject;
                        Sort();
                    }
                }
            }
        }
        public void Sort()
        {
            bool swapped = false;
            int currentToCheck = 0;
            while(currentToCheck < stack.Length - 1)
            {
                if(stack[currentToCheck].distanceFromReferencePoint > stack[currentToCheck + 1].distanceFromReferencePoint || stack[currentToCheck + 1].thisObject == null)
                {
                    DistanceObject backupStackObject = stack[currentToCheck + 1];
                    stack[currentToCheck + 1] = stack[currentToCheck];
                    stack[currentToCheck] = backupStackObject;
                    swapped = true;
                }
                currentToCheck++;
            }
            if (swapped)
            {
                Sort();
            }
        }
        public GameObject PopFarthestObjectFromStack()
        {
            DistanceObject backupStackObject = stack[stack.Length - 1];
            stack[stack.Length - 1] = null;
            Sort();
            return (backupStackObject.thisObject);
        }
    }
    [System.Serializable]
    public class DistanceObject
    {
        public GameObject thisObject;
        public float distanceFromReferencePoint;

        public DistanceObject(GameObject thisObject_, float distance)
        {
            thisObject = thisObject_;
            distanceFromReferencePoint = distance;
        }
    }
}
