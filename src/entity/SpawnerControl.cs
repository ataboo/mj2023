using Godot;
using System;

public class SpawnerControl : Area
{
    [Export]
    public PackedScene entityPrefab;



    public override void _Ready()
    {    
        CallDeferred(nameof(CheckSpawn));
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
        }
    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
