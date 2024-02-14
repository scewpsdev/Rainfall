#ifndef __UTILS_SH__
#define __UTILS_SH__


float remap(float f, float min, float max, float newMin, float newMax)
{
	return (f - min) / (max - min) * (newMax - newMin) + newMin;
}

float depthToDistance(float depth, float near, float far)
{
	return 2.0 * near * far / (far + near - depth * (far - near));
}

float distanceToDepth(float distance, float near, float far)
{
	float a = -(far + near) / (far - near);
	float b = -2.0 * far * near / (far - near);
	return (-a * distance + b) / distance;
}

vec3 SRGBToLinear(vec3 color)
{
	float gamma = 2.2;
	return pow(color, vec3_splat(gamma));
}

vec4 SRGBToLinear(vec4 color)
{
	return vec4(SRGBToLinear(color.rgb), color.a);
}

vec3 linearToSRGB(vec3 color)
{
	float gamma = 2.2;
	return pow(color, vec3_splat(1.0 / gamma));
}

vec4 linearToSRGB(vec4 color)
{
	return vec4(linearToSRGB(color.rgb), color.a);
}

float RGBToLuminance(vec3 rgb)
{
	return dot(rgb, vec3(0.2126, 0.7152, 0.0722));
}


#endif // __UTILS_SH__
