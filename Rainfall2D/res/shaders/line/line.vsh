$input a_position, a_color0
$output v_position, v_ndc, v_color0


#include "../bgfx/common.shader"


void main()
{
	vec4 projectedPosition = vec4(a_position.x, a_position.y, 0.0, 1.0);
	gl_Position = mul(u_viewProj, projectedPosition);
	
	v_position = a_position;
	v_ndc = gl_Position.xyz;
	v_color0 = a_color0;
}
