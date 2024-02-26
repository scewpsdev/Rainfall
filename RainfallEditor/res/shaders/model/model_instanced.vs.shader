$input a_position, a_normal, a_tangent, a_texcoord0, i_data0, i_data1, i_data2, i_data3
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


void main()
{
	mat4 model = mtxFromCols(i_data0, i_data1, i_data2, i_data3);

	vec4 worldPosition = mul(model, vec4(a_position, 1.0));
	vec4 worldNormal = mul(model, vec4(a_normal, 0.0));
	vec4 worldTangent = mul(model, vec4(a_tangent, 0.0));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = worldTangent.xyz;
	v_bitangent = cross(v_normal, v_tangent);
	v_texcoord0 = a_texcoord0;
}
