$input v_position


#include "../bgfx/common.shader"


uniform vec4 u_cameraPosition;


void main()
{
	float distance = length(v_position - u_cameraPosition.xyz);
	gl_FragColor = vec4(distance, 0, 0, 0);
}
