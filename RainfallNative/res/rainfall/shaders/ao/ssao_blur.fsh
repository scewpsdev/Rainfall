$input v_texcoord0

#include "../common/common.shader"


#define KERNEL_SIZE 5
#define DEPTH_THRESHHOLD 0.1


SAMPLER2D(s_depthBuffer, 0);
SAMPLER2D(s_ao, 1);

uniform vec4 u_cameraFrustum;


void main()
{
	int x0 = -(KERNEL_SIZE - 1) / 2;
	int x1 = (KERNEL_SIZE - 1) / 2;
	int y0 = -(KERNEL_SIZE - 1) / 2;
	int y1 = (KERNEL_SIZE - 1) / 2;

	float near = u_cameraFrustum[0];
	float far = u_cameraFrustum[1];

	float depth = texture2D(s_depthBuffer, v_texcoord0);
	float distance = depthToDistance(depth, near, far);

	float result = 0.0;
	float totalSamples = 0.0;
	for (int y = y0; y <= y1; y++)
	{
		for (int x = x0; x <= x1; x++)
		{
			vec2 offset = vec2(x, y) / textureSize(s_ao, 0.0);
			float value = texture2D(s_ao, v_texcoord0 + offset).r;

			float sampleDepth = texture2D(s_depthBuffer, v_texcoord0 + offset);
			float sampleDistance = depthToDistance(sampleDepth, near, far);
			float similarity = abs(sampleDistance - distance) < DEPTH_THRESHHOLD ? 1.0 : 0.0;
			//float similarity = max(1.0 - pow(abs(sampleDistance - distance) / DEPTH_THRESHHOLD, 2.0), 0.0);

			result += value * similarity;
			totalSamples += similarity;
		}
	}
	result *= 1.0 / max(totalSamples, 0.0001);

	gl_FragColor = vec4(result, 1.0, 1.0, 1.0);
}
