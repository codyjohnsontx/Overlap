using Overlap.Core;

namespace Overlap.Core.Tests;

public sealed class RadarProcessorTests
{
    [Fact]
    public void ComputeClosest_FiltersOnTrackAndLimitsCount()
    {
        var lapPct = new float[10];
        var surface = new int[10];
        Array.Fill(surface, (int)CarTrackSurface.OnTrack);

        lapPct[0] = 0.5f; // player
        for (var i = 1; i < 10; i++)
        {
            lapPct[i] = 0.5f + i * 0.0002f;
        }

        surface[9] = (int)CarTrackSurface.OffTrack;

        var frame = new TelemetryFrame
        {
            IsConnected = true,
            PlayerCarIdx = 0,
            TrackLengthMeters = 5000,
            CarIdxLapDistPct = lapPct,
            CarIdxTrackSurface = surface,
            CarIdxLeftRight = Array.Empty<int>()
        };

        var processor = new RadarProcessor(maxCars: 6);
        var result = processor.ComputeClosest(frame);

        Assert.True(result.Length <= 6);
        Assert.All(result.ToArray(), car => Assert.True(MathF.Abs(car.DeltaMeters) <= RadarMath.RadarRangeMeters));
        Assert.DoesNotContain(result.ToArray(), car => car.CarIdx == 9);
    }
}
