using Godot;
using System;
using System.Reflection.Metadata;
using static Godot.Mathf;

public partial class RandomVectorTest : Node2D
{
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

    public int amount = 300;
    public float size = 200, circleSize = 50, circlesSizes = 5;

    public Vector2 CirclePos = new Vector2(100, 0);

    public Color circleColor = new Color(1, 1, 1);
    public override void _Draw()
    {
        for (int i = 0; i < amount; i++)
            DrawCircle(OtherExtension.RandomTools.RandomVectorAt(new Vector2(200, 200), new Vector2(250, 500),(int)circleSize), circlesSizes, circleColor);
    }

    public void SetAmount(float value)
    {
        amount = (int)value;
    }

    public void SetSize(float value)
    {
        size = value;
    }

    public void SetCircleSize(float value)
    {
        circleSize = value;
    }

    public void SetCirclesSizes(float value)
    { circlesSizes = value; }

    public void SetCirclePosX(float value)
    {
        CirclePos.X = value;
    }

    public void SetCirclePosY(float value)
    {
        CirclePos.Y = value;
    }

    public void SetCircleColor(Color value)
    {
        circleColor = value;
    }
}
