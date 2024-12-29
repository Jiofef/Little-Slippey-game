using Godot;
using Godot.Collections;
using System;

public partial class GlitchLevelScript : BaseLevelScript
{
    private Player _player;
    private MainScript _main;
    public override void _Ready()
    {
        base._Ready();

        //Initialising nodes
        _player = GetNode<Player>("Player");
        _main = GetNode<MainScript>("..");

        //Removing transitive values from level 10
        G.TransitiveVariant[0] = ""; 
        G.TransitiveObject[0] = null;
        G.TransitiveObject[1] = null;

        //Loading saved values from dictionary
        float savedScores = (float)G.TakeAndRemoveFromTrVaD("SavedScores");
        G.Scores = savedScores;

        Vector2 playerSavedPos = (Vector2)G.TakeAndRemoveFromTrVaD("PlayerSavedPos");
        _player.Position = playerSavedPos;

        //Binding important events
        _main.OnLevelResetting += OnLevelReset;
    }

    public void OnLevelReset()
    {
        G.TransitiveVariantD.Add("ImFromLevel000000000", true);
    }

}
