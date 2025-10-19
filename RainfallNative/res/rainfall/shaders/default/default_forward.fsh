$input v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../deferred/pbr.shader"
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

uniform vec4 u_lightDirection;
uniform vec4 u_lightColor;

SAMPLER2DSHADOW(s_shadowMap0, 6);
SAMPLER2DSHADOW(s_shadowMap1, 7);
SAMPLER2DSHADOW(s_shadowMap2, 8);
uniform vec4 u_params;
#define u_shadowMapFar0 u_params[0]
#define u_shadowMapFar1 u_params[1]
#define u_shadowMapFar2 u_params[2]
uniform mat4 u_toLightSpace0;
uniform mat4 u_toLightSpace1;
uniform mat4 u_toLightSpace2;


void main()
{
    vec4 color = u_materialData0;
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

		int numLayers = 8;
		float parallaxStrength = 0.005;
		vec2 p = view.xy / view.z * vec2(-1, 1) * parallaxStrength;
		float currentLayer = 0.5;
		float nextLayerDepth = 0.25;
		vec2 offset = p * currentLayer;
		for (int i = 0; i < numLayers; i++)
		{
			float currentDepth = 1 - texture2D(s_height, v_texcoord0 + offset).r;
			currentLayer += (currentDepth >= currentLayer ? 1 : -1) * nextLayerDepth;
			nextLayerDepth *= 0.5;
			offset = p * currentLayer;
		}
		vec2 interpolatedOffset = offset;

		v_texcoord0 += interpolatedOffset;
	}

	vec4 albedo = mix(vec4_splat(1.0), texture2D(s_diffuse, v_texcoord0), u_hasTexCoords * u_hasDiffuse) * color;
	float roughness = mix(roughnessFactor, texture2D(s_roughness, v_texcoord0).g, u_hasTexCoords * u_hasRoughness);
	float metallic = mix(metallicFactor, texture2D(s_metallic, v_texcoord0).b, u_hasTexCoords * u_hasMetallic);
	vec3 emissive = mix(emissionColor, texture2D(s_emissive, v_texcoord0).rgb, u_hasTexCoords * u_hasEmissive);
    
	vec3 normalMapValue = texture2D(s_normal, v_texcoord0).rgb;
	normalMapValue = vec3(2.0 * normalMapValue.rg - 1.0, 1);

	vec3 position = v_position;
	vec3 normal = (u_hasTexCoords * u_hasNormal > 0.5) ? mul(tbn, normalMapValue) : norm;

	vec3 toCamera = u_cameraPosition.xyz - position;
	float distance = length(toCamera);
	vec3 view = toCamera / distance;

	vec3 lightS = RenderDirectionalLight(position, normal, view, distance, albedo.rgb, roughness, metallic, u_lightDirection.xyz, u_lightColor.rgb, s_shadowMap0, u_shadowMapFar0, u_toLightSpace0, s_shadowMap1, u_shadowMapFar1, u_toLightSpace1, s_shadowMap2, u_shadowMapFar2, u_toLightSpace2, gl_FragCoord);

	vec3 final = albedo.rgb * lightS + emissive * emissionStrength;

	gl_FragColor = vec4(final, albedo.a);
}
