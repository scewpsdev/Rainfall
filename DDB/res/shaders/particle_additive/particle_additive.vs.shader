$input a_position, a_normal, a_texcoord0, a_color0
$output v_position, v_texcoord0, v_color0


#include "../common/common.shader"


void main()
{
	vec3 cameraRight = vec3(u_view[0][0], u_view[0][1], u_view[0][2]);
	vec3 cameraUp = vec3(u_view[1][0], u_view[1][1], u_view[1][2]);

	gl_Position = mul(u_viewProj, vec4(a_position + a_normal.x * cameraRight + a_normal.y * cameraUp, 1.0));

	v_position = a_position;
	v_texcoord0 = a_texcoord0;
	v_color0 = a_color0;
}
