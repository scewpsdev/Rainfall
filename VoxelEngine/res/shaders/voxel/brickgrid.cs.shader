$input v_view


#include "../bgfx/common.shader"
#include "../bgfx/bgfx_compute.shader"
#include "raytracing.shader"



uniform mat4 u_projInv;
uniform mat4 u_viewInv;
uniform vec4 u_gridPosition;
uniform vec4 u_gridSize;

IMAGE2D_WR(s_screen, rgba8, 0);

USAMPLER3D(s_brickgrid, 1);
USAMPLER3D(s_brickgridLod, 2);
USAMPLER3D(s_brickgridLod2, 3);


vec3 createRay(ivec2 pixel, mat4 projInv, mat4 viewInv)
{
	ivec2 resolution = imageSize(s_screen);
	vec2 nds = ((pixel + 0.5) / resolution * 2 - 1) * vec2(1, -1);
	vec3 pointNds = vec3(nds, 1);
	vec4 pointNdsh = vec4(pointNds, 1);
	vec4 dirEye = mul(projInv, pointNdsh);
	dirEye.w = 0;
	vec3 dirWorld = mul(viewInv, dirEye).xyz;
	return normalize(dirWorld);
}

NUM_THREADS(32, 32, 1)
void main()
{
	ivec2 pixel = gl_GlobalInvocationID.xy;
	vec3 view = createRay(pixel, u_projInv, u_viewInv);

	vec3 offset = u_gridPosition.xyz;
	vec3 size = u_gridSize.xyz;
	vec3 camera = vec3(u_viewInv[0].w, u_viewInv[1].w, u_viewInv[2].w) - offset;

	vec3 position, color, normal;
	int numSteps;
	bool hit = TraceBrickgrid(camera, view, offset, size, s_brickgrid, s_brickgridLod, s_brickgridLod2, position, color, normal, numSteps);

	if (hit)
	{
		vec3 toLight = normalize(vec3(-1, 2, -1));
		vec3 lightColor = vec3(1, 1, 1);

		float ndotl = max(dot(normal, toLight), 0.0);
		vec3 diffuse = ndotl * color * lightColor;
		vec3 final = diffuse;

		imageStore(s_screen, pixel, vec4(final, 1.0));
	}
	else
	{
		imageStore(s_screen, pixel, vec4(0, 0, 0, 1.0));
	}
}
