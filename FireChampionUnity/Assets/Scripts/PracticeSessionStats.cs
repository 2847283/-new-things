public sealed class PracticeSessionSummary
{
    public int targetAttempts;
    public int targetHits;
    public int bestTargetStreak;
    public int hitContacts;
    public int sweetSpotHits;
    public int solidHits;
    public int edgeHits;
    public int bestCleanStreak;

    public bool HasActivity
    {
        get { return targetAttempts > 0 || hitContacts > 0; }
    }

    public int CleanHits
    {
        get { return sweetSpotHits + solidHits; }
    }

    public float TargetAccuracyPercent()
    {
        return targetAttempts <= 0 ? 0.0f : targetHits * 100.0f / targetAttempts;
    }

    public float SweetSpotRatePercent()
    {
        return hitContacts <= 0 ? 0.0f : sweetSpotHits * 100.0f / hitContacts;
    }

    public float CleanRatePercent()
    {
        return hitContacts <= 0 ? 0.0f : CleanHits * 100.0f / hitContacts;
    }
}

public sealed class PracticeSessionStats
{
    private int sessionId = 1;
    private int lastCommittedSessionId;

    public void BeginNewSession()
    {
        sessionId++;
    }

    public PracticeSessionSummary Capture(PracticeDrillState drill, HitTimingFeedback timing)
    {
        PracticeSessionSummary summary = new PracticeSessionSummary();
        if (drill != null)
        {
            summary.targetAttempts = drill.Attempts;
            summary.targetHits = drill.Hits;
            summary.bestTargetStreak = drill.BestStreak;
        }

        if (timing != null)
        {
            summary.hitContacts = timing.Hits;
            summary.sweetSpotHits = timing.SweetSpotHits;
            summary.solidHits = timing.SolidHits;
            summary.edgeHits = timing.EdgeHits;
            summary.bestCleanStreak = timing.BestCleanStreak;
        }

        return summary;
    }

    public bool CommitToProfile(ProfileData profile, PracticeDrillState drill, HitTimingFeedback timing)
    {
        if (profile == null || lastCommittedSessionId == sessionId)
        {
            return false;
        }

        PracticeSessionSummary summary = Capture(drill, timing);
        if (!summary.HasActivity)
        {
            return false;
        }

        profile.RecordPracticeSession(summary);
        lastCommittedSessionId = sessionId;
        return true;
    }
}
