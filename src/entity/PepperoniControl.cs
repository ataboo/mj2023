using Godot;
using System;

public class PepperoniControl : RigidBody, ICookable
{
	[Export]
	public PackedScene[] RawPrefabs = new PackedScene[3];
	[Export]
	public PackedScene[] CookedPrefabs = new PackedScene[3];

	private Spatial _rawSpatial;

	private Spatial _cookedSpatial;

	private MeshInstance _cookedMesh;
	
	private Random rnd = new Random();

	[Export]
	Color burnedColour;
	
	private int i = 0;

	[Export]
	float lifetimeSeconds = 20;

	private SceneTreeTimer _deathTimer;

	Color _originalCookedColor;

	private CookedState _cookedState;
	public CookedState CookedState {
		get  => _cookedState;
		set {
			this.SetCooked(value);
		}
	}
	
	public override void _Ready()
	{
		i = rnd.Next(3);
		_rawSpatial = RawPrefabs[i].Instance<Spatial>();
		_cookedSpatial = CookedPrefabs[i].Instance<Spatial>();
		AddChild(_rawSpatial);
		AddChild(_cookedSpatial);
		_cookedSpatial.Visible = false;
		_cookedMesh = _cookedSpatial.GetChild<MeshInstance>(0) ?? throw new NullReferenceException();
		if(_cookedMesh.GetActiveMaterial(0) is SpatialMaterial mat) {
			_originalCookedColor = mat.AlbedoColor;
		}

		_deathTimer = GetTree().CreateTimer(lifetimeSeconds);
		_deathTimer.Connect("timeout", this, nameof(_HandleDeathTimer));
	}

	public void CancelDeath() {
		_deathTimer.Disconnect("timeout", this, nameof(_HandleDeathTimer));
	}

	private void SetCooked(CookedState cooked)
	{
		_cookedState = cooked;

		_rawSpatial.Visible = cooked == CookedState.Raw;
		_cookedSpatial.Visible = cooked != CookedState.Raw;

		if(cooked != CookedState.Raw) {
			var cookedMaterial = _cookedMesh.GetActiveMaterial(0) as SpatialMaterial;
			cookedMaterial.AlbedoColor = cooked == CookedState.Burned ? burnedColour : _originalCookedColor;
		}
	}

	private void _HandleDeathTimer() {
		QueueFree();
	}
}
