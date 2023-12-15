$input v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


#define DEFAULT_ALBEDO vec4(1.0, 1.0, 1.0, 1.0)


SAMPLER2D(s_diffuse, 0);
SAMPLER2D(s_normal, 1);
SAMPLER2D(s_roughness, 2);
SAMPLER2D(s_metallic, 3);
SAMPLER2D(s_emissive, 4);

uniform vec4 u_attributeInfo;
uniform vec4 u_materialInfo;
uniform vec4 u_materialInfo2;
uniform vec4 u_materialInfo3;
uniform vec4 u_materialInfo4;


void main()
{
    float hasTexCoords = u_attributeInfo[0];
    float hasDiffuse = u_materialInfo[0];
    float hasNormal = u_materialInfo[1];
    float hasRoughness = u_materialInfo[2];
    float hasMetallic = u_materialInfo[3];
    float hasEmissive = u_materialInfo2[3];
    vec3 color = u_materialInfo2.rgb;
    float metallicFactor = u_materialInfo3[0];
    float roughnessFactor = u_materialInfo3[1];
    vec3 emissionColor = u_materialInfo4.rgb;
    float emissionStrength = u_materialInfo4[3];


    float lod = max(log2(textureSize(s_diffuse, 0).x) - 6, 0);

	vec4 albedo = mix(DEFAULT_ALBEDO, texture2DLod(s_diffuse, v_texcoord0, lod), hasTexCoords * hasDiffuse) * vec4(linearToSRGB(color), 1.0);
	float roughness = mix(roughnessFactor, texture2DLod(s_roughness, v_texcoord0, lod).g, hasTexCoords * hasRoughness);
	float metallic = mix(metallicFactor, texture2DLod(s_metallic, v_texcoord0, lod).b, hasTexCoords * hasMetallic);
	vec3 emissive = mix(emissionColor, texture2DLod(s_emissive, v_texcoord0, lod).rgb, hasTexCoords * hasEmissive);
    
	vec3 normalMapValue = 2.0 * texture2DLod(s_normal, v_texcoord0, lod).rgb - 1.0;
	vec3 norm = normalize(v_normal);
	vec3 tang = normalize(v_tangent);
	vec3 bitang = normalize(v_bitangent);
	mat3 tbn = mat3(
		tang.x, bitang.x, norm.x,
		tang.y, bitang.y, norm.y,
		tang.z, bitang.z, norm.z
	);

	vec3 normal = (hasTexCoords * hasNormal > 0.5) ? mul(tbn, normalMapValue) : norm;


    gl_FragData[0] = vec4(v_position, 1.0);
    gl_FragData[1] = vec4(normal * 0.5 + 0.5, 0.0);
    gl_FragData[2] = vec4(albedo.rgb, roughness);
    gl_FragData[3] = vec4(emissive, metallic);
}
