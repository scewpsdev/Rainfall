using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public class ShadowMap
{
	public const int NUM_SHADOW_CASCADES = 3;

	static readonly float[] NEAR_PLANES = new float[] { -40.0f, -40.0f, -40.0f };
	public static readonly float[] FAR_PLANES = new float[] { 40.0f, 100.0f, 200.0f };


	public RenderTarget[] renderTargets = new RenderTarget[NUM_SHADOW_CASCADES];
	public Matrix[] cascadeViews = new Matrix[NUM_SHADOW_CASCADES];
	public Matrix[] cascadeProjections = new Matrix[NUM_SHADOW_CASCADES];

	DirectionalLight light;
	int resolution;


	public ShadowMap(int resolution, DirectionalLight light, GraphicsDevice graphics)
	{
		this.light = light;
		this.resolution = resolution;

		for (int i = 0; i < NUM_SHADOW_CASCADES; i++)
		{
			renderTargets[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(resolution, resolution, TextureFormat.D32F, (ulong)TextureFlags.RenderTarget | (uint)SamplerFlags.UClamp | (uint)SamplerFlags.VClamp | (uint)SamplerFlags.CompareLEqual)
			});
		}
	}

	void calculateViewTransform(Vector3 cameraPosition, Quaternion cameraRotation, float cameraFov, float cameraAspect, float near, float far, out Matrix lightProjection, out Matrix lightView)
	{
		Vector3 forward = cameraRotation.forward;
		Vector3 up = cameraRotation.up;
		Vector3 right = cameraRotation.right;

		float halfHeight = MathF.Tan(MathHelper.ToRadians(cameraFov * 0.5f));
		float farHalfHeight = far * halfHeight;
		float nearHalfHeight = near * halfHeight;

		float farHalfWidth = farHalfHeight * cameraAspect;
		float nearHalfWidth = nearHalfHeight * cameraAspect;

		Vector3 centerFar = cameraPosition + forward * far;
		Vector3 centerNear = cameraPosition + forward * near;

		Vector3[] corners = new Vector3[8];
		corners[0] = centerFar + up * farHalfHeight + right * farHalfWidth;
		corners[1] = centerFar + up * farHalfHeight - right * farHalfWidth;
		corners[2] = centerFar - up * farHalfHeight + right * farHalfWidth;
		corners[3] = centerFar - up * farHalfHeight - right * farHalfWidth;
		corners[4] = centerNear + up * nearHalfHeight + right * nearHalfWidth;
		corners[5] = centerNear + up * nearHalfHeight - right * nearHalfWidth;
		corners[6] = centerNear - up * nearHalfHeight + right * nearHalfWidth;
		corners[7] = centerNear - up * nearHalfHeight - right * nearHalfWidth;

		Quaternion lightRotation = Quaternion.LookAt(light.direction);
		Quaternion lightRotationInv = lightRotation.conjugated;

		for (int i = 0; i < 8; i++)
			corners[i] = lightRotationInv * corners[i];

		Vector3 min = corners[0];
		Vector3 max = corners[0];
		for (int i = 0; i < 8; i++)
		{
			min = Vector3.Min(min, corners[i]);
			max = Vector3.Max(max, corners[i]);
		}

		Vector3 center = 0.5f * (min + max);

		Vector3 localMin = min - center;
		Vector3 localMax = max - center;

		Vector3 size = max - min;
		Vector3 unitsPerTexel = size / resolution;
		//localMin = Vector3.Floor(localMin / unitsPerTexel) * unitsPerTexel;
		//localMax = Vector3.Floor(localMax / unitsPerTexel) * unitsPerTexel;

		Vector3 boxPosition = lightRotation * center;

		lightProjection = Matrix.CreateOrthographic(localMin.x, localMax.x, localMin.y, localMax.y, localMin.z, localMax.z);
		lightView = (Matrix.CreateTranslation(boxPosition) * Matrix.CreateRotation(lightRotation)).inverted;
	}

	public void calculateCascadeTransforms(Vector3 cameraPosition, Quaternion cameraRotation, float cameraFov, float cameraAspect)
	{
		for (int i = 0; i < NUM_SHADOW_CASCADES; i++)
		{
			calculateViewTransform(cameraPosition, cameraRotation, cameraFov, cameraAspect, NEAR_PLANES[i], FAR_PLANES[i], out cascadeProjections[i], out cascadeViews[i]);
		}
		//calculateViewTransform(cameraPosition, cameraRotation, cameraFov, cameraAspect, -20.0f, 20.0f, out cascadeProjections[0], out cascadeViews[0]);
	}
}
