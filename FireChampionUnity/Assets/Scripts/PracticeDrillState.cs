using UnityEngine;

public sealed class PracticeDrillState
{
    public float TargetX { get; private set; } = 4.6f;
    public float TargetRadius { get; private set; } = 0.85f;
    public int Hits { get; private set; }
    public int Attempts { get; private set; }
    public int Streak { get; private set; }
    public int BestStreak { get; private set; }
    public string LastFeedback { get; private set; } = "";
    public bool LastTargetHit { get; private set; }
    public float LastTargetMiss { get; private set; }
    public float LastLandingX { get; private set; }

    public void Reset(float courtHalfWidth)
    {
        Hits = 0;
        Attempts = 0;
        Streak = 0;
        BestStreak = 0;
        LastFeedback = "";
        LastTargetHit = false;
        LastTargetMiss = 0.0f;
        LastLandingX = 0.0f;
        RandomizeTarget(courtHalfWidth);
    }

    public void RandomizeTarget(float courtHalfWidth)
    {
        TargetX = UnityEngine.Random.Range(2.0f, courtHalfWidth - 1.0f);
    }

    public bool TryEvaluateLanding(float shuttleX, Side lastHitter, float courtHalfWidth)
    {
        if (lastHitter != Side.Left)
        {
            return false;
        }

        Attempts++;
        LastLandingX = shuttleX;
        float miss = Mathf.Abs(shuttleX - TargetX);
        LastTargetMiss = miss;
        if (shuttleX > 0 && miss <= TargetRadius)
        {
            Hits++;
            Streak++;
            BestStreak = Mathf.Max(BestStreak, Streak);
            RandomizeTarget(courtHalfWidth);
            LastTargetHit = true;
            LastFeedback = "落点命中！连续 " + Streak + "，新目标已刷新";
        }
        else
        {
            Streak = 0;
            LastTargetHit = false;
            LastFeedback = "落点偏差 " + miss.ToString("0.0") + " 米，调整拍面和力度";
        }

        return true;
    }
}
