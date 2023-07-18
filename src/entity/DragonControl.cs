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

    public override void _Ready()
    {
        _mouth = GetNode<DragonMouthControl>(dragonMouthPath) ?? throw new NullReferenceException();
        _animationTree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback") ?? throw new NullReferenceException();
        _levelManager = LevelManager.MustGetNode(this);
        _ordersManager = _levelManager.OrdersManager;
        _entityHolder = _levelManager.EntityHolder;
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
            return;
        }

        _heldPizza.QueueFree();
        _heldPizza = null;
    }
}
