$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_frame, 0);
SAMPLER2D(s_depth, 1);
uniform vec4 u_fogData;
uniform vec4 u_cameraFrustum;

void main()
{
	vec3 color = texture2D(s_frame, v_texcoord0).rgb;

	float depth = texture2D(s_depth, v_texcoord0).r;
	float distance = depthToDistance(depth, u_cameraFrustum.x, u_cameraFrustum.y);
	//if (depth > 0.99999)
	//	distance = 0.0;

	vec3 fogColor = u_fogData.rgb;
	float fogStrength = u_fogData[3];
	float fogFactor = 1.0 - exp(-distance * fogStrength);
	vec3 final = mix(color, fogColor, fogFactor);

	gl_FragColor = vec4(final, 1.0);
}
