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
    NodePath abilityControlPath;
    private AbilityControl _abilityControl;

    [Export]
    NodePath footColliderPath;
    private KinematicBody _footCollider;

    private CollisionShape _footColShape;

    private PhysicsShapeQueryParameters _shapeQueryParams;

    private bool _kickArmed = false;

    [Export]
    public float kickForce = 250f;

    [Export]
    NodePath kickAudioPath;
    private AudioStreamPlayer3D _kickAudio;

    private Random _rng = new Random();

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback");
        _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
        _footCollider = GetNode<KinematicBody>(footColliderPath) ?? throw new NullReferenceException();
        _footColShape = _footCollider.GetChild<CollisionShape>(0) ?? throw new NullReferenceException();
        _kickAudio = GetNode<AudioStreamPlayer3D>(kickAudioPath);

        _shapeQueryParams = new PhysicsShapeQueryParameters {
            ShapeRid = _footColShape.Shape.GetRid(),
            Transform = _footColShape.GlobalTransform,
            CollisionMask = 1U<<1,
            Exclude = new Godot.Collections.Array(_footCollider),
        };

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChange));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));
    }

    public void _HandleLegStateChange() {
        var abilityState = _abilityControl.AbilityState;
        var legKick = abilityState.legKick && abilityState.activeAbility == MechAbility.Kick;
        var legWindup = abilityState.legWindUp && abilityState.activeAbility == MechAbility.Kick;

        _tree.Set("parameters/conditions/kick", legKick);
        _tree.Set("parameters/conditions/windup", legWindup);
        _tree.Set("parameters/conditions/not_windup", !legWindup);
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
                        PlayKickSound();
                    } else if(instance is IKickable kickable) {
                        kickable.Kick(Vector3.Zero, GlobalTransform.basis.z * kickForce);
                        _kickArmed = false;
                        PlayKickSound();
                    }
                }
            }

            // GD.InstanceFromId()
        } else if (currentState == "KickWindup") {
            _kickArmed = true;
        }
    }

    private void PlayKickSound() {
        _kickAudio.PitchScale = MechGameHelpers.RandomRangef(_rng, 0.9f, 1.1f);
        _kickAudio.Playing = true;
    }

    private void _HandleAbilityChange(MechAbility ability) {
        if(ability == MechAbility.Kick) {
            if(_stateMachine.GetCurrentNode() == "Hidden") {
                _stateMachine.Travel("KickIdle");   
            }
        } else {
            if(_stateMachine.GetCurrentNode() != "Hidden") {
                _stateMachine.Travel("Hidden");
            }

            _tree.Set("parameters/conditions/kick", false);
            _tree.Set("parameters/conditions/windup", false);
            _tree.Set("parameters/conditions/not_windup", true);
            _playerControl.SetMoveLocked(false);
        }
    }

    private void _HandleClick(bool isPressed, bool isLeft)
    {
        if(_abilityControl.AbilityState.activeAbility != MechAbility.Kick) {
            return;
        }

        if(isLeft) {
            _tree.Set("parameters/conditions/kick", isPressed);
        } else {
            _tree.Set("parameters/conditions/windup", isPressed);
            _tree.Set("parameters/conditions/not_windup", !isPressed);
            _playerControl.SetMoveLocked(isPressed);
        }
    }
}
