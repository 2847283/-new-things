public static class FireChampionMatchRules
{
    public static bool IsGameWon(RulesetConfig rules, int score, int opponentScore)
    {
        if (rules == null)
        {
            return false;
        }

        if (score >= rules.hardCap)
        {
            return true;
        }

        return score >= rules.pointsToWin && score - opponentScore >= rules.winBy;
    }

    public static int GamesNeededToWinMatch(RulesetConfig rules)
    {
        if (rules == null)
        {
            return 1;
        }

        int bestOf = rules.bestOf < 1 ? 1 : rules.bestOf;
        return bestOf / 2 + 1;
    }

    public static bool ShouldStartNextGame(RulesetConfig rules, int leftGames, int rightGames)
    {
        if (rules == null || rules.bestOf <= 1)
        {
            return false;
        }

        int currentLeaderGames = leftGames > rightGames ? leftGames : rightGames;
        return currentLeaderGames < GamesNeededToWinMatch(rules);
    }

    public static bool IsServeFault(int rallyHits, Side scorer, Side lastHitter)
    {
        return rallyHits == 0 && scorer != lastHitter;
    }
}
