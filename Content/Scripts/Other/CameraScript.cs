using Godot;
using System;
using System.Drawing.Drawing2D;

public class CameraScript : Camera2D
{
    private void LimitsSetting(int plus1, int plus2, int plus3, int plus4)
    {
        int[] Defaultlimits = {0, 1024, 600, 0};
        LimitTop = Defaultlimits[0] + plus1;
        LimitRight = Defaultlimits[1] + plus2;
        LimitBottom = Defaultlimits[2] + plus3;
        LimitLeft = Defaultlimits[3] + plus4;
    }
    public override void _Process(float delta)
    {
        int IntPos1 = 0, IntPos2 = 0;
        var restartNoise = GetNode<AnimatedSprite>("GUI/RestartNoise");
        if (G._resettimer != 0)
        {
            if (SmoothingEnabled)
                SmoothingEnabled = false;
            if (!restartNoise.Playing)
                restartNoise.Play();
            restartNoise.Modulate = new Color(restartNoise.Modulate.r, restartNoise.Modulate.g, restartNoise.Modulate.b, G._resettimer / 2);
            var restartNoiseSound = GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound");
            if (!restartNoiseSound.Playing)
                restartNoiseSound.Play();
            restartNoiseSound.VolumeDb = -5 + G._resettimer * 10;

            Random random = new Random();
            float RndPos1 = random.Next(-50, 50) * G._resettimer, RndPos2 = random.Next(-50, 50) * G._resettimer;
            IntPos1 = (int)RndPos1;
            IntPos2 = (int)RndPos2;
            Position = new Vector2(RndPos1, RndPos2);
            LimitsSetting(IntPos1, IntPos1, IntPos2, IntPos2);
        }
        else if (restartNoise.Playing)
        {
            SmoothingEnabled = true;
            var restartnoise = GetNode<AnimatedSprite>("GUI/RestartNoise");
            restartnoise.Stop();
            restartnoise.Modulate = new Color(restartnoise.Modulate.r, restartnoise.Modulate.g, restartnoise.Modulate.b, 0);
            GetNode<AudioStreamPlayer>("GUI/RestartNoise/Sound").Stop();
            LimitsSetting(0, 0, 0, 0);
            IntPos1 = 0; IntPos2 = 0;
        }

        if (!G._playerdead) return;

        float zoom = G._playerdeathtimer < 4 ? 0.7f - G._playerdeathtimer * 0.1f : 0.3f;
        Zoom = new Vector2(zoom, zoom);
        int TimerX50 = (int)(G._playerdeathtimer * 50);
        LimitsSetting(-TimerX50 - IntPos2, TimerX50 + IntPos1, TimerX50 + IntPos2, -TimerX50 - IntPos1);

        if (G._playerdeathtimer >= 4.5f)
        {
            var emergingElements = GetNode<Node2D>("GUI/EmergingElements");
            if (!emergingElements.Visible) 
            {
                emergingElements.Visible = true;
                GetNode<RichTextLabel>("GUI/EmergingElements/FinalScore/RichTextLabel").Text = Convert.ToString((int)G._scores);
            }
            if (emergingElements.Modulate.a < 1)
                emergingElements.Modulate = new Color(emergingElements.Modulate.r, emergingElements.Modulate.g, emergingElements.Modulate.b, emergingElements.Modulate.a + 0.005f);
        }
    }
    public void CameraZoom()
    {
        G._playerdead = true;
        SmoothingEnabled = false;
        GetNode<Label>("GUI/Scores").Visible = false;
    }
}
