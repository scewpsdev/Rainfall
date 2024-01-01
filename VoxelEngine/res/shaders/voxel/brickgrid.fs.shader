$input v_position, v_size, v_camera, v_view


#include "../bgfx/common.shader"
#include "raytracing.shader"


USAMPLER3D(s_brickgrid, 0);
USAMPLER3D(s_brickgridLod, 1);


void main()
{
	vec3 view = normalize(v_view);
	
	vec3 position, color, normal;
	int numSteps;
	bool hit = TraceBrickgrid(v_camera, view, v_size, s_brickgrid, s_brickgridLod, position, color, normal, numSteps);

	if (!hit)
		discard;

	gl_FragColor = vec4(position / 256, 1.0);
}
