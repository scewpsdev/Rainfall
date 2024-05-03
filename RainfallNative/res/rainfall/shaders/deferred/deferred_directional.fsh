$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

uniform vec4 u_lightDirection;
uniform vec4 u_lightColor;

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

	vec3 lightS = RenderDirectionalLight(position, normal, view, distance, albedo, roughness, metallic, u_lightDirection.xyz, u_lightColor.rgb);
	
	gl_FragColor = vec4(lightS, 1.0);

	//if (v_texcoord0.x > 0.75 && v_texcoord0.y > 0.75)
	//	gl_FragColor = vec4(vec3_splat(shadow2D(s_directionalLightShadowMap0, vec3(v_texcoord0.x * 4 - 3, v_texcoord0.y * 4 - 3, u_cameraPosition.w))), 1.0);
}
