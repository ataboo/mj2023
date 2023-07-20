using Godot;
using System;

public class DragonControl : Spatial
{
    [Export]
    NodePath animationTreePath;

    private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _stateMachine;

    [Export]
    private NodePath dragonMouthPath;
    private DragonMouthControl _mouth;

    private OrdersManager _ordersManager;

    [Signal]
    public delegate void OnPizzaAte(PizzaState pizza);

    private PizzaControl _heldPizza;

    private LevelManager _levelManager;

    private Spatial _entityHolder;

    [Export]
    private AudioStream roarStream;

    [Export]
    private AudioStream eatStream;
    
    [Export]
    private AudioStream growlStream;
    
    [Export]
    private AudioStream spitStream;

    [Export]
    private NodePath audioPlayerPath;
    private AudioStreamPlayer3D _audioPlayer;


    [Export]
    private Vector2 growlRateRange = new Vector2(20, 40);

    private float _growlCountdown;

    private Random _rng = new Random();
    

    public override void _Ready()
    {
        _mouth = GetNode<DragonMouthControl>(dragonMouthPath) ?? throw new NullReferenceException();
        _animationTree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback") ?? throw new NullReferenceException();
        _levelManager = LevelManager.MustGetNode(this);
        _ordersManager = _levelManager.OrdersManager;
        _entityHolder = _levelManager.EntityHolder;
        _audioPlayer=  GetNode<AudioStreamPlayer3D>(audioPlayerPath) ?? throw new NullReferenceException();

        ScheduleGrowl();
    }

    public bool EatPizza(PizzaControl pizza) {
        var pizzaParent = pizza.GetParent();
        if(pizzaParent == _mouth) {
            return false;
        }

        if(_stateMachine.GetCurrentNode() != "DragonIdle") {
            return false;
        }

        _stateMachine.Travel("Chew");
        _heldPizza = pizza;

        PlaySound(eatStream);

        var validPizza = _ordersManager.PizzaOrderValid(_heldPizza.PizzaState);

        EmitSignal(nameof(OnPizzaAte), pizza.PizzaState);
        
        var oldPizzaPos = pizza.GlobalTranslation;

        pizzaParent.RemoveChild(pizza);
        _mouth.AddChild(pizza);
        pizza.GlobalTranslation = oldPizzaPos;

        GetTree().CreateTimer(1.5f).Connect("timeout", this, nameof(_RemoveHeldPizza), new Godot.Collections.Array(validPizza));

        return true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(_heldPizza != null) {
            _heldPizza.Translation = _heldPizza.Translation.LinearInterpolate(Vector3.Zero, 0.8f);
            _heldPizza.Rotation = _heldPizza.Rotation.LinearInterpolate(new Vector3(0, _heldPizza.Rotation.y, 0), 0.8f);
        }
    }

    public override void _Process(float delta)
    {
        if((_growlCountdown-=delta) <= 0) {
            ScheduleGrowl();
            if(!_audioPlayer.Playing) {
                _audioPlayer.Stream = MechGameHelpers.RandomBool(_rng) ? growlStream : roarStream;
                _audioPlayer.Seek(0);
                _audioPlayer.Playing = true;
            }
        }
    }

    private void _RemoveHeldPizza(bool validPizza) {
        if(_heldPizza == null || _heldPizza.GetParent() != _mouth) {
            GD.PushWarning("No pizza to remove.");
            return;
        }

        if(!validPizza) {
            var oldPos = _heldPizza.GlobalTranslation;
            var oldRotation = _heldPizza.GlobalRotation;
            _mouth.RemoveChild(_heldPizza);
            _entityHolder.AddChild(_heldPizza);
            _heldPizza.GlobalTranslation = oldPos;
            _heldPizza.GlobalRotation = oldRotation;

            _heldPizza.SetRBActive(true);
            _heldPizza.ApplyCentralImpulse((_levelManager.PlayerControl.GlobalTranslation - _mouth.GlobalTranslation) * 3f);

            _heldPizza = null;

            PlaySound(spitStream);

            return;
        }

        _heldPizza.QueueFree();
        _heldPizza = null;
    }

    private void PlaySound(AudioStream sound) {
        _audioPlayer.Stream = sound;
        _audioPlayer.Seek(0);
        _audioPlayer.Playing = true;
    }

    private void ScheduleGrowl() {
        _growlCountdown = MechGameHelpers.RandomRangef(_rng, growlRateRange.x, growlRateRange.y);
    }
}
