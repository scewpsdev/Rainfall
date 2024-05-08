#include "../bgfx/bgfx_compute.shader"
#include "occlusion_culling.shader"


BUFFER_RO(pointLightBuffer, vec4, 0);
BUFFER_RW(instanceCount, uint, 1);
BUFFER_WR(instancePredicates, bool, 2);

SAMPLER2D(s_hzb, 3);

BUFFER_RW(drawcallData, uvec4, 4);

uniform vec4 u_params;
uniform mat4 u_pv;


NUM_THREADS(64, 1, 1)
void main()
{
    int i = gl_GlobalInvocationID.x;

	bool predicate = false;

	if (i < uint(u_params.x + 0.5))
	{
		vec3 lightPosition = pointLightBuffer[i * 2 + 0].xyz;
		float lightRadius = pointLightBuffer[i * 2 + 0].w;
		vec3 lightColor = pointLightBuffer[i * 2 + 1].xyz;

		predicate = OcclusionCulling(lightPosition - lightRadius, lightPosition + lightRadius, u_pv, s_hzb);

		//if (predicate)
		//	atomicAdd(instanceCount[0], 1);
	}
	
	if (predicate)
	{
		drawIndexedIndirect(
			drawcallData,
			i,
			6*6, 			//number of indices
			1, 				//number of instances
			0,
			0,			//offset into the vertex buffer
			i							//offset into the instance buffer
			);
	}

	instancePredicates[i] = predicate;
}
