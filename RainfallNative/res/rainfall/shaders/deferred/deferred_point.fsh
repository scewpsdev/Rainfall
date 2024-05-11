$input v_data0, v_data1

#include "../common/common.shader"
#include "../bgfx/bgfx_compute.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

uniform vec4 u_lightPosition;
uniform vec4 u_lightColor;

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
	vec3 lightColor = v_data1.xyz;
	vec3 lightS = RenderPointLight(position, normal, view, albedo, roughness, metallic, lightPosition, lightColor);

	gl_FragColor = vec4(lightS, 1.0);
	//gl_FragColor.rgb += 0.01;
}
