$input a_position, a_normal, a_tangent, a_texcoord0
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));
	vec4 worldNormal = mul(u_model[0], vec4(a_normal, 0.0));
	vec4 worldTangent = mul(u_model[0], vec4(a_tangent, 0.0));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = worldTangent.xyz;
	v_bitangent = cross(v_normal, v_tangent);
	v_texcoord0 = a_texcoord0;
}
