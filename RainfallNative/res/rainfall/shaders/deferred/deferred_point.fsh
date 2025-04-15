$input v_data0, v_color0

#include "../common/common.shader"
#include "../bgfx/bgfx_shader.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ao, 4);

SAMPLERCUBE(s_shadowMap0, 5);
SAMPLERCUBE(s_shadowMap1, 6);
SAMPLERCUBE(s_shadowMap2, 7);
SAMPLERCUBE(s_shadowMap3, 8);
SAMPLERCUBE(s_shadowMap4, 9);
SAMPLERCUBE(s_shadowMap5, 10);
SAMPLERCUBE(s_shadowMap6, 11);
SAMPLERCUBE(s_shadowMap7, 12);

uniform vec4 u_cameraPosition;


void main()
{
	vec2 v_texcoord0 = gl_FragCoord.xy * u_viewTexel.xy;
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

	vec3 lightPosition = v_data0.xyz;
	vec3 lightColor = v_color0.xyz;
	vec3 lightS = RenderPointLight(position, normal, view, albedo, roughness, metallic, lightPosition, lightColor);

	float shadowMapID = v_color0.w;
	if (shadowMapID >= 0)
	{
		float shadow = 1;
		if (shadowMapID < 0.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap0, 0.1);
		else if (shadowMapID < 1.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap1, 0.1);
		else if (shadowMapID < 2.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap2, 0.1);
		else if (shadowMapID < 3.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap3, 0.1);
		else if (shadowMapID < 4.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap4, 0.1);
		else if (shadowMapID < 5.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap5, 0.1);
		else if (shadowMapID < 6.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap6, 0.1);
		else if (shadowMapID < 7.5) shadow = CalculatePointShadow(position, lightPosition, s_shadowMap7, 0.1);
		lightS *= shadow;
	}

	float ao = texture2D(s_ao, v_texcoord0).r;
	lightS *= ao;

	gl_FragColor = vec4(lightS, 1.0);
	//gl_FragColor.rgb += 0.01;
}
