$input v_camera, v_view, v_size


#include "../bgfx/common.shader"
#include "raytracing.shader"


SAMPLER3D(u_voxels, 0);

uniform vec4 u_textureOffset;
uniform vec4 u_textureDim;


void main()
{
	vec3 view = normalize(v_view);
	
	vec3 position, color, normal;
	bool hit = RayTraceVoxelGrid(v_camera, view, v_size, u_voxels, (ivec3)(u_textureOffset.xyz + 0.5), (ivec3)(u_textureDim.xyz + 0.5), position, color, normal);
	
	vec3 toLight = normalize(vec3(-1, 1, -1));
	float ndotl = dot(normal, toLight);
	vec3 diffuse = 0.5 + vec3_splat(0.5) * ndotl;
	vec3 result = diffuse * color;
	
	if (!hit)
		discard;
	
	gl_FragColor = vec4(result, 1.0);
	//gl_FragColor = vec4(1.0 - exp(-depth), 0, 1.0 - exp(-depth), 1);
}
