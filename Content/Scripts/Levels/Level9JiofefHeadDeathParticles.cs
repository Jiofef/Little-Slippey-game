using Godot;

public partial class Level9JiofefHeadDeathParticles : CpuParticles2D
{
    public void OnTimerTimeout()
    {
        G.CrossSpawnMultiplier = 1;
        QueueFree();
    }
    public void ChaosIsOver()
    {
        GetNode<AudioStreamPlayer>("Chaos").QueueFree();
        GetNode<AudioStreamPlayer>("ChaosIsOver").Play();
    }
}
