$input a_position
$output v_camera, v_view, v_size


#include "../bgfx/common.shader"


uniform vec4 u_cameraPosition;
uniform vec4 u_boxSize;


void main()
{
	v_camera = u_cameraPosition.xyz;
	
	//vec4 worldPosition = mul(u_model[0], vec4(v_local, 1.0));
	vec3 local = a_position * u_boxSize.xyz;
	v_view = local - u_cameraPosition.xyz; //mul(transpose(u_model[0]), vec4(toFragment, 0.0)).xyz;
	
	v_size = u_boxSize.xyz;

	vec4 worldPosition = mul(u_model[0], vec4(local, 1.0));
	gl_Position = mul(u_viewProj, worldPosition);
}
