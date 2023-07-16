using System;
using Godot;

public class DragonMouthControl: Area {
    
    [Export]
    NodePath dragonControlPath;
    
    [Export]
    NodePath followTargetPath;
    private Spatial _followTarget;

    public override void _Ready()
    {
        _followTarget = GetNode<Spatial>(followTargetPath) ?? throw new NullReferenceException();
    }

    public override void _Process(float delta)
    {
        GlobalTranslation = _followTarget.GlobalTranslation;
    }

    private DragonControl _dragonControl;
    public DragonControl DragonControl {
        get {
            if(_dragonControl == null) {
                _dragonControl = GetNode<DragonControl>(dragonControlPath) ?? throw new NullReferenceException();
            }

            return _dragonControl;
        }   
    }

}