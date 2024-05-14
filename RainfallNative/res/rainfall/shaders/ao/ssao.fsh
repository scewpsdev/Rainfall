$input v_texcoord0

#include "../common/common.shader"


#define KERNEL_SIZE 64
#define SSAO_RADIUS 1.0
#define SSAO_POWER 2.0
#define DEPTH_BIAS 0.000001


SAMPLER2D(s_depthBuffer, 0);
SAMPLER2D(s_normalBuffer, 1);

//uniform vec4 u_ssaoKernel[KERNEL_SIZE];
SAMPLER2D(s_noise, 2);

uniform vec4 u_cameraFrustum;
uniform mat4 u_viewMatrix;
uniform mat4 u_viewInv;
uniform mat4 u_projectionInv;
uniform mat4 u_projectionView;
uniform mat4 u_projectionViewInv;


#define FK(k) floatBitsToInt(cos(k))^floatBitsToInt(k)
float hash(float a, float b)
{
    int x = FK(a);
    int y = FK(b);
    return float((x * x + y) * (y * y - x) + x) / 2.14e9;
}

vec3 randvec(float seed)
{
    float h1 = hash(seed, seed);
    float h2 = hash(h1, seed);
    float h3 = hash(h2, seed);
    return vec3(h1, h2, h3);
}

float rand(float seed)
{
    return hash(seed, seed);
}

/*
void main_()
{
	float near = u_cameraFrustum[0];
	float far = u_cameraFrustum[1];

	float depth = texture2DLod(s_depthBuffer, v_texcoord0, 0.0).r;
	float distance = depthToDistance(depth, near, far);
	vec3 positionClipSpace = vec3(v_texcoord0.x * 2.0 - 1.0, v_texcoord0.y * -2.0 + 1.0, depth);
	vec3 position = clipToWorld(u_projectionViewInv, positionClipSpace);
	//vec3 position = texture2DLod(s_positionBuffer, v_texcoord0, 0.0).xyz;
	//vec3 viewSpacePos = mul(u_viewMatrix, vec4(position, 1.0)).xyz;

	vec3 normal = normalize(texture2D(s_normalBuffer, v_texcoord0).xyz * 2.0 - 1.0);
	//vec3 randomVector = normalize(vec3(texture2D(s_ssaoNoise, v_texcoord0 / (u_viewTexel.xy * 4.0)).xy * 2.0 - 1.0, 0.0));
	vec2 fragmentCoord = v_texcoord0 * u_viewRect.zw;
	float fragmentID = fragmentCoord.x + fragmentCoord.y * u_viewRect.z;
	float angle = rand(fragmentID);
	vec3 randomVector = vec3(cos(angle), 0.0, -sin(angle));
	//vec3 randomVector = normalize(vec3(randvec(hash(fragmentID, -fragmentID)).xy, 0.0));

	vec3 tangent = normalize(randomVector - normal * dot(randomVector, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 tbn = mat3(
		tangent.x, bitangent.x, normal.x,
		tangent.y, bitangent.y, normal.y,
		tangent.z, bitangent.z, normal.z
	);

	//float lodLevel = log(min(-positionViewSpace.z / 16.0, 1.0)) / log(0.5);
	//float radius = min(distance / 5.0, 1.0);
	float radius = SSAO_RADIUS;

	float occlusion = 0.0;
	for (int i = 0; i < KERNEL_SIZE; i++)
	{
		vec3 samplePos = position + mul(tbn, u_ssaoKernel[i].xyz) * radius;
		//vec3 samplePosView = mul(u_viewMatrix, vec4(samplePos, 1.0)).xyz;

		vec4 samplePosClipSpace = mul(u_projectionView, vec4(samplePos, 1.0));
		samplePosClipSpace.xyz /= samplePosClipSpace.w;

		vec2 samplePosTexCoord = vec2(samplePosClipSpace.x * 0.5 + 0.5, -samplePosClipSpace.y * 0.5 + 0.5);

		float surfaceDepth = texture2DLod(s_depthBuffer, samplePosTexCoord, 0.0).r;
		float surfaceDistance = depthToDistance(surfaceDepth, near, far);
		//vec3 surfacePosition = texture2DLod(s_positionBuffer, samplePosTexCoord, 0.0).xyz;
		//vec3 surfacePositionView = mul(u_viewMatrix, vec4(surfacePosition, 1.0)).xyz;

		float rangeCheck = smoothstep(0.0, 1.0, 0.1 * radius / abs(distance - surfaceDistance));
		occlusion += (surfaceDepth < samplePosClipSpace.z - DEPTH_BIAS ? 1.0 : 0.0) * rangeCheck;
	}
	occlusion *= 1.0 / KERNEL_SIZE;

	float shading = pow(1.0 - occlusion, SSAO_POWER);
	gl_FragColor = vec4(1.0 - shading, 1.0, 1.0, 1.0);
	//gl_FragColor = vec4(float(int(lodLevel + 0.5)) / 4.0, 1.0, 1.0, 1.0);
}
*/


// https://www.shadertoy.com/view/Ms33WB


#define SAMPLES 16
#define INTENSITY 5
#define SCALE 2.5
#define BIAS 0.05
#define SAMPLE_RAD 0.05
#define MAX_DISTANCE 0.5

#define MOD3 vec3(.1031,.11369,.13787)

float hash12(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * MOD3);
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z);
}

vec2 hash22(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * MOD3);
    p3 += dot(p3, p3.yzx + 19.19);
    return fract(vec2((p3.x + p3.y) * p3.z, (p3.x + p3.z) * p3.y));
}

vec3 getPosition(vec2 uv)
{
    float near = u_cameraFrustum[0];
    float far = u_cameraFrustum[1];

    float depth = texture2DLod(s_depthBuffer, uv, 2.0).r;
    float distance = depthToDistance(depth, near, far);
    vec3 positionClipSpace = vec3(uv.x * 2.0 - 1.0, uv.y * -2.0 + 1.0, depth);
    vec3 positionViewSpace = clipToWorld(u_projectionInv, positionClipSpace);
	//vec3 position = clipToWorld(u_projectionViewInv, positionClipSpace);

    return positionViewSpace * 0.1;
}

vec3 getNormal(vec2 uv)
{
    vec3 normal = normalize(texture2D( s_normalBuffer, uv).xyz * 2.0 - 1.0);
    vec3 normalViewSpace = mul(u_viewMatrix, vec4(normal, 0.0)).xyz;
	//vec3 normalViewSpace = normalize(mul(u_viewMatrix, vec4(normal, 0.0)).xyz);
    return normalViewSpace;
}

vec2 getRandom(vec2 uv)
{
    return normalize(hash22(uv * 126.1231) * 2. - 1.);
}


float doAmbientOcclusion(in vec2 tcoord, in vec2 uv, in vec3 p, in vec3 cnorm, float scale)
{
    vec3 diff = (getPosition(tcoord + uv) - p) / scale;
    float l = length(diff);
    vec3 v = diff / l;
    float d = l * SCALE;
    float ao = max(0.0, dot(cnorm, v) - BIAS) * (1.0 / (1.0 + d / scale));
	//ao *= smoothstep(MAX_DISTANCE, MAX_DISTANCE * 0.5, l);
    ao *= smoothstep(MAX_DISTANCE, MAX_DISTANCE * 0.5, diff.z);
    return ao;
}

float spiralAO(vec2 uv, vec3 p, vec3 n, float scale)
{
    float goldenAngle = 2.4;
    float ao = 0.;
    float inv = 1. / float(SAMPLES);
    float radius = 0.;

    float rotatePhase = texture2D(s_noise, uv * u_viewRect.zw / textureSize(s_noise, 0)).r * 6.28;
    float rStep = inv * SAMPLE_RAD;
    vec2 spiralUV;

    for (int i = 0; i < SAMPLES; i++)
    {
        spiralUV.x = sin(rotatePhase);
        spiralUV.y = cos(rotatePhase);
        radius += rStep;
        ao += doAmbientOcclusion(uv, spiralUV * radius, p, n, scale);
        rotatePhase += goldenAngle;
    }
    ao *= inv;
    return ao;
}

void main()
{
    vec2 uv = v_texcoord0;

    vec3 p = getPosition(uv);
    vec3 n = getNormal(uv);

    float ao = 0.;
	//float rad = min(SAMPLE_RAD / -p.z, SAMPLE_RAD / 0.5);
    float scale = -p.z;

    ao = spiralAO(uv, p, n, scale);

    ao = pow(1 - ao, INTENSITY);
    //ao = 1. - ao;

    gl_FragColor = vec4(ao, ao, ao, 1.0);
	//gl_FragColor = vec4(n.y, 0.0, 0.0, 1.0);
}
