using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[WithAll(typeof(Passenger))]
partial struct PassengerMovementJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public BufferLookup<Waypoint> WaypointLookup;
    public float DeltaTime;

    void Execute(ref PassengerAspect passenger)
    {
        if (!WaypointLookup.IsBufferEnabled(passenger.Self))
            return;
        if (passenger.State == PassengerState.Idle || passenger.State == PassengerState.InQueue || passenger.State == PassengerState.Seated)
            return;
        WaypointLookup.TryGetBuffer(passenger.Self, out var waypoints);
        if (waypoints.Length == 0)
        {
            if (passenger.State == PassengerState.OffBoarding)
                passenger.State = PassengerState.Idle;
            else
                passenger.State++;
            if (passenger.State != PassengerState.InQueue && passenger.State != PassengerState.Seated)
                WaypointLookup.SetBufferEnabled(passenger.Self, false);
            return;
        }

        float3 destination = waypoints[0].Value;
        var direction = destination - passenger.Position;
        var distance = math.lengthsq(direction);
        if (distance > 0.02f)
        {
            var nextSuggestedPosition = passenger.Position + math.normalize(direction) * (DeltaTime * passenger.Speed);
            var distanceToNextPosition = math.distancesq(passenger.Position, nextSuggestedPosition);
            if (distanceToNextPosition < distance)
                passenger.Position = nextSuggestedPosition;
            else
                passenger.Position = destination;
        }
        else
            waypoints.RemoveAt(0);
    }
}

[BurstCompile]
[RequireMatchingQueriesForUpdate]
partial struct PassengerMovementSystem : ISystem
{
    BufferLookup<Waypoint> m_WaypointLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_WaypointLookup = state.GetBufferLookup<Waypoint>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_WaypointLookup.Update(ref state);
        float dt = SystemAPI.Time.DeltaTime;
        var movementJob = new PassengerMovementJob
        {
            WaypointLookup = m_WaypointLookup,
            DeltaTime = dt
        };
        movementJob.ScheduleParallel();
    }
}