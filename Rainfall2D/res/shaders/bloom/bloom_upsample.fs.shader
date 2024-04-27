$input v_texcoord0

#include "../bgfx/common.shader"


#define KERNEL_SIZE 3


SAMPLER2D(s_input0, 0);
SAMPLER2D(s_input1, 1);


void main()
{
	int x0 = -(KERNEL_SIZE - 1) / 2;
	int x1 = x0 + KERNEL_SIZE - 1;
	int y0 = -(KERNEL_SIZE - 1) / 2;
	int y1 = y0 + KERNEL_SIZE - 1;

	vec3 result = vec3_splat(0.0);
	for (int y = y0; y <= y1; y++)
	{
		for (int x = x0; x <= x1; x++)
		{
			vec2 offset = vec2(x, y) / textureSize(s_input0, 0.0);
			vec3 value = texture2D(s_input0, v_texcoord0 + offset).rgb;
			result += value;
		}
	}

	vec3 input0 = result / (KERNEL_SIZE * KERNEL_SIZE);
	vec3 input1 = texture2D(s_input1, v_texcoord0).rgb;

	gl_FragColor = vec4(input0 + input1, 1.0);
}
