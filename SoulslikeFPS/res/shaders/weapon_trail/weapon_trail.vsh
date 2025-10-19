$input a_position, a_normal
$output v_position, v_texcoord0


#include "../common/common.shader"


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = a_position;
	v_texcoord0 = a_normal;
}
