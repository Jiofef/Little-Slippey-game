using Godot;
using System;

public partial class AdditionalGuiLayer : CanvasLayer
{
    // Object with this script should be only 1 on the scene, otherwise the previous ones will stop working.


    public override void _Ready()
	{
        Achievements.CurrentAdditionalGuiLayer = this;
	}

    public override void _ExitTree()
    {
        if (Achievements.CurrentAdditionalGuiLayer == this)
        {
            Achievements.CurrentAdditionalGuiLayer = null;
        }
    }

    public void OnGoldenCrossesRecieved()
    {
        // Updating visible amount
        GetNode<Label>("ScreenControl/MarginC/GoldenCrossesAmount/HBoxContainer/AmountLabel").Text = UnchangableMeta.GoldenCrossesAmount.ToString();

        // Effects when amount gets visible
        //GetNode<AudioStreamPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/OnGoldenCrossesRecievedSound").Play();
        GetNode<AnimationPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/AnimationPlayer").Play("Disappearing");
    }
}
