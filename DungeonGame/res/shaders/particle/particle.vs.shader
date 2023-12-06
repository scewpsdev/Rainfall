$input a_position, a_texcoord0, i_data0, i_data1, i_data2
$output v_position, v_texcoord0, v_color0


#include "../common/common.shader"


uniform vec4 u_cameraAxisRight;
uniform vec4 u_cameraAxisUp;


void main()
{
	vec3 position = i_data0.xyz;
	float rotation = i_data0.w;
	vec4 color = i_data1;
	float size = i_data2.x;
	float animation = i_data2.y;
	
	vec2 localPosition = vec2(a_position.x * cos(rotation) - a_position.y * sin(rotation),
							  cos(rotation) * a_position.y + sin(rotation) * a_position.x);

	position += localPosition.x * size * u_cameraAxisRight.xyz + localPosition.y * size * u_cameraAxisUp.xyz;

	gl_Position = mul(u_viewProj, vec4(position, 1.0));

	v_position = position;
	v_texcoord0 = vec3(a_texcoord0.xy, animation);
	v_color0 = color;
}
