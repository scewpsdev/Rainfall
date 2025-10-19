$input a_position, a_normal, a_tangent, a_texcoord0


#include "../common/common.shader"


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1.0));

	gl_Position = mul(u_viewProj, worldPosition);
}
