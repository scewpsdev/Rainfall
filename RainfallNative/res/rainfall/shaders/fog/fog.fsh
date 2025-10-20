$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_hdrBuffer, 0);
SAMPLER2D(s_depth, 1);

SAMPLERCUBE(s_environmentMap, 4);
uniform mat4 u_projectionViewInv;
uniform vec4 u_cameraPosition;

uniform vec4 u_fogData;
#define u_fogColor u_fogData.rgb
#define u_fogStrength u_fogData.w
uniform vec4 u_cameraFrustum;
#define u_cameraNear u_cameraFrustum.x
#define u_cameraFar u_cameraFrustum.y


vec3 getWorldPosition(vec4 fragCoord)
{
	vec2 ndc = fragCoord.xy * u_viewTexel.xy * 2 - 1;
	vec4 worldSpacePosition = mul(ndc, u_projectionViewInv);
	worldSpacePosition.xyz /= worldSpacePosition.w;
	return worldSpacePosition;
}

void main()
{
	vec3 hdr = texture2D(s_hdrBuffer, v_texcoord0).rgb;

	float depth = texture2D(s_depth, v_texcoord0).r;
	if (depth < 0.999999)
	{
		float distance = depthToDistance(depth, u_cameraNear, u_cameraFar);
		float fogFactor = 1.0 - exp(-distance * u_fogStrength);
		vec3 position = getWorldPosition(gl_FragCoord);
		vec3 view = normalize(position - u_cameraPosition);
		vec3 fogColor = textureCubeLod(s_environmentMap, view, 20).rgb * 0.5;
		hdr = mix(hdr, fogColor, fogFactor);
	}

	gl_FragColor = vec4(hdr, 1.0);
}
