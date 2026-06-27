using System;

public enum NetworkPacketKind
{
    Input,
    Snapshot,
    Ping,
    Pong
}

public sealed class NetworkDiagnostics
{
    private readonly object sync = new object();
    private DateTime startedUtc;
    private DateTime connectedUtc;
    private DateTime lastSentUtc;
    private DateTime lastReceivedUtc;
    private int sentInputs;
    private int sentSnapshots;
    private int sentPings;
    private int sentPongs;
    private int receivedInputs;
    private int receivedSnapshots;
    private int receivedPings;
    private int receivedPongs;
    private double lastRoundTripMs = -1.0;
    private double bestRoundTripMs = -1.0;
    private double worstRoundTripMs = -1.0;

    public void Reset()
    {
        lock (sync)
        {
            startedUtc = DateTime.UtcNow;
            connectedUtc = DateTime.MinValue;
            lastSentUtc = DateTime.MinValue;
            lastReceivedUtc = DateTime.MinValue;
            sentInputs = 0;
            sentSnapshots = 0;
            sentPings = 0;
            sentPongs = 0;
            receivedInputs = 0;
            receivedSnapshots = 0;
            receivedPings = 0;
            receivedPongs = 0;
            lastRoundTripMs = -1.0;
            bestRoundTripMs = -1.0;
            worstRoundTripMs = -1.0;
        }
    }

    public void MarkConnected()
    {
        lock (sync)
        {
            connectedUtc = DateTime.UtcNow;
        }
    }

    public void MarkSent(NetworkPacketKind kind)
    {
        lock (sync)
        {
            lastSentUtc = DateTime.UtcNow;
            if (kind == NetworkPacketKind.Input) sentInputs++;
            else if (kind == NetworkPacketKind.Snapshot) sentSnapshots++;
            else if (kind == NetworkPacketKind.Ping) sentPings++;
            else if (kind == NetworkPacketKind.Pong) sentPongs++;
        }
    }

    public void MarkReceived(NetworkPacketKind kind)
    {
        lock (sync)
        {
            lastReceivedUtc = DateTime.UtcNow;
            if (kind == NetworkPacketKind.Input) receivedInputs++;
            else if (kind == NetworkPacketKind.Snapshot) receivedSnapshots++;
            else if (kind == NetworkPacketKind.Ping) receivedPings++;
            else if (kind == NetworkPacketKind.Pong) receivedPongs++;
        }
    }

    public void MarkRoundTrip(double milliseconds)
    {
        lock (sync)
        {
            lastRoundTripMs = Math.Max(0.0, milliseconds);
            if (bestRoundTripMs < 0.0 || lastRoundTripMs < bestRoundTripMs) bestRoundTripMs = lastRoundTripMs;
            if (worstRoundTripMs < 0.0 || lastRoundTripMs > worstRoundTripMs) worstRoundTripMs = lastRoundTripMs;
        }
    }

    public string BuildSummary(bool connected)
    {
        lock (sync)
        {
            DateTime now = DateTime.UtcNow;
            string connectedFor = connectedUtc == DateTime.MinValue ? "--" : SecondsSince(connectedUtc, now) + "s";
            string lastSent = lastSentUtc == DateTime.MinValue ? "--" : SecondsSince(lastSentUtc, now) + "s";
            string lastReceived = lastReceivedUtc == DateTime.MinValue ? "--" : SecondsSince(lastReceivedUtc, now) + "s";
            string roundTrip = lastRoundTripMs < 0.0 ? "--" : Math.Round(lastRoundTripMs).ToString("0") + "ms";
            string roundTripRange = bestRoundTripMs < 0.0 ? "--" : Math.Round(bestRoundTripMs).ToString("0") + "-" + Math.Round(worstRoundTripMs).ToString("0") + "ms";
            string warning = "";
            if (connected && lastReceivedUtc != DateTime.MinValue && SecondsSince(lastReceivedUtc, now) >= 3)
            {
                warning = "\n提示: 超过 3 秒未收到对端数据，可能卡顿或断线。";
            }

            return "连接时长: " + connectedFor
                + "\n发送 输入/快照: " + sentInputs + "/" + sentSnapshots + " · 最近发送: " + lastSent
                + "\n接收 输入/快照: " + receivedInputs + "/" + receivedSnapshots + " · 最近接收: " + lastReceived
                + "\n延迟 RTT: " + roundTrip + " · 范围: " + roundTripRange + " · Ping/Pong: " + sentPings + "/" + receivedPongs
                + warning;
        }
    }

    private static int SecondsSince(DateTime thenUtc, DateTime nowUtc)
    {
        return Math.Max(0, (int)(nowUtc - thenUtc).TotalSeconds);
    }
}
