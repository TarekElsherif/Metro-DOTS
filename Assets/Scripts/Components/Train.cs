using Unity.Entities;
using Unity.Mathematics;

public struct Train : IComponentData
{
    public int DestinationIndex;
    public float3 Destination;
    public RailwayPointType DestinationType;
    public float DistanceToNextTrain;
    public Entity MetroLine;
}

public struct TrainIndexOnMetroLine : IComponentData
{
    public int IndexOnMetroLine;
    public int AmountOfTrainsOnMetroLine;
}

public struct UniqueTrainID : IComponentData
{
    public int ID;
    public int NextTrainID;
}