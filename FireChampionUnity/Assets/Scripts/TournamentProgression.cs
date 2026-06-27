public static class TournamentProgression
{
    private static readonly TournamentOpponent[] Opponents =
    {
        new TournamentOpponent("屋顶快手", "预选赛", "速度型对手，会主动抢平抽节奏。", 0.74f, 1),
        new TournamentOpponent("道场重炮", "半决赛", "力量型对手，喜欢等待高球后下压。", 0.86f, 2),
        new TournamentOpponent("未来幻线", "冠军战", "控球型对手，会更频繁地调动落点。", 0.98f, 5)
    };

    public static int RoundCount
    {
        get { return Opponents.Length; }
    }

    public static TournamentOpponent OpponentForRound(int round)
    {
        return Opponents[ClampRound(round)];
    }

    public static string RoundLabel(int round)
    {
        TournamentOpponent opponent = OpponentForRound(round);
        return opponent.roundName + " " + (ClampRound(round) + 1) + "/" + Opponents.Length;
    }

    public static bool IsFinalRound(int round)
    {
        return ClampRound(round) >= Opponents.Length - 1;
    }

    public static float DifficultyForRound(int round)
    {
        return OpponentForRound(round).difficulty;
    }

    public static int RewardMedalsForRound(int round)
    {
        return OpponentForRound(round).rewardMedals;
    }

    public static string RewardTextForRound(int round)
    {
        int reward = RewardMedalsForRound(round);
        return "+" + reward + " 冠军奖牌";
    }

    public static string ProgressLabel(int reachedRoundCount)
    {
        if (reachedRoundCount <= 0)
        {
            return "未开始";
        }

        int index = ClampRound(reachedRoundCount - 1);
        TournamentOpponent opponent = OpponentForRound(index);
        if (reachedRoundCount >= Opponents.Length)
        {
            return opponent.roundName;
        }

        return opponent.roundName;
    }

    private static int ClampRound(int round)
    {
        if (round < 0) return 0;
        if (round >= Opponents.Length) return Opponents.Length - 1;
        return round;
    }
}

public struct TournamentOpponent
{
    public readonly string displayName;
    public readonly string roundName;
    public readonly string scoutingReport;
    public readonly float difficulty;
    public readonly int rewardMedals;

    public TournamentOpponent(string displayName, string roundName, string scoutingReport, float difficulty, int rewardMedals)
    {
        this.displayName = displayName;
        this.roundName = roundName;
        this.scoutingReport = scoutingReport;
        this.difficulty = difficulty;
        this.rewardMedals = rewardMedals;
    }
}
