$input a_position, a_color0
$output v_position, v_color0


#include "../common/common.shader"


void main()
{
	gl_Position = mul(u_viewProj, vec4(a_position, 1.0));

	v_position = a_position;
	v_color0 = a_color0;
}
