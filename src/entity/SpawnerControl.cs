using Godot;
using System;

public class SpawnerControl : Area
{
    [Export]
    public PackedScene entityPrefab;

    [Export]
    private NodePath timerPath;
    public Timer _timer;



    public override void _Ready()
    {    
        _timer = GetNode<Timer>(timerPath) ?? throw new NullReferenceException();
        _timer.Connect("timeout", this, nameof(CheckSpawn));
    }

    public void _HandleBodyExited(Node body) {
        CheckSpawn();
    }

    private void CheckSpawn() {
        var bodies = this.GetOverlappingBodies();
        if(bodies.Count == 0) {
            var newInstance = entityPrefab.Instance<Spatial>();
            newInstance.Translation = Vector3.Zero;
            AddChild(newInstance);
        } else {
            var body = bodies[0] as Spatial;
        }
    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
