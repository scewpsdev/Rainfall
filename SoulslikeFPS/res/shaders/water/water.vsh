$input a_position
$output v_position, v_normal, v_localposition, v_texcoord0

#include "../common/common.shader"
#include "wave.shader"


uniform vec4 u_cameraPosition;
uniform vec4 u_materialData0;


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));

	int numWaves = 64;

	float time = u_cameraPosition.w;
	float amplitude = u_materialData0.x;
	float frequency = u_materialData0.y;

	vec3 animatedPosition, animatedNormal;
	animateWater(worldPosition.xz, time, numWaves, amplitude, frequency, animatedPosition, animatedNormal);

	gl_Position = mul(u_viewProj, vec4(worldPosition.xyz + animatedPosition, 1.0));
	
	v_position = worldPosition.xyz;
	v_normal = animatedNormal;
	v_localposition = animatedPosition;
	v_texcoord0 = vec2(0, 0);
}
