using Godot;

public class BarrierHolderControl : Spatial
{
    public override void _Ready()
    {
        RecurseAndSetStaticBodyLayers(this, (1<<0|1<<1|1<<2));
    }

    private void RecurseAndSetStaticBodyLayers(Node parent, uint layerMask) {
        if(parent is StaticBody stat) {
            stat.CollisionLayer = layerMask;
            stat.CollisionMask = layerMask;
        }

        foreach(var c in parent.GetChildren()) {
            if(c is Node n) {
                RecurseAndSetStaticBodyLayers(n, layerMask);
            }
        }
    }
}
