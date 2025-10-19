$input v_texcoord0

#include "../bgfx/common.shader"


SAMPLER2D(s_color, 0);
SAMPLER2D(s_material, 1);
SAMPLER2D(s_lighting, 2);


void main()
{
	vec4 hdr = texture2D(s_color, v_texcoord0);
	hdr.rgb *= hdr.a;
	vec4 material = texture2D(s_material, v_texcoord0);
	vec3 lighting = texture2D(s_lighting, v_texcoord0).rgb;

	vec3 final = SRGBToLinear(linearToSRGB(hdr.rgb) * mix(vec4_splat(1), linearToSRGB(lighting), material.r));

	gl_FragColor = vec4(final, 1.0);
}
