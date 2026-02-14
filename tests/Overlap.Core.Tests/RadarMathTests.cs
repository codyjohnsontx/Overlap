using Overlap.Core;

namespace Overlap.Core.Tests;

public sealed class RadarMathTests
{
    [Theory]
    [InlineData(0.75f, -0.25f)]
    [InlineData(-0.75f, 0.25f)]
    [InlineData(0.2f, 0.2f)]
    public void WrapLapDifference_WrapsIntoExpectedRange(float input, float expected)
    {
        var actual = RadarMath.WrapLapDifference(input);

        Assert.Equal(expected, actual, 3);
    }

    [Fact]
    public void DeltaMeters_UsesTrackLengthWithWrap()
    {
        var delta = RadarMath.DeltaMeters(myPct: 0.98f, otherPct: 0.01f, trackLengthMeters: 5000f);

        Assert.Equal(150f, delta, 2);
    }

    [Theory]
    [InlineData(1.9f, true)]
    [InlineData(-1.99f, true)]
    [InlineData(2.0f, false)]
    [InlineData(5.0f, false)]
    public void IsDanger_UsesStrictThreshold(float deltaMeters, bool expected)
    {
        Assert.Equal(expected, RadarMath.IsDanger(deltaMeters));
    }
}
