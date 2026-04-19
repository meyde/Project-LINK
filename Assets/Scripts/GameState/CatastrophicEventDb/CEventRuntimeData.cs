using UnityEngine;
using Unity.Netcode;
using System;

public partial struct CEventRuntimeData : INetworkSerializable, IEquatable<CEventRuntimeData>
{
    public int eventId;
    public int state; //0: not started, 1: occuring, 2:succeeded, 3: failed
    public int modulesDone; //counter
    public int currentLevel; // 1 to 3 depending on the level of modules needed to do. 
    public int eventLives;
    public int module1Option;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref eventId);
        serializer.SerializeValue(ref state);
        serializer.SerializeValue(ref modulesDone);
        serializer.SerializeValue(ref currentLevel);
        serializer.SerializeValue(ref eventLives);
        serializer.SerializeValue(ref module1Option);

    }

    public bool Equals(CEventRuntimeData other)
    {
        bool isEqual = eventId == other.eventId &&
            state == other.state &&
            modulesDone == other.modulesDone &&
            currentLevel == other.currentLevel &&
            eventLives == other.eventLives && 
            module1Option == other.module1Option;

        return isEqual;
    }

}
