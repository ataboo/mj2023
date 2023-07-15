using Godot;

public class SauceGlobControl : RigidBody
{

    private bool _hit = false;

	private SceneTreeTimer _deathTimer;

	[Export]
	float lifetimeSeconds = 5;

    public override void _Ready()
    {
        
		_deathTimer = GetTree().CreateTimer(lifetimeSeconds);
		_deathTimer.Connect("timeout", this, nameof(_HandleDeathTimer));
    }

    public override void _PhysicsProcess(float delta)
    {
        if(_hit) {
            return;
        }

        // _velocity += new Vector3(0, -_gravityAccel * delta, 0);
        // Translate(_velocity * delta);

        if(Translation.y < -100) {
            QueueFree();
        }
    }

    public void _HandleBodyEntered(Node node) {
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        // Mode = ModeEnum.Static;
        _hit = true;
        // Scale = new Vector3(3, 0.5f, 3f);
    }

	public void CancelDeath() {
		_deathTimer.Disconnect("timeout", this, nameof(_HandleDeathTimer));
	}

	private void _HandleDeathTimer() {
		QueueFree();
	}

}
