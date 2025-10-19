$input a_position
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


SAMPLER2D(s_heightMap, 0);
SAMPLER2D(s_normalMap, 1);

uniform vec4 u_terrainScale;


void main()
{
	ivec2 heightmapSize = textureSize(s_heightMap, 0);
	vec2 uv = a_position.xz * (heightmapSize - 1) / heightmapSize + 0.5 / heightmapSize;
	float height = texture2DLod(s_heightMap, uv, 0).r;
	vec3 normal = texture2DLod(s_normalMap, uv, 0).xyz;

	vec3 position = u_terrainScale.xyz * a_position + vec3(0.0, height, 0.0);

	vec4 worldPosition = mul(u_model[0], vec4(position, 1.0));
	vec4 worldNormal = mul(u_model[0], vec4(normal, 0.0));
	vec3 worldTangent = vec3(1.0, 0.0, 0.0);
	//vec4 worldTangent = mul(u_model[0], vec4(a_tangent, 0.0));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = vec3(1.0, 0.0, 0.0);
	v_bitangent = vec3(0.0, 0.0, -1.0);
	v_texcoord0 = vec3(a_position.xz, u_terrainScale.x);
}
