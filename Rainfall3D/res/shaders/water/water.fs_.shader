$input v_position, v_normal

#include "../common/common.shader"
#include "wave.shader"


#define PI 3.141592653589793

//#define WATER_COLOR SRGBToLinear(vec3(28, 71, 68) / 255.0) * 4.0
//#define SCATTER_COLOR SRGBToLinear(vec3(28, 71, 68) / 255.0) * 2.0
#define WATER_COLOR vec3(0.1, 0.5, 1.0)
#define SCATTER_COLOR vec3(0.1, 0.5, 1.0)


uniform vec4 u_cameraPosition;

uniform vec4 u_directionalLightDirection;
uniform vec4 u_directionalLightColor;

uniform vec4 u_directionalLightFarPlane;
SAMPLER2DSHADOW(s_directionalLightShadowMap, 0);
uniform mat4 u_directionalLightToLightSpace;

SAMPLERCUBE(s_environmentMap, 1);


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
	vec3 albedo = WATER_COLOR;
	vec3 position = v_position;
	vec3 normal = normalize(v_normal);
	vec3 cameraPosition = u_cameraPosition.xyz;
	float time = u_cameraPosition.w;

	float roughness = 0.03;
	
	int numWaves = 64;

	vec3 animatedPosition, animatedNormal;
	animateWater(position.xz, time, numWaves, animatedPosition, animatedNormal);
	position += animatedPosition;
	normal = animatedNormal;

	vec3 toLight = -u_directionalLightDirection.xyz;
	vec3 view = normalize(cameraPosition - position);

	float ndotl = max(dot(normal, toLight), 0.0);
	vec3 diffuse = ndotl * u_directionalLightColor.rgb;
	//diffuse = textureCubeLod(s_environmentMap, normal, 12.0).rgb;

	/*
	float hdotv = ;
	float lightFresnel = pow(clamp(1.0 - max(dot(h, view), 0.0), 0.0, 1.0), 5.0);
	float specularFactor = pow(max(dot(h, normal), 0.0), 400.0);
	vec3 specular = lightFresnel * specularFactor * u_directionalLightColor.rgb;
	*/

	// Cook-Torrance BRDF
	vec3 h = normalize(toLight + view);
	vec3 wi = toLight;
	float d = normalDistribution(normal, h, roughness);
	float g = geometrySmith(normal, view, wi, roughness);
	float f = fresnel2(max(dot(h, view), 0.0));
	vec3 numerator = d * f * g;
	float denominator = 4.0 * max(dot(view, normal), 0.0) * max(dot(wi, normal), 0.0);
	vec3 specular = numerator / max(denominator, 0.001);

	float scatterStrength = 0.01;
	float wavePeakScatterStrength = 0.1;
	float scatterShadowStrength = 0.01;
	float H = animatedPosition.y > 0.8 ? max(animatedPosition.y - 0.8, 0.0) / 0.2 : 0.0;
	float k1 = wavePeakScatterStrength * H * pow(max(dot(toLight, -view), 0.0), 4.0) * pow(0.5 - 0.5 * dot(toLight, normal), 3.0);
	float k2 = scatterStrength * pow(max(dot(view, normal), 0.0), 2.0);
	float k3 = scatterShadowStrength * ndotl;
	float k4 = 0.01;
	float lightMask = SmithMaskingBeckmann(h, toLight, roughness);
	vec3 scatter = (k1 + k2) * SCATTER_COLOR * u_directionalLightColor.rgb * rcp(1.0 + lightMask);
	scatter += k3 * SCATTER_COLOR * u_directionalLightColor.rgb + k4 * albedo * textureCubeLod(s_environmentMap, normal, 12.0).rgb;

	vec3 reflected = reflect(-view, normal);
	float vdotn = max(dot(view, normal), 0.0);
	vec3 fresnel = pow(clamp(1.0 - vdotn, 0.0, 1.0), 5.0);
	vec3 environment = sampleEnvironment(s_environmentMap, reflected);

	vec3 final = scatter + specular + environment * fresnel; //albedo + scatter + specular + environment * fresnel * 0.3; //mix(diffuse, environment, fresnel) + specular;

	gl_FragColor = vec4(final, 1.0);
}
