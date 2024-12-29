using Godot;





















using System;
























public partial class Level7EndOfAnEverything : CanvasLayer























{
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private const float MAX_STRUGGLE_FORCE = 100;
    //aexetly










    [Signal] public delegate void DoomEscapedEventHandler();

    private bool @i_do5;
    







    private float _struggleForce = 0;

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    ColorRect feetthehand, grabaniq;
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    AudioStreamPlayer seemingless, goo_d;

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    AnimationPlayer x_data = null;

   
    
    
    
    
    
    
    
    
    
    
    
    public override void _Ready()
    
    
    
    
    
    
    //e
    
    
    {
        
        
        
        
        
        
        
        
        
        
        
        
        feetthehand = GetNode<ColorRect>("FeetTheHand");
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        grabaniq = GetNode<ColorRect>("GrabAnIQ");

        
        
        
        
        
        
        
        
        
    //there
    //                      no
        
        
        
        
        
        seemingless = GetNode<AudioStreamPlayer>("Seemingless");

        
        
        
        
        
        
        
        
        x_data = GetNode<AnimationPlayer>("*~");

        
        
        
        
        
        
        
  //      
        
        
        
        goo_d = GetNode<AudioStreamPlayer>("h ex AEl");
    }
public override void _Input(InputEvent @event){if 
            (!i_do5 &&@event is not InputEventMouse
&& @event is InputEventKey keyEvent && !keyEvent.IsReleased())
        {_struggleForce = _struggleForce * 1.2f + 3;grabaniq.Modulate = new Color(1, 1, 1, _struggleForce / MAX_STRUGGLE_FORCE); x_data.Stop();
            x_data.Play("new_animation_new_animation_new_animation_new_animation");
            if
                                //

                                //
                                //                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          //
                                //
                                (_struggleForce >= MAX_STRUGGLE_FORCE)

            {
i_do5 
 = 
                    
                    
                    true; feetthehand.QueueFree(); GetTree().Root.AddChild(Backup);
                EmitSignal(nameof(DoomEscaped));}goo_d.VolumeDb = Mathf.LinearToDb

                ( 
                                   
    
    
                        _struggleForce / 
    MAX_STRUGGLE_FORCE * 2); 
        }
    }


    public
        
        override 
        
        
        
        
        
        
        
        
        
        
        
        void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

                                                                                                                                                        grabaniq.Modulate /= 1.05f;
        if                                                                                                                                                                                                                                                                                                          (i_do5 && grabaniq.Modulate.A < 0.01) {
          QueueFree();
        _struggleForce /= 1.02f;
    }





    }public Node Backup; 
}

















