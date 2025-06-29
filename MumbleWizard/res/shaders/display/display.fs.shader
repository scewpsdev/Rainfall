$input v_texcoord0

#include "../bgfx/common.shader"


SAMPLER2D(s_frame, 0);


//uniformly distributed, normalized rand, [0, 1)
float nrand(in vec2 n)
{
	return fract(sin(dot(n.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

float n4rand_ss(in vec2 n)
{
	float nrnd0 = nrand(n + 0.07 * fract(0.0));
	float nrnd1 = nrand(n + 0.11 * fract(0.0 + 0.573953));
	return 0.23 * sqrt(-log(nrnd0 + 0.00001)) * cos(2.0 * 3.141592 * nrnd1) + 0.5;
}

// yea ik ik not real dithering who cares
vec3 Dither(vec3 color, vec2 uv)
{
	float r = n4rand_ss(uv);
	return color + vec3(r, r, r) / 80.0;
}

void main()
{
	vec3 final = texture2D(s_frame, v_texcoord0).rgb;

	final = linearToSRGB(final);
	final = Dither(final, v_texcoord0);

	gl_FragColor = vec4(final, 1.0);
}
