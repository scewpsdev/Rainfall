$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_input, 0);


void main()
{
	vec2 texelSize = 1.0 / textureSize(s_input, 0.0);
	float x = texelSize.x;
	float y = texelSize.y;

	vec3 a = texture2D(s_input, vec2(v_texcoord0.x - 2 * x, v_texcoord0.y + 2 * y)).rgb;
	vec3 b = texture2D(s_input, vec2(v_texcoord0.x, v_texcoord0.y + 2 * y)).rgb;
	vec3 c = texture2D(s_input, vec2(v_texcoord0.x + 2 * x, v_texcoord0.y + 2 * y)).rgb;

	vec3 d = texture2D(s_input, vec2(v_texcoord0.x - 2 * x, v_texcoord0.y)).rgb;
	vec3 e = texture2D(s_input, vec2(v_texcoord0.x, v_texcoord0.y)).rgb;
	vec3 f = texture2D(s_input, vec2(v_texcoord0.x + 2 * x, v_texcoord0.y)).rgb;

	vec3 g = texture2D(s_input, vec2(v_texcoord0.x - 2 * x, v_texcoord0.y - 2 * y)).rgb;
	vec3 h = texture2D(s_input, vec2(v_texcoord0.x, v_texcoord0.y - 2 * y)).rgb;
	vec3 i = texture2D(s_input, vec2(v_texcoord0.x + 2 * x, v_texcoord0.y - 2 * y)).rgb;

	vec3 j = texture2D(s_input, vec2(v_texcoord0.x - x, v_texcoord0.y + y)).rgb;
	vec3 k = texture2D(s_input, vec2(v_texcoord0.x + x, v_texcoord0.y + y)).rgb;
	vec3 l = texture2D(s_input, vec2(v_texcoord0.x - x, v_texcoord0.y - y)).rgb;
	vec3 m = texture2D(s_input, vec2(v_texcoord0.x + x, v_texcoord0.y - y)).rgb;

	vec3 result = e * 0.125;
	result += (a + c + g + i) * 0.03125;
	result += (b + d + f + h) * 0.0625;
	result += (j + k + l + m) * 0.125;

	//result = max(result, 0.0001);

	gl_FragColor = vec4(result, 1.0);
}
