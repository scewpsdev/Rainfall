$input v_position, v_normal, v_color0, v_texcoord0


#include "../common/common.shader"


//#define BOTTOM_COLOR SRGBToLinear(vec3(129 / 255.0, 173 / 255.0, 64 / 255.0))
//#define TOP_COLOR SRGBToLinear(vec3(182 / 255.0, 216 / 255.0, 104 / 255.0))
//#define TIP_COLOR SRGBToLinear(vec3(242 / 255.0, 241 / 255.0, 179 / 255.0))
#define BOTTOM_COLOR SRGBToLinear(vec3(74 / 255.0, 97 / 255.0, 47 / 255.0))
#define TOP_COLOR SRGBToLinear(vec3(161 / 255.0, 189 / 255.0, 118 / 255.0))
//#define BOTTOM_COLOR vec3(0.05, 0.1, 0.01)
//#define TOP_COLOR vec3(0.3, 0.4, 0.1)
//#define TIP_COLOR vec3(242 / 255.0, 241 / 255.0, 179 / 255.0)

SAMPLER2D(s_texture, 0);


void main()
{
	float heightFactor = v_color0.x;
	//float heightMultiplier = v_color0.y;

	vec3 color = mix(linearToSRGB(BOTTOM_COLOR), linearToSRGB(TOP_COLOR), heightFactor * heightFactor);
	//vec4 textureColor = texture2D(s_texture, v_texcoord0);
	//color *= textureColor.rgb;
	//if (textureColor.a < 0.5)
	//	discard;

	//float tipLength = 0.1;
	//float tipFactor = max(remap(heightFactor * heightMultiplier, heightMultiplier - tipLength, heightMultiplier, 0.0, 1.0), 0.0);
	//float tipFactor = max(heightFactor * heightMultiplier - 1.0, 0.0) * 6.0;
	//color = mix(color, SRGBToLinear(TIP_COLOR), tipFactor);

	float roughness = max(remap(heightFactor, 0.0, 1.0, 1.0, 0.5), 0.0);
	//float roughness = remap(reflectivity, 0.0, 1.0, 1.0, 0.7);
	//float fresnelStrength = pow(1.0 - abs(dot(view, normal)), 1.0) * 1.0;
	//color += vec3_splat(1.0) * fresnelStrength * heightFactor;

	vec3 normal = normalize(v_normal);
	float metallic = 0.0;
	vec3 emissive = vec3_splat(0.0);

	//color = normal * 0.5 + 0.5;

	gl_FragData[0] = vec4(v_position, 1.0);
	gl_FragData[1] = vec4(normal * 0.5 + 0.5, 1.0);
	gl_FragData[2] = vec4(color, roughness);
	gl_FragData[3] = vec4(emissive, metallic);
}
