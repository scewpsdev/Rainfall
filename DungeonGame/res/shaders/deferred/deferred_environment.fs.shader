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


bool BoxContainsPosition(vec3 position, vec3 size, vec3 p)
{
	float x0 = position.x - 0.5 * size.x;
	float y0 = position.y - 0.5 * size.y;
	float z0 = position.z - 0.5 * size.z;
	float x1 = position.x + 0.5 * size.x;
	float y1 = position.y + 0.5 * size.y;
	float z1 = position.z + 0.5 * size.z;
	return p.x >= x0 && p.x <= x1 && p.y >= y0 && p.y <= y1 && p.z >= z0 && p.z <= z1;
}

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

	return -1.0;
}

/*
vec4 SampleCubemapParallax(samplerCube cubemap, vec3 direction, vec3 cubemapPosition, vec3 cubemapSize, vec3 cubemapOrigin, vec3 position)
{
	vec3 boxMax = cubemapPosition + 0.5 * cubemapSize;
	vec3 boxMin = cubemapPosition - 0.5 * cubemapSize;

	vec3 firstPlaneIntersect = (boxMax - position) / direction;
	vec3 secondPlaneIntersect = (boxMin - position) / direction;
	vec3 furthestPlane = max(firstPlaneIntersect, secondPlaneIntersect);
	float distance = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);

	vec3 intersection = position + direction * distance;
	vec3 boxToIntersection = intersection - cubemapOrigin;

	return textureCube(cubemap, boxToIntersection);
}
*/

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

	vec3 ambient = RenderEnvironmentMap(normal, view, albedo, roughness, metallic, ao, s_environmentMap, u_environmentMapIntensities[0]);

	if (BoxContainsPosition(u_reflectionProbePosition0, u_reflectionProbeSize0 + 2 * vec3_splat(ENVIRMAP_FADEOUT_DISTANCE), position))
	{
		vec3 envirAmbient = RenderEnvironmentMapParallax(position, normal, view, albedo, roughness, metallic, ao, s_reflectionProbe0, u_reflectionProbePosition0, u_reflectionProbeSize0, u_reflectionProbeOrigin0, 1.0);
		float distanceToBox = DistanceToBox(u_reflectionProbePosition0, u_reflectionProbeSize0, position);
		float fadeOut = distanceToBox != -1.0 ? max(1.0 - distanceToBox / ENVIRMAP_FADEOUT_DISTANCE, 0.0) : 1.0;
		ambient = mix(ambient, envirAmbient, fadeOut);
	}
	if (BoxContainsPosition(u_reflectionProbePosition1, u_reflectionProbeSize1 + 2 * vec3_splat(ENVIRMAP_FADEOUT_DISTANCE), position))
	{
		vec3 envirAmbient = RenderEnvironmentMapParallax(position, normal, view, albedo, roughness, metallic, ao, s_reflectionProbe1, u_reflectionProbePosition1, u_reflectionProbeSize1, u_reflectionProbeOrigin1, 1.0);
		float distanceToBox = DistanceToBox(u_reflectionProbePosition1, u_reflectionProbeSize1, position);
		float fadeOut = distanceToBox != -1.0 ? max(1.0 - distanceToBox / ENVIRMAP_FADEOUT_DISTANCE, 0.0) : 1.0;
		ambient = mix(ambient, envirAmbient, fadeOut);
	}
	if (BoxContainsPosition(u_reflectionProbePosition2, u_reflectionProbeSize2 + 2 * vec3_splat(ENVIRMAP_FADEOUT_DISTANCE), position))
	{
		vec3 envirAmbient = RenderEnvironmentMapParallax(position, normal, view, albedo, roughness, metallic, ao, s_reflectionProbe2, u_reflectionProbePosition2, u_reflectionProbeSize2, u_reflectionProbeOrigin2, 1.0);
		float distanceToBox = DistanceToBox(u_reflectionProbePosition2, u_reflectionProbeSize2, position);
		float fadeOut = distanceToBox != -1.0 ? max(1.0 - distanceToBox / ENVIRMAP_FADEOUT_DISTANCE, 0.0) : 1.0;
		ambient = mix(ambient, envirAmbient, fadeOut);
	}
	if (BoxContainsPosition(u_reflectionProbePosition3, u_reflectionProbeSize3 + 2 * vec3_splat(ENVIRMAP_FADEOUT_DISTANCE), position))
	{
		vec3 envirAmbient = RenderEnvironmentMapParallax(position, normal, view, albedo, roughness, metallic, ao, s_reflectionProbe3, u_reflectionProbePosition3, u_reflectionProbeSize3, u_reflectionProbeOrigin3, 1.0);
		float distanceToBox = DistanceToBox(u_reflectionProbePosition3, u_reflectionProbeSize3, position);
		float fadeOut = distanceToBox != -1.0 ? max(1.0 - distanceToBox / ENVIRMAP_FADEOUT_DISTANCE, 0.0) : 1.0;
		ambient = mix(ambient, envirAmbient, fadeOut);
	}
	
	gl_FragColor = vec4(ambient, 1.0);

	if (positionW.a < 0.5)
		discard;
}
