using Godot;
using System;

public class DragonControl : Area
{
    [Export]
    NodePath animationTreePath;

    private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _stateMachine;

    [Export]
    private NodePath dragonMouthPath;
    private DragonMouthControl _mouth;

    public override void _Ready()
    {
        _mouth = GetNode<DragonMouthControl>(dragonMouthPath) ?? throw new NullReferenceException();
        _animationTree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback") ?? throw new NullReferenceException();
    }

    public void EatPizza(PizzaControl pizza) {
        var pizzaParent = pizza.GetParent();
        if(pizzaParent == _mouth) {
            return;
        }
        
        _stateMachine.Travel("Chew");

        pizzaParent.RemoveChild(pizza);
        _mouth.AddChild(pizza);
    }
}
