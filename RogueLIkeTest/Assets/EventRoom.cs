using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventRoom : BaseRoom
{
    public override void Initialize(DungeonCreator owner, GameObject parentRoom_ = null, GameObject entrance = null)
    {
        base.Initialize(owner, parentRoom_, entrance);
        creator.eventRoomsThisFloor++;
        DungeonCreator.eventRoomCount++;
    }
    public override void SpawnRoom(DungeonDoor.DoorDirection wantedDir, Transform doorPoint)
    {
        creator.SpawnDungeonPartAlt(creator.hallways, wantedDir, gameObject, doorPoint, RoomTypes.Hallway);
    }
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        creator.eventRoomsThisFloor--;
        DungeonCreator.eventRoomCount--;
    }
}
