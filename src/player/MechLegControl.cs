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

    private bool _kickArmed = false;

    [Export]
    public float kickForce = 250f;

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
        var currentState = _stateMachine.GetCurrentNode();

        if(_kickArmed && currentState == "KickStrike") {
            var progress = _stateMachine.GetCurrentPlayPosition() / _stateMachine.GetCurrentLength();
            if(progress < 0.3f) {

                _shapeQueryParams.Transform = _footColShape.GlobalTransform;
                var spaceState = GetWorld().DirectSpaceState;
                var results = spaceState.GetRestInfo(_shapeQueryParams);

                if(results.Contains("collider_id")) {
                    var colliderId = (ulong)(int)results["collider_id"];
                    var point = (Vector3)results["point"];
                    var linearVelocity = (Vector3)results["linear_velocity"];
                    var instance = GD.InstanceFromId(colliderId);

                    if(instance is GridMap gridMap) {
                        _playerControl.QueueImpulse(-GlobalTransform.basis.z * kickForce);       
                        _kickArmed = false;
                    } else if(instance is IKickable kickable) {
                        kickable.Kick(Vector3.Zero, GlobalTransform.basis.z * kickForce);
                        _kickArmed = false;
                    }
                }
            }

            // GD.InstanceFromId()
        } else if (currentState == "KickWindup") {
            _kickArmed = true;
        }
    }
}
