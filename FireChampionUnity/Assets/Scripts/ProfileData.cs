using System;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class KeyBinding
{
    public KeyCode left;
    public KeyCode right;
    public KeyCode up;
    public KeyCode down;
    public KeyCode swing;
    public KeyCode skill;

    public static KeyBinding DefaultP1()
    {
        KeyBinding b = new KeyBinding();
        b.left = KeyCode.A;
        b.right = KeyCode.D;
        b.up = KeyCode.W;
        b.down = KeyCode.S;
        b.swing = KeyCode.F;
        b.skill = KeyCode.G;
        return b;
    }

    public static KeyBinding DefaultP2()
    {
        KeyBinding b = new KeyBinding();
        b.left = KeyCode.LeftArrow;
        b.right = KeyCode.RightArrow;
        b.up = KeyCode.UpArrow;
        b.down = KeyCode.DownArrow;
        b.swing = KeyCode.K;
        b.skill = KeyCode.L;
        return b;
    }
}

[Serializable]
public sealed class ProfileData
{
    public string nickname = "Player";
    public KeyBinding p1 = KeyBinding.DefaultP1();
    public KeyBinding p2 = KeyBinding.DefaultP2();
    public int selectedLeftCharacter;
    public int selectedRightCharacter = 1;
    public int selectedLeftCosmetic;
    public int selectedRightCosmetic;
    public int selectedCourt;
    public int totalWins;
    public int totalLosses;
    public int tournamentWins;
    public int tournamentRunsStarted;
    public int tournamentRoundsWon;
    public int tournamentBestRoundReached;
    public int tournamentFinalsReached;
    public int tournamentRunnerUps;
    public int tournamentMedals;
    public bool tournamentRunActive;
    public int tournamentResumeRound;
    public int badges;
    public float masterVolume = 0.8f;
    public bool screenShake = true;
    public bool highContrast;
    public int aiHighSamples;
    public int aiFlatSamples;
    public int aiSmashSamples;
    public float aiHighRatio;
    public float aiFlatRatio;
    public float aiSmashRatio;
    public int practiceSessions;
    public int practiceTargetAttempts;
    public int practiceTargetHits;
    public int practiceBestTargetStreak;
    public int practiceHitContacts;
    public int practiceSweetSpotHits;
    public int practiceSolidHits;
    public int practiceEdgeHits;
    public int practiceBestCleanStreak;

    public void RecordHumanShot(bool isPrimaryPlayer, ShotType shot)
    {
        if (!isPrimaryPlayer)
        {
            return;
        }

        if (shot == ShotType.High) aiHighSamples++;
        else if (shot == ShotType.Smash) aiSmashSamples++;
        else aiFlatSamples++;

        int total = TotalAiSamples();
        if (total > 0)
        {
            aiHighRatio = aiHighSamples / (float)total;
            aiFlatRatio = aiFlatSamples / (float)total;
            aiSmashRatio = aiSmashSamples / (float)total;
        }
    }

    public int TotalAiSamples()
    {
        return aiHighSamples + aiFlatSamples + aiSmashSamples;
    }

    public float DominantShotBias()
    {
        return Mathf.Max(aiHighRatio, Mathf.Max(aiFlatRatio, aiSmashRatio));
    }

    public void ResetAiMemory()
    {
        aiHighSamples = 0;
        aiFlatSamples = 0;
        aiSmashSamples = 0;
        aiHighRatio = 0;
        aiFlatRatio = 0;
        aiSmashRatio = 0;
    }

    public void RecordPracticeSession(PracticeSessionSummary summary)
    {
        if (summary == null || !summary.HasActivity)
        {
            return;
        }

        practiceSessions++;
        practiceTargetAttempts += summary.targetAttempts;
        practiceTargetHits += summary.targetHits;
        practiceBestTargetStreak = Mathf.Max(practiceBestTargetStreak, summary.bestTargetStreak);
        practiceHitContacts += summary.hitContacts;
        practiceSweetSpotHits += summary.sweetSpotHits;
        practiceSolidHits += summary.solidHits;
        practiceEdgeHits += summary.edgeHits;
        practiceBestCleanStreak = Mathf.Max(practiceBestCleanStreak, summary.bestCleanStreak);
    }

    public float PracticeTargetAccuracyPercent()
    {
        return practiceTargetAttempts <= 0 ? 0.0f : practiceTargetHits * 100.0f / practiceTargetAttempts;
    }

    public float PracticeSweetSpotRatePercent()
    {
        return practiceHitContacts <= 0 ? 0.0f : practiceSweetSpotHits * 100.0f / practiceHitContacts;
    }

    public float PracticeCleanRatePercent()
    {
        int cleanHits = practiceSweetSpotHits + practiceSolidHits;
        return practiceHitContacts <= 0 ? 0.0f : cleanHits * 100.0f / practiceHitContacts;
    }

    public void ResetPracticeHistory()
    {
        practiceSessions = 0;
        practiceTargetAttempts = 0;
        practiceTargetHits = 0;
        practiceBestTargetStreak = 0;
        practiceHitContacts = 0;
        practiceSweetSpotHits = 0;
        practiceSolidHits = 0;
        practiceEdgeHits = 0;
        practiceBestCleanStreak = 0;
    }

    public void StartTournamentRun(int roundCount)
    {
        tournamentRunsStarted++;
        tournamentRunActive = roundCount > 0;
        tournamentResumeRound = 0;
        if (roundCount > 0)
        {
            tournamentBestRoundReached = Mathf.Max(tournamentBestRoundReached, 1);
        }
    }

    public bool HasActiveTournamentRun(int roundCount)
    {
        return tournamentRunActive && roundCount > 0 && tournamentResumeRound >= 0 && tournamentResumeRound < roundCount;
    }

    public int CurrentTournamentRound(int roundCount)
    {
        if (roundCount <= 0)
        {
            return 0;
        }

        return Mathf.Clamp(tournamentResumeRound, 0, roundCount - 1);
    }

    public void SetTournamentResumeRound(int roundIndex, int roundCount)
    {
        if (roundCount <= 0)
        {
            ClearTournamentRun();
            return;
        }

        tournamentRunActive = true;
        tournamentResumeRound = Mathf.Clamp(roundIndex, 0, roundCount - 1);
        tournamentBestRoundReached = Mathf.Max(tournamentBestRoundReached, tournamentResumeRound + 1);
    }

    public void ClearTournamentRun()
    {
        tournamentRunActive = false;
        tournamentResumeRound = 0;
    }

    public void NormalizeTournamentRun(int roundCount)
    {
        if (!tournamentRunActive || roundCount <= 0)
        {
            ClearTournamentRun();
            return;
        }

        tournamentResumeRound = Mathf.Clamp(tournamentResumeRound, 0, roundCount - 1);
        tournamentBestRoundReached = Mathf.Max(tournamentBestRoundReached, tournamentResumeRound + 1);
    }

    public int RecordTournamentMatchResult(int roundIndex, bool won, bool finalRound, int medalReward, int roundCount)
    {
        int reachedRound = won && !finalRound ? roundIndex + 2 : roundIndex + 1;
        if (roundCount > 0)
        {
            reachedRound = Mathf.Clamp(reachedRound, 1, roundCount);
        }

        tournamentBestRoundReached = Mathf.Max(tournamentBestRoundReached, reachedRound);

        if (finalRound)
        {
            tournamentFinalsReached++;
            if (!won)
            {
                tournamentRunnerUps++;
            }
        }

        if (!won)
        {
            ClearTournamentRun();
            return 0;
        }

        tournamentRoundsWon++;
        if (finalRound)
        {
            ClearTournamentRun();
        }
        else
        {
            SetTournamentResumeRound(roundIndex + 1, roundCount);
        }

        int earned = Mathf.Max(0, medalReward);
        tournamentMedals += earned;
        return earned;
    }
}

public static class ProfileStore
{
    private const string FileName = "fire_champion_profile.json";

    public static string LastStatus { get; private set; } = "尚未读取档案";

    public static bool LastOperationSucceeded { get; private set; } = true;

    public static string DataDirectory
    {
        get
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "FireChampionRecords"));
        }
    }

    private static string ProfilePath
    {
        get
        {
            return Path.Combine(DataDirectory, FileName);
        }
    }

    public static ProfileData Load()
    {
        string path = ProfilePath;
        if (!File.Exists(path))
        {
            SetStatus(true, "未找到旧档案，已创建新档案");
            return new ProfileData();
        }

        try
        {
            ProfileData data = JsonUtility.FromJson<ProfileData>(File.ReadAllText(path));
            if (data == null)
            {
                data = new ProfileData();
            }

            if (data.p1 == null) data.p1 = KeyBinding.DefaultP1();
            if (data.p2 == null) data.p2 = KeyBinding.DefaultP2();
            SetStatus(true, "档案已读取");
            return data;
        }
        catch (Exception ex)
        {
            SetStatus(false, "档案读取失败，已使用新档案: " + ex.Message);
            return new ProfileData();
        }
    }

    public static bool Save(ProfileData data)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            File.WriteAllText(ProfilePath, JsonUtility.ToJson(data, true));
            SetStatus(true, "档案已保存");
            return true;
        }
        catch (Exception ex)
        {
            SetStatus(false, "档案保存失败: " + ex.Message);
            return false;
        }
    }

    private static void SetStatus(bool succeeded, string message)
    {
        LastOperationSucceeded = succeeded;
        LastStatus = message;
    }
}
