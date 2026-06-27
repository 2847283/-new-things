using System.Collections.Generic;
using UnityEngine;

public sealed class PracticeShotTimelineEntry
{
    public int sequence;
    public ShotType shot;
    public int qualityPercent;
    public string contactFeedback = "";
    public string contactHint = "";
    public bool targetEvaluated;
    public bool targetHit;
    public float targetMissMeters;

    public string BuildLine()
    {
        string result = targetEvaluated ? (targetHit ? "落点命中" : "偏差 " + targetMissMeters.ToString("0.0") + "m") : "等待落点";
        return "#" + sequence + " " + ShotLabel(shot) + " · 质量 " + qualityPercent + "% · " + result;
    }

    private static string ShotLabel(ShotType shot)
    {
        if (shot == ShotType.High) return "高远";
        if (shot == ShotType.Drop) return "短吊";
        if (shot == ShotType.Smash) return "扣杀";
        return "平抽";
    }
}

public sealed class PracticeShotTimeline
{
    private readonly List<PracticeShotTimelineEntry> entries = new List<PracticeShotTimelineEntry>();
    private int nextSequence = 1;

    public IReadOnlyList<PracticeShotTimelineEntry> Entries
    {
        get { return entries; }
    }

    public int Count
    {
        get { return entries.Count; }
    }

    public void Reset()
    {
        entries.Clear();
        nextSequence = 1;
    }

    public void RecordContact(ShotType shot, int qualityPercent, string contactFeedback, string contactHint, int capacity)
    {
        PracticeShotTimelineEntry entry = new PracticeShotTimelineEntry
        {
            sequence = nextSequence++,
            shot = shot,
            qualityPercent = Mathf.Clamp(qualityPercent, 0, 100),
            contactFeedback = contactFeedback ?? "",
            contactHint = contactHint ?? ""
        };

        entries.Insert(0, entry);
        Trim(capacity);
    }

    public void RecordTargetResult(bool targetHit, float targetMissMeters)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (!entries[i].targetEvaluated)
            {
                entries[i].targetEvaluated = true;
                entries[i].targetHit = targetHit;
                entries[i].targetMissMeters = Mathf.Max(0.0f, targetMissMeters);
                return;
            }
        }
    }

    private void Trim(int capacity)
    {
        int safeCapacity = Mathf.Clamp(capacity, 1, 24);
        while (entries.Count > safeCapacity)
        {
            entries.RemoveAt(entries.Count - 1);
        }
    }
}
