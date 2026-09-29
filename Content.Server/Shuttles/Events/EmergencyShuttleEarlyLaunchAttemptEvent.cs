namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised before an early emergency shuttle launch is authorized.
/// </summary>
[ByRefEvent]
public record struct EmergencyShuttleEarlyLaunchAttemptEvent
{
    public EntityUid? Station;
    public EntityUid? Shuttle;
    public bool Cancelled;

    public EmergencyShuttleEarlyLaunchAttemptEvent(EntityUid? station, EntityUid? shuttle, bool cancelled)
    {
        Station = station;
        Shuttle = shuttle;
        Cancelled = cancelled;
    }

    public EmergencyShuttleEarlyLaunchAttemptEvent(EntityUid? station, EntityUid? shuttle)
        : this(station, shuttle, false)
    {
    }

    public EmergencyShuttleEarlyLaunchAttemptEvent(bool cancelled)
        : this(null, null, cancelled)
    {
    }
}

[ByRefEvent]
public record struct EmergencyShuttleLaunchAttemptEvent(EntityUid? Station, EntityUid? Shuttle)
{
    public bool Cancelled;
}
