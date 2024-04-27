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


float DistanceToBox(vec3 position, vec3 size, vec3 p)
{
	p -= position;

	vec3 scale = size / 2.0;
	p /= scale;

	vec3 ap = abs(p);
	if (ap.x <= 1.0)
	{
		if (ap.y <= 1.0)
		{
			if (ap.z > 1.0)
				return (ap.z - 1.0) * scale.z;
		}
		else
		{
			if (ap.z <= 1.0)
				return (ap.y - 1.0) * scale.y;
			else
				return length((ap.yz - 1.0) * scale.yz);
		}
	}
	else
	{
		if (ap.y <= 1.0)
		{
			if (ap.z <= 1.0)
				return (ap.x - 1.0) * scale.x;
			else
				return length((ap.xz - 1.0) * scale.xz);
		}
		else
		{
			if (ap.z <= 1.0)
				return length((ap.xy - 1.0) * scale.xy);
			else
				return length((ap - 1.0) * scale);
		}
	}

	return 0.0;
}

#define ENVIRMAP_FADEOUT_DISTANCE 0.998

vec4 CalculateReflectionWeights(vec3 position,
	vec3 reflectionPosition0, vec3 reflectionSize0, vec3 reflectionOrigin0,
	vec3 reflectionPosition1, vec3 reflectionSize1, vec3 reflectionOrigin1,
	vec3 reflectionPosition2, vec3 reflectionSize2, vec3 reflectionOrigin2,
	vec3 reflectionPosition3, vec3 reflectionSize3, vec3 reflectionOrigin3)
{
	float distance0 = DistanceToBox(reflectionPosition0, reflectionSize0, position);
	float fadeOut0 = max(1.0 - distance0 / ENVIRMAP_FADEOUT_DISTANCE, 0.0);

	float distance1 = DistanceToBox(reflectionPosition1, reflectionSize1, position);
	float fadeOut1 = max(1.0 - distance1 / ENVIRMAP_FADEOUT_DISTANCE, 0.0);

	float distance2 = DistanceToBox(reflectionPosition2, reflectionSize2, position);
	float fadeOut2 = max(1.0 - distance2 / ENVIRMAP_FADEOUT_DISTANCE, 0.0);

	float distance3 = DistanceToBox(reflectionPosition3, reflectionSize3, position);
	float fadeOut3 = max(1.0 - distance3 / ENVIRMAP_FADEOUT_DISTANCE, 0.0);

	return vec4(fadeOut0, fadeOut1, fadeOut2, fadeOut3);
}

vec4 SampleCubemapParallax(vec3 position, vec3 direction, float lod, samplerCube cubemap, vec3 cubemapPosition, vec3 cubemapSize, vec3 cubemapOrigin)
{
	vec3 boxMax = cubemapPosition + 0.5 * cubemapSize;
	vec3 boxMin = cubemapPosition - 0.5 * cubemapSize;

	vec3 firstPlaneIntersect = (boxMax - position) / direction;
	vec3 secondPlaneIntersect = (boxMin - position) / direction;

	vec3 furthestPlane = max(firstPlaneIntersect, secondPlaneIntersect);
	float distance = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);
	distance = abs(distance);

	vec3 intersection = position + direction * distance;
	vec3 boxToIntersection = intersection - cubemapOrigin;

	return textureCubeLod(cubemap, boxToIntersection, lod);
}

vec3 SampleEnvironmentIrradiance(vec3 position, vec3 normal,
	samplerCube environmentMap, float environmentMapIntensity,
	samplerCube reflection0, vec3 reflectionPosition0, vec3 reflectionSize0, vec3 reflectionOrigin0, float reflectionIntensity0,
	samplerCube reflection1, vec3 reflectionPosition1, vec3 reflectionSize1, vec3 reflectionOrigin1, float reflectionIntensity1,
	samplerCube reflection2, vec3 reflectionPosition2, vec3 reflectionSize2, vec3 reflectionOrigin2, float reflectionIntensity2,
	samplerCube reflection3, vec3 reflectionPosition3, vec3 reflectionSize3, vec3 reflectionOrigin3, float reflectionIntensity3)
{
	vec3 irradiance = textureCubeLod(environmentMap, normal, log2(textureSize(environmentMap, 0).x)).rgb * environmentMapIntensity;

	vec3 reflectionIrradiance0 = SampleCubemapParallax(position, normal, log2(textureSize(reflection0, 0).x), reflection0, reflectionPosition0, reflectionSize0, reflectionOrigin0).rgb * reflectionIntensity0;
	vec3 reflectionIrradiance1 = SampleCubemapParallax(position, normal, log2(textureSize(reflection1, 0).x), reflection1, reflectionPosition1, reflectionSize1, reflectionOrigin1).rgb * reflectionIntensity1;
	vec3 reflectionIrradiance2 = SampleCubemapParallax(position, normal, log2(textureSize(reflection2, 0).x), reflection2, reflectionPosition2, reflectionSize2, reflectionOrigin2).rgb * reflectionIntensity2;
	vec3 reflectionIrradiance3 = SampleCubemapParallax(position, normal, log2(textureSize(reflection3, 0).x), reflection3, reflectionPosition3, reflectionSize3, reflectionOrigin3).rgb * reflectionIntensity3;

	vec4 reflectionWeights = CalculateReflectionWeights(position,
		reflectionPosition0, reflectionSize0, reflectionOrigin0,
		reflectionPosition1, reflectionSize1, reflectionOrigin1,
		reflectionPosition2, reflectionSize2, reflectionOrigin2,
		reflectionPosition3, reflectionSize3, reflectionOrigin3);
	float skyboxWeight = 1 - reflectionWeights[0] - reflectionWeights[1] - reflectionWeights[2] - reflectionWeights[3];

	irradiance = skyboxWeight * irradiance +
		reflectionWeights[0] * reflectionIrradiance0 + 
		reflectionWeights[1] * reflectionIrradiance1 + 
		reflectionWeights[2] * reflectionIrradiance2 + 
		reflectionWeights[3] * reflectionIrradiance3;

	return irradiance;
}

vec3 SampleEnvironmentPrefiltered(vec3 position, vec3 normal, vec3 view, float roughness,
	samplerCube environmentMap, float environmentMapIntensity,
	samplerCube reflection0, vec3 reflectionPosition0, vec3 reflectionSize0, vec3 reflectionOrigin0, float reflectionIntensity0,
	samplerCube reflection1, vec3 reflectionPosition1, vec3 reflectionSize1, vec3 reflectionOrigin1, float reflectionIntensity1,
	samplerCube reflection2, vec3 reflectionPosition2, vec3 reflectionSize2, vec3 reflectionOrigin2, float reflectionIntensity2,
	samplerCube reflection3, vec3 reflectionPosition3, vec3 reflectionSize3, vec3 reflectionOrigin3, float reflectionIntensity3)
{
	vec3 r = reflect(-view, normal);
	float lodFactor = 1.0 - exp(-roughness * 12);

	vec3 prefiltered = textureCubeLod(environmentMap, r, lodFactor * log2(textureSize(environmentMap, 0).x)).rgb * environmentMapIntensity;

	vec3 reflectionPrefiltered0 = SampleCubemapParallax(position, r, lodFactor * log2(textureSize(reflection0, 0).x), reflection0, reflectionPosition0, reflectionSize0, reflectionOrigin0).rgb * reflectionIntensity0;
	vec3 reflectionPrefiltered1 = SampleCubemapParallax(position, r, lodFactor * log2(textureSize(reflection1, 0).x), reflection1, reflectionPosition1, reflectionSize1, reflectionOrigin1).rgb * reflectionIntensity1;
	vec3 reflectionPrefiltered2 = SampleCubemapParallax(position, r, lodFactor * log2(textureSize(reflection2, 0).x), reflection2, reflectionPosition2, reflectionSize2, reflectionOrigin2).rgb * reflectionIntensity2;
	vec3 reflectionPrefiltered3 = SampleCubemapParallax(position, r, lodFactor * log2(textureSize(reflection3, 0).x), reflection3, reflectionPosition3, reflectionSize3, reflectionOrigin3).rgb * reflectionIntensity3;

	vec4 reflectionWeights = CalculateReflectionWeights(position,
		reflectionPosition0, reflectionSize0, reflectionOrigin0,
		reflectionPosition1, reflectionSize1, reflectionOrigin1,
		reflectionPosition2, reflectionSize2, reflectionOrigin2,
		reflectionPosition3, reflectionSize3, reflectionOrigin3);
	float skyboxWeight = 1 - reflectionWeights[0] - reflectionWeights[1] - reflectionWeights[2] - reflectionWeights[3];

	prefiltered = skyboxWeight * prefiltered +
		reflectionWeights[0] * reflectionPrefiltered0 + 
		reflectionWeights[1] * reflectionPrefiltered1 + 
		reflectionWeights[2] * reflectionPrefiltered2 + 
		reflectionWeights[3] * reflectionPrefiltered3;

	return prefiltered;
}

vec3 RenderEnvironment(vec3 position, vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao,
	samplerCube environmentMap, float environmentMapIntensity,
	samplerCube reflection0, vec3 reflectionPosition0, vec3 reflectionSize0, vec3 reflectionOrigin0, vec3 reflectionIntensity0,
	samplerCube reflection1, vec3 reflectionPosition1, vec3 reflectionSize1, vec3 reflectionOrigin1, vec3 reflectionIntensity1,
	samplerCube reflection2, vec3 reflectionPosition2, vec3 reflectionSize2, vec3 reflectionOrigin2, vec3 reflectionIntensity2,
	samplerCube reflection3, vec3 reflectionPosition3, vec3 reflectionSize3, vec3 reflectionOrigin3, vec3 reflectionIntensity3)
{
	vec3 irradiance = SampleEnvironmentIrradiance(position, normal,
		environmentMap, environmentMapIntensity,
		reflection0, reflectionPosition0, reflectionSize0, reflectionOrigin0, reflectionIntensity0,
		reflection1, reflectionPosition1, reflectionSize1, reflectionOrigin1, reflectionIntensity1,
		reflection2, reflectionPosition2, reflectionSize2, reflectionOrigin2, reflectionIntensity2,
		reflection3, reflectionPosition3, reflectionSize3, reflectionOrigin3, reflectionIntensity3);

	vec3 diffuse = irradiance * albedo;

	vec3 ks = vec3_splat(1.0 - roughness) * pow(1.0 - dot(normal, view), 5.0);
	vec3 kd = 1.0 - ks;

	vec3 prefiltered = SampleEnvironmentPrefiltered(position, normal, view, roughness,
		environmentMap, environmentMapIntensity,
		reflection0, reflectionPosition0, reflectionSize0, reflectionOrigin0, reflectionIntensity0,
		reflection1, reflectionPosition1, reflectionSize1, reflectionOrigin1, reflectionIntensity1,
		reflection2, reflectionPosition2, reflectionSize2, reflectionOrigin2, reflectionIntensity2,
		reflection3, reflectionPosition3, reflectionSize3, reflectionOrigin3, reflectionIntensity3);

	vec2 brdfInteg = vec2(1.0, 0.0);
	vec3 specular = prefiltered * (ks * brdfInteg.r + brdfInteg.g);

	vec3 ambient = kd * diffuse * ao + specular * ao;

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

	vec3 ambient = RenderEnvironment(position, normal, view, albedo, roughness, metallic, ao, s_environmentMap, u_environmentMapIntensities[0],
		s_reflectionProbe0, u_reflectionProbePosition0, u_reflectionProbeSize0, u_reflectionProbeOrigin0, 1.0,
		s_reflectionProbe1, u_reflectionProbePosition1, u_reflectionProbeSize1, u_reflectionProbeOrigin1, 1.0,
		s_reflectionProbe2, u_reflectionProbePosition2, u_reflectionProbeSize2, u_reflectionProbeOrigin2, 1.0,
		s_reflectionProbe3, u_reflectionProbePosition3, u_reflectionProbeSize3, u_reflectionProbeOrigin3, 1.0);

	vec3 final = ambient;
	final = textureCubeLod(s_reflectionProbe0, normal, 0);
	
    gl_FragColor = vec4(final, 1.0);

    if (positionW.a < 0.5)
        discard;
}
