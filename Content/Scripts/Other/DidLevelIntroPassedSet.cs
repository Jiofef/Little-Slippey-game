using Godot;
using System;

public partial class DidLevelIntroPassedSet : AnimationPlayer
{
    public void Set(bool value)
    {
        G.DidLevelIntroPassed = value;
    }
}
