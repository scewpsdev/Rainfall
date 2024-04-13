$input v_position


#include "../common/common.shader"


SAMPLERCUBE(s_skyTexture, 0);


void main()
{
	vec3 direction = normalize(v_position);

	gl_FragColor = SRGBToLinear(textureCube(s_skyTexture, direction));
	gl_FragDepth = 0.9999999;
}
