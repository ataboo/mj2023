using System;
using System.Linq;
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

    public ARProgressGroupControl CreateProgressBar(Spatial target, string[] labels) {
        var newGroup = barPrefab.Instance<ARProgressGroupControl>();
        AddChild(newGroup);
        newGroup.SetTarget(target);
        newGroup.SetBarCount(labels.Length);
        newGroup.SetLabels(labels);
        newGroup.SetProgresses(Enumerable.Repeat(0f, labels.Length).ToArray());

        return newGroup;
    }
}
