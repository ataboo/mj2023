using Godot;
using System;

public class OrdersUIControl: Control
{
    [Export]
    private PackedScene orderPrefab;

    [Export]
    private NodePath orderHolderPath;
    private Control _orderHolder;

    public override void _Ready()
    {
        _orderHolder = GetNode<Control>(orderHolderPath) ?? throw new NullReferenceException();
    }

    public OrderCardControl AddOrder(OrderState order) {
        var newOrder = orderPrefab.Instance<OrderCardControl>();
        _orderHolder.AddChild(newOrder);
        newOrder.SetOrder(order);
        newOrder.SetProgress(0);

        return newOrder;
    }
}
