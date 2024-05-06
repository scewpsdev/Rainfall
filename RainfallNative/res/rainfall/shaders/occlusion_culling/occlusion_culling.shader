


bool OcclusionCulling(vec3 aabbMin, vec3 aabbMax, mat4 pv, sampler2D hzb)
{
	vec4 point0 = mul(pv, vec4(aabbMin.x, aabbMin.y, aabbMin.z, 1));
    vec4 point1 = mul(pv, vec4(aabbMax.x, aabbMin.y, aabbMin.z, 1));
    vec4 point2 = mul(pv, vec4(aabbMin.x, aabbMin.y, aabbMax.z, 1));
    vec4 point3 = mul(pv, vec4(aabbMax.x, aabbMin.y, aabbMax.z, 1));
    vec4 point4 = mul(pv, vec4(aabbMin.x, aabbMax.y, aabbMin.z, 1));
    vec4 point5 = mul(pv, vec4(aabbMax.x, aabbMax.y, aabbMin.z, 1));
    vec4 point6 = mul(pv, vec4(aabbMin.x, aabbMax.y, aabbMax.z, 1));
    vec4 point7 = mul(pv, vec4(aabbMax.x, aabbMax.y, aabbMax.z, 1));
    
    point0.xyz /= point0.w;
    point1.xyz /= point1.w;
    point2.xyz /= point2.w;
    point3.xyz /= point3.w;
    point4.xyz /= point4.w;
    point5.xyz /= point5.w;
    point6.xyz /= point6.w;
    point7.xyz /= point7.w;
	
    float x0 = min(min(min(point0.x, point1.x), min(point2.x, point3.x)), min(min(point4.x, point5.x), min(point6.x, point7.x)));
    float x1 = max(max(max(point0.x, point1.x), max(point2.x, point3.x)), max(max(point4.x, point5.x), max(point6.x, point7.x)));
    float y0 = min(min(min(point0.y, point1.y), min(point2.y, point3.y)), min(min(point4.y, point5.y), min(point6.y, point7.y)));
    float y1 = max(max(max(point0.y, point1.y), max(point2.y, point3.y)), max(max(point4.y, point5.y), max(point6.y, point7.y)));
	
    float z0 = min(min(min(point0.z, point1.z), min(point2.z, point3.z)), min(min(point4.z, point5.z), min(point6.z, point7.z)));
    
    float u0 = x0 * 0.5 + 0.5;
    float u1 = x1 * 0.5 + 0.5;
    float v0 = -y1 * 0.5 + 0.5;
    float v1 = -y0 * 0.5 + 0.5;
	
    ivec2 hzbSize = textureSize(hzb, 0);
    float pixelWidth = (u1 - u0) * hzbSize.x;
    float pixelHeight = (v1 - v0) * hzbSize.y;
    int mipLevel = floor(log2(max(pixelWidth, pixelHeight)));
	
    float sample0 = texture2DLod(hzb, vec2(u0, v0), mipLevel).r;
    float sample1 = texture2DLod(hzb, vec2(u1, v0), mipLevel).r;
    float sample2 = texture2DLod(hzb, vec2(u0, v1), mipLevel).r;
    float sample3 = texture2DLod(hzb, vec2(u1, v1), mipLevel).r;
    float maxDepth = max(max(sample0, sample1), max(sample2, sample3));
	
    bool visible = z0 < maxDepth;

    return visible;
}