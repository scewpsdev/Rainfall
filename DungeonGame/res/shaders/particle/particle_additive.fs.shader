$input v_position, v_texcoord0, v_color0

#include "../common/common.shader"


SAMPLER2D(u_textureAtlas, 0);
uniform vec4 u_atlasSize;


void main()
{
	vec2 uv = v_texcoord0.xy;
	float animationFrame = v_texcoord0.z;
	float frameIdx = max(animationFrame * u_atlasSize.x * u_atlasSize.y - 1, 0.0);

	int frameX = int(frameIdx) % int(u_atlasSize.x + 0.5);
	int frameY = int(frameIdx) / int(u_atlasSize.x + 0.5);
	vec2 frameUV = (uv + vec2(frameX, frameY)) / u_atlasSize.xy;
	vec4 frameColor = SRGBToLinear(texture2D(u_textureAtlas, frameUV));

	int nextFrameX = int(frameIdx + 1) % int(u_atlasSize.x + 0.5);
	int nextFrameY = int(frameIdx + 1) / int(u_atlasSize.x + 0.5);
	vec2 nextFrameUV = (uv + vec2(nextFrameX, nextFrameY)) / u_atlasSize.xy;
	vec4 nextFrameColor = SRGBToLinear(texture2D(u_textureAtlas, nextFrameUV));

	float blend = fract(frameIdx);
	vec4 textureColor = mix(vec4(1.0, 1.0, 1.0, 1.0), mix(frameColor, nextFrameColor, blend), u_atlasSize.z);
	vec4 albedo = textureColor * v_color0;

	gl_FragColor = albedo;
}
