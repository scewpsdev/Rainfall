$input v_position


#include "../common/common.shader"


SAMPLERCUBE(s_skyTexture, 0);

uniform vec4 u_skyData;


void main()
{
	vec3 direction = normalize(v_position);

	float skyIntensity = u_skyData[0];
	gl_FragColor = textureCube(s_skyTexture, direction) * skyIntensity;
	gl_FragDepth = 0.9999999;
}
