$input v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


SAMPLER2D(s_diffuse, 0);
SAMPLER2D(s_normal, 1);
SAMPLER2D(s_roughness, 2);
SAMPLER2D(s_metallic, 3);
SAMPLER2D(s_emissive, 4);
SAMPLER2D(s_height, 5);

uniform vec4 u_attributeInfo0;
uniform vec4 u_attributeInfo1;

uniform vec4 u_materialData0;
uniform vec4 u_materialData1;
uniform vec4 u_materialData2;
uniform vec4 u_materialData3;

#define u_hasDiffuse u_attributeInfo0[0]
#define u_hasNormal u_attributeInfo0[1]
#define u_hasRoughness u_attributeInfo0[2]
#define u_hasMetallic u_attributeInfo0[3]
#define u_hasEmissive u_attributeInfo1[0]
#define u_hasHeight u_attributeInfo1[1]

#define u_hasTexCoords u_attributeInfo1[3]

uniform vec4 u_cameraPosition;


void main()
{
    vec3 color = u_materialData0.rgb;
    float roughnessFactor = u_materialData1.r;
    float metallicFactor = u_materialData1.g;
    vec3 emissionColor = u_materialData2.rgb;
    float emissionStrength = u_materialData2.a;

	vec3 norm = normalize(v_normal);
	vec3 tang = normalize(v_tangent);
	vec3 bitang = normalize(v_bitangent);
	mat3 tbn = mat3(
		tang.x, bitang.x, norm.x,
		tang.y, bitang.y, norm.y,
		tang.z, bitang.z, norm.z
	);
	mat3 invTbn = transpose(tbn);

	// Parallax mapping
	if (u_hasHeight > 0.5 && u_hasTexCoords > 0.5)
	{
		vec3 toCamera = u_cameraPosition.xyz - v_position;
		vec3 view = normalize(mul(invTbn, toCamera));

		int minLayers = 8;
		int maxLayers = 32;
		float viewSteepness = clamp(dot(vec3(0, 0, 1), view), 0, 1);
		int numLayers = int(mix(maxLayers, minLayers, viewSteepness));
		float layerDepth = 1.0 / numLayers;
		float currentLayer = 0.0;

		vec2 p = view.xy / view.z * vec2(-1, 1) * 0.01;
		vec2 delta = p / numLayers;
		vec2 offset = vec2(0, 0);
		float currentDepth = 1 - texture2D(s_height, v_texcoord0).r;

		vec2 lastOffset = offset;
		float lastDepth = currentDepth;

		for (int i = 0; i < numLayers; i++)
		{
			lastOffset = offset;
			lastDepth = currentDepth;

			offset += delta;
			currentDepth = 1 - texture2D(s_height, v_texcoord0 + offset).r;
			currentLayer += layerDepth;

			if (currentLayer >= currentDepth)
				break;
		}

		float lastLayerDepth = currentLayer - layerDepth;
		float lastDepthDelta = lastDepth - lastLayerDepth;
		float currentDepthDelta = currentLayer - currentDepth;
		float weight = lastDepthDelta / (lastDepthDelta + currentDepthDelta);
		vec2 interpolatedOffset = mix(lastOffset, offset, weight);

		v_texcoord0 += interpolatedOffset;
	}

	vec4 albedo = mix(vec4_splat(1.0), linearToSRGB(texture2D(s_diffuse, v_texcoord0)), u_hasTexCoords * u_hasDiffuse) * vec4(linearToSRGB(color), 1.0);
	float roughness = mix(roughnessFactor, texture2D(s_roughness, v_texcoord0).g, u_hasTexCoords * u_hasRoughness);
	float metallic = mix(metallicFactor, texture2D(s_metallic, v_texcoord0).b, u_hasTexCoords * u_hasMetallic);
	vec3 emissive = mix(emissionColor, texture2D(s_emissive, v_texcoord0).rgb, u_hasTexCoords * u_hasEmissive);
    
	vec3 normalMapValue = 2.0 * texture2D(s_normal, v_texcoord0).rgb - 1.0;

	vec3 normal = (u_hasTexCoords * u_hasNormal > 0.5) ? mul(tbn, normalMapValue) : norm;


    gl_FragData[0] = vec4(v_position, 1.0);
    gl_FragData[1] = vec4(normal * 0.5 + 0.5, emissionStrength);
    gl_FragData[2] = vec4(albedo.rgb, roughness);
    gl_FragData[3] = vec4(emissive, metallic);
}
