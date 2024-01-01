$input a_position
$output v_position, v_size, v_camera, v_view


#include "../bgfx/common.shader"


uniform vec4 u_cameraPosition;
uniform vec4 u_gridPosition;
uniform vec4 u_gridSize;


void main()
{
	vec3 local = a_position * u_gridSize.xyz;
	v_position = local;
	v_size = u_gridSize.xyz;

	v_camera = u_cameraPosition.xyz; // u_cameraPosition is relative to the voxel space
	v_view = local - u_cameraPosition.xyz;

	vec3 worldPosition = u_gridPosition.xyz + local;
	gl_Position = mul(u_viewProj, vec4(worldPosition, 1.0));
}
