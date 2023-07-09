using Godot;
using System;

public class MechLegControl : Spatial
{
    [Export]
    public NodePath animationTreePath;
    private AnimationTree _tree;

    private AnimationNodeStateMachinePlayback _stateMachine;

    [Export]
    NodePath playerControlPath;
    private PlayerControl _playerControl;

    [Export]
    NodePath footColliderPath;
    private KinematicBody _footCollider;

    private CollisionShape _footColShape;

    private PhysicsShapeQueryParameters _shapeQueryParams;

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback");
        _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
        _footCollider = GetNode<KinematicBody>(footColliderPath) ?? throw new NullReferenceException();
        _footColShape = _footCollider.GetChild<CollisionShape>(0) ?? throw new NullReferenceException();

        _shapeQueryParams = new PhysicsShapeQueryParameters {
            ShapeRid = _footColShape.Shape.GetRid(),
            Transform = _footColShape.GlobalTransform,
            CollisionMask = 1U<<1,
            Exclude = new Godot.Collections.Array(_footCollider),
        };

        _playerControl.Connect(nameof(PlayerControl.OnLegStateChange), this, nameof(_HandleLegStateChange));
    }

    public void _HandleLegStateChange() {

        var mechState = _playerControl.MechState;
        _tree.Set("parameters/conditions/kick", mechState.legKick);
        _tree.Set("parameters/conditions/not_kick", !mechState.legKick);
        _tree.Set("parameters/conditions/windup", mechState.legWindUp);
        _tree.Set("parameters/conditions/not_windup", !mechState.legWindUp);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(_stateMachine.GetCurrentNode() == "KickStrike") {
            var progress = _stateMachine.GetCurrentPlayPosition() / _stateMachine.GetCurrentLength();
            if(progress < 0.3f) {

                _shapeQueryParams.Transform = _footColShape.GlobalTransform;
                var spaceState = GetWorld().DirectSpaceState;
                var results = spaceState.GetRestInfo(_shapeQueryParams);

                if(results.Contains("collider_id")) {
                    var colliderId = (ulong)(int)results["collider_id"];
                    var instance = GD.InstanceFromId(colliderId);

                    if(instance is GridMap gridMap) {
                        _playerControl.QueueImpulse(-GlobalTransform.basis.z * 1);       
                    }
                }
            }

            // GD.InstanceFromId()
        }
    }
}
