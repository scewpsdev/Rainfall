


void animateWater(vec2 p, float t, int numWaves, out vec3 outPosition, out vec3 outNormal)
{
	float amplitudeMultiplier = 0.82;
	float frequencyMultiplier = 1.18;

	float amplitude = 1.0;
	float frequency = 0.1;
	float result = 0.0;
	vec2 derivative = vec2_splat(0.0);
	for (int i = 0; i < numWaves; i++)
	{
		float waveSpeed = 1.0 + sin(i * 34871) * 0.2;
		vec2 waveDirection = vec2(cos(-0.7 + i * 34871), sin(-0.7 + i * 34871));
		float waveOffset = i * 8437.3439847;

		float x = dot(waveDirection, p) * frequency + t * waveSpeed + waveOffset;

		result += (exp(sin(x) - 1.0) * 2.0 - 1.0) * amplitude;

		derivative += waveDirection * cos(x) * exp(sin(x) - 1.0) * amplitude;

		p -= derivative * 0.1;
		amplitude *= amplitudeMultiplier;
		frequency *= frequencyMultiplier;
	}

	vec3 tangent = vec3(1, derivative.x, 0);
	vec3 bitangent = vec3(0, derivative.y, 1);
	vec3 normal = normalize(cross(bitangent, tangent));
	
	outPosition = vec3(0.0, result, 0.0);
	outNormal = normal;
}
