$input v_position, v_normal, v_texcoord0

#include "../common/common.shader"

#define MAX_LIGHTS 16

#define DEFAULT_ALBEDO vec4(1.0, 1.0, 1.0, 1.0)


SAMPLER2D(s_diffuse, 0);
SAMPLER2D(s_normal, 1);
SAMPLER2D(s_roughness, 2);
SAMPLER2D(s_metallic, 3);
SAMPLER2D(s_emissive, 4);

uniform vec4 u_attributeInfo;
uniform vec4 u_materialInfo;
uniform vec4 u_materialInfo2;
uniform vec4 u_materialInfo3;
uniform vec4 u_materialInfo4;

uniform vec4 u_cameraPosition;
uniform vec4 u_lightPosition[MAX_LIGHTS];
uniform vec4 u_lightColor[MAX_LIGHTS];

uniform vec4 u_directionalLightDirection;
uniform vec4 u_directionalLightColor;

uniform vec4 u_directionalLightFarPlane;
SAMPLER2DSHADOW(s_directionalLightShadowMap, 5);
uniform mat4 u_directionalLightToLightSpace;


vec3 attenuate(vec3 color, float distanceSq)
{
	float maxBrightness = 1.0;
	float attenuation = 1.0 / (1.0 / maxBrightness + distanceSq);
	vec3 radiance = color * attenuation;

	return radiance;
}

vec3 RenderPointLightSimple(vec3 position, vec3 normal, vec3 albedo, vec3 lightPosition, vec3 lightColor)
{
	vec3 toLight = lightPosition - position;
	float distanceSq = dot(toLight, toLight);
	float distance = sqrt(distanceSq);
	vec3 lightDir = toLight / distance;
	float diffuseFactor = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = albedo * diffuseFactor * lightColor;

	diffuse = attenuate(diffuse, distanceSq);

	return diffuse;
}

float CalculateDirectionalShadowSimple(vec3 position, float distance, sampler2DShadow shadowMap, float shadowMapFar, mat4 toLightSpace)
{
	const float SHADOW_MAP_EPSILON = 0.001;

	vec4 lightSpacePosition = mul(toLightSpace, vec4(position, 1.0));
	vec3 projectedCoords = lightSpacePosition.xyz / lightSpacePosition.w;
	vec2 sampleCoords = 0.5 * projectedCoords.xy * vec2(1.0, -1.0) + 0.5;

	ivec2 shadowMapSize = textureSize(shadowMap, 0);

	float result = shadow2D(shadowMap, vec3(sampleCoords.xy, projectedCoords.z - SHADOW_MAP_EPSILON));

	float fadeOut = clamp(remap(distance / shadowMapFar, 0.9, 1.0, 1.0, 0.0), 0.0, 1.0);
	result = 1.0 - ((1.0 - result) * fadeOut);

	//result = 0.2 + result * 0.8;

	return result;
}

vec3 RenderDirectionalLightSimple(vec3 position, vec3 normal, float distance, vec3 albedo, vec3 lightDirection, vec3 lightColor, sampler2DShadow shadowMap, float shadowMapFar, mat4 toLightSpace)
{
	float diffuseFactor = max(dot(normal, -lightDirection), 0.0);
	vec3 diffuse = albedo * diffuseFactor * lightColor;

	float shadow = CalculateDirectionalShadowSimple(position, distance, shadowMap, shadowMapFar, toLightSpace);

	return diffuse * shadow;
}

void main()
{
	float hasTexCoords = u_attributeInfo[0];
	float hasDiffuse = u_materialInfo[0];
	float hasNormal = u_materialInfo[1];
	float hasRoughness = u_materialInfo[2];
	float hasMetallic = u_materialInfo[3];
	float hasEmissive = u_materialInfo2[3];
	vec3 color = u_materialInfo2.rgb;
	float metallicFactor = u_materialInfo3[0];
	float roughnessFactor = u_materialInfo3[1];
	vec3 emissionColor = u_materialInfo4.rgb;
	float emissionStrength = u_materialInfo4[3];


	float lod = max(log2(textureSize(s_diffuse, 0).x) - 6, 0);

	vec4 albedo = mix(DEFAULT_ALBEDO, texture2DLod(s_diffuse, v_texcoord0, lod), hasTexCoords * hasDiffuse) * vec4(linearToSRGB(color), 1.0);
	float roughness = mix(roughnessFactor, texture2DLod(s_roughness, v_texcoord0, lod).g, hasTexCoords * hasRoughness);
	float metallic = mix(metallicFactor, texture2DLod(s_metallic, v_texcoord0, lod).b, hasTexCoords * hasMetallic);
	vec3 emissive = mix(emissionColor, texture2DLod(s_emissive, v_texcoord0, lod).rgb, hasTexCoords * hasEmissive);

	vec3 position = v_position;
	vec3 normal = normalize(v_normal);

	vec3 toCamera = u_cameraPosition.xyz - position;
    float distance = length(toCamera);

	vec3 lightS = vec3_splat(0.0);
    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        vec3 lightPosition = u_lightPosition[i].xyz;
        vec3 lightColor = u_lightColor[i].rgb;
        lightS += RenderPointLightSimple(position, normal, albedo, lightPosition, lightColor);
    }

	if (dot(u_directionalLightColor, u_directionalLightColor) > 0.001)
    {
        lightS += RenderDirectionalLightSimple(
			position, normal, distance, albedo, u_directionalLightDirection.xyz, u_directionalLightColor.rgb,
			s_directionalLightShadowMap, u_directionalLightFarPlane[0], u_directionalLightToLightSpace);
    }

	vec3 final = lightS + emissive * emissionStrength;

	gl_FragColor = vec4(lightS, 1.0);
}
