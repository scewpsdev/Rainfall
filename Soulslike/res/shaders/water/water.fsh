$input v_position, v_normal, v_localposition, v_texcoord0

#include "../common/common.shader"
#include "wave.shader"


#define PI 3.141592653589793

#define AMBIENT_COLOR SRGBToLinear(vec3(9, 83, 82) / 255.0) * 0.005 //0.02
#define SCATTER_COLOR SRGBToLinear(vec3(64, 157, 150) / 255.0) * 0.5


uniform vec4 u_cameraPosition;
uniform vec4 u_materialData0;

uniform vec4 u_directionalLightDirection;
uniform vec4 u_directionalLightColor;

uniform vec4 u_directionalLightFarPlane;
SAMPLER2DSHADOW(s_directionalLightShadowMap, 5);
uniform mat4 u_directionalLightToLightSpace;

SAMPLER2D(s_deferredFrame, 0);
SAMPLER2D(s_deferredDepth, 1);

SAMPLERCUBE(s_environmentMap, 4);


vec3 sampleEnvironment(samplerCube environmentMap, vec3 dir)
{
	//float groundBlend = max(dot(dir, vec3(0, 1, 0)), 0.0);
	//groundBlend = dir.y < 0.0 ? 1.0 : 0.0; //1.0 - pow(1.0 - groundBlend, 50.0);
	//float groundBlend = dir.y > 0.0 ? 1.0 - pow(1.0 - dir.y, 20.0) : 0.0;
	if (dir.y > 0.0)
		return textureCube(environmentMap, dir).rgb;
	else
	{
		vec3 normal = vec3(0, 1, 0);
		float vdotn = max(dot(-dir, normal), 0.0);
		vec3 fresnel = pow(clamp(1.0 - vdotn, 0.0, 1.0), 5.0);

		vec3 reflected = reflect(dir, normal);
		vec3 sample = textureCube(environmentMap, reflected).rgb;
		return sample * fresnel; //groundColor + fresnel * sample;
	}
	//vec3 sample = textureCube(environmentMap, dir).rgb * 1.5;
	//vec3 groundColor = vec3(0.1, 0.6, 1.0) * 0.02;
	//return mix(groundColor, sample, groundBlend);
}

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
	vec3 position = v_position;
	vec3 normal = normalize(v_normal);
	vec3 cameraPosition = u_cameraPosition.xyz;
	float time = u_cameraPosition.w;

	int numWaves = 64;

	float amplitude = u_materialData0.x;
	float frequency = u_materialData0.y;

	vec3 animatedPosition, animatedNormal;
	animateWater(position.xz, time, numWaves, amplitude, frequency, animatedPosition, animatedNormal);
	position += animatedPosition;
	normal = animatedNormal;

	vec3 toLight = -u_directionalLightDirection.xyz;
	vec3 view = normalize(cameraPosition - position);

	float k1 = 0.5;
	float k2 = 0.1;
	float k3 = 0.1;

	float roughness = 0.03;
	vec3 h = normalize(toLight + view);
	float lightMask = SmithMaskingBeckmann(h, toLight, roughness);
	float scatter0 = k1 * max(v_localposition.y, 0) * pow(max(dot(toLight, -view), 0.0), 4) * pow((0.5 - 0.5 * dot(toLight, normal)), 3);

	float scatter1 = k2 * pow(max(dot(normal, view), 0.0), 2);
	float scatter2 = k3 * max(dot(normal, toLight), 0.0);
	vec3 scatter = ((scatter0 + scatter1) * rcp(1.0 + lightMask) + scatter2) * SCATTER_COLOR * u_directionalLightColor.rgb + AMBIENT_COLOR;

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
	vec3 fresnel = pow(clamp(1.0 - vdotn, 0.0, 1.0), 5.0);
	vec3 environment = sampleEnvironment(s_environmentMap, reflected) * 1 * fresnel;

	vec2 texcoord = gl_FragCoord.xy * u_viewTexel.xy;
	float geometryDepth = texture2D(s_deferredDepth, texcoord).x;
	float geometryDistance = depthToDistance(geometryDepth, 0.05, 500);
	float waterDepth = gl_FragCoord.z;
	float waterDistance = depthToDistance(waterDepth, 0.05, 500);
	float distance = geometryDistance - waterDistance;
	float alpha = 1 - exp(-distance * 0.5);

	vec3 geometryColor = texture2D(s_deferredFrame, texcoord).rgb;

	vec3 final = mix(geometryColor, scatter, alpha) + specular + environment;

	gl_FragColor = vec4(final, 1.0);
}
