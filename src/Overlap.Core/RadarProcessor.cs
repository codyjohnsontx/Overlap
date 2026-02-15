namespace Overlap.Core;

public sealed class RadarProcessor
{
    private readonly CarProximity[] _buffer;

    public RadarProcessor(int maxCars = 6)
    {
        if (maxCars <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCars));
        }

        MaxCars = maxCars;
        _buffer = new CarProximity[maxCars];
    }

    public int MaxCars { get; }

    public ReadOnlySpan<CarProximity> ComputeClosest(TelemetryFrame frame)
    {
        if (!frame.IsConnected || frame.PlayerCarIdx < 0 || frame.TrackLengthMeters <= 0)
        {
            return ReadOnlySpan<CarProximity>.Empty;
        }

        var lapPcts = frame.CarIdxLapDistPct.Span;
        var surfaces = frame.CarIdxTrackSurface.Span;
        var leftRight = frame.CarIdxLeftRight.Span;

        if (frame.PlayerCarIdx >= lapPcts.Length || frame.PlayerCarIdx >= surfaces.Length)
        {
            return ReadOnlySpan<CarProximity>.Empty;
        }

        var myPct = lapPcts[frame.PlayerCarIdx];
        var count = 0;

        for (var i = 0; i < lapPcts.Length && i < surfaces.Length; i++)
        {
            if (i == frame.PlayerCarIdx)
            {
                continue;
            }

            if ((CarTrackSurface)surfaces[i] != CarTrackSurface.OnTrack)
            {
                continue;
            }

            var deltaMeters = RadarMath.DeltaMeters(myPct, lapPcts[i], frame.TrackLengthMeters);
            if (MathF.Abs(deltaMeters) > RadarMath.RadarRangeMeters)
            {
                continue;
            }

            var item = new CarProximity(i, deltaMeters, RadarMath.IsDanger(deltaMeters), i < leftRight.Length ? leftRight[i] : null);
            InsertByDistance(item, ref count);
        }

        return _buffer.AsSpan(0, count);
    }

    private void InsertByDistance(CarProximity candidate, ref int count)
    {
        var absDistance = MathF.Abs(candidate.DeltaMeters);
        var limit = Math.Min(count, MaxCars - 1);

        var insertIdx = 0;
        while (insertIdx < count && insertIdx < MaxCars && MathF.Abs(_buffer[insertIdx].DeltaMeters) <= absDistance)
        {
            insertIdx++;
        }

        if (insertIdx >= MaxCars)
        {
            return;
        }

        for (var i = limit; i >= insertIdx; i--)
        {
            if (i + 1 < MaxCars)
            {
                _buffer[i + 1] = _buffer[i];
            }
        }

        _buffer[insertIdx] = candidate;
        if (count < MaxCars)
        {
            count++;
        }
    }
}
