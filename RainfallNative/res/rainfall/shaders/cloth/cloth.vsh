$input a_position, a_normal, a_tangent, a_texcoord0, a_texcoord1, a_texcoord2, a_weight
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


void main()
{
	vec3 animatedPosition = a_texcoord1.xyz;
	vec3 animatedNormal = a_texcoord2.xyz;
	vec3 animatedTangent = a_weight.xyz;

	vec4 worldPosition = mul(u_model[0], vec4(animatedPosition, 1.0));
	vec4 worldNormal = mul(u_model[0], vec4(animatedNormal, 0.0));
	vec4 worldTangent = mul(u_model[0], vec4(animatedTangent, 0.0));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = worldTangent.xyz;
	v_bitangent = cross(v_normal, v_tangent);
	v_texcoord0 = a_texcoord0;
}
