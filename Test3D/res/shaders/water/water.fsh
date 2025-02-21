$input v_position, v_normal, v_localposition

#include "../common/common.shader"
#include "wave.shader"


#define PI 3.141592653589793

#define AMBIENT_COLOR SRGBToLinear(vec3(9, 83, 82) / 255.0) * 0.02
#define SCATTER_COLOR SRGBToLinear(vec3(64, 157, 150) / 255.0) * 0.5


uniform vec4 u_materialData0;
#define u_cameraPosition u_materialData0.xyz

uniform vec4 u_materialData1;
#define u_cameraNear u_materialData1.x
#define u_cameraFar u_materialData1.y
#define u_time u_materialData1.w

uniform vec4 u_materialData2;
#define u_directionalLightDirection u_materialData2.xyz

uniform vec4 u_materialData3;
#define u_directionalLightColor u_materialData3.xyz

SAMPLER2D(s_deferredFrame, 0);
SAMPLER2D(s_deferredDepth, 1);

SAMPLERCUBE(s_environmentMap, 2);


// Simulates microfacet model (Trowbridge-Reitz GGX)
float normalDistribution(vec3 normal, vec3 h, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float ndoth = max(dot(normal, h), 0.0);

	float denom = max(ndoth * ndoth * (a2 - 1.0) + 1.0, 0.000001);

	return a2 / (PI * denom * denom);
}

// Self shadowing of microfacets (Schlick-GGX)
float geometryGGX(float ndotv, float k)
{
	return ndotv / (ndotv * (1.0 - k) + k);
}

float geometrySmith(vec3 normal, vec3 view, vec3 wi, float roughness)
{
	// Roughness remapping
	float r = roughness + 1.0;
	float k = r * r / 8.0;

	float ndotv = max(dot(normal, view), 0.0); // TODO precalculate this
	float ndotl = max(dot(normal, wi), 0.0); // TODO precalculate this

	return geometryGGX(ndotv, k) * geometryGGX(ndotl, k);
}

// Simplified fresnel function
float fresnel2(float hdotv)
{
	return pow(clamp(1.0 - hdotv, 0.0, 1.0), 5.0);
	//return f0 + (max(vec3_splat(1.0 - roughness), f0) - f0) * pow(1.0 - hdotv, 5.0);
}

float SmithMaskingBeckmann(float3 H, float3 S, float roughness)
{
	float hdots = max(0.001, max(dot(H, S), 0.0));
	float a = hdots / max(roughness * sqrt(1 - hdots * hdots), 0.001);
	float a2 = a * a;

	return a < 1.6f ? (1.0f - 1.259f * a + 0.396f * a2) / (3.535f * a + 2.181 * a2) : 0.0f;
}

void main()
{
	vec3 position = v_position.xyz;
	float depth = v_position.w;
	vec3 normal = normalize(v_normal);

	vec3 toLight = -u_directionalLightDirection;
	vec3 toCamera = u_cameraPosition - position;
	vec3 view = normalize(toCamera);
	float distance = length(toCamera);// depthToDistance(depth, u_cameraNear, u_cameraFar);

	int numWaves = 48;

	vec3 animatedPosition, animatedNormal;
	animateWater(position.xz, u_time, numWaves, animatedPosition, animatedNormal);

	position += animatedPosition;
	normal = animatedNormal;
	//normal = mix(normal, animatedNormal, 1.0 / (1 + distanceSq * 0.001));

	float k1 = 0.5;
	float k2 = 0.1;
	float k3 = 0.1;

	float roughness = 0.03;
	vec3 h = normalize(toLight + view);
	float lightMask = SmithMaskingBeckmann(h, toLight, roughness);
	float scatter0 = k1 * max(v_localposition.y, 0) * pow(max(dot(toLight, -view), 0.0), 4) * pow((0.5 - 0.5 * dot(toLight, normal)), 3);

	float scatter1 = k2 * pow(max(dot(normal, view), 0.0), 2);
	float scatter2 = k3 * max(dot(normal, toLight), 0.0);
	vec3 scatter = ((scatter0 + scatter1) * rcp(1.0 + lightMask) + scatter2) * SCATTER_COLOR * u_directionalLightColor + AMBIENT_COLOR;

	// Cook-Torrance BRDF
	vec3 wi = toLight;
	float d = normalDistribution(normal, h, roughness);
	float g = geometrySmith(normal, view, wi, roughness);
	float f = fresnel2(max(dot(h, view), 0.0));
	vec3 numerator = d * f * g;
	float denominator = 4.0 * max(dot(view, normal), 0.0) * max(dot(wi, normal), 0.0);
	vec3 specular = numerator / max(denominator, 0.001);

	vec3 reflected = reflect(-view, normal);
	float vdotn = max(dot(view, normal), 0.0);
	vec3 fresnel = pow(clamp(1.0 - vdotn, 0.0, 1.0), 2.0);
	vec3 environment = textureCube(s_environmentMap, reflected).rgb * fresnel;

	//vec3 refracted = refract(-view, normal, 1.33);

	float sceneDepth = texture2D(s_deferredDepth, gl_FragCoord.xy * u_viewTexel.xy).r;
	float sceneDistance = depthToDistance(sceneDepth, u_cameraNear, u_cameraFar);
	float distanceUnderwater = sceneDistance - distance;

	vec2 distortion = normal.xz * 0.2;
	vec2 refractionSamplePoint = gl_FragCoord.xy * u_viewTexel.xy + distortion;

	vec3 refractedColor = texture2D(s_deferredFrame, refractionSamplePoint).rgb * (1 - fresnel);
	float underwaterFog = exp(-distanceUnderwater * 0.05);
	refractedColor *= underwaterFog;
	//refractedColor = vec3(1, 1, 1) * distanceUnderwater * 0.01;

	vec3 final = scatter * 0.5 + specular + environment * 0.5 + refractedColor * 0.1;
	
	float fog = exp(-distance * distance * 0.000001);
	vec3 fogColor = textureCubeLod(s_environmentMap, -view, 12).rgb;
	final = mix(fogColor, final, fog);

	gl_FragColor = vec4(final, 1.0);
}
