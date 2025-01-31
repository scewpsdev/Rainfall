$input v_texcoord0

#include "../bgfx/common.shader"


SAMPLER2D(s_midground, 0);
SAMPLER2D(s_parallax, 1);
SAMPLER2D(s_material, 2);
SAMPLER2D(s_lighting, 3);

uniform vec4 u_cameraSettings;
#define u_cameraFract u_cameraSettings.xy


void main()
{
	ivec2 midgroundResolution = textureSize(s_midground, 0);
	vec2 cameraFract = u_cameraFract * vec2(1, -1) / midgroundResolution;

	vec4 hdr = texture2D(s_midground, v_texcoord0 + cameraFract);
	vec4 parallax = texture2D(s_parallax, v_texcoord0 + cameraFract);
	//hdr.rgb = mix(hdr.rgb * hdr.a, parallax.rgb, parallax.a);
	hdr.rgb = mix(parallax.rgb, hdr.rgb, hdr.a);
	vec4 material = texture2D(s_material, v_texcoord0 + cameraFract);
	vec3 lighting = texture2D(s_lighting, v_texcoord0 + cameraFract).rgb;

	vec3 final = SRGBToLinear(linearToSRGB(hdr.rgb) * mix(vec4_splat(1), linearToSRGB(lighting), material.r));

	gl_FragColor = vec4(final, 1.0);
}
