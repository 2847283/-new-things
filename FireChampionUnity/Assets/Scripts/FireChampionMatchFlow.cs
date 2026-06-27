public struct FireChampionMatchCompletion
{
    public bool playerWon;
    public int tournamentMedalsEarned;
    public string summaryTitle;
    public string summaryDetails;
}

public static class FireChampionMatchFlow
{
    public static FireChampionMatchCompletion ApplyCompletedMatch(ProfileData profile, GameMode mode, int tournamentRound, Side winner, int leftScore, int rightScore, int leftGames, int rightGames, MatchStats stats)
    {
        FireChampionMatchCompletion completion = new FireChampionMatchCompletion();
        completion.playerWon = winner == Side.Left;
        completion.summaryTitle = completion.playerWon ? "胜利！" : "惜败";

        if (completion.playerWon)
        {
            profile.totalWins++;
            if (IsTournamentFinal(mode, tournamentRound))
            {
                profile.tournamentWins++;
            }
        }
        else
        {
            profile.totalLosses++;
        }

        if (mode == GameMode.Tournament)
        {
            bool finalRound = TournamentProgression.IsFinalRound(tournamentRound);
            completion.tournamentMedalsEarned = profile.RecordTournamentMatchResult(
                tournamentRound,
                completion.playerWon,
                finalRound,
                TournamentProgression.RewardMedalsForRound(tournamentRound),
                TournamentProgression.RoundCount);
        }

        ApplyBadges(profile, mode, tournamentRound, completion.playerWon, stats);
        completion.summaryDetails = BuildSummary(profile, mode, tournamentRound, completion.playerWon, completion.tournamentMedalsEarned, leftScore, rightScore, leftGames, rightGames);
        return completion;
    }

    private static void ApplyBadges(ProfileData profile, GameMode mode, int tournamentRound, bool playerWon, MatchStats stats)
    {
        if (playerWon && profile.totalWins == 1)
        {
            profile.badges++;
        }

        int smashWinners = stats == null ? 0 : stats.smashWinners;
        if (smashWinners >= 3)
        {
            profile.badges = UnityEngine.Mathf.Max(profile.badges, 2);
        }

        if (IsTournamentFinal(mode, tournamentRound) && playerWon)
        {
            profile.badges = UnityEngine.Mathf.Max(profile.badges, 3);
        }
    }

    private static string BuildSummary(ProfileData profile, GameMode mode, int tournamentRound, bool playerWon, int medalsEarned, int leftScore, int rightScore, int leftGames, int rightGames)
    {
        if (mode == GameMode.Tournament)
        {
            TournamentOpponent opponent = TournamentProgression.OpponentForRound(tournamentRound);
            return playerWon
                ? TournamentVictorySummary(opponent, tournamentRound, medalsEarned)
                : TournamentLossSummary(profile, opponent, tournamentRound);
        }

        return "比分 " + leftScore + ":" + rightScore + "，局分 " + leftGames + ":" + rightGames + "。";
    }

    private static string TournamentVictorySummary(TournamentOpponent opponent, int tournamentRound, int medalsEarned)
    {
        if (TournamentProgression.IsFinalRound(tournamentRound))
        {
            return "击败 " + opponent.displayName + "，夺得火柴冠军赛冠军！获得 " + medalsEarned + " 枚冠军奖牌。奖牌只记录荣誉/外观，不提升强度。";
        }

        return "战胜 " + opponent.displayName + "，通过 " + TournamentProgression.RoundLabel(tournamentRound) + "。获得 " + medalsEarned + " 枚冠军奖牌，下一轮已解锁。";
    }

    private static string TournamentLossSummary(ProfileData profile, TournamentOpponent opponent, int tournamentRound)
    {
        return "锦标赛止步 " + TournamentProgression.RoundLabel(tournamentRound) + "，负于 " + opponent.displayName + "。历史最远: " + TournamentProgression.ProgressLabel(profile.tournamentBestRoundReached) + "，累计奖牌: " + profile.tournamentMedals + "。";
    }

    private static bool IsTournamentFinal(GameMode mode, int tournamentRound)
    {
        return mode == GameMode.Tournament && TournamentProgression.IsFinalRound(tournamentRound);
    }
}
