using Godot;
using System;

public class SpinDoughControl : Spatial
{
    [Export]
    NodePath doughMeshPath;
    MeshInstance _doughMesh;

    private float _maxXZScale = 2.2f;
    private float _minYScale = 0.2f;

    public override void _Ready()
    {
        _doughMesh = GetNode<MeshInstance>(doughMeshPath) ?? throw new NullReferenceException();
    }

    public void SetStretchProgress(float t) {
        t = Mathf.Clamp(t, 0f, 1f);

        var xzScale = Mathf.Lerp(1, _maxXZScale, t);
        var yScale = Mathf.Lerp(1, _minYScale, t);
        Scale = new Vector3(xzScale, yScale, xzScale);
    }
}
