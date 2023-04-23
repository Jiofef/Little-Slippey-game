using Godot;
using System;

public partial class CameraScript : Camera2D
{
    public override void _Ready()
    {
        LimitsChangingBy(0, 0, 0, 0);
    }
    private void LimitsChangingBy(float plus1, float plus2, float plus3, float plus4)
    {
        float[] Defaultlimits = {-25, G.LevelXYSizes[G.CurrentLevel].X + 30, G.LevelXYSizes[G.CurrentLevel].Y + 100, -25};
        LimitTop = (int)(Defaultlimits[0] + plus1);
        LimitRight = (int)(Defaultlimits[1] + plus2);
        LimitBottom = (int)(Defaultlimits[2] + plus3);
        LimitLeft = (int)(Defaultlimits[3] + plus4);
    }
    public override void _Process(double delta)
    {
        float IntPos1 = 0, IntPos2 = 0;
        var restartNoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
        if (G.ResetTimer != 0)
        {
            if (PositionSmoothingEnabled)
                PositionSmoothingEnabled = false;
            if (!restartNoise.IsPlaying())
                restartNoise.Play();
            restartNoise.Modulate = new Color(restartNoise.Modulate.R, restartNoise.Modulate.G, restartNoise.Modulate.B, G.ResetTimer / 2);
            var restartNoiseSound = GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound");
            if (!restartNoiseSound.Playing)
                restartNoiseSound.Play();
            restartNoiseSound.VolumeDb = -5 + G.ResetTimer * 10;

            Random random = new Random();
            float RndPos1 = random.Next(-50, 50) * G.ResetTimer, RndPos2 = random.Next(-50, 50) * G.ResetTimer;
            IntPos1 = RndPos1;
            IntPos2 = RndPos2;
            Position = new Vector2(RndPos1, RndPos2);
            LimitsChangingBy(IntPos1, IntPos1, IntPos2, IntPos2);
        }
        else if (restartNoise.IsPlaying())
        {
            PositionSmoothingEnabled = true;
            var restartnoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
            restartnoise.Stop();
            restartnoise.Modulate = new Color(restartnoise.Modulate.R, restartnoise.Modulate.G, restartnoise.Modulate.B, 0);
            GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound").Stop();
            LimitsChangingBy(0, 0, 0, 0);
            IntPos1 = 0; IntPos2 = 0;
        }

        if (G.PlayerDead)
        {
            float zoom = G.PlayerDeathTimer < 4 ? 1.5f + G.PlayerDeathTimer * 0.75f : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float TimerX50 = G.PlayerDeathTimer * 50;
            LimitsChangingBy(-TimerX50 - IntPos2, TimerX50 + IntPos1, TimerX50 + IntPos2, -TimerX50 - IntPos1);

            if (G.PlayerDeathTimer >= 4.5f)
            {
                var emergingElements = GetNode<Node2D>("GUI/EmergingElements");
                if (!emergingElements.Visible)
                {
                    emergingElements.Visible = true;
                    if (G.DoNewRecordReached)
                    {
                        string link = "GUI/EmergingElements/NewRecordScores";
                        var newRecordScores = GetNode<Label>(link);
                        newRecordScores.Text = "New Record!\nScore: " + (int)G.Scores;
                        newRecordScores.Visible = true;
                        GetNode<CpuParticles2D>(link + "/Particles1").Emitting = true;
                        GetNode<CpuParticles2D>(link + "/Particles2").Emitting = true;
                    }
                    else
                    {
                        var scores = GetNode<Label>("GUI/EmergingElements/Scores");
                        scores.Visible = true;
                        scores.Text = Convert.ToString("Score: " + (int)G.Scores);
                    }
                }
                if (emergingElements.Modulate.A < 1)
                    emergingElements.Modulate = new Color(emergingElements.Modulate.R, emergingElements.Modulate.G, emergingElements.Modulate.B, emergingElements.Modulate.A + 0.005f);
            }
        }
    }
    public void CameraZoom(Area2D junk)
    {
        PositionSmoothingEnabled = false;
        GetNode<Label>("GUI/Scores").Visible = false;
    }
}
