using System;
using Godot;

public class LevelManager : Node
{
	private GameManager _gameManager;

	public static LevelManager MustGetNode(Node self) {
		return self.GetNode<LevelManager>("/root/Level") ?? throw new System.NullReferenceException();
	}

	[Export]
	private NodePath arHolderPath;
	public ARHolderControl ARHolder {get; private set;}

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>("/root/GameManager") ?? throw new System.NullReferenceException();
		ARHolder = GetNode<ARHolderControl>(arHolderPath) ?? throw new NullReferenceException();
	}
}
