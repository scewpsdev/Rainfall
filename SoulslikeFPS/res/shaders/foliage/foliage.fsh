$input v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


#define DEFAULT_ALBEDO vec4(1.0, 1.0, 1.0, 1.0)
#define DEFAULT_ROUGHNESS 1.0
#define DEFAULT_METALLIC 0.0
#define DEFAULT_NORMAL vec3(0.0, 0.0, 1.0)
#define DEFAULT_EMISSIVE vec3(0.0, 0.0, 0.0)


SAMPLER2D(s_diffuse, 0);
SAMPLER2D(s_normal, 1);
SAMPLER2D(s_roughness, 2);
SAMPLER2D(s_metallic, 3);
SAMPLER2D(s_emissive, 4);

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
uniform vec4 u_mainLightDirection;
uniform vec4 u_mainLightColor;


void main()
{
	vec3 color = u_materialData0.rgb;
    float roughnessFactor = u_materialData1.r;
    float metallicFactor = u_materialData1.g;
    vec3 emissionColor = u_materialData2.rgb;
    float emissionStrength = u_materialData2.a;


	vec4 albedo = mix(DEFAULT_ALBEDO, texture2D(s_diffuse, v_texcoord0), u_hasTexCoords * u_hasDiffuse) * vec4(color, 1.0);
	float roughness = mix(DEFAULT_ROUGHNESS, texture2D(s_roughness, v_texcoord0).g, u_hasTexCoords * u_hasRoughness);
	float metallic = mix(DEFAULT_METALLIC, texture2D(s_metallic, v_texcoord0).b, u_hasTexCoords * u_hasMetallic);
	vec3 emissive = mix(DEFAULT_EMISSIVE, texture2D(s_emissive, v_texcoord0).rgb, u_hasTexCoords * u_hasEmissive);

	vec3 normalMapValue = 2.0 * texture2D(s_normal, v_texcoord0).rgb - 1.0;
	vec3 norm = normalize(v_normal);
	vec3 tang = normalize(v_tangent);
	vec3 bitang = normalize(v_bitangent);
	mat3 tbn = mat3(
		tang.x, bitang.x, norm.x,
		tang.y, bitang.y, norm.y,
		tang.z, bitang.z, norm.z
	);

	// were using the normal for sss here, since its a solid mesh
	// for alpha cards or more complex meshes the foliage should be separated into multiple objects so the origin->vertex position vector can be used
	vec3 normal = (u_hasTexCoords * u_hasNormal > 0.5) ? mul(tbn, normalMapValue) : norm;
	vec3 lightDirection = u_mainLightDirection.xyz;
	vec3 viewDirection = normalize(v_position - u_cameraPosition.xyz);
	float viewDot = clamp(dot(viewDirection, -lightDirection), 0, 1);
	float normalDot = 1 - abs(dot(normal, -lightDirection));
	float scatteringAmount = pow(viewDot, 10) * normalDot * 0.1;
	emissive += scatteringAmount * u_mainLightColor.xyz * albedo.rgb;


	gl_FragData[0] = vec4(v_position, 1.0);
	gl_FragData[1] = vec4(normal * 0.5 + 0.5, emissionStrength);
	gl_FragData[2] = vec4(albedo.rgb, roughness);
	gl_FragData[3] = vec4(emissive, metallic);
}
