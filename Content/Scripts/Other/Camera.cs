using Godot;
using System;

public partial class Camera : Camera2D
{
    AnimatedSprite2D _restartNoise;
    CharacterBody2D _player;
    Label _scores;
    public override void _Ready()
    {
        G.CameraLimits = new Vector4(0, G.LevelXYSizes[G.CurrentLevel].X, G.LevelXYSizes[G.CurrentLevel].Y, 0);
        LimitsChangingBy(0, 0, 0, 0);
        _restartNoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
        _player = GetNode<CharacterBody2D>("..");
        _scores = GetNode<Label>("GUI/Scores");

        if (G.CurrentLevel == 9)
            Zoom = new Vector2(1.2f, 1.2f);
    }
    private void LimitsChangingBy(float plus1, float plus2, float plus3, float plus4)
    {
        float[] Defaultlimits = {G.CameraLimits.X - 30, G.CameraLimits.Y + 30, G.CameraLimits.Z + 100, G.CameraLimits.W - 30};
        LimitTop = (int)(Defaultlimits[0] + plus1);
        LimitRight = (int)(Defaultlimits[1] + plus2);
        LimitBottom = (int)(Defaultlimits[2] + plus3);
        LimitLeft = (int)(Defaultlimits[3] + plus4);
    }
    public override void _PhysicsProcess(double delta)
    {
        float IntPos1 = 0, IntPos2 = 0;
        if (G.ResetTimer != 0)
        {
            if (PositionSmoothingEnabled)
                PositionSmoothingEnabled = false;
            if (!_restartNoise.IsPlaying())
                _restartNoise.Play();
            _restartNoise.Modulate = new Color(_restartNoise.Modulate.R, _restartNoise.Modulate.G, _restartNoise.Modulate.B, G.ResetTimer / 2);
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
        else if (_restartNoise.IsPlaying())
        {
            PositionSmoothingEnabled = true;
            var restartnoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
            restartnoise.Stop();
            restartnoise.Modulate = new Color(restartnoise.Modulate.R, restartnoise.Modulate.G, restartnoise.Modulate.B, 0);
            GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound").Stop();
            LimitsChangingBy(0, 0, 0, 0);
            IntPos1 = 0; IntPos2 = 0;
        }

        if (G.IsPlayerDead)
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? 1.5f + G.PlayerCorpseFlightTimer * 0.75f : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float TimerX50 = G.PlayerCorpseFlightTimer * 50;
            LimitsChangingBy(-TimerX50 - IntPos2, TimerX50 + IntPos1, TimerX50 + IntPos2, -TimerX50 - IntPos1);

            if (G.PlayerCorpseFlightTimer >= 4.5f)
            {
                var emergingElements = GetNode<Node2D>("GUI/EmergingElements");
                if (!emergingElements.Visible)
                {
                    emergingElements.Visible = true;
                    if (G.IsNewRecordReached)
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
                        var emergingScores = GetNode<Label>("GUI/EmergingElements/Scores");
                        emergingScores.Visible = true;
                        emergingScores.Text = Convert.ToString("Score: " + (int)G.Scores);
                    }
                }
                if (emergingElements.Modulate.A < 1)
                    emergingElements.Modulate = new Color(emergingElements.Modulate.R, emergingElements.Modulate.G, emergingElements.Modulate.B, emergingElements.Modulate.A + 0.005f);
            }
        }
        else
            _scores.Modulate = new Color(_scores.Modulate.R, _scores.Modulate.G, _scores.Modulate.B, _player.Position.Y > 200 ? 1 : _player.Position.Y / 200);

    }
    public void CameraZoom()
    {
        PositionSmoothingEnabled = false;
        _scores.Visible = false;
    }
}
