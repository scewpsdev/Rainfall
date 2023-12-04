$input v_texcoord0

#include "../common/common.shader"

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


// Simulates microfacet model (Trowbridge-Reitz GGX)
float normalDistribution(vec3 normal, vec3 h, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float ndoth = max(dot(normal, h), 0.0);

	float denom = ndoth * ndoth * (a2 - 1.0) + 1.0;

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

// Variant fresnel equation taking the roughness into account;
vec3 fresnel2(float hdotv, vec3 f0, float roughness)
{
	//return f0 + (1.0 - f0) * pow(clamp(1.0 - hdotv, 0.0, 1.0), 5.0);
	return f0 + (max(vec3_splat(1.0 - roughness), f0) - f0) * pow(1.0 - hdotv, 5.0);
}

// Radiance calculation for radial flux over the angle w
vec3 L(vec3 color, float distanceSq)
{
	float maxBrightness = 1.0;
	float attenuation = 1.0 / (1.0 / maxBrightness + distanceSq);
	vec3 radiance = color * attenuation;

	return radiance;
}

// Point light indirect specular lighting
vec3 RenderPointLight(vec3 position, vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao, vec3 lightPosition, vec3 lightColor)
{
	vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);
	vec3 fLambert = albedo * PI;

	// Per light radiance
	vec3 toLight = lightPosition - position;
	vec3 wi = normalize(toLight);
	vec3 h = normalize(view + wi);

	float distanceSq = dot(toLight, toLight);
	vec3 radiance = L(lightColor, distanceSq);

	// Cook-Torrance BRDF
	float d = normalDistribution(normal, h, roughness);
	float g = geometrySmith(normal, view, wi, roughness);
	vec3 f = fresnel2(max(dot(h, view), 0.0), f0, roughness);
	vec3 numerator = d * f * g;
	float denominator = 4.0 * max(dot(view, normal), 0.0) * max(dot(wi, normal), 0.0) + 0.0001;
	vec3 specular = numerator / max(denominator, 0.0001);
	
	vec3 ks = f;
	vec3 kd = (1.0 - ks) * (1.0 - metallic);

	float ndotwi = max(dot(wi, normal), 0.0);
	float shadow = 1.0; // Shadow mapping

	vec3 s = (fLambert * kd * ao) * radiance * ndotwi * shadow;

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

	float ao = 1.0 - texture2D(s_ambientOcclusion, v_texcoord0).r;
	
	position.x = floor(position.x * 16) / 16.0;
	position.y = floor(position.y * 16) / 16.0;
	position.z = floor(position.z * 16) / 16.0;

	vec3 toCamera = u_cameraPosition.xyz - position;
	float distance = length(toCamera);

	vec3 lightS = vec3_splat(0.0);
	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		vec3 lightPosition = u_lightPosition[i].xyz;
		vec3 lightColor = u_lightColor[i].rgb;
		lightS += RenderPointLight(position, normal, vec3(0.0, 1.0, 0.0), albedo, roughness, metallic, ao, lightPosition, lightColor);
	}

	gl_FragColor = vec4(lightS, 1.0);

	if (positionW.a < 0.5)
		discard;
}
