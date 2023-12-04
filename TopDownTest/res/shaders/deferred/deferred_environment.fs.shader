$input v_texcoord0

#include "../common/common.shader"

#include "pbr.shader"

#define ENVIRMAP_FADEOUT_DISTANCE 0.998
#define MAX_REFLECTION_PROBES 4


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ambientOcclusion, 4);

uniform vec4 u_cameraPosition;

uniform vec4 u_ambientColor;


void main()
{
    vec4 positionW = texture2D( s_gbuffer0, v_texcoord0);
    vec4 normalEmissionStrength = texture2D( s_gbuffer1, v_texcoord0);
    vec4 albedoRoughness = texture2D( s_gbuffer2, v_texcoord0);
    vec4 emissiveMetallic = texture2D( s_gbuffer3, v_texcoord0);

    vec3 position = positionW.xyz;
    vec3 normal = normalize(normalEmissionStrength.xyz * 2.0 - 1.0);
    vec3 albedo = SRGBToLinear(albedoRoughness.rgb);
    vec3 emissionColor = emissiveMetallic.rgb;
    float emissionStrength = normalEmissionStrength.a;
    vec3 emissive = albedo * emissionStrength;
    float roughness = albedoRoughness.a;
    float metallic = emissiveMetallic.a;

    float ao = 1.0 - texture2D( s_ambientOcclusion, v_texcoord0).r;

	vec3 toCamera = u_cameraPosition.xyz - position;
    float distance = length(toCamera);
    vec3 view = toCamera / distance;

    vec3 ambient = u_ambientColor.rgb * u_ambientColor.a;
    vec3 final = ambient * albedo * ao + emissive;
	
    gl_FragColor = vec4(emissive, 1.0);

    if (positionW.a < 0.5)
        discard;
}
