$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"

#define MAX_LIGHTS 8


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ambientOcclusion, 4);

uniform vec4 u_cameraPosition;
uniform vec4 u_lightPosition[MAX_LIGHTS];
uniform vec4 u_lightColor[MAX_LIGHTS];

SAMPLERCUBE(s_lightShadowMap0, 5);
SAMPLERCUBE(s_lightShadowMap1, 6);
SAMPLERCUBE(s_lightShadowMap2, 7);
SAMPLERCUBE(s_lightShadowMap3, 8);
SAMPLERCUBE(s_lightShadowMap4, 9);
SAMPLERCUBE(s_lightShadowMap5, 10);
SAMPLERCUBE(s_lightShadowMap6, 11);
SAMPLERCUBE(s_lightShadowMap7, 12);


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
	
	float ao = 1.0 - texture2D(s_ambientOcclusion, v_texcoord0).r;

	vec3 toCamera = u_cameraPosition.xyz - position;
	float distance = length(toCamera);
	vec3 view = toCamera / distance;

	vec3 lightS = vec3_splat(0.0);
	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		vec3 lightPosition = u_lightPosition[i].xyz;
		vec3 lightColor = u_lightColor[i].rgb;

		if (i == 0) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap0);
		if (i == 1) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap1);
		if (i == 2) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap2);
		if (i == 3) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap3);
		if (i == 4) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap4);
		if (i == 5) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap5);
		if (i == 6) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap6);
		if (i == 7) lightS += RenderPointLightShadow(position, normal, view, albedo, roughness, metallic, ao, lightPosition, lightColor, s_lightShadowMap7);
	}

	gl_FragColor = vec4(lightS, 1.0);
	//gl_FragColor = vec4(vec3_splat(textureCube(s_lightShadowMap0, -view).r), 1.0);
	//gl_FragColor = vec4(vec3_splat(textureCubeLod(s_lightShadowMap0, position - u_lightPosition[0].xyz, 0).r), 1.0);

	if (positionW.a < 0.5)
		discard;
}
