using Godot;
using System;

public partial class SavePage : Control
{
    public LevelEditor L;

    public void SetCrutch(int index, bool value)
    {
        L._editorCrutches[index] = value;
    }
    public void Leave()
    {
        G.CompletelyResetValues();
        L.Leave();
    }

    public void TestLevel()
    {
        L.TestLevel();
    }
    public void SaveLevel()
    {
        L.SaveLevel();
    }
}
