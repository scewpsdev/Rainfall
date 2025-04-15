$input a_position, i_data0, i_data1
$output v_data0, v_color0


#include "../common/common.shader"


void main()
{
	vec3 lightPosition = i_data0.xyz;
	float lightRadius = i_data0.w;
	gl_Position = mul(u_viewProj, vec4(lightPosition + a_position * lightRadius, 1.0));

	v_data0 = i_data0.xyz;
	v_color0 = i_data1;
}
