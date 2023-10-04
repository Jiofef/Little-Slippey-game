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
        LimitsChangingBy(true, 0, 0, 0, 0);
        ResetSmoothing();

        _restartNoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
        _player = GetNode<CharacterBody2D>("..");
        _scores = GetNode<Label>("GUI/Scores");

        SetZoom(new Vector2(Meta.Instance.CameraZoom, Meta.Instance.CameraZoom));
        _scores.Visible = Meta.Instance.ScoresShowingFormatIndex == 0;
    }
    private void LimitsChangingBy(bool DoResetSmoothing = false, float plus1 = 0, float plus2 = 0, float plus3 = 0, float plus4 = 0)
    {
        float[] Defaultlimits = {G.CameraLimits.X - 30, G.CameraLimits.Y + 30, G.CameraLimits.Z + 100, G.CameraLimits.W - 30};
        LimitTop = (int)(Defaultlimits[0] + plus1);
        LimitRight = (int)(Defaultlimits[1] + plus2);
        LimitBottom = (int)(Defaultlimits[2] + plus3);
        LimitLeft = (int)(Defaultlimits[3] + plus4);

        if (DoResetSmoothing)
            ResetSmoothing();

    }
    public override void _PhysicsProcess(double delta)
    {
        if (_scores.Visible)
            _scores.Text = ((int)G.Scores).ToString();
        float IntPos1 = 0, IntPos2 = 0;
        if (G.ResetTimer != 0)
        {
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
            LimitsChangingBy(true, IntPos1, IntPos1, IntPos2, IntPos2);
        }
        else if (_restartNoise.IsPlaying())
        {
            var restartnoise = GetNode<AnimatedSprite2D>("GUI/RestartNoise");
            restartnoise.Stop();
            restartnoise.Modulate = new Color(restartnoise.Modulate.R, restartnoise.Modulate.G, restartnoise.Modulate.B, 0);
            GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound").Stop();
            LimitsChangingBy(false, 0, 0, 0, 0);
            IntPos1 = 0; IntPos2 = 0;
        }

        if (G.IsPlayerDead)
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float TimerX50 = G.PlayerCorpseFlightTimer * 50;
            LimitsChangingBy(false, -TimerX50 - IntPos2, TimerX50 + IntPos1, TimerX50 + IntPos2, -TimerX50 - IntPos1);

            if (G.PlayerCorpseFlightTimer >= 4.5f && !G._isLevel10Finaling)
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
            _scores.Modulate = new Color(_scores.Modulate.R, _scores.Modulate.G, _scores.Modulate.B, _player.Position.Y > G.CameraLimits.X + 200 ? 1 : _player.Position.Y / (G.CameraLimits.X + 200));

    }
    public void CameraZoom()
    {
        PositionSmoothingEnabled = false;
        _scores.Visible = false;
    }

    public void SetZoom(Vector2 value)
    {
        Zoom = value;
    }

    public void OptionsChanged()
    {
        float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.CameraZoom) / 4) : 4.5f;
        Zoom = new Vector2(zoom, zoom);
        _scores.Visible = Meta.Instance.ScoresShowingFormatIndex == 0;
    }
}
