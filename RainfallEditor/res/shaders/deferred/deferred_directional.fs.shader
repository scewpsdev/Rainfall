$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ambientOcclusion, 4);

uniform vec4 u_cameraPosition;
uniform vec4 u_directionalLightDirection;
uniform vec4 u_directionalLightColor;

uniform vec4 u_directionalLightCascadeFarPlanes;
SAMPLER2DSHADOW(s_directionalLightShadowMap0, 10);
uniform mat4 u_directionalLightToLightSpace0;
SAMPLER2DSHADOW(s_directionalLightShadowMap1, 11);
uniform mat4 u_directionalLightToLightSpace1;
SAMPLER2DSHADOW(s_directionalLightShadowMap2, 12);
uniform mat4 u_directionalLightToLightSpace2;


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
	lightS += RenderDirectionalLight(
		position, normal, view, distance, albedo, roughness, metallic, ao, u_directionalLightDirection.xyz, u_directionalLightColor.rgb,
		s_directionalLightShadowMap0, u_directionalLightCascadeFarPlanes[0], u_directionalLightToLightSpace0,
		s_directionalLightShadowMap1, u_directionalLightCascadeFarPlanes[1], u_directionalLightToLightSpace1,
		s_directionalLightShadowMap2, u_directionalLightCascadeFarPlanes[2], u_directionalLightToLightSpace2,
		gl_FragCoord);
	
	gl_FragColor = vec4(lightS, 1.0);

	//if (v_texcoord0.x > 0.75 && v_texcoord0.y > 0.75)
	//	gl_FragColor = vec4(vec3_splat(shadow2D(s_directionalLightShadowMap0, vec3(v_texcoord0.x * 4 - 3, v_texcoord0.y * 4 - 3, u_cameraPosition.w))), 1.0);

	if (positionW.a < 0.5)
		discard;
}
