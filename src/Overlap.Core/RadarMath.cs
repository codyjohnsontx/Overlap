namespace Overlap.Core;

public static class RadarMath
{
    public const float RadarRangeMeters = 10f;
    public const float DangerThresholdMeters = 2f;

    public static float WrapLapDifference(float deltaPct)
    {
        if (deltaPct > 0.5f)
        {
            deltaPct -= 1f;
        }
        else if (deltaPct < -0.5f)
        {
            deltaPct += 1f;
        }

        return deltaPct;
    }

    public static float DeltaMeters(float myPct, float otherPct, float trackLengthMeters)
    {
        var wrappedPct = WrapLapDifference(otherPct - myPct);
        return wrappedPct * trackLengthMeters;
    }

    public static bool IsDanger(float deltaMeters) => MathF.Abs(deltaMeters) < DangerThresholdMeters;
}
