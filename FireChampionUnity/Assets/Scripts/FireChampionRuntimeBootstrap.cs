using UnityEngine;

public static class FireChampionRuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Bootstrap()
    {
        if (UnityEngine.Object.FindAnyObjectByType<FireChampionGame>() != null)
        {
            return;
        }

        GameObject game = new GameObject("FireChampionGame");
        game.AddComponent<FireChampionGame>();
    }
}
