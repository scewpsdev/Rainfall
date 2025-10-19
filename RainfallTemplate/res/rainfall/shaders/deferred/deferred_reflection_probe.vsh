$input a_position


#include "../common/common.shader"


void main()
{
	gl_Position = mul(u_viewProj, mul(u_model[0], vec4(a_position, 1.0)));
}
