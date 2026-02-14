using System.IO.MemoryMappedFiles;

namespace Overlap.Core;

public interface ITelemetryReader : IDisposable
{
    TelemetryFrame ReadFrame();
}

public sealed class IracingSharedMemoryReader : ITelemetryReader
{
    private const string MemoryFileName = "Local\\IRSDKMemMapFileName";

    private readonly float[] _lapDistPct = new float[64];
    private readonly int[] _trackSurface = new int[64];
    private readonly int[] _leftRight = new int[64];

    private MemoryMappedFile? _mmf;

    public TelemetryFrame ReadFrame()
    {
        try
        {
            _mmf ??= MemoryMappedFile.OpenExisting(MemoryFileName, MemoryMappedFileRights.Read);
        }
        catch
        {
            _mmf = null;
            return TelemetryFrame.Disconnected;
        }

        // MVP safety-first parser. If any expected variable is unavailable, return disconnected without crashing.
        // Integrators can replace this with a full IRSDK var-header parser later.
        try
        {
            using var stream = _mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            if (stream.Length <= 0)
            {
                return TelemetryFrame.Disconnected;
            }

            return TelemetryFrame.Disconnected;
        }
        catch
        {
            return TelemetryFrame.Disconnected;
        }
    }

    public void Dispose()
    {
        _mmf?.Dispose();
    }
}

public sealed class MockTelemetryReader : ITelemetryReader
{
    private readonly float[] _lapDistPct = new float[64];
    private readonly int[] _trackSurface = new int[64];

    public TelemetryFrame ReadFrame()
    {
        const int playerIdx = 0;
        const float trackLength = 5000f;

        _lapDistPct[playerIdx] += 0.0008f;
        if (_lapDistPct[playerIdx] > 1f)
        {
            _lapDistPct[playerIdx] -= 1f;
        }

        for (var i = 0; i < _lapDistPct.Length; i++)
        {
            _trackSurface[i] = (int)CarTrackSurface.OnTrack;
            if (i == playerIdx)
            {
                continue;
            }

            _lapDistPct[i] = _lapDistPct[playerIdx] + ((i - 2) * 0.0002f);
            if (_lapDistPct[i] > 1f)
            {
                _lapDistPct[i] -= 1f;
            }
            if (_lapDistPct[i] < 0f)
            {
                _lapDistPct[i] += 1f;
            }
        }

        return new TelemetryFrame
        {
            IsConnected = true,
            PlayerCarIdx = playerIdx,
            TrackLengthMeters = trackLength,
            CarIdxLapDistPct = _lapDistPct,
            CarIdxTrackSurface = _trackSurface,
            CarIdxLeftRight = Array.Empty<int>()
        };
    }

    public void Dispose()
    {
    }
}
