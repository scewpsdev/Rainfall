$input v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


#define DEFAULT_ALBEDO vec4(1.0, 1.0, 1.0, 1.0)
#define DEFAULT_ROUGHNESS 1.0
#define DEFAULT_METALLIC 0.0
#define DEFAULT_NORMAL vec3(0.0, 0.0, 1.0)
#define DEFAULT_EMISSIVE vec3(0.0, 0.0, 0.0)


SAMPLER2D(s_splatMap, 2);

SAMPLER2D(s_diffuse0, 3);
SAMPLER2D(s_normal0, 4);
SAMPLER2D(s_roughness0, 5);
uniform vec4 u_materialInfo0;

SAMPLER2D(s_diffuse1, 6);
SAMPLER2D(s_normal1, 7);
SAMPLER2D(s_roughness1, 8);
uniform vec4 u_materialInfo1;

SAMPLER2D(s_diffuse2, 9);
SAMPLER2D(s_normal2, 10);
SAMPLER2D(s_roughness2, 11);
uniform vec4 u_materialInfo2;

SAMPLER2D(s_diffuse3, 12);
SAMPLER2D(s_normal3, 13);
SAMPLER2D(s_roughness3, 14);
uniform vec4 u_materialInfo3;

uniform vec4 u_attributeInfo;


void main()
{
	float height = v_tangent.y;

	float hasTexCoords = u_attributeInfo[0];

	vec4 albedo0 = mix(DEFAULT_ALBEDO, texture2D(s_diffuse0, v_texcoord0.xy * v_texcoord0.z * u_materialInfo0[3]), hasTexCoords * u_materialInfo0[0]);
	vec3 normal0 = mix(DEFAULT_NORMAL, 2.0 * texture2D(s_normal0, v_texcoord0).rgb - 1.0, hasTexCoords * u_materialInfo0[1]);
	float roughness0 = mix(DEFAULT_ROUGHNESS, texture2D(s_roughness0, v_texcoord0).g, hasTexCoords * u_materialInfo0[2]);

	vec4 albedo1 = mix(DEFAULT_ALBEDO, texture2D(s_diffuse1, v_texcoord0.xy * v_texcoord0.z * u_materialInfo1[3]), hasTexCoords * u_materialInfo1[0]);
	vec3 normal1 = mix(DEFAULT_NORMAL, 2.0 * texture2D(s_normal1, v_texcoord0).rgb - 1.0, hasTexCoords * u_materialInfo1[1]);
	float roughness1 = mix(DEFAULT_ROUGHNESS, texture2D(s_roughness1, v_texcoord0).g, hasTexCoords * u_materialInfo1[2]);

	vec4 albedo2 = mix(DEFAULT_ALBEDO, texture2D(s_diffuse2, v_texcoord0.xy * v_texcoord0.z * u_materialInfo2[3]), hasTexCoords * u_materialInfo2[0]);
	vec3 normal2 = mix(DEFAULT_NORMAL, 2.0 * texture2D(s_normal2, v_texcoord0).rgb - 1.0, hasTexCoords * u_materialInfo2[1]);
	float roughness2 = mix(DEFAULT_ROUGHNESS, texture2D(s_roughness2, v_texcoord0).g, hasTexCoords * u_materialInfo2[2]);

	vec4 albedo3 = mix(DEFAULT_ALBEDO, texture2D(s_diffuse3, v_texcoord0.xy * v_texcoord0.z * u_materialInfo3[3]), hasTexCoords * u_materialInfo3[0]);
	vec3 normal3 = mix(DEFAULT_NORMAL, 2.0 * texture2D(s_normal3, v_texcoord0).rgb - 1.0, hasTexCoords * u_materialInfo3[1]);
	float roughness3 = mix(DEFAULT_ROUGHNESS, texture2D(s_roughness3, v_texcoord0).g, hasTexCoords * u_materialInfo3[2]);

	vec4 splatValue = texture2D(s_splatMap, v_texcoord0);
	splatValue.a = 1.0 - splatValue.r - splatValue.g - splatValue.b;

	vec4 albedo = albedo0 * splatValue[0] + albedo1 * splatValue[1] + albedo2 * splatValue[2] + albedo3 * splatValue[3];
	vec3 normal = normal0 * splatValue[0] + normal1 * splatValue[1] + normal2 * splatValue[2] + normal3 * splatValue[3];
	float roughness = roughness0 * splatValue[0] + roughness1 * splatValue[1] + roughness2 * splatValue[2] + roughness3 * splatValue[3];


	vec3 norm = normalize(v_normal);
	vec3 tang = normalize(v_tangent);
	vec3 bitang = normalize(v_bitangent);
	mat3 tbn = mat3(
		tang.x, bitang.x, norm.x,
		tang.y, bitang.y, norm.y,
		tang.z, bitang.z, norm.z
	);
	normal = mul(tbn, normal);


	gl_FragData[0] = vec4(v_position, 1.0);
	gl_FragData[1] = vec4(normalize(v_normal) * 0.5 + 0.5, 1.0);
	gl_FragData[2] = vec4(albedo.rgb, roughness);
	gl_FragData[3] = vec4(0.0, 0.0, 0.0, 0.0);
}
