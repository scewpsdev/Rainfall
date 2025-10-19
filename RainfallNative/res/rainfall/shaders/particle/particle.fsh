$input v_position, v_texcoord0, v_color0

#include "../common/common.shader"


SAMPLER2D(s_textureAtlas, 0);
uniform vec4 u_atlasSize;

SAMPLERCUBE(s_environmentMap, 1);

uniform vec4 u_pointLight_position[16];
uniform vec4 u_pointLight_color[16];
uniform vec4 u_lightInfo; // numPointLights, emissiveStrength, lightInfluence, additive
#define u_numPointLights int(u_lightInfo[0] + 0.5)
#define u_emissiveStrength u_lightInfo[1]
#define u_lightInfluence u_lightInfo[2]
#define u_additive u_lightInfo[3]


vec3 L(vec3 color, float distanceSq)
{
	//float maxBrightness = 400.0;
	//float attenuation = 1.0 / (1.0 / maxBrightness + distanceSq);
	float dist = sqrt(distanceSq);
	float attenuation = 1.0 / (1.0 + 1 * dist + 2 * distanceSq);
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

		vec3 toLight = lightPosition - position;
		float distanceSq = dot(toLight, toLight);
		vec3 light = L(lightColor, distanceSq);
		float ndotl = 1;

		result += i < u_numPointLights ? light * ndotl : vec3(0.0, 0.0, 0.0);
	}

	return result;
}

vec3 CalculateEnvironmentLighting(vec3 position)
{
	ivec2 size = textureSize(s_environmentMap, 0);
	int maxLod = int(log2(max(size.x, size.y)) + 0.001);
	return textureCubeLod(s_environmentMap, vec3(0, 1, 0), maxLod).rgb;
}

void main()
{
	vec2 uv = v_texcoord0.xy;
	float animationFrame = v_texcoord0.z;
	float frameIdx = max(animationFrame * u_atlasSize.x * u_atlasSize.y - 1, 0.0);

	int frameX = int(frameIdx) % int(u_atlasSize.x + 0.5);
	int frameY = int(frameIdx) / int(u_atlasSize.x + 0.5);
	vec2 frameUV = (uv + vec2(frameX, frameY)) / u_atlasSize.xy;
	vec4 frameColor = SRGBToLinear(texture2D(s_textureAtlas, frameUV));

	int nextFrameX = int(frameIdx + 1) % int(u_atlasSize.x + 0.5);
	int nextFrameY = int(frameIdx + 1) / int(u_atlasSize.x + 0.5);
	vec2 nextFrameUV = (uv + vec2(nextFrameX, nextFrameY)) / u_atlasSize.xy;
	vec4 nextFrameColor = SRGBToLinear(texture2D(s_textureAtlas, nextFrameUV));

	float blend = fract(frameIdx);
	vec4 textureColor = mix(vec4(1.0, 1.0, 1.0, 1.0), mix(frameColor, nextFrameColor, blend), u_atlasSize.z);
	vec4 albedo = textureColor * v_color0;
	//if (albedo.a < 0.001)
	//	discard;

	vec3 light = CalculatePointLights(v_position) + CalculateEnvironmentLighting(v_position);
	vec3 color = mix(albedo.rgb * light, albedo.rgb, u_emissiveStrength);
	vec4 final = vec4(color, albedo.a);

	if (u_additive > 0.5)
	{
		final.rgb *= final.a;
		final.a = 1;
	}

	gl_FragColor = final;
}
