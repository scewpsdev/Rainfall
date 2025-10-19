$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_hdrBuffer, 0);
SAMPLER2D(s_depth, 1);
SAMPLER2D(s_ao, 2);
SAMPLER2D(s_positions, 3);

SAMPLERCUBE(s_environmentMap, 4);
uniform vec4 u_cameraPosition;

uniform vec4 u_fogData;
#define u_fogColor u_fogData.rgb
#define u_fogStrength u_fogData.w
uniform vec4 u_cameraFrustum;
#define u_cameraNear u_cameraFrustum.x
#define u_cameraFar u_cameraFrustum.y


void main()
{
	vec3 hdr = texture2D(s_hdrBuffer, v_texcoord0).rgb;

	float depth = texture2D(s_depth, v_texcoord0).r;
	if (depth < 0.999999)
	{
		float distance = depthToDistance(depth, u_cameraNear, u_cameraFar);
		float fogFactor = 1.0 - exp(-distance * u_fogStrength);
		vec3 position = texture2D(s_positions, v_texcoord0).rgb;
		vec3 direction = normalize(position - u_cameraPosition.xyz);
		vec3 fogColor = textureCubeLod(s_environmentMap, direction, 20).rgb;
		hdr = mix(hdr, fogColor, fogFactor);
	}

	gl_FragColor = vec4(hdr, 1.0);
}
