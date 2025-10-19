$input a_position
$output v_position, v_normal

#include "../common/common.shader"
#include "wave.shader"


uniform vec4 u_time;


void main()
{
	vec4 vertexPosition = mul(u_model[0], vec4(a_position, 1.0));

	int numWaves = 64;

	vec3 animatedPosition, animatedNormal;
	animateWater(vertexPosition.xz, u_time.x, numWaves, animatedPosition, animatedNormal);

	gl_Position = mul(u_viewProj, vec4(vertexPosition.xyz + animatedPosition, 1.0));
	
	v_position = vertexPosition;
	v_normal = animatedNormal;
}
