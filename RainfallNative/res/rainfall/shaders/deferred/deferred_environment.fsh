$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLERCUBE(s_environmentMap, 4);
uniform vec4 u_environmentData;
#define u_environmentIntensity u_environmentData[0]
#define u_numMasks int(u_environmentData[1] + 0.5)

uniform vec4 u_maskPosition[4];
uniform vec4 u_maskSize[4];

uniform vec4 u_cameraPosition;


void main()
{
	vec4 positionW = texture2D( s_gbuffer0, v_texcoord0);
	vec4 normalEmissionStrength = texture2D( s_gbuffer1, v_texcoord0);
	vec4 albedoRoughness = texture2D( s_gbuffer2, v_texcoord0);
	vec4 emissiveMetallic = texture2D( s_gbuffer3, v_texcoord0);

	vec3 position = positionW.xyz;
	vec3 normal = normalize(normalEmissionStrength.xyz * 2.0 - 1.0);
	vec3 albedo = SRGBToLinear(albedoRoughness.rgb);
	vec3 emissionColor = SRGBToLinear(emissiveMetallic.rgb);
	float emissionStrength = normalEmissionStrength.a;
	vec3 emissive = emissionColor * emissionStrength;
	float roughness = albedoRoughness.a;
	float metallic = emissiveMetallic.a;
	
	vec3 toCamera = u_cameraPosition.xyz - position;
	float distance = length(toCamera);
	vec3 view = toCamera / distance;

	vec3 ambient = RenderEnvironment(position, normal, view, albedo, roughness, metallic, s_environmentMap, u_environmentIntensity);
	
	for (int i = 0; i < u_numMasks; i++)
	{
		float distanceToBounds = DistanceToBox(u_maskPosition[i].xyz + 0.5 * u_maskSize[i].xyz, u_maskSize[i].xyz, position);
		distanceToBounds = max(-distanceToBounds, 0);
		float falloff = u_maskSize[i].w;
		float attenuation = 1 / (1 + falloff * distanceToBounds * distanceToBounds) - 0.01;
		ambient *= attenuation;
	}

	gl_FragColor = vec4(ambient, 1.0);

	if (positionW.a < 0.5)
		discard;
}
