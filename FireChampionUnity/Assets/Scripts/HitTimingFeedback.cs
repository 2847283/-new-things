using UnityEngine;

public sealed class HitTimingFeedback
{
    public int Hits { get; private set; }
    public int SweetSpotHits { get; private set; }
    public int SolidHits { get; private set; }
    public int EdgeHits { get; private set; }
    public int CleanStreak { get; private set; }
    public int BestCleanStreak { get; private set; }
    public int LastQualityPercent { get; private set; }
    public string LastFeedback { get; private set; } = "等待第一次击球";
    public string LastHint { get; private set; } = "尽量让球靠近拍心再按挥拍。";

    public void Reset()
    {
        Hits = 0;
        SweetSpotHits = 0;
        SolidHits = 0;
        EdgeHits = 0;
        CleanStreak = 0;
        BestCleanStreak = 0;
        LastQualityPercent = 0;
        LastFeedback = "等待第一次击球";
        LastHint = "尽量让球靠近拍心再按挥拍。";
    }

    public void RecordHit(float distanceFromRacket, float hitRadius, ShotType shot, bool goodTiming)
    {
        Hits++;
        float normalized = hitRadius <= 0.01f ? 0.0f : Mathf.Clamp01(1.0f - distanceFromRacket / hitRadius);
        LastQualityPercent = Mathf.RoundToInt(normalized * 100.0f);

        if (distanceFromRacket <= hitRadius * 0.28f)
        {
            SweetSpotHits++;
            CleanStreak++;
            LastFeedback = "甜区命中 · " + ShotLabel(shot) + " · 质量 " + LastQualityPercent + "%";
            LastHint = "节奏很好。保持站位，让下一拍也落在拍心附近。";
        }
        else if (goodTiming)
        {
            SolidHits++;
            CleanStreak++;
            LastFeedback = "稳定命中 · " + ShotLabel(shot) + " · 质量 " + LastQualityPercent + "%";
            LastHint = "命中可靠。再早半步站位，能更容易打出甜区。";
        }
        else
        {
            EdgeHits++;
            CleanStreak = 0;
            LastFeedback = "擦边命中 · " + ShotLabel(shot) + " · 质量 " + LastQualityPercent + "%";
            LastHint = "球离拍心偏远。提前移动到落点，或稍晚一点挥拍。";
        }

        BestCleanStreak = Mathf.Max(BestCleanStreak, CleanStreak);
    }

    public float SweetSpotRate()
    {
        return Hits <= 0 ? 0.0f : SweetSpotHits * 100.0f / Hits;
    }

    public float CleanRate()
    {
        return Hits <= 0 ? 0.0f : (SweetSpotHits + SolidHits) * 100.0f / Hits;
    }

    private static string ShotLabel(ShotType shot)
    {
        if (shot == ShotType.High) return "高远/挑高";
        if (shot == ShotType.Drop) return "短吊";
        if (shot == ShotType.Smash) return "扣杀";
        return "平抽";
    }
}
