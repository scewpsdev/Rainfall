$input v_camera, v_view, v_size


#include "../bgfx/common.shader"
#include "raytracing.shader"


SAMPLER3D(u_voxels, 0);


void main()
{
	vec3 view = normalize(v_view);
	
	vec3 position, color, normal;
	int numSteps;
	bool hit = RayTraceVoxelGrid(v_camera, view, v_size, u_voxels, position, color, normal, numSteps);
	
	vec3 toLight = normalize(vec3(-1, 2, -1));
	float ndotl = dot(normal, toLight) * 0.5 + 0.5;
	vec3 diffuse = ndotl * color;
	vec3 result = diffuse;
	
	if (!hit)
		discard;
	
	//gl_FragColor = vec4(vec3_splat(numSteps / 64.0), 1.0);
	gl_FragColor = vec4(hit ? result : color, 1.0);
}
