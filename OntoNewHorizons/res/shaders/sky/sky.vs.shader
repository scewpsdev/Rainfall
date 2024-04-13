$input a_position
$output v_position


#include "../common/common.shader"


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));

	//mat4 view = u_view;
	//view[3] = vec4(0.0, 0.0, 0.0, 1.0);
	vec4 viewSpacePosition = mul(u_view, vec4(worldPosition.xyz, 0.0));

	gl_Position = mul(u_proj, vec4(viewSpacePosition.xyz, 1.0));

	v_position = a_position;
}
