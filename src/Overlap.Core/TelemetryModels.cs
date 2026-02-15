namespace Overlap.Core;

public enum CarTrackSurface
{
    NotInWorld = -1,
    OffTrack = 0,
    InPitStall = 1,
    ApproachingPits = 2,
    OnTrack = 3
}

public readonly record struct CarProximity(int CarIdx, float DeltaMeters, bool IsDanger, int? LeftRight);

public sealed class TelemetryFrame
{
    public bool IsConnected { get; init; }
    public int PlayerCarIdx { get; init; }
    public float TrackLengthMeters { get; init; }
    public ReadOnlyMemory<float> CarIdxLapDistPct { get; init; }
    public ReadOnlyMemory<int> CarIdxTrackSurface { get; init; }
    public ReadOnlyMemory<int> CarIdxLeftRight { get; init; }

    public static TelemetryFrame Disconnected => new()
    {
        IsConnected = false,
        PlayerCarIdx = -1,
        TrackLengthMeters = 0,
        CarIdxLapDistPct = ReadOnlyMemory<float>.Empty,
        CarIdxTrackSurface = ReadOnlyMemory<int>.Empty,
        CarIdxLeftRight = ReadOnlyMemory<int>.Empty
    };
}
