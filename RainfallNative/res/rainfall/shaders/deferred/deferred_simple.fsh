$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLERCUBE(s_skybox, 4);
uniform vec4 u_skySettings;
#define u_skyIntensity u_skySettings[0]

uniform vec4 u_cameraPosition;

uniform vec4 u_ambientLight;

uniform vec4 u_pointLightPositions[16];
uniform vec4 u_pointLightColors[16];

SAMPLERCUBESHADOW(s_shadowMap0, 5);
SAMPLERCUBESHADOW(s_shadowMap1, 6);
SAMPLERCUBESHADOW(s_shadowMap2, 7);
SAMPLERCUBESHADOW(s_shadowMap3, 8);

uniform vec4 u_directionalLightDirection;
uniform vec4 u_directionalLightColor;

SAMPLER2DSHADOW(s_shadowMap4, 9);
SAMPLER2DSHADOW(s_shadowMap5, 10);
SAMPLER2DSHADOW(s_shadowMap6, 11);
uniform vec4 u_params;
#define u_shadowMapFar4 u_params[0]
#define u_shadowMapFar5 u_params[1]
#define u_shadowMapFar6 u_params[2]
uniform mat4 u_toLightSpace4;
uniform mat4 u_toLightSpace5;
uniform mat4 u_toLightSpace6;


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

	roughness = 1;

	vec3 lightS = vec3(0, 0, 0);

	lightS += u_ambientLight.rgb * albedo;

	lightS += emissive;

	int numPointLights = int(u_cameraPosition.w + 0.5);
	for (int i = 0; i < numPointLights; i++)
	{
		vec3 lightPosition = u_pointLightPositions[i].xyz;
		vec3 lightColor = u_pointLightColors[i].rgb;
		vec3 radiance = RenderPointLight(position, normal, view, albedo, roughness, metallic, lightPosition, lightColor);

		float lightRadius = u_pointLightPositions[i].w;
		float shadowMapID = u_pointLightColors[i].w;
		if (shadowMapID >= 0)
		{
			float shadow = 1;
			if (shadowMapID < 0.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap0, 0.1, lightRadius);
			else if (shadowMapID < 1.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap1, 0.1, lightRadius);
			else if (shadowMapID < 2.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap2, 0.1, lightRadius);
			else if (shadowMapID < 3.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap3, 0.1, lightRadius);
			radiance *= shadow;
		}

		lightS += radiance;
	}

	if (u_directionalLightDirection.w > 0.5)
	{
		lightS += RenderDirectionalLight(position, normal, view, distance, albedo, roughness, metallic, u_directionalLightDirection.xyz, u_directionalLightColor.rgb, s_shadowMap4, u_shadowMapFar4, u_toLightSpace4, s_shadowMap5, u_shadowMapFar5, u_toLightSpace5, s_shadowMap6, u_shadowMapFar6, u_toLightSpace6, gl_FragCoord);
	}

	//float ao = texture2D(s_ao, v_texcoord0).r;
	//lightS *= ao;

	gl_FragColor = vec4(lightS, 1.0);

	if (positionW.a < 0.5)
		discard;
}
