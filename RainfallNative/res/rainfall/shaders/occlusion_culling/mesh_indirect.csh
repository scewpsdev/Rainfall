#include "../bgfx/bgfx_compute.shader"
#include "occlusion_culling.shader"


BUFFER_RO(aabbBuffer, vec4, 0);
BUFFER_WR(indirectBuffer, uvec4, 1);

SAMPLER2D(s_hzb, 2);

uniform vec4 u_params;
uniform mat4 u_pv;


NUM_THREADS(64, 1, 1)
void main()
{
    int i = gl_GlobalInvocationID.x;
	
    vec3 aabbMin = aabbBuffer[i * 2 + 0].xyz;
    vec3 aabbMax = aabbBuffer[i * 2 + 1].xyz;
	
    bool predicate = OcclusionCulling(aabbMin, aabbMax, u_pv, s_hzb);
	
	int numIndices = predicate ? int(aabbBuffer[i * 2 + 1].w + 0.5) : 0;
	int numInstances = predicate ? 1 : 0;
	int instanceOffset = predicate ? i : 0;

	drawIndexedIndirect(
		// target location params
		indirectBuffer, // target buffer
		i, // index in buffer
		// draw call params
		numIndices, // number of indices for this draw call
		1, // number of instances for this draw call. You can disable this draw call by setting to zero
		0, // offset in the index buffer
		0, // offset in the vertex buffer. Note that you can use this to "reindex" submeshses - all indicies in this draw will be decremented by this amount
		i           // offset in the instance buffer. If you are drawing more than 1 instance per call see gpudrivenrendering for how to handle
	);
}
