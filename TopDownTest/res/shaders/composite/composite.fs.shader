$input v_texcoord0

#include "../common/common.shader"


#define BLOOM_STRENGTH 0.02
#define BLOOM_FALLOFF 10.0


SAMPLER2D(s_hdrBuffer, 0);
SAMPLER2D(s_bloom, 1);

uniform vec4 u_vignetteColor;


vec3 TreshholdBloom(vec3 bloom)
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

void main()
{
	vec3 hdr = texture2D(s_hdrBuffer, v_texcoord0).rgb;
	vec3 bloom = texture2D(s_bloom, v_texcoord0).rgb;
	vec3 color = hdr + TreshholdBloom(bloom) * BLOOM_STRENGTH;
	vec3 final = Vignette(color, v_texcoord0);

	gl_FragColor = vec4(final, 1.0);
}
