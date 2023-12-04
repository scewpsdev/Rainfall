$input a_position, a_normal, a_tangent, a_texcoord0
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"


uniform vec4 u_animationData;


vec3 noise3d(vec3 position, float frequency, float amplitude, float time, float timeFrequency)
{
	/*
	vec4 noise0 = texture2DLod(s_perlinTexture, vec2(x, z) * frequency + vec2(time, time) * timeFrequency, 0) - 0.125;
	vec4 noise1 = texture2DLod(s_perlinTexture, vec2(-x, -z) * frequency + vec2(time, time) * timeFrequency, 0) - 0.125;
	vec2 octave0 = vec2(noise0.r, noise1.r);
	vec2 octave1 = vec2(noise0.g, noise1.g);
	vec2 octave2 = vec2(noise0.b, noise1.b);
	vec2 result = (octave0 + 0.5 * octave1 + 0.25 * octave2) / 1.75;
	return result * amplitude;
	*/

	float noiseX = sin(time * timeFrequency * 1.2343284759 + position.x * frequency) * amplitude;
	float noiseY = sin(time * timeFrequency * 1.1039481034 + position.y * frequency) * amplitude * 0.1;
	float noiseZ = cos(time * timeFrequency * 0.8294339528 + position.z * frequency) * amplitude;

	return vec3(noiseX, noiseY, noiseZ);
}

void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));
	vec4 worldNormal = mul(u_model[0], vec4(a_normal, 0.0));
	vec4 worldTangent = mul(u_model[0], vec4(a_tangent, 0.0));

	float waveFrequency = 4.0;
	float waveAmplitude = 0.2;
	float waveTimeFrequency = 4.0;

	float time = u_animationData.x;
	float animationStrength = clamp(a_position.y / 10.0, 0.0, 1.0);
	float amplitude = waveAmplitude * animationStrength;
	vec3 displacement = noise3d(worldPosition.xyz, waveFrequency, amplitude, time, waveTimeFrequency);
	worldPosition.xyz += displacement;

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = worldTangent.xyz;
	v_bitangent = cross(v_normal, v_tangent);
	v_texcoord0 = a_texcoord0;
}
