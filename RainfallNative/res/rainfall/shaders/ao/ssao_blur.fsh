$input v_texcoord0

#include "../common/common.shader"


#define KERNEL_SIZE 1


SAMPLER2D(s_ao, 0);

uniform vec4 u_cameraFrustum;


void main()
{
	int x0 = -KERNEL_SIZE;
	int x1 = KERNEL_SIZE;
	int y0 = -KERNEL_SIZE;
	int y1 = KERNEL_SIZE;

	float near = u_cameraFrustum[0];
	float far = u_cameraFrustum[1];

	float result = 0.0;
	float totalSamples = 0.0;
	for (int y = y0; y <= y1; y++)
	{
		for (int x = x0; x <= x1; x++)
		{
			vec2 offset = vec2(x, y) / textureSize(s_ao, 0.0);
			float value = texture2D(s_ao, v_texcoord0 + offset).r;
			result += value;
			totalSamples += 1;
		}
	}
	result *= 1.0 / max(totalSamples, 0.0001);

	gl_FragColor = vec4(result, 1.0, 1.0, 1.0);
}
