using Godot;
using System;

public class pepperoni : Spatial
{
	public bool initialForce = false;
	
	[Export]
	public PackedScene[] Raw = new PackedScene[3];
	[Export]
	public PackedScene[] Cooked = new PackedScene[3];
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	[Export]
	NodePath rigidBodyPath;
	private RigidBody _RBNode;
	
	private Random rnd = new Random();
	// Called when the node enters the scene tree for the first time.
	
	private int i = 0;
	
	public override void _Ready()
	{
		i = rnd.Next(3);
		var rawInstance = Raw[i].Instance<Spatial>();
		AddChild(rawInstance);
	}

	private void cook()
	{
		//get existing child, delete it, instantiate new cooked instance
	}
	
	
	public override void _PhysicsProcess(float delta)
	{
		if(initialForce)
		{
			//_RBNode.add_force(-Transform.basis.z, Transform.basis.z);
			initialForce = false;
		}
	}
	
}
