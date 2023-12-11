


#include "../common/common.shader"

#include "shadow_mapping.shader"


#define PI 3.14159265359


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
	float attenuation = 1.0 / (1.0 + distanceSq);
	vec3 radiance = color * attenuation;

	return radiance;
}

// Point light indirect specular lighting
vec3 RenderPointLight(vec3 position, vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao, vec3 lightPosition, vec3 lightColor)
{
	vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);
	vec3 fLambert = albedo / PI; // TODO fix

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

	vec3 s = (specular * ao + fLambert * kd * ao) * radiance * ndotwi * shadow;

	return s;
}

// Directional light indirect specular lighting
vec3 RenderDirectionalLight(vec3 position, vec3 normal, vec3 view, float distance, vec3 albedo, float roughness, float metallic, float ao, vec3 lightDirection, vec3 lightColor,
	sampler2DShadow shadowMap0, float shadowMapFar0, mat4 toLightSpace0,
	sampler2DShadow shadowMap1, float shadowMapFar1, mat4 toLightSpace1,
	sampler2DShadow shadowMap2, float shadowMapFar2, mat4 toLightSpace2,
	vec4 fragCoord)
{
	vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);
	vec3 fLambert = albedo / PI;

	// Per light radiance
	vec3 wi = -lightDirection;
	vec3 h = normalize(view + wi);

	vec3 radiance = lightColor;

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

	// Shadow mapping
	int cascadeID = distance < shadowMapFar0 ? 0 : distance < shadowMapFar1 ? 1 : 2;
	float shadow = 0.0;
	switch (cascadeID)
	{
	case 0: shadow = CalculateDirectionalShadow(position, distance, shadowMap0, shadowMapFar0, toLightSpace0, 0.0, fragCoord); break;
	case 1: shadow = CalculateDirectionalShadow(position, distance, shadowMap1, shadowMapFar1, toLightSpace1, 0.0, fragCoord); break;
	default: shadow = CalculateDirectionalShadow(position, distance, shadowMap2, shadowMapFar2, toLightSpace2, 1.0, fragCoord); break;
	//case 0: radiance = vec3(1, 0, 0); break;
	//case 1: radiance = vec3(0, 1, 0); break;
	//default: radiance = vec3(0, 0, 1); break;
	}

	vec3 s = (specular * ao + fLambert * kd * ao) * radiance * ndotwi; //	 * shadow;

	return s;
}

// Environment mapping
vec3 RenderEnvironmentMap(vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao, samplerCube environmentMap, float environmentMapIntensity)
{
	vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);

	vec3 ks = fresnel2(max(dot(normal, view), 0.0), f0, roughness);
	vec3 kd = (1.0 - ks) * (1.0 - metallic);

	vec3 irradiance = textureCubeLod(environmentMap, normal, 12.0).rgb * environmentMapIntensity;
	vec3 diffuse = irradiance * albedo;

	vec3 r = reflect(-view, normal);
	float maxLod = log2(textureSize(environmentMap, 0).x);
	float minLod = max(maxLod - 8, 0);
	float lodFactor = (1.0 - exp(-roughness * 2.5)) * (maxLod - minLod) + minLod;
	vec3 prefiltered = textureCubeLod(environmentMap, r, lodFactor).rgb * environmentMapIntensity;

	vec2 brdfInteg = vec2(1.0, 0.0);
	vec3 specular = prefiltered * (ks * brdfInteg.r + brdfInteg.g);

	vec3 ambient = kd * diffuse * ao + specular * ao;

	return ambient;
}

vec4 SampleCubemapParallax(samplerCube cubemap, vec3 direction, float lod, vec3 cubemapPosition, vec3 cubemapSize, vec3 cubemapOrigin, vec3 position)
{
	vec3 boxMax = cubemapPosition + 0.5 * cubemapSize;
	vec3 boxMin = cubemapPosition - 0.5 * cubemapSize;

	vec3 firstPlaneIntersect = (boxMax - position) / direction;
	vec3 secondPlaneIntersect = (boxMin - position) / direction;
	vec3 furthestPlane = max(firstPlaneIntersect, secondPlaneIntersect);
	float distance = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);

	vec3 intersection = position + direction * distance;
	vec3 boxToIntersection = intersection - cubemapOrigin;

	return textureCubeLod(cubemap, boxToIntersection, lod);
	//return textureCube(cubemap, direction);
}

// Environment mapping
vec3 RenderEnvironmentMapParallax(vec3 position, vec3 normal, vec3 view, vec3 albedo, float roughness, float metallic, float ao, samplerCube environmentMap, vec3 environmentMapPosition, vec3 environmentMapSize, vec3 environmentMapOrigin, float environmentMapIntensity){
	vec3 f0 = mix(vec3_splat(0.04), albedo, metallic);

	vec3 ks = fresnel2(max(dot(normal, view), 0.0), f0, roughness);
	vec3 kd = (1.0 - ks) * (1.0 - metallic);

	vec3 irradiance = SampleCubemapParallax(environmentMap, normal, 12.0, environmentMapPosition, environmentMapSize, environmentMapOrigin, position).rgb * environmentMapIntensity;
	vec3 diffuse = irradiance * albedo;

	vec3 r = reflect(-view, normal);
	float maxLod = log2(textureSize(environmentMap, 0).x);
	float lodFactor = (1.0 - exp(-roughness * 2.5)) * maxLod;
	vec3 prefiltered = SampleCubemapParallax(environmentMap, r, lodFactor, environmentMapPosition, environmentMapSize, environmentMapOrigin, position).rgb * environmentMapIntensity;

	vec2 brdfInteg = vec2(1.0, 0.0);
	vec3 specular = prefiltered * (ks * brdfInteg.r + brdfInteg.g);

	vec3 ambient = kd * diffuse * ao + specular * ao;

	return ambient;
}
