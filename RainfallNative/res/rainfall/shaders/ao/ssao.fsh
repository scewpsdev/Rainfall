$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_depthBuffer, 0);
SAMPLER2D(s_normalBuffer, 1);
SAMPLER2D(s_lastAO, 2);

SAMPLER2D(s_noise, 3);

uniform vec4 u_cameraFrustum;
uniform mat4 u_viewMatrix;
uniform mat4 u_projectionInv;

uniform vec4 u_params;
#define u_idx u_params[0]
#define u_lod u_params[1]
#define u_width u_params[2]
#define u_height u_params[3]
#define u_size u_params.zw
uniform vec4 u_params2;
#define u_finestLod u_params2[0]


// https://www.shadertoy.com/view/Ms33WB


#define SAMPLES 6
#define INTENSITY 2
#define SCALE 2.5
#define BIAS 0.05
#define SAMPLE_RAD 0.02
#define MAX_DISTANCE 0.025


float getDistance(vec2 uv)
{
    float near = u_cameraFrustum[0];
    float far = u_cameraFrustum[1];
    float depth = texture2DLod(s_depthBuffer, uv, u_lod).r;
    float dist = depthToDistance(depth, near, far);
    return dist;
}

float getDistanceLod(vec2 uv, float lod)
{
    float near = u_cameraFrustum[0];
    float far = u_cameraFrustum[1];
    float depth = texture2DLod(s_depthBuffer, uv, lod).r;
    float dist = depthToDistance(depth, near, far);
    return dist;
}

vec3 getPosition(vec2 uv)
{
    float depth = texture2DLod(s_depthBuffer, uv, u_lod).r;
    vec4 positionClipSpace = vec4(uv.x * 2 - 1, uv.y * -2 + 1, depth, 1);
    vec4 positionViewSpace = mul(u_projectionInv, positionClipSpace);
    positionViewSpace.xyz /= positionViewSpace.w;

    return positionViewSpace.xyz;
}

vec3 getNormal(vec2 uv)
{
    // TODO normalize in downsampling step
    vec3 normal = normalize(texture2DLod(s_normalBuffer, uv, u_lod).xyz * 2.0 - 1.0);
    //vec3 normalViewSpace = mul(u_viewMatrix, vec4(normal, 0.0)).xyz;
    //return normalViewSpace;
    return normal;
}

vec3 getNormalLod(vec2 uv, float lod)
{
    vec3 normal = normalize(texture2DLod(s_normalBuffer, uv, lod).xyz * 2.0 - 1.0);
    //vec3 normalViewSpace = mul(u_viewMatrix, vec4(normal, 0.0)).xyz;
    //return normalViewSpace;
    return normal;
}

float doAmbientOcclusion(in vec2 tcoord, in vec2 uv, in vec3 p, in vec3 cnorm, float scale, inout float influence)
{
    uv.x *= u_viewRect.w / u_viewRect.z;
    vec3 diff = (getPosition(tcoord + uv) - p) / scale;
    float l = length(diff);
    vec3 v = diff / l;
    float d = l * SCALE;
    float ao = max(0.0, dot(v, cnorm) - BIAS) * (1.0 / (1.0 + d / scale));
    influence = smoothstep(MAX_DISTANCE, MAX_DISTANCE * 0.5, diff.z);
    ao *= influence;
    return ao;
}

float spiralAO(vec2 uv, vec3 p, vec3 n, float scale)
{
    float goldenAngle = 2.4;
    float ao = 0.;
    float inv = 1. / float(SAMPLES);
    float radius = 0.;

    float rotatePhase = texture2D(s_noise, uv * u_viewRect.zw / textureSize(s_noise, 0)).r * 6.28;
    float rStep = inv * SAMPLE_RAD * 1000.0 / u_viewRect.w;
    vec2 spiralUV;

    float sampleInfluence = 0.0;
    for (int i = 0; i < SAMPLES; i++)
    {
        spiralUV.x = sin(rotatePhase);
        spiralUV.y = cos(rotatePhase);
        radius += rStep;
        float influence;
        ao += doAmbientOcclusion(uv, spiralUV * radius, p, n, scale, influence);
        sampleInfluence += influence;
        rotatePhase += goldenAngle;
    }
    ao /= sampleInfluence;
    return ao;
}

float computeAO(vec2 uv, vec3 position, vec3 normal)
{
    float maxDistance = 2;
    int maxKernelSize = u_lod == 0 ? 2 : 5;

    int kernelSize = int((u_height * maxDistance) / (2 * -position.z * 1.0 /*tan(PI * 0.25)*/));
    kernelSize = min(kernelSize, maxKernelSize);

    if (kernelSize == 0)
        return 0;

    ivec2 bufferSize = u_size / pow(2, int(u_lod + 0.5));

    float sum = 0.0;
    float ao = 0.0;
    for (int y = -kernelSize; y <= kernelSize; y += 2)
    {
        for (int x = -kernelSize; x <= kernelSize; x += 2)
        {
            if (x == 0 && y == 0)
                continue;

            vec2 offset = vec2(x, y) / bufferSize;
            vec3 samplePosition = getPosition(uv + offset);
            vec3 toSample = samplePosition - position;
            float dist = length(toSample);
            float d = max(dot(normal, toSample / dist), 0);
            float falloff = 1 - min(1, dist * dist / (maxDistance * maxDistance));

            //vec3 sampleNormal = getNormal(uv + offset);
            //if (dot(sampleNormal, normal) < 0.1)
            //    ao = 1;

            ao += falloff * d;
            sum += 1;
        }
    }

    ao /= sum;

    return ao;
}

void getCoarseSample(vec2 uv, int idx, out float depth, out vec3 normal, out float sample, out float bilinearWeight)
{
    ivec2 bufferSize = u_size / pow(2, int(u_lod + 0.5));
    ivec2 lastBufferSize = bufferSize / 2;

    int x = idx % 2;
    int y = idx / 2;

    vec2 sampleUV = (floor(uv * lastBufferSize) + vec2(x, y)) / lastBufferSize;

    depth = getDistanceLod(sampleUV, u_lod + 1);
    normal = getNormalLod(sampleUV, u_lod + 1);

    sample = texture2D(s_lastAO, sampleUV).r;

    vec2 localUV = uv * lastBufferSize - floor(uv * lastBufferSize);

    float bilinearX = 1 - abs(localUV.x - x);
    float bilinearY = 1 - abs(localUV.y - y);
    bilinearWeight = bilinearX * bilinearY;
}

float upsample(vec2 uv, float depth, vec3 normal)
{
    float ao = 0.0;
    float sum = 0.0;
    for (int i = 0; i < 4; i++)
    {
        float sampleDepth;
        vec3 sampleNormal;
        float sample;
        float bilinearWeight;
        getCoarseSample(uv, i, sampleDepth, sampleNormal, sample, bilinearWeight);

        float tz = 16;
        float depthWeight = pow(1.0 / (1 + abs(sampleDepth - depth)), tz);

        float tn = 8;
        float normalWeight = pow((dot(normal, sampleNormal) + 1) / 2, tn);

        float weight = bilinearWeight * depthWeight * normalWeight;
        ao += weight * sample;
        sum += weight;
    }

    ao /= sum;

    return ao;
}

void main()
{
    vec2 uv = v_texcoord0;

    float near = u_cameraFrustum[0];
    float far = u_cameraFrustum[1];

    float depth = getDistance(uv);
    if (depth == 1)
        discard;

    vec3 position = getPosition(uv);
    vec3 normal = getNormal(uv);

    float ao1 = computeAO(uv, position, normal);

    float ao;
    if (u_idx == 0)
    {
        ao = ao1;
    }
    else
    {
        float ao2 = upsample(uv, depth, normal);
        float ao3 = max(ao1, ao2);

        if (u_lod == u_finestLod)
            ao = 1 - ao3;
        else
            ao = ao3;
    }

    gl_FragColor = vec4(vec3_splat(ao), 1.0);
    return;


    /*
    vec3 p = getPosition(uv);
    vec3 n = getNormal(uv);

    float scale = -p.z;

    float ao = spiralAO(uv, p, n, scale);
    ao = 1 - ao;
    //ao = pow(1 - ao, INTENSITY);

    gl_FragColor = vec4(ao, ao, ao, 1.0);
    */
}
