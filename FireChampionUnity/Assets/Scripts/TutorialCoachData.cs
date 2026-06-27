using UnityEngine;

public struct TutorialCoachStep
{
    public readonly string title;
    public readonly string instruction;
    public readonly string hint;
    public readonly string stuckHint;
    public readonly string markerLabel;
    public readonly Vector2 markerWorld;
    public readonly float markerRadius;

    public TutorialCoachStep(string title, string instruction, string hint, string stuckHint, string markerLabel, Vector2 markerWorld, float markerRadius)
    {
        this.title = title;
        this.instruction = instruction;
        this.hint = hint;
        this.stuckHint = stuckHint;
        this.markerLabel = markerLabel;
        this.markerWorld = markerWorld;
        this.markerRadius = markerRadius;
    }
}

public static class TutorialCoachData
{
    private static readonly TutorialCoachStep[] Steps =
    {
        new TutorialCoachStep(
            "移动与跳跃",
            "按 A/D 左右移动，并按 W 跳跃。先感受起跳和落地节奏。",
            "A/D 和 W 会实时亮起；先不用管球，确认移动手感。",
            "还没完成移动/跳跃。先踩到脚下亮圈，再按 W 跳一下。",
            "移动区",
            new Vector2(-5.3f, 0.05f),
            0.9f),
        new TutorialCoachStep(
            "发球",
            "等待发球时按 F，把球发到对面。发球成功后进入下一步。",
            "球停在拍前时按 F；如果没出球，等球回到拍前再按。",
            "发球还没出去。看拍前的发球提示，站稳后按 F。",
            "发球点",
            new Vector2(-5.0f, 1.1f),
            0.75f),
        new TutorialCoachStep(
            "挥拍接球",
            "靠近回球时按 F 挥拍。现在有短暂挥拍窗口，早一点点也能接到。",
            "站到球的落点前一点，提前按 F 也会进入短缓冲。",
            "还没击中球。跟着亮圈靠近球路，在球到身前时按 F。",
            "击球窗口",
            new Vector2(-3.0f, 1.15f),
            0.8f),
        new TutorialCoachStep(
            "下压扣杀",
            "球高于头顶时按 S + F，尝试下压扣杀。扣杀更快，但失误风险也更高。",
            "球太低时不会算扣杀，先用 W/F 把球挑高，再按 S+F。",
            "还没打出扣杀。先把球挑高，等球在头顶附近时按住 S 再按 F。",
            "扣杀区",
            new Vector2(-2.2f, 2.15f),
            0.7f),
        new TutorialCoachStep(
            "角色技能",
            "能量条亮起后按 G 使用角色技能。满 3 段时可按住 G 配合扣杀打强化球。",
            "能量满 1 段即可按 G；满 3 段可按住 G 再扣杀。",
            "能量还不够或还没按 G。继续击球攒能量，看到能量条亮起后按 G。",
            "能量/技能",
            new Vector2(-4.6f, 1.65f),
            0.72f)
    };

    public static int StepCount
    {
        get { return Steps.Length; }
    }

    public static TutorialCoachStep Step(int index)
    {
        if (index < 0) return Steps[0];
        if (index >= Steps.Length) return Steps[Steps.Length - 1];
        return Steps[index];
    }
}
