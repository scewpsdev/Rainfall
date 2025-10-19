$input a_position, a_color0, a_texcoord0, i_data0, i_data1
$output v_position, v_normal, v_color0, v_texcoord0


#include "../common/common.shader"


#define TILE_SIZE 20.0
#define BLADE_SIZE 0.75


//SAMPLER2D(s_heightmap, 0);
//SAMPLER2D(s_normalmap, 1);
//SAMPLER2D(s_splatMap, 2);
SAMPLER2D(s_perlinTexture, 1);

uniform vec4 u_materialData0;


vec2 noise2d(float x, float z, float frequency, float amplitude, float time, float timeFrequency)
{
	/*
	frequency *= 0.5;
	amplitude *= 10;
	timeFrequency *= 0.05;

	vec4 noise0 = texture2DLod(s_perlinTexture, vec2(x, z) * frequency + vec2(time, time) * timeFrequency, 0) - 0.75;
	vec4 noise1 = texture2DLod(s_perlinTexture, vec2(-x, -z) * frequency + vec2(time, time) * timeFrequency, 0) - 0.75;
	vec2 octave0 = vec2(noise0.r, noise1.r);
	vec2 octave1 = vec2(noise0.g, noise1.g);
	vec2 octave2 = vec2(noise0.b, noise1.b);
	vec2 result = (octave0 + 0.5 * octave1 + 0.25 * octave2);
	return result * amplitude;
	*/

	vec2 noise = vec2(0, 0);
	int octaves = 1;
	for (int i = 0; i < octaves; i++)
	{
		noise.x += sin(time * timeFrequency * 1.2343284759 + x * frequency) * amplitude;
		noise.y += cos(time * timeFrequency * 1.1294339528 + z * frequency) * amplitude;
		amplitude *= 0.5;
		frequency *= 2;
	}

	return noise;
}

float noise2(vec2 v)
{
	return texture2DLod(s_perlinTexture, v / TILE_SIZE, 0).r;
}

mat3 rotateX(float angle) {
  float s = sin(angle);
  float c = cos(angle);

  return mat3(
    1.0, 0.0, 0.0,
    0.0, c, s,
    0.0, -s, c
  );
}

mat3 rotateY(float angle) {
  float s = sin(angle);
  float c = cos(angle);

  return mat3(
    c, 0.0, -s,
    0.0, 1.0, 0.0,
    s, 0.0, c
  );
}

mat3 axisAngle(vec3 axis, float angle)
{
	float half = angle * 0.5;
	float s = sin(half);
	float x = axis.x * s;
	float y = axis.y * s;
	float z = axis.z * s;
	float w = cos(half);

	float m00, m10, m20;
	float m01, m11, m21;
	float m02, m12, m22;

	m00 = 1.0f - 2.0f * y * y - 2.0f * z * z;
	m01 = 2.0f * x * y + 2.0f * z * w;
	m02 = 2.0f * x * z - 2.0f * y * w;

	m10 = 2.0f * x * y - 2.0f * z * w;
	m11 = 1.0f - 2.0f * x * x - 2.0f * z * z;
	m12 = 2.0f * y * z + 2.0f * x * w;

	m20 = 2.0f * x * z + 2.0f * y * w;
	m21 = 2.0f * y * z - 2.0f * x * w;
	m22 = 1.0f - 2.0f * x * x - 2.0f * y * y;

	return mat3(
		m00, m10, m20,
		m01, m11, m21,
		m02, m12, m22
	);
}

void main()
{
	vec2 offset = u_materialData0.xy;
	float terrainSize = u_materialData0.z;
	float time = u_materialData0.w;

	vec3 bladePosition = i_data0.xyz;
	float bladeRotation = i_data0.w;
	//vec3 normal = i_data1.xyz;
	//float heightMultiplier = i_data1.w;

	//ivec2 heightmapSize = textureSize(s_heightmap, 0);
	//vec2 uv = (bladePosition.xz + offset) / terrainSize * (heightmapSize - 1) / heightmapSize + 0.5 / heightmapSize;
	float height = 0; //texture2DLod(s_heightmap, uv, 0).r;
	//vec3 normal = vec3(0, 1, 0); //normalize((texture2DLod(s_normalmap, uv, 0) * 2.0 - 1.0).rgb);
	float heightMultiplier = 1; //texture2DLod(s_splatMap, uv, 0).r;

	float curveAmount = 0.5;
	//curveAmount += noise2(vec2_splat(time * 0.35) * bladePosition.xz) * 0.1;

	// WIND

	float windDir = noise2(bladePosition.xz * 0.05 + time * 0.05);
	windDir = remap(windDir, 0, 1, 0, 2 * 3.1415);

	float windNoise = noise2(bladePosition.xz * 0.25 + time);
	windNoise = windNoise * windNoise;
	windNoise = remap(windNoise, 0, 1, 0.0, 1.5);

	mat3 windCurvatureMat = rotateX(windNoise);
	mat3 windDirectionMat = rotateY(windDir);
	vec3 windAxis = mul(windDirectionMat, vec3(1, 0, 0));
	mat3 windMat = axisAngle(windAxis, windNoise);
	//mat3 windMat = windDirectionMat * windCurvatureMat * transpose(windDirectionMat);

	vec3 localPosition = a_position;
	mat3 bladeCurvatureMat = rotateX(curveAmount * a_position.y);
	mat3 bladeRotationMat = rotateY(bladeRotation);
	localPosition = mul(bladeRotationMat, mul(bladeCurvatureMat, localPosition));
	localPosition = mul(windMat, localPosition);
	localPosition = localPosition * BLADE_SIZE;
	//localPosition.z -= localPosition.y * localPosition.y * 0.3;
	//localPosition = vec3(localPosition.x * cos(bladeRotation) + localPosition.z * sin(bladeRotation), localPosition.y, -localPosition.x * sin(bladeRotation) + localPosition.z * cos(bladeRotation));
	vec3 position = localPosition + bladePosition;
	vec4 worldPosition = mul(u_model[0], vec4(position, 1.0));

	vec3 localNormal = a_position.x < -0.01 ? vec3(-1, 1, 1) : a_position.x > 0.01 ? vec3(1, 1, 1) : vec3(0, 1, 1);
	localNormal = normalize(localNormal);
	localNormal = mul(bladeRotationMat, mul(bladeCurvatureMat, localNormal));
	localNormal = mul(windMat, localNormal);
	vec4 worldNormal = mul(u_model[0], vec4(localNormal, 0.0));

	vec3 viewDir = mul(transpose(u_view), vec4(0, 0, -1, 0)).xyz;
	vec4 mvNormal = mul(u_view, worldNormal);
	float viewDirDot = abs(dot(viewDir, mvNormal.xyz));
	float thickenFactor = (1 - viewDirDot) * 0.015;


	


	float waveFrequency = 0.5 * 2;
	float waveAmplitude = 0.2 * 2;
	float waveTimeFrequency = 0.5 * 2;

	//vec4 noise = texture2DLod(s_perlinTexture, worldPosition.xz, 0);
	vec2 displacement = noise2d(worldPosition.x, worldPosition.z, waveFrequency, waveAmplitude, time, waveTimeFrequency);

	/*
	displacement.x += sin(time * waveFrequency * 1.2343284759 + worldPosition.x * waveSpacialFrequency) * waveAmplitude;
	displacement.y += cos(time * waveFrequency * 1.1294339528 + worldPosition.z * waveSpacialFrequency) * waveAmplitude;
	displacement.x += 0.5 * sin(2.0 * (time * waveFrequency * 1.0537537883 + worldPosition.x * waveSpacialFrequency)) * waveAmplitude;
	displacement.y += 0.5 * cos(2.0 * (time * waveFrequency * 1.2749836812 + worldPosition.z * waveSpacialFrequency)) * waveAmplitude;
	*/

	//worldPosition.xz += displacement * vertexHeight;
	//normal.xz += displacement * vertexHeight * 1.0;


	vec4 mvPosition = mul(u_view, worldPosition);
	vec3 mvRight = vec3(sign(a_position.x) * sign(mvNormal.z), 0, 0);
	//mvPosition.x += thickenFactor * mvRight;
	gl_Position = mul(u_proj, mvPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_color0 = vec4(a_position.y, /*uv*/ 0, 0, 1);
	v_texcoord0 = a_texcoord0;
}
