$input v_position, v_normal, v_texcoord0, v_color0

#include "../bgfx/common.shader"

#define MAX_TEXTURES 16


SAMPLER2D(s_texture0, 0);
SAMPLER2D(s_texture1, 1);
SAMPLER2D(s_texture2, 2);
SAMPLER2D(s_texture3, 3);
SAMPLER2D(s_texture4, 4);
SAMPLER2D(s_texture5, 5);
SAMPLER2D(s_texture6, 6);
SAMPLER2D(s_texture7, 7);
SAMPLER2D(s_texture8, 8);
SAMPLER2D(s_texture9, 9);
SAMPLER2D(s_texture10, 10);
SAMPLER2D(s_texture11, 11);
SAMPLER2D(s_texture12, 12);
SAMPLER2D(s_texture13, 13);
SAMPLER2D(s_texture14, 14);
SAMPLER2D(s_texture15, 15);


vec4 SampleTextureByID(int id, vec2 texcoord)
{
	switch (id)
	{
	case 0: return texture2D(s_texture0, texcoord);
	case 1: return texture2D(s_texture1, texcoord);
	case 2: return texture2D(s_texture2, texcoord);
	case 3: return texture2D(s_texture3, texcoord);
	case 4: return texture2D(s_texture4, texcoord);
	case 5: return texture2D(s_texture5, texcoord);
	case 6: return texture2D(s_texture6, texcoord);
	case 7: return texture2D(s_texture7, texcoord);
	case 8: return texture2D(s_texture8, texcoord);
	case 9: return texture2D(s_texture9, texcoord);
	case 10: return texture2D(s_texture10, texcoord);
	case 11: return texture2D(s_texture11, texcoord);
	case 12: return texture2D(s_texture12, texcoord);
	case 13: return texture2D(s_texture13, texcoord);
	case 14: return texture2D(s_texture14, texcoord);
	case 15: return texture2D(s_texture15, texcoord);
	default: return vec4(0.0, 0.0, 0.0, 0.0);
	}
}

void main()
{
	float textureID = v_texcoord0.z;
	vec4 textureColor = mix(vec4(1.0, 1.0, 1.0, 1.0), SRGBToLinear(SampleTextureByID(int(textureID + 0.5), v_texcoord0.xy)), textureID > -0.5 ? 1.0 : 0.0);
	textureColor.rgb *= v_color0.rgb;
	
	if (textureColor.a < 0.01)
		discard;
	
	vec3 normal = normalize(v_normal);
	
	float roughness = 0.5;
	vec3 emissive = vec3(0.0, 0.0, 0.0);
	float emissionStrength = 0.0;
	float metallic = 0.0;
	
	emissive = textureColor.rgb;
	emissionStrength = max(v_color0.a - 1.0, 0.0);

	gl_FragColor = vec4(linearToSRGB(textureColor.rgb), textureColor.a);

	/*
	gl_FragData[0] = vec4(v_position, 1);
    gl_FragData[1] = vec4(normal * 0.5 + 0.5, emissionStrength);
    gl_FragData[2] = vec4(textureColor.rgb, roughness);
    gl_FragData[3] = vec4(emissive, metallic);
	*/
}
