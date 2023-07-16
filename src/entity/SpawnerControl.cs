using Godot;

public class SpawnerControl : Area
{
    [Export]
    public PackedScene entityPrefab;

    public void _HandleTimerTimeout() {
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
}
