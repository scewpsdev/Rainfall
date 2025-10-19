$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

uniform vec4 u_lightDirection;
uniform vec4 u_lightColor;

SAMPLER2DSHADOW(s_shadowMap0, 4);
SAMPLER2DSHADOW(s_shadowMap1, 5);
SAMPLER2DSHADOW(s_shadowMap2, 6);
uniform vec4 u_params;
#define u_shadowMapFar0 u_params[0]
#define u_shadowMapFar1 u_params[1]
#define u_shadowMapFar2 u_params[2]
uniform mat4 u_toLightSpace0;
uniform mat4 u_toLightSpace1;
uniform mat4 u_toLightSpace2;

SAMPLER2D(s_ao, 7);

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

	vec3 lightS = RenderDirectionalLight(position, normal, view, distance, albedo, roughness, metallic, u_lightDirection.xyz, u_lightColor.rgb, s_shadowMap0, u_shadowMapFar0, u_toLightSpace0, s_shadowMap1, u_shadowMapFar1, u_toLightSpace1, s_shadowMap2, u_shadowMapFar2, u_toLightSpace2, gl_FragCoord);

	float ao = texture2D(s_ao, v_texcoord0).r;
	lightS *= ao;

	gl_FragColor = vec4(lightS, 1.0);

	//if (v_texcoord0.x > 0.75 && v_texcoord0.y > 0.75)
	//	gl_FragColor = vec4(vec3_splat(shadow2D(s_directionalLightShadowMap0, vec3(v_texcoord0.x * 4 - 3, v_texcoord0.y * 4 - 3, u_cameraPosition.w))), 1.0);
}
