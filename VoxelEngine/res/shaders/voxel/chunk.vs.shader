$input a_position
$output v_position


#include "../bgfx/common.shader"


uniform vec4 u_boxPosition;
uniform vec4 u_boxSize;


void main()
{
	vec3 worldPosition = u_boxPosition.xyz + a_position * u_boxSize.xyz;
	gl_Position = mul(u_viewProj, vec4(worldPosition, 1.0));

	v_position = a_position * u_boxSize.xyz;
}
