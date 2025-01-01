$input v_position, v_texcoord0, v_color0

#include "../common/common.shader"


SAMPLER2D(u_textureAtlas, 0);
uniform vec4 u_atlasSize;

uniform vec4 u_pointLight_position[16];
uniform vec4 u_pointLight_color[16];
uniform vec4 u_lightInfo; // numPointLights, emissiveStrength, lightInfluence, additive
#define u_numPointLights int(u_lightInfo[0] + 0.5)
#define u_emissiveStrength u_lightInfo[1]
#define u_lightInfluence u_lightInfo[2]
#define u_additive u_lightInfo[3]


vec3 L(vec3 color, float distanceSq)
{
	float maxBrightness = 400.0;
	float attenuation = 1.0 / (1.0 / maxBrightness + distanceSq);
	vec3 radiance = color * attenuation;

	return radiance;
}

vec3 CalculatePointLights(vec3 position)
{
	vec3 result = vec3(0.0, 0.0, 0.0);

	for (int i = 0; i < 16; i++)
	{
		vec3 lightPosition = u_pointLight_position[i].xyz;
		vec3 lightColor = u_pointLight_color[i].rgb * u_pointLight_color[i].a;

		float distanceSq = dot(lightPosition - position, lightPosition - position);
		vec3 light = L(lightColor, distanceSq);

		result += i < u_numPointLights ? light : vec3(0.0, 0.0, 0.0);
	}

	return result;
}

void main()
{
	vec2 uv = v_texcoord0.xy;
	float animationFrame = v_texcoord0.z;
	float frameIdx = max(animationFrame * u_atlasSize.x * u_atlasSize.y - 1, 0.0);

	int frameX = int(frameIdx) % int(u_atlasSize.x + 0.5);
	int frameY = int(frameIdx) / int(u_atlasSize.x + 0.5);
	vec2 frameUV = (uv + vec2(frameX, frameY)) / u_atlasSize.xy;
	vec4 frameColor = SRGBToLinear(texture2D(u_textureAtlas, frameUV));

	int nextFrameX = int(frameIdx + 1) % int(u_atlasSize.x + 0.5);
	int nextFrameY = int(frameIdx + 1) / int(u_atlasSize.x + 0.5);
	vec2 nextFrameUV = (uv + vec2(nextFrameX, nextFrameY)) / u_atlasSize.xy;
	vec4 nextFrameColor = SRGBToLinear(texture2D(u_textureAtlas, nextFrameUV));

	float blend = fract(frameIdx);
	vec4 textureColor = mix(vec4(1.0, 1.0, 1.0, 1.0), mix(frameColor, nextFrameColor, blend), u_atlasSize.z);
	vec4 albedo = textureColor * v_color0;
	//if (albedo.a < 0.001)
	//	discard;

	vec4 final = vec4(0, 0, 0, albedo.a);
	final.rgb += u_emissiveStrength * albedo.rgb;

	vec3 light = CalculatePointLights(v_position);
	light = max(light, vec3_splat(1));
	final.rgb += light * u_lightInfluence * albedo.rgb;

	if (u_additive > 0.5)
	{
		final.rgb *= final.a;
		final.a = 1;
	}

	gl_FragColor = final;
}
