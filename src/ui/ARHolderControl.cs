using System;
using Godot;

public class ARHolderControl : Control
{
    [Export]
    PackedScene barPrefab;

    [Export]
    NodePath cameraPath;

    private Camera _camera;

    public override void _Ready()
    {
        _camera = GetNode<Camera>(cameraPath) ?? throw new NullReferenceException();
    }

    public ARProgressControl CreateProgressBar(Spatial target) {
        var instance = barPrefab.Instance<ARProgressControl>();
        instance.SetTarget(target, _camera);
        AddChild(instance);

        return instance;
    }
}
