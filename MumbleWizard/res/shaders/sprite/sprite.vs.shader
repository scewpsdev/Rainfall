$input a_position, a_normal, a_texcoord0, a_color0
$output v_position, v_normal, v_texcoord0, v_color0


#include "../bgfx/common.shader"


void main()
{
	vec4 projectedPosition = vec4(a_position.x, a_position.y + a_position.z, a_position.z, 1.0);
	gl_Position = mul(u_viewProj, projectedPosition);
	
	v_position = a_position;
	v_normal = a_normal;
	v_texcoord0 = a_texcoord0;
	v_color0 = a_color0;
}
