$input v_texcoord0

#include "../bgfx/common.shader"


#define BLOOM_STRENGTH 0.1
#define BLOOM_FALLOFF 4.0


SAMPLER2D(s_color, 0);
SAMPLER2D(s_lighting, 1);
SAMPLER2D(s_bloom, 2);

uniform vec4 u_vignetteColor;


vec3 ThreshholdBloom(vec3 bloom)
{
	return bloom * (1.0 - exp(-RGBToLuminance(bloom) * BLOOM_FALLOFF));
	//return bloom;
}

// https://www.shadertoy.com/view/lsKSWR
vec3 Vignette(vec3 color, vec2 uv)
{
	float intensity = 15.0;
	float falloff = u_vignetteColor.a;

	uv *= 1.0 - uv.yx;
	float vig = uv.x * uv.y * intensity;
	vig = pow(vig, falloff);

	return mix(u_vignetteColor.rgb, color, vig);
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
	vec3 hdr = texture2D(s_color, v_texcoord0).rgb;
	vec3 lighting = texture2D(s_lighting, v_texcoord0).rgb;
	vec3 bloom = texture2D(s_bloom, v_texcoord0).rgb;
	vec3 color = hdr + ThreshholdBloom(bloom) * BLOOM_STRENGTH;

	vec3 final = linearToSRGB(color) * linearToSRGB(lighting);
	final = Vignette(final, v_texcoord0);
	final = Dither(final, v_texcoord0);

	gl_FragColor = vec4(final, 1.0);
}
