#include "../common/common.shader"


bool OcclusionCulling(vec3 aabbMin, vec3 aabbMax, mat4 pv, sampler2D hzb)
{
    vec3 boxSize = aabbMax - aabbMin;

    vec3 boxCorners[] = {
        aabbMin,
        aabbMin + vec3(boxSize.x, 0, 0),
        aabbMin + vec3(0, boxSize.y, 0),
        aabbMin + vec3(0, 0, boxSize.z),
        aabbMin + vec3(boxSize.xy, 0),
        aabbMin + vec3(0, boxSize.yz),
        aabbMin + vec3(boxSize.x, 0, boxSize.z),
        aabbMin + boxSize,
    };

    float minZ = 1.0;
    vec2 minXY = vec2(1.0, 1.0);
	vec2 maxXY = vec2(0.0, 0.0);

	UNROLL
	for (int i = 0; i < 8; i++)
	{
		//transform World space aaBox to NDC
		vec4 clipPos = mul( pv, vec4(boxCorners[i], 1) );

#if BGFX_SHADER_LANGUAGE_GLSL 
		clipPos.z = 0.5 * ( clipPos.z + clipPos.w );
#endif
		clipPos.z = max(clipPos.z, 0);

		clipPos.xyz = clipPos.xyz / clipPos.w;

		clipPos.xy = clamp(clipPos.xy, -1, 1);
		clipPos.xy = clipPos.xy * vec2(0.5, -0.5) + vec2(0.5, 0.5);

		minXY = min(clipPos.xy, minXY);
		maxXY = max(clipPos.xy, maxXY);

		minZ = saturate(min(minZ, clipPos.z));
	}

	vec4 boxUVs = vec4(minXY, maxXY);

	// Calculate hi-Z buffer mip
    ivec2 u_inputRTSize = textureSize(hzb, 0);
	ivec2 size = ivec2( (maxXY - minXY) * u_inputRTSize.xy);
	float mip = ceil(log2(max(size.x, size.y)));

    int maxMip = floor(log2(max(u_inputRTSize.x, u_inputRTSize.y)));
	mip = clamp(mip, 0, maxMip);

	// Texel footprint for the lower (finer-grained) level
	float level_lower = max(mip - 1, 0);
	vec2 scale = vec2_splat(exp2(-level_lower) );
	vec2 a = floor(boxUVs.xy*scale);
	vec2 b = ceil(boxUVs.zw*scale);
	vec2 dims = b - a;

	// Use the lower level if we only touch <= 2 texels in both dimensions
	if (dims.x <= 2 && dims.y <= 2)
		mip = level_lower;

#if BGFX_SHADER_LANGUAGE_GLSL
	boxUVs.y = 1.0 - boxUVs.y;
	boxUVs.w = 1.0 - boxUVs.w;
#endif
	//load depths from high z buffer
	vec4 depth =
	{
		texture2DLod(hzb, boxUVs.xy, mip).x,
		texture2DLod(hzb, boxUVs.zy, mip).x,
		texture2DLod(hzb, boxUVs.xw, mip).x,
		texture2DLod(hzb, boxUVs.zw, mip).x,
	};

	//find the max depth
	float maxDepth = max( max(depth.x, depth.y), max(depth.z, depth.w) );

	return minZ <= maxDepth;
}
