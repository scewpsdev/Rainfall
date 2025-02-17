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
	const float wheelRadius = 0.138185f;
	static readonly Vector3[] tireLocalPositions = [
		new Vector3(0.37931f, 0.138185f, 0.598f),
		new Vector3(-0.37931f, 0.138185f, 0.598f),
		new Vector3(0.37931f, 0.138185f, -0.598f),
		new Vector3(-0.37931f, 0.138185f, -0.598f)
	];

	DriveMode driveMode = DriveMode.RearWheel;
	float frontWheelGrip = 1;
	float rearWheelGrip = 1;

	float[] tireHeights = new float[4];
	Vector3[] suspensionForces = new Vector3[4];
	Vector3[] steeringForces = new Vector3[4];
	Vector3[] forwardForces = new Vector3[4];

	float currentWheelSteer = 0;
	Vector3 currentSteerDirection;
	float currentMotor = 0;

	Model wheel;

	public Cart()
	{
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
		const float springDamper = 25;

		Matrix tireTransform = getModelMatrix() * Matrix.CreateTranslation(tireLocalPositions[i]);
		Vector3 springDir = tireTransform.rotation.up;
		Vector3 tireVelocity = body.getPointVelocity(tireTransform.translation);

		float raycastHeight = 1;
		float suspensionRestPosition = raycastHeight;
		HitData? hit = Physics.Raycast(getModelMatrix() * (tireLocalPositions[i] * new Vector3(1, 0, 1)) + raycastHeight * tireTransform.rotation.up, tireTransform.rotation.down, raycastHeight + 2 * wheelRadius, QueryFilterFlags.Static);

		if (hit == null)
		{
			tireHeights[i] = -wheelRadius;
			suspensionForces[i] = Vector3.Zero;
			steeringForces[i] = Vector3.Zero;
			forwardForces[i] = Vector3.Zero;
			return;
		}

		float offset = suspensionRestPosition - hit.Value.distance;
		float velocity = Vector3.Dot(springDir, tireVelocity);

		// more ground sucction
		if (offset < 0)
			offset *= 4;

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
		float tireMass = 5 / 60.0f;
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

		float frictionCoefficient = 1;
		float maxFrictionForce = frictionCoefficient * MathF.Max(suspensionForce, 0);
		if (steerForce.length > maxFrictionForce)
			steerForce = steerForce.normalized * maxFrictionForce;
		body.addForceAtPosition(steerForce, tireTransform.translation);
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		currentSteerDirection = new Vector3(Vector2.Rotate(Vector2.Down, -currentWheelSteer), 0).xzy;

		float maxSpeed = 20;
		float forwardSpeed = Vector3.Dot(rotation * currentSteerDirection, body.getVelocity());
		float motorIncrease = 15;
		float acceleration = 5;
		float rollingFriction = 1;
		if (Input.IsKeyDown(KeyCode.W))
			currentMotor = MathHelper.Linear(currentMotor, acceleration * MathF.Max(1 - MathF.Max(forwardSpeed, 0) / maxSpeed, 0), motorIncrease * delta);
		if (Input.IsKeyDown(KeyCode.S))
			currentMotor = MathHelper.Linear(currentMotor, forwardSpeed > 0 ? -acceleration : -3, motorIncrease * delta);
		if (!Input.IsKeyDown(KeyCode.W) && !Input.IsKeyDown(KeyCode.S))
			currentMotor = MathHelper.Linear(currentMotor, -MathF.Sign(forwardSpeed) * rollingFriction, motorIncrease * delta);

		float steer = 2.0f;
		if (Input.IsKeyDown(KeyCode.D))
			currentWheelSteer -= delta * steer;
		if (Input.IsKeyDown(KeyCode.A))
			currentWheelSteer += delta * steer;
		if (!Input.IsKeyDown(KeyCode.D) && !Input.IsKeyDown(KeyCode.A))
			currentWheelSteer = MathHelper.Linear(currentWheelSteer, 0, delta * steer);

		float maxSteer = 0.2f * MathF.PI * (1 / (1 + MathF.Abs(forwardSpeed) * 0.3f));
		currentWheelSteer = MathHelper.Clamp(currentWheelSteer, -maxSteer, maxSteer);

		float rocketForce = 40;
		if (Input.IsKeyDown(KeyCode.Shift))
			body.addForce(rotation.forward * rocketForce);

		for (int i = 0; i < tireLocalPositions.Length; i++)
			simulateTire(i, i >= 2, delta);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		for (int i = 0; i < 4; i++)
		{
			Vector3 wheelLocalPosition = tireLocalPositions[i] * new Vector3(1, 0, 1) + new Vector3(0, tireHeights[i] + wheelRadius, 0);
			Matrix wheelTransform = getModelMatrix() * Matrix.CreateTranslation(wheelLocalPosition);
			if (i >= 2)
				wheelTransform = wheelTransform * Matrix.CreateRotation(Vector3.Up, currentWheelSteer);
			Renderer.DrawModel(wheel, wheelTransform);

			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + suspensionForces[i] * 0.1f, 0xFF0000FF);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + steeringForces[i] * 0.1f, 0xFFFF0000);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + forwardForces[i] * 0.1f, 0xFF00FF00);
		}
	}
}
