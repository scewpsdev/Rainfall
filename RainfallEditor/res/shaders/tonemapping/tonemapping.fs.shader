$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_hdrBuffer, 0);


vec3 ACESCinematic(vec3 color)
{
	mat3 m1 = mat3(
        0.59719, 0.07600, 0.02840,
        0.35458, 0.90834, 0.13383,
        0.04823, 0.01566, 0.83777
	);
	mat3 m2 = mat3(
        1.60475, -0.10208, -0.00327,
        -0.53108,  1.10813, -0.07276,
        -0.07367, -0.00605,  1.07602
	);
	vec3 v = mul(m1, color);
	vec3 a = v * (v + 0.0245786) - 0.000090537;
	vec3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
	return pow(clamp(mul(m2, a / b), 0.0, 1.0), vec3_splat(1.0 / 2.2));
}

vec3 ACESFilmic(vec3 color)
{
	const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return clamp((color * (a * color + b)) / (color * (c * color + d ) + e), 0.0, 1.0);
}

vec3 Tonemap(vec3 color, float exposure)
{
	//color = toFilmic(color);
	//color = toAcesFilmic(color * 4);
	//color = pow(color, vec3_splat(1.0 / 2.2)); // Gamma correction
	//return color;

	color = vec3(1.0, 1.0, 1.0) - exp(-color * exposure); // Convert to LDR space
	color = pow(color, vec3_splat(1.0 / 2.2)); // Gamma correction

	return color;
}

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
	vec4 hdra = texture2D(s_hdrBuffer, v_texcoord0);
	vec3 tonemapped = Tonemap(hdra.rgb, 1.0);
	tonemapped = Dither(tonemapped, v_texcoord0);

	gl_FragColor = vec4(tonemapped, hdra.a);
}
