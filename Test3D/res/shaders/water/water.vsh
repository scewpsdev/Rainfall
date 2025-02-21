$input a_position
$output v_position, v_normal, v_localposition

#include "../common/common.shader"
#include "wave.shader"


uniform vec4 u_materialData0;
#define u_time u_materialData0.w


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));

	int numWaves = 64;

	vec3 animatedPosition, animatedNormal;
	animateWater(worldPosition.xz, u_time.x, numWaves, animatedPosition, animatedNormal);
	animatedPosition.y = 0;

	gl_Position = mul(u_viewProj, vec4(worldPosition.xyz + animatedPosition, 1.0));
	
	v_position = vec4(worldPosition.xyz, gl_Position.z / gl_Position.w);
	v_normal = animatedNormal;
	v_localposition = animatedPosition;
}
