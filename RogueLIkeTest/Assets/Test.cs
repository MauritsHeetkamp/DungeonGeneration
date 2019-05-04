using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject start;
    public GameObject part;
    public DungeonCreator DG;
    // Start is called before the first frame update
    void Start()
    {
        //Hi(part);
        SpawnDungeonPart(start);
    }

    public void SpawnDungeonPart(GameObject roomToSpawn)
    {
        GameObject spawnedRoom = Instantiate(roomToSpawn, Vector3.zero, Quaternion.identity);
        spawnedRoom.GetComponent<DungeonRoom>().Initialize(DG);
        DG.SpawnDungeonPartAlt(part, DungeonDoor.DoorDirection.Down, spawnedRoom, spawnedRoom.GetComponent<DungeonRoom>().availableDoors[0].transform);
        spawnedRoom.GetComponent<DungeonRoom>().Initialize(DG);
        DG.SpawnDungeonPartAlt(part, DungeonDoor.DoorDirection.Down, spawnedRoom, spawnedRoom.GetComponent<DungeonRoom>().availableDoors[0].transform);
    }
    public void Hi(GameObject room)
    {
        GameObject hi = Instantiate(room);
        hi.transform.position = new Vector3(1, 1, 1);
        print(hi.GetComponent<DungeonRoom>().HasCollision(true));
        GameObject oi = Instantiate(room);
        oi.transform.position = new Vector3(1, 1, 1);
        print(hi.GetComponent<DungeonRoom>().HasCollision(true));
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
