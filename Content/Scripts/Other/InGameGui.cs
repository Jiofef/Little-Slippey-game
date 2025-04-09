using Godot;
using System;
using System.Diagnostics;

public partial class InGameGui : Control
{
    // Nodes
    public StandBar StandBar;
    public Label Scores;

    // After death gui nodes
    public Control AfterDeathGui;
    public Label AfterDeathScores;
    public Label NewRecordLabel;
    public RichTextLabel HoldRText;
    public MenuOnlyTextButton ReturnToMenuButton;
    public ConfirmMenuOnlyTextButton ResurrectButton;

    public class GuiOptions
    {
        // Main GUI
        public bool DisableScoresLabel, DisableStandBar;

        // After death GUI
        public bool DisableAfterDeathGui, DisableAfterDeathScoresLabel, DisableNewRecordLabel, DisableHoldRText, DisableReturnToMenuButton, DisableResurrectButton;
    }

    public GuiOptions Options = new();


    public override void _Ready()
    {
        // Initializing the nodes
        StandBar = GetNode<StandBar>("StandBar");

        Scores = GetNode<Label>("Scores");

        AfterDeathGui = GetNode<Control>("AfterDeathElements");
        AfterDeathScores = GetNode<Label>("AfterDeathElements/Scores");
        NewRecordLabel = GetNode<Label>("AfterDeathElements/NewRecord");
        HoldRText = GetNode<RichTextLabel>("AfterDeathElements/HoldR");
        ReturnToMenuButton = GetNode<MenuOnlyTextButton>("AfterDeathElements/Buttons/ReturnToMenu");
        ResurrectButton = GetNode<ConfirmMenuOnlyTextButton>("AfterDeathElements/Buttons/Resurrect");

        ResurrectButton.OnBought += BuyAResurrection;

        UpdateAllTheOptions();
    }


    private bool _isScoreDisabled = false;
    public override void _PhysicsProcess(double delta)
    {
        if (Scores.Visible)
        {
            Scores.Text = ((int)G.Scores).ToString();
            if (Meta.Instance.Video.ScoresLabelLocationY == 0)
                Scores.Modulate = new Color(Scores.Modulate.R, Scores.Modulate.G, Scores.Modulate.B, G.Player.Position.Y > G.CameraLimits.Position.Y + 200 ? 1 : G.Player.Position.Y / (G.CameraLimits.Position.Y + 200));

            if (_isScoreDisabled != G.IsProgressPaused)
            {
                _isScoreDisabled = G.IsProgressPaused;
                if (_isScoreDisabled)
                    Scores.Modulate = new Color(0.6f, 0.6f, 0.6f);
                else
                    Scores.Modulate = new Color(1, 1, 1);
            }
        }
    }


    public void OnPlayerDead()
    {
        UpdateAfterDeathGui();

        Options.DisableScoresLabel = true;
        UpdateScoresOptions();

        Options.DisableStandBar = true;
        UpdateStandingBarOptions();

        if (G.Player.DisableAfterDeathGui) return;

        G.ShowMouseDuringGameplay++;
        G.AdditionalGuiLayer.AlwaysShowGoldenCrossesAmount = G.Player.ShowGoldenCrossesAmountAfterDeath;

        if (G.IsNewRecordReached)
        {
            NewRecordLabel.GetNode<AnimationPlayer>("AnimationPlayer").Play("Appearing");
        }

        ResurrectButton.Disabled = false;


        UpdateAfterDeathGuiOptions(true);

        var animationPlayer = GetNode<AnimationPlayer>("GuiAnimations");
        animationPlayer.Play("OnDeath");
    }

    public void OnPlayerResurrected()
    {
        Options.DisableScoresLabel = false;
        UpdateScoresOptions();

        Options.DisableStandBar = !G.Player.EnableStandingPenalty;
        UpdateStandingBarOptions();

        G.ShowMouseDuringGameplay--;
        G.AdditionalGuiLayer.AlwaysShowGoldenCrossesAmount = false;

        UpdateAfterDeathGuiOptions(false);
    }

    public void UpdateAfterDeathGui()
    {
        // Scores
        AfterDeathScores.Text = Tr("Score: ") + ((int)G.Scores).ToString();

        // When level is completed
        if (G.HasLevelBeenCompleted)
        {
            NewRecordLabel.Text = Tr("A new level is open!");
        }

        // Hold R text
        void SetHoldIMG(string ImageName)
        {
            HoldRText.Text = Tr("HoldR Part1") + "[img={width}96{height}]res://Content/Sprites/Interface/ControllerButtons/" + ImageName + ".png[/img]" + Tr("HoldR Part2");
        }
        switch (G.TypeOfUsedController)
        {
            case "Keyboard":
                SetHoldIMG("KeyboardButtonBigR");
                break;
            case "PS Gamepad":
                SetHoldIMG("PSControllerBigTriangle");
                break;
            default:
                SetHoldIMG("XControllerBigY");
                break;
        }

        // Resurrection
        int cost = G.GetResurrectionCost();
        ResurrectButton.Text = cost.ToString() + " [img]res://Content/Sprites/Interface/4XMiniGoldenCross.png[/img]\r\n[center]" + Tr("Resurrect");
        ResurrectButton.ConfirmationText = cost.ToString() + " [img]res://Content/Sprites/Interface/4XMiniGoldenCross.png[/img]\r\n[center]" + Tr("Click again to purchase");
        ResurrectButton.BuyingPrice = cost;
    }

    public void FocusAfterDeathButton()
    {
        foreach(MenuOnlyTextButton button in AfterDeathGui.GetNode("Buttons").GetChildren())
        {
            // Under focus should be the first visible button
            if (button.Visible)
            {
                button.GrabFocus();
                break;
            }
        }
    }

    public void BuyAResurrection()
    {
        G.ResurrectionsInARow++;

        G.Player.Resurrect();
    }

    public void ReturnToMenu()
    {
        var pause = G.Main.GetNodeOrNull<Pause>("Pause");
        if (pause == null) return;

        pause.Menu();
    }



    #region Updating the elements options
    public void UpdateAllTheOptions()
    {
        UpdateScoresOptions();

        UpdateStandingBarOptions();
        UpdateAfterDeathGuiOptions(G.IsPlayerDead);
        UpdateAfterDeathGui();
        UpdateAfterDeathScoresLabelOptions();
        UpdateNewRecordLabelOptions();
        UpdateHoldRTextOptions();
        UpdateReturnToMenuButtonOptions();
        UpdateResurrectButtonOptions();
    }

    public void UpdateScoresOptions()
    {
        Scores.Visible = !Options.DisableScoresLabel && Meta.Instance.Video.ScoresShowingFormatIndex != 2; // Second is the hide scores option

        int scoresFontSize = Meta.Instance.Video.ScoresShowingFormatIndex == 0 ? 132 : 88;
        Scores.AddThemeFontSizeOverride("font_size", scoresFontSize);

        Scores.HorizontalAlignment = (HorizontalAlignment)Meta.Instance.Video.ScoresLabelLocationX;
        Scores.VerticalAlignment = (VerticalAlignment)Meta.Instance.Video.ScoresLabelLocationY;
    }

    public void UpdateStandingBarOptions()
    {
        GetNode<StandBar>("StandBar").Visible = !Options.DisableStandBar;
    }

    public void UpdateAfterDeathGuiOptions(bool isDead = false)
    {
        GetNode<Control>("AfterDeathElements").Visible = !Options.DisableAfterDeathGui && isDead;
    }
    public void UpdateAfterDeathScoresLabelOptions()
    {
        GetNode<Label>("AfterDeathElements/Scores").Visible = !Options.DisableAfterDeathScoresLabel;
    }
    public void UpdateNewRecordLabelOptions()
    {
        GetNode<Label>("AfterDeathElements/NewRecord").Visible = !Options.DisableNewRecordLabel;
    }
    public void UpdateHoldRTextOptions()
    {
        GetNode<RichTextLabel>("AfterDeathElements/HoldR").Visible = !Options.DisableHoldRText;
    }
    public void UpdateReturnToMenuButtonOptions()
    {
        GetNode<MenuOnlyTextButton>("AfterDeathElements/Buttons/ReturnToMenu").Visible = !Options.DisableReturnToMenuButton;
    }
    public void UpdateResurrectButtonOptions()
    {
        GetNode<ConfirmMenuOnlyTextButton>("AfterDeathElements/Buttons/Resurrect").Visible = !Options.DisableResurrectButton;
    }
    #endregion
}