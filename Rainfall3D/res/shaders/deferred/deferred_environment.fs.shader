$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"

#define MAX_REFLECTION_PROBES 4


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ambientOcclusion, 4);

uniform vec4 u_cameraPosition;

uniform vec4 u_environmentMapIntensities;
SAMPLERCUBE(s_environmentMap, 5);

SAMPLERCUBE(s_reflectionProbe0, 6);
uniform vec4 u_reflectionProbePosition0;
uniform vec4 u_reflectionProbeSize0;
uniform vec4 u_reflectionProbeOrigin0;

SAMPLERCUBE(s_reflectionProbe1, 7);
uniform vec4 u_reflectionProbePosition1;
uniform vec4 u_reflectionProbeSize1;
uniform vec4 u_reflectionProbeOrigin1;

SAMPLERCUBE(s_reflectionProbe2, 8);
uniform vec4 u_reflectionProbePosition2;
uniform vec4 u_reflectionProbeSize2;
uniform vec4 u_reflectionProbeOrigin2;

SAMPLERCUBE(s_reflectionProbe3, 9);
uniform vec4 u_reflectionProbePosition3;
uniform vec4 u_reflectionProbeSize3;
uniform vec4 u_reflectionProbeOrigin3;


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

	vec3 ambient = RenderEnvironment(position, normal, view, albedo, roughness, metallic, ao, s_environmentMap, u_environmentMapIntensities[0],
		s_reflectionProbe0, u_reflectionProbePosition0, u_reflectionProbeSize0, u_reflectionProbeOrigin0, 1.0,
		s_reflectionProbe1, u_reflectionProbePosition1, u_reflectionProbeSize1, u_reflectionProbeOrigin1, 1.0,
		s_reflectionProbe2, u_reflectionProbePosition2, u_reflectionProbeSize2, u_reflectionProbeOrigin2, 1.0,
		s_reflectionProbe3, u_reflectionProbePosition3, u_reflectionProbeSize3, u_reflectionProbeOrigin3, 1.0);

	vec3 final = ambient;

	gl_FragColor = vec4(final, 1.0);

	if (positionW.a < 0.5)
		discard;
}
