using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DriveMode
{
	FrontWheel,
	RearWheel,
	AllWheel,
}

public class Cart : Entity
{
	const float wheelRadius = 0.138185f * 0.56f;
	static readonly Vector3[] tireLocalPositions = [
		new Vector3(0.37931f, 0.138185f, 0.598f) * 0.56f,
		new Vector3(-0.37931f, 0.138185f, 0.598f) * 0.56f,
		new Vector3(0.37931f, 0.138185f, -0.598f) * 0.56f,
		new Vector3(-0.37931f, 0.138185f, -0.598f) * 0.56f
	];

	DriveMode driveMode = DriveMode.FrontWheel;
	float frontWheelGrip = 0.5f;
	float rearWheelGrip = 0.7f;

	float maxSpeed = 30;
	float acceleration = 3;
	float rollingFriction = 1;

	float[] tireHeights = new float[4];
	Vector3[] suspensionForces = new Vector3[4];
	Vector3[] steeringForces = new Vector3[4];
	Vector3[] forwardForces = new Vector3[4];
	Vector3[] tireVelocities = new Vector3[4];
	float[] wheelRotations = new float[4];
	bool[] wheelGrounded = new bool[4];

	float currentWheelSteer = 0;
	Vector3 currentSteerDirection;
	float currentMotor = 0;

	long turnedAroundSince = -1;

	Vector3 lastVelocity;

	bool ejected = false;

	public float waterLevel = float.MaxValue;

	Model wheel;


	public Cart()
	{
		bodyFriction = 0.0f;
		bodyRestitution = 1.0f;

		bodyFilterGroup = PhysicsFilter.Cart;
		bodyFilterMask = PhysicsFilter.Default;

		load("cart.rfs");

		wheel = Resource.GetModel("cart_wheel.gltf");
	}

	public override void init()
	{
		base.init();

		//body.setCenterOfMass(Vector3.Zero);
	}

	void simulateTire(int i, bool frontWheel, float delta)
	{
		// Suspension

		//const float suspensionRestPosition = 0.138185f;
		const float springStrength = 200;
		const float springDamper = 15;

		Matrix tireTransform = getModelMatrix() * Matrix.CreateTranslation(tireLocalPositions[i]);
		Vector3 springDir = tireTransform.rotation.up;
		Vector3 tireVelocity = body.getPointVelocity(tireTransform.translation);

		tireVelocities[i] = tireVelocity;

		Vector3 localTireVelocity = rotation.conjugated * tireVelocities[i];
		wheelRotations[i] = MathHelper.Lerp(wheelRotations[i], MathF.Atan2(-localTireVelocity.x, -localTireVelocity.z), 1 * delta * tireVelocities[i].length);

		float raycastHeight = 1;
		float suspensionRestPosition = raycastHeight;
		HitData? hit = Physics.Raycast(getModelMatrix() * (tireLocalPositions[i] * new Vector3(1, 0, 1)) + raycastHeight * tireTransform.rotation.up, tireTransform.rotation.down, raycastHeight + wheelRadius, QueryFilterFlags.Static);

		if (hit == null && waterLevel != float.MaxValue)
		{
			Vector3 origin = getModelMatrix() * (tireLocalPositions[i] * new Vector3(1, 0, 1)) + raycastHeight * tireTransform.rotation.up;
			float range = raycastHeight + wheelRadius;
			Vector3 dir = tireTransform.rotation.down;
			Vector3 dest = origin + range * dir;
			if (dest.y <= waterLevel)
			{
				hit = new HitData()
				{
					distance = (origin.y - waterLevel) / dir.y,
					normal = Vector3.Up
				};
			}
		}

		if (hit == null || hit.Value.normal.y < 0.5f)
		{
			tireHeights[i] = -wheelRadius;
			wheelGrounded[i] = false;
			suspensionForces[i] = Vector3.Zero;
			steeringForces[i] = Vector3.Zero;
			forwardForces[i] = Vector3.Zero;
			return;
		}

		wheelGrounded[i] = true;

		float offset = suspensionRestPosition - hit.Value.distance;
		float velocity = Vector3.Dot(springDir, tireVelocity);

		// more ground sucction
		if (offset < 0)
			offset *= 8;

		float suspensionForce = (offset * springStrength) - (velocity * springDamper);
		body.addForceAtPosition(springDir * suspensionForce, tireTransform.translation);

		tireHeights[i] = offset;
		suspensionForces[i] = springDir * suspensionForce;

		// Steering

		Vector3 steeringDirection = frontWheel ? currentSteerDirection : Vector3.Forward;
		Vector3 steeringDirectionRight = Vector3.Cross(steeringDirection, Vector3.Up);
		steeringDirection = rotation * steeringDirection;
		steeringDirectionRight = rotation * steeringDirectionRight;

		float slideAmount = Vector3.Dot(tireVelocity, steeringDirectionRight);
		Vector3 slide = slideAmount * steeringDirectionRight;
		float grip = frontWheel ? frontWheelGrip : rearWheelGrip;
		float gripMultiplier = 1 - MathF.Abs(Vector3.Dot(tireVelocity.normalized, steeringDirectionRight));
		//Console.WriteLine(gripMultiplier);

		//float gripLoseThreshold = 0.7f;
		//gripMultiplier = gripMultiplier > gripLoseThreshold ? MathHelper.Remap(gripMultiplier, gripLoseThreshold, 1.0f, 0.1f, 1.0f) : MathHelper.Remap(gripMultiplier, 0.0f, gripLoseThreshold, 0.05f, 0.1f);
		Vector3 steerAcceleration = -slide * grip; // * gripMultiplier;
		float tireMass = 4 / 60.0f;
		Vector3 steerForce = steerAcceleration * tireMass / delta;
		//body.addForceAtPosition(steerForce, tireTransform.translation);

		steeringForces[i] = steerForce;

		// Forward force

		bool drive = driveMode == DriveMode.AllWheel || (!frontWheel && driveMode == DriveMode.RearWheel) || (frontWheel && driveMode == DriveMode.FrontWheel);
		if (drive)
		{
			Vector3 motorForce = steeringDirection * currentMotor;
			//body.addForceAtPosition(motorForce, tireTransform.translation);
			steerForce += motorForce;
			forwardForces[i] = motorForce;
		}

		// Apply force & Drifting

		float frictionCoefficient = 5;
		float maxFrictionForce = frictionCoefficient * MathF.Abs(suspensionForce);
		if (steerForce.length > maxFrictionForce)
			steerForce = steerForce.normalized * maxFrictionForce;
		body.addForceAtPosition(steerForce, tireTransform.translation);
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		currentSteerDirection = new Vector3(Vector2.Rotate(Vector2.Down, -currentWheelSteer), 0).xzy;

		lastVelocity = body.getVelocity();

		float forwardSpeed = Vector3.Dot(rotation * currentSteerDirection, body.getVelocity());
		float motorIncrease = 15;
		if (Input.IsKeyDown(KeyCode.W) && !ejected)
			currentMotor = MathHelper.Linear(currentMotor, acceleration * MathF.Max(1 - MathF.Max(forwardSpeed, 0) / maxSpeed, 0), motorIncrease * delta);
		if (Input.IsKeyDown(KeyCode.S) && !ejected)
			currentMotor = MathHelper.Linear(currentMotor, forwardSpeed > 0 ? -2 * acceleration : -3, motorIncrease * delta);
		if (!Input.IsKeyDown(KeyCode.W) && !Input.IsKeyDown(KeyCode.S) || ejected)
			currentMotor = MathHelper.Linear(currentMotor, -MathF.Sign(forwardSpeed) * rollingFriction, motorIncrease * delta);

		float steer = 2.0f;
		if (Input.IsKeyDown(KeyCode.D) && !ejected)
			currentWheelSteer -= delta * steer;
		if (Input.IsKeyDown(KeyCode.A) && !ejected)
			currentWheelSteer += delta * steer;
		if (!Input.IsKeyDown(KeyCode.D) && !Input.IsKeyDown(KeyCode.A) || ejected)
			currentWheelSteer = MathHelper.Linear(currentWheelSteer, 0, delta * steer);

		float maxSteer = 0.2f * MathF.PI * (1 / (1 + MathF.Abs(forwardSpeed) * 0.3f));
		currentWheelSteer = MathHelper.Clamp(currentWheelSteer, -maxSteer, maxSteer);

		float rocketForce = 40;
		if (Input.IsKeyDown(KeyCode.Shift) && !ejected)
			body.addForce(rotation.forward * rocketForce);

		for (int i = 0; i < tireLocalPositions.Length; i++)
			simulateTire(i, i >= 2, delta);


		// Autocorrect rotation mid air
		bool isGrounded = false;
		for (int i = 0; i < wheelGrounded.Length; i++)
		{
			if (wheelGrounded[i])
				isGrounded = true;
		}
		if (isGrounded)
		{
			HitData? hit = Physics.Raycast(getModelMatrix().translation + rotation.up, rotation.down, 2, QueryFilterFlags.Static);
			if (hit != null)
			{
				body.addForceAtPosition(hit.Value.normal * 10, position + rotation.up * 3);
				body.addForceAtPosition(-hit.Value.normal * 10, position + rotation.down * 3);
			}
		}

		if (rotation.up.y < 0.1f)
		{
			if (turnedAroundSince == -1)
				turnedAroundSince = Time.currentTime;

			if ((Time.currentTime - turnedAroundSince) / 1e9f > 3)
			{
				respawn();
			}
		}
		else
		{
			turnedAroundSince = -1;
		}
	}

	public void respawn()
	{
		setTransform(GameState.instance.spawnPoint);
		body.setVelocity(Vector3.Zero);
		body.setRotationVelocity(Vector3.Zero);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (!ejected)
		{
			Vector3 velocity = body.getVelocity();
			Vector3 sum = lastVelocity - velocity;
			float d = sum.length;
			if (d > 20)
			{
				// eject jerome
				GameState.instance.player.eject(lastVelocity);
				ejected = true;
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		for (int i = 0; i < 4; i++)
		{
			Vector3 wheelLocalPosition = tireLocalPositions[i] * new Vector3(1, 0, 1) + new Vector3(0, tireHeights[i] + wheelRadius, 0);
			Matrix wheelTransform = getModelMatrix() * Matrix.CreateTranslation(wheelLocalPosition);
			//if (i >= 2)
			//	wheelTransform = wheelTransform * Matrix.CreateRotation(Vector3.Up, currentWheelSteer);
			wheelTransform = wheelTransform * Matrix.CreateRotation(Vector3.Up, wheelRotations[i]);
			Renderer.DrawModel(wheel, wheelTransform);

			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + suspensionForces[i] * 0.1f, 0xFF0000FF);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + steeringForces[i] * 0.1f, 0xFFFF0000);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + forwardForces[i] * 0.1f, 0xFF00FF00);
		}
	}
}
