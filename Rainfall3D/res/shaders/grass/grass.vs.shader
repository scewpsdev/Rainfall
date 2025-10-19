$input a_position, a_color0, i_data0, i_data1
$output v_position, v_normal, v_color0


#include "../common/common.shader"


SAMPLER2D(s_heightmap, 0);
SAMPLER2D(s_normalmap, 1);
SAMPLER2D(s_splatMap, 2);
//SAMPLER2D(s_perlinTexture, 0);

uniform vec4 u_animationData;


vec2 noise2d(float x, float z, float frequency, float amplitude, float time, float timeFrequency)
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

	float noiseX = sin(time * timeFrequency * 1.2343284759 + x * frequency) * amplitude;
	float noiseZ = cos(time * timeFrequency * 1.1294339528 + z * frequency) * amplitude;
	return vec2(noiseX, noiseZ);
}

void main()
{
	vec2 offset = u_animationData.xy;
	float terrainSize = u_animationData.z;
	float time = u_animationData.w;

	vec3 bladePosition = i_data0.xyz;
	float bladeRotation = i_data0.w;
	//vec3 normal = i_data1.xyz;
	//float heightMultiplier = i_data1.w;

	ivec2 heightmapSize = textureSize(s_heightmap, 0);
	vec2 uv = (bladePosition.xz + offset) / terrainSize * (heightmapSize - 1) / heightmapSize + 0.5 / heightmapSize;
	float height = texture2DLod(s_heightmap, uv, 0).r;
	vec3 normal = normalize((texture2DLod(s_normalmap, uv, 0) * 2.0 - 1.0).rgb);
	float grassValue = texture2DLod(s_splatMap, uv, 0).r;
	grassValue = 1.0;
	float heightMultiplier = grassValue > 0.05 ? grassValue : 0.0;
	//normal = vec3(0.0, 1.0, 0.0);


	float vertexHeight = a_position.y * heightMultiplier;
	vec3 localPosition = vec3(a_position.x * cos(bladeRotation), vertexHeight, -a_position.x * sin(bladeRotation));
	//localPosition.y *= heightMultiplier;
	vec3 position = localPosition + bladePosition;
	vec4 worldPosition = mul(u_model[0], vec4(position, 1.0));


	bladePosition.y = height;

	bladePosition.y += height;
	position.y += height;
	worldPosition.y += height;


	float waveFrequency = 0.5;
	float waveAmplitude = 0.2;
	float waveTimeFrequency = 0.5;

	//vec4 noise = texture2DLod(s_perlinTexture, worldPosition.xz, 0);
	vec2 displacement = noise2d(worldPosition.x, worldPosition.z, waveFrequency, waveAmplitude, time, waveTimeFrequency);

	/*
	displacement.x += sin(time * waveFrequency * 1.2343284759 + worldPosition.x * waveSpacialFrequency) * waveAmplitude;
	displacement.y += cos(time * waveFrequency * 1.1294339528 + worldPosition.z * waveSpacialFrequency) * waveAmplitude;
	displacement.x += 0.5 * sin(2.0 * (time * waveFrequency * 1.0537537883 + worldPosition.x * waveSpacialFrequency)) * waveAmplitude;
	displacement.y += 0.5 * cos(2.0 * (time * waveFrequency * 1.2749836812 + worldPosition.z * waveSpacialFrequency)) * waveAmplitude;
	*/

	worldPosition.xz += displacement * vertexHeight;
	normal.xz += displacement * vertexHeight * 1.0;


	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = normal;
	v_color0 = vec4(a_position.y, uv, 1.0);
}
