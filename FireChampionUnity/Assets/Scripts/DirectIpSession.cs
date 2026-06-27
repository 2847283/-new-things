using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public sealed class DirectIpSession
{
    private readonly object sync = new object();
    private readonly NetworkDiagnostics diagnostics = new NetworkDiagnostics();
    private readonly TimeSpan pingInterval;
    private readonly TimeSpan pingRetryInterval;
    private TcpListener listener;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread thread;
    private bool running;
    private volatile bool connected;
    private int nextPingId = 1;
    private int pendingPingId;
    private long pendingPingTicks;
    private DateTime nextPingUtc;
    private string latestSnapshot = "";
    private string status = "未连接";
    private GameInput remoteInput;

    public DirectIpSession()
        : this(1.0f, 2.5f)
    {
    }

    public DirectIpSession(float pingIntervalSeconds, float pingRetrySeconds)
    {
        float normalizedInterval = Math.Max(0.1f, pingIntervalSeconds);
        float normalizedRetry = Math.Max(normalizedInterval + 0.1f, pingRetrySeconds);
        pingInterval = TimeSpan.FromSeconds(normalizedInterval);
        pingRetryInterval = TimeSpan.FromSeconds(normalizedRetry);
    }

    public NetworkDiagnostics Diagnostics
    {
        get { return diagnostics; }
    }

    public string Status
    {
        get
        {
            lock (sync)
            {
                return status;
            }
        }

        private set
        {
            lock (sync)
            {
                status = value;
            }
        }
    }

    public GameInput RemoteInput
    {
        get
        {
            lock (sync)
            {
                return remoteInput;
            }
        }

        private set
        {
            lock (sync)
            {
                remoteInput = value;
            }
        }
    }

    public bool IsConnected
    {
        get { return connected; }
    }

    public void Host(int port)
    {
        Stop();
        diagnostics.Reset();
        ResetPingState();
        running = true;
        connected = false;
        Status = "等待连接 0.0.0.0:" + port;
        thread = new Thread(delegate()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                client = listener.AcceptTcpClient();
                AttachStreams();
                connected = true;
                diagnostics.MarkConnected();
                Status = "已连接客机";
                ReadLoop();
            }
            catch (Exception e)
            {
                Status = "主机错误: " + e.Message;
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    public void Join(string ip, int port)
    {
        Stop();
        diagnostics.Reset();
        ResetPingState();
        running = true;
        connected = false;
        Status = "连接中 " + ip + ":" + port;
        thread = new Thread(delegate()
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                AttachStreams();
                connected = true;
                diagnostics.MarkConnected();
                Status = "已连接主机";
                ReadLoop();
            }
            catch (Exception e)
            {
                Status = "连接失败: " + e.Message;
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    public void Stop()
    {
        diagnostics.Reset();
        ResetPingState();
        running = false;
        connected = false;
        try { if (client != null) client.Close(); } catch { }
        try { if (listener != null) listener.Stop(); } catch { }
        lock (sync)
        {
            client = null;
            listener = null;
            reader = null;
            writer = null;
            latestSnapshot = "";
            remoteInput = new GameInput();
        }

        Status = "未连接";
    }

    public void SendInput(GameInput input)
    {
        SendLine("I|" + input.Encode(), NetworkPacketKind.Input);
    }

    public void SendSnapshot(GameSnapshot snapshot)
    {
        SendLine("S|" + snapshot.Encode(), NetworkPacketKind.Snapshot);
    }

    public void Tick()
    {
        if (!connected)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        bool shouldSend;
        int pingId;
        long pingTicks;
        lock (sync)
        {
            if (nextPingUtc == DateTime.MinValue)
            {
                nextPingUtc = now;
            }

            bool pendingExpired = pendingPingId != 0 && new DateTime(pendingPingTicks, DateTimeKind.Utc) + pingRetryInterval <= now;
            shouldSend = now >= nextPingUtc && (pendingPingId == 0 || pendingExpired);
            if (!shouldSend)
            {
                return;
            }

            pingId = nextPingId++;
            pingTicks = now.Ticks;
            pendingPingId = pingId;
            pendingPingTicks = pingTicks;
            nextPingUtc = now + pingInterval;
        }

        SendLine("P|" + pingId + "|" + pingTicks, NetworkPacketKind.Ping);
    }

    public bool TryGetSnapshot(out GameSnapshot snapshot)
    {
        snapshot = null;
        string packet;
        lock (sync)
        {
            packet = latestSnapshot;
            latestSnapshot = "";
        }

        if (string.IsNullOrEmpty(packet))
        {
            return false;
        }

        return GameSnapshot.TryDecode(packet, out snapshot);
    }

    private void AttachStreams()
    {
        client.NoDelay = true;
        client.ReceiveTimeout = 30000;
        client.SendTimeout = 30000;
        NetworkStream stream = client.GetStream();
        StreamReader nextReader = new StreamReader(stream);
        StreamWriter nextWriter = new StreamWriter(stream);
        nextWriter.AutoFlush = true;
        lock (sync)
        {
            reader = nextReader;
            writer = nextWriter;
        }
    }

    private void ReadLoop()
    {
        while (running && client != null && client.Connected)
        {
            StreamReader activeReader;
            lock (sync)
            {
                activeReader = reader;
            }

            if (activeReader == null)
            {
                break;
            }

            string line = activeReader.ReadLine();
            if (line == null)
            {
                break;
            }

            if (line.StartsWith("I|", StringComparison.Ordinal))
            {
                RemoteInput = GameInput.Decode(line.Substring(2));
                diagnostics.MarkReceived(NetworkPacketKind.Input);
            }
            else if (line.StartsWith("S|", StringComparison.Ordinal))
            {
                lock (sync)
                {
                    latestSnapshot = line.Substring(2);
                }
                diagnostics.MarkReceived(NetworkPacketKind.Snapshot);
            }
            else if (line.StartsWith("P|", StringComparison.Ordinal))
            {
                diagnostics.MarkReceived(NetworkPacketKind.Ping);
                SendPong(line.Substring(2));
            }
            else if (line.StartsWith("O|", StringComparison.Ordinal))
            {
                diagnostics.MarkReceived(NetworkPacketKind.Pong);
                MarkPong(line.Substring(2));
            }
        }

        Status = "连接断开，可返回菜单重连";
        connected = false;
        running = false;
    }

    private void SendLine(string line, NetworkPacketKind kind)
    {
        try
        {
            StreamWriter w;
            lock (sync)
            {
                w = writer;
            }

            if (w != null)
            {
                w.WriteLine(line);
                diagnostics.MarkSent(kind);
            }
        }
        catch (Exception e)
        {
            Status = "发送失败: " + e.Message;
        }
    }

    private void ResetPingState()
    {
        lock (sync)
        {
            nextPingId = 1;
            pendingPingId = 0;
            pendingPingTicks = 0;
            nextPingUtc = DateTime.MinValue;
        }
    }

    private void SendPong(string pingPayload)
    {
        if (string.IsNullOrEmpty(pingPayload))
        {
            return;
        }

        SendLine("O|" + pingPayload, NetworkPacketKind.Pong);
    }

    private void MarkPong(string payload)
    {
        string[] parts = payload.Split('|');
        if (parts.Length < 2)
        {
            return;
        }

        int pingId;
        long sentTicks;
        if (!int.TryParse(parts[0], out pingId) || !long.TryParse(parts[1], out sentTicks))
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        lock (sync)
        {
            if (pendingPingId != 0 && pingId == pendingPingId)
            {
                pendingPingId = 0;
                pendingPingTicks = 0;
            }
        }

        double milliseconds = new TimeSpan(Math.Max(0, now.Ticks - sentTicks)).TotalMilliseconds;
        diagnostics.MarkRoundTrip(milliseconds);
    }
}
