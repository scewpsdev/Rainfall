


vec2 StratifiedPoisson(int hash)
{
	vec2 poissonDisk[16] = {
		vec2(-0.94201624, -0.39906216),
		vec2(0.94558609, -0.76890725),
		vec2(-0.094184101, -0.92938870),
		vec2(0.34495938, 0.29387760),
		vec2(-0.91588581, 0.45771432),
		vec2(-0.81544232, -0.87912464),
		vec2(-0.38277543, 0.27676845),
		vec2(0.97484398, 0.75648379),
		vec2(0.44323325, -0.97511554),
		vec2(0.53742981, -0.47373420),
		vec2(-0.26496911, -0.41893023),
		vec2(0.79197514, 0.19090188),
		vec2(-0.24188840, 0.99706507),
		vec2(-0.81409955, 0.91437590),
		vec2(0.19984126, 0.78641367),
		vec2(0.14383161, -0.14100790)
	};
	return poissonDisk[hash % 16];
}

int hash4(vec4 seed)
{
	float d = dot(seed, vec4(12.9898, 78.233, 45.164, 94.673));
	float f = fract(sin(d) * 43758.5453);
	int hash = int(16.0 * f);
	return hash;
}

float hash2(vec2 seed)
{
	float d = dot(seed, vec2(12.9898, 78.233));
	float f = fract(sin(d) * 43758.5453);
	return f;
}

float CalculateDirectionalShadow(vec3 position, float distance, sampler2DShadow shadowMap, float shadowMapFar, mat4 toLightSpace, float fadeOutFactor, vec4 fragCoord)
{
	const float SHADOW_MAP_EPSILON = 0.001;
	const int NUM_SAMPLES = 16;

	vec4 lightSpacePosition = mul(toLightSpace, vec4(position, 1.0));
	vec3 projectedCoords = lightSpacePosition.xyz / lightSpacePosition.w;
	vec2 sampleCoords = 0.5 * projectedCoords.xy * vec2(1.0, -1.0) + 0.5;

	//if (sampleCoords.x < 0.0 || sampleCoords.x > 1.0 || sampleCoords.y < 0.0 || sampleCoords.y > 1.0)
	//	return 1.0;


	ivec2 shadowMapSize = textureSize(shadowMap, 0);

	/*
	vec2 sampleMatrix[4] = {
		vec2(-0.25, -0.25),
		vec2(0.25, -0.25),
		vec2(0.25, 0.25),
		vec2(-0.25, 0.25)
	};
	*/

	float result = 0.0;
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		//int idx = int(hash2(fragCoord.xy) + i) % 16;
		//vec2 poissonValue = StratifiedPoisson(idx);
		vec2 sampleStride = 1.0 / shadowMapSize;
		vec2 sampleOffset = StratifiedPoisson(i) * sampleStride;
		//vec4 shadowSample = textureGather(shadowMap, sampleCoords.xy + sampleOffset, 0);
		result += shadow2D(shadowMap, vec3(sampleCoords.xy + sampleOffset, projectedCoords.z + SHADOW_MAP_EPSILON));
		//result += texture2D(shadowMap, sampleCoords + sampleOffset).r <= projectedCoords.z - SHADOW_MAP_EPSILON ? 0.0 : 1.0;
	}
	result /= NUM_SAMPLES;

	float fadeOut = clamp(remap(distance / shadowMapFar, 0.9, 1.0, 1.0, 0.0), 0.0, 1.0);
	fadeOut = 1.0 - ((1.0 - fadeOut) * fadeOutFactor);
	//float fadeOut = (sampleCoords.x < 0.001 || sampleCoords.y < 0.001 || sampleCoords.x > 0.999 || sampleCoords.y > 0.999) ? 0.0 : 1.0;
	result = 1.0 - ((1.0 - result) * fadeOut);


	/*
	ivec2 shadowMapSize = textureSize(shadowMap, 0);




	*/

	//float closestDepth = texture2D(shadowMap, sampleCoords.xy).r;
	//float result = closestDepth <= projectedCoords.z - SHADOW_MAP_EPSILON ? 0.0 : 1.0;
	//float result = shadow2D(shadowMap, sampleCoords);


	//if (sampleCoords.x > 1.0 || sampleCoords.x < 0.0 || sampleCoords.y > 1.0 || sampleCoords.y < 0.0)
	//	return -1.0;

	return result;

	/*
	vec4 lightSpacePosition = mul(toLightSpace, vec4(position, 1.0));
	vec3 projectedCoords = lightSpacePosition.xyz / lightSpacePosition.w;
	vec3 sampleCoords = 0.5 * projectedCoords.xyz + 0.5;

	ivec2 shadowMapSize = textureSize(shadowMap, 0);

	const float SHADOW_MAP_EPSILON = 0.002;
	const int NUM_SAMPLES = 4;

	sampleCoords.z -= SHADOW_MAP_EPSILON;

	float result = 0.0f;
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		//int idx = hash(vec4(int(gl_FragCoord.x), int(gl_FragCoord.y), int(gl_FragCoord.y), i));
		int idx = 0;
		vec2 poissonValue = StratifiedPoisson(idx);
		vec2 sampleStride = 1.0 / shadowMapSize;
		result += shadow2D(shadowMap, vec3(sampleCoords)/* + vec3(poissonValue * sampleStride, 0.0));
	}
	result /= NUM_SAMPLES;

	//float fadeOut = 0.001; //clamp(remap(dist / un_StaticShadowMapFarPlane, 0.9, 1.0, 1.0, 0.0), 0.0, 1.0);
	float fadeOut = (sampleCoords.x < 0.001 || sampleCoords.y < 0.001 || sampleCoords.x > 0.999 || sampleCoords.y > 0.999) ? 0.0 : 1.0;
	result = 1.0 - ((1.0 - result) * fadeOut);

	return result;
	*/
}
