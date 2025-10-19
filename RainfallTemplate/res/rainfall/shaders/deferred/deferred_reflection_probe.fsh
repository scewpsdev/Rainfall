

#include "../common/common.shader"
#include "../bgfx/bgfx_compute.shader"

#include "pbr.shader"


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLERCUBE(s_environmentMap, 4);

uniform vec4 u_reflectionPosition;
uniform vec4 u_reflectionSize;
uniform vec4 u_reflectionOrigin;

SAMPLER2D(s_ao, 5);

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

	vec4 lightS = RenderReflectionsParallax(position, normal, view, albedo, roughness, metallic, s_environmentMap, u_reflectionPosition, u_reflectionSize, u_reflectionOrigin);

	float ao = texture2D(s_ao, v_texcoord0).r;
	lightS *= ao;

	//lightS = textureCubeLod(s_environmentMap, -view, 0).rgb;

	gl_FragColor = lightS;
	//gl_FragColor = vec4(ao, ao, ao, 1.0);
}
