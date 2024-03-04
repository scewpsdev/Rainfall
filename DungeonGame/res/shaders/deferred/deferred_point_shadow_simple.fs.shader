$input v_texcoord0

#include "../common/common.shader"
#include "shadow_mapping.shader"

#define MAX_LIGHTS 16
#define PI 3.14159265359


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
uniform vec4 u_lightShadowMapNear0;
uniform vec4 u_lightShadowMapNear1;


// Variant fresnel equation taking the roughness into account;
vec3 fresnel2(float hdotv, vec3 f0, float roughness)
{
	//return f0 + (1.0 - f0) * pow(clamp(1.0 - hdotv, 0.0, 1.0), 5.0);
    return f0 + (max(vec3_splat(1.0 - roughness), f0) - f0) * pow(1.0 - hdotv, 5.0);
}

// Radiance calculation for radial flux over the angle w
vec3 L(vec3 color, float distanceSq)
{
    float attenuation = 1.0 / (1.0 + distanceSq);
    vec3 radiance = color * attenuation;

    return radiance;
}

// Point light indirect specular lighting
vec3 RenderPointLight(vec3 position, vec3 normal, vec3 view, vec3 albedo, float ao, vec3 lightPosition, vec3 lightColor)
{
    vec3 fLambert = albedo / PI;

	// Per light radiance
    vec3 toLight = lightPosition - position;
    vec3 wi = normalize(toLight);

    float distanceSq = dot(toLight, toLight);
    vec3 radiance = L(lightColor, distanceSq);
    
    float ndotwi = max(dot(wi, normal), 0.0);

    vec3 s = fLambert * radiance * ndotwi * ao;

    return s;
}

// Point light indirect specular lighting
vec3 RenderPointLightShadow(vec3 position, vec3 normal, vec3 view, vec3 albedo, float ao, vec3 lightPosition, vec3 lightColor, samplerCube shadowMap, float shadowMapNear)
{
	vec3 fLambert = albedo / PI;

	// Per light radiance
	vec3 toLight = lightPosition - position;
	vec3 wi = normalize(toLight);

	float distanceSq = dot(toLight, toLight);
	vec3 radiance = L(lightColor, distanceSq);

	float ndotwi = max(dot(wi, normal), 0.0);
	float shadow = CalculatePointShadow(position, lightPosition, shadowMap, shadowMapNear);

	vec3 s = fLambert * radiance * ndotwi * ao * shadow;

	return s;
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

    vec3 lightS = vec3_splat(0.0);
    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        vec3 lightPosition = u_lightPosition[i].xyz;
        vec3 lightColor = u_lightColor[i].rgb;

        if (i == 0) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear0[0]);
        if (i == 1) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear0[1]);
        if (i == 2) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear0[2]);
        if (i == 3) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear0[3]);
        if (i == 4) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear1[0]);
        if (i == 5) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear1[1]);
        if (i == 6) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear1[2]);
        if (i == 7) lightS += RenderPointLightShadow(position, normal, view, albedo, ao, lightPosition, lightColor, s_lightShadowMap0, u_lightShadowMapNear1[3]);
    }

    gl_FragColor = vec4(lightS, 1.0);

    if (positionW.a < 0.5)
        discard;
}
