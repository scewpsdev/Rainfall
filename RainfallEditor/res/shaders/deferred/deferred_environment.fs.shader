$input v_texcoord0

#include "../common/common.shader"

#define ENVIRMAP_FADEOUT_DISTANCE 0.998
#define MAX_REFLECTION_PROBES 4


SAMPLER2D(s_gbuffer0, 0);
SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer2, 2);
SAMPLER2D(s_gbuffer3, 3);

SAMPLER2D(s_ambientOcclusion, 4);

uniform vec4 u_cameraPosition;

uniform vec4 u_environmentMapIntensities;
SAMPLERCUBE(s_environmentMap, 5);


// Environment mapping
vec3 RenderEnvironmentMap(vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao, samplerCube environmentMap, float environmentMapIntensity)
{
    vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);

    vec3 irradiance = textureCubeLod(environmentMap, normal, 12.0).rgb * environmentMapIntensity;
    vec3 diffuse = irradiance * albedo;

    vec3 ambient = diffuse * ao;

    return ambient;
}

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

    float ao = 1.0 - texture2D( s_ambientOcclusion, v_texcoord0).r;

	vec3 toCamera = u_cameraPosition.xyz - position;
    float distance = length(toCamera);
    vec3 view = toCamera / distance;

    //vec3 ambient = RenderEnvironmentMap(normal, view, albedo, roughness, metallic, ao, s_environmentMap, u_environmentMapIntensities[0]);
    float brightness = dot(normal, normalize(vec3(0.2, 1.0, 0.2))) * 0.5 + 0.5;
    vec3 ambient = brightness * albedo;
	
    gl_FragColor = vec4(ambient, 1.0);

    if (positionW.a < 0.5)
        discard;
}
