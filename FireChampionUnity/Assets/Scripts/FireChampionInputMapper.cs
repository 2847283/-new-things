using UnityEngine;

public struct TutorialInputState
{
    public bool moving;
    public bool jumping;
    public bool pressingDown;
    public bool swinging;
    public bool usingSkill;
}

public static class FireChampionInputMapper
{
    public const string P1Left = "p1.left";
    public const string P1Right = "p1.right";
    public const string P1Up = "p1.up";
    public const string P1Down = "p1.down";
    public const string P1Swing = "p1.swing";
    public const string P1Skill = "p1.skill";
    public const string P2Left = "p2.left";
    public const string P2Right = "p2.right";
    public const string P2Up = "p2.up";
    public const string P2Down = "p2.down";
    public const string P2Swing = "p2.swing";
    public const string P2Skill = "p2.skill";

    public static GameInput Read(KeyBinding binding)
    {
        GameInput input = new GameInput();
        input.horizontal = (Input.GetKey(binding.right) ? 1 : 0) - (Input.GetKey(binding.left) ? 1 : 0);
        input.vertical = (Input.GetKey(binding.up) ? 1 : 0) - (Input.GetKey(binding.down) ? 1 : 0);
        input.jumpPressed = Input.GetKeyDown(binding.up);
        input.swingPressed = Input.GetKeyDown(binding.swing);
        input.skillPressed = Input.GetKeyDown(binding.skill);
        input.skillHeld = Input.GetKey(binding.skill);
        return input;
    }

    public static TutorialInputState ReadTutorialState(KeyBinding binding)
    {
        TutorialInputState state = new TutorialInputState();
        state.moving = Input.GetKey(binding.left) || Input.GetKey(binding.right);
        state.jumping = Input.GetKey(binding.up);
        state.pressingDown = Input.GetKey(binding.down);
        state.swinging = Input.GetKey(binding.swing);
        state.usingSkill = Input.GetKey(binding.skill);
        return state;
    }

    public static bool TryGetRebindKey(Event currentEvent, out KeyCode key)
    {
        key = KeyCode.None;
        if (currentEvent == null || currentEvent.type != EventType.KeyDown)
        {
            return false;
        }

        key = currentEvent.keyCode;
        return key != KeyCode.None;
    }

    public static bool Assign(ProfileData profile, string target, KeyCode key)
    {
        if (profile == null)
        {
            return false;
        }

        if (profile.p1 == null) profile.p1 = KeyBinding.DefaultP1();
        if (profile.p2 == null) profile.p2 = KeyBinding.DefaultP2();

        if (target == P1Left) profile.p1.left = key;
        else if (target == P1Right) profile.p1.right = key;
        else if (target == P1Up) profile.p1.up = key;
        else if (target == P1Down) profile.p1.down = key;
        else if (target == P1Swing) profile.p1.swing = key;
        else if (target == P1Skill) profile.p1.skill = key;
        else if (target == P2Left) profile.p2.left = key;
        else if (target == P2Right) profile.p2.right = key;
        else if (target == P2Up) profile.p2.up = key;
        else if (target == P2Down) profile.p2.down = key;
        else if (target == P2Swing) profile.p2.swing = key;
        else if (target == P2Skill) profile.p2.skill = key;
        else return false;

        return true;
    }
}
