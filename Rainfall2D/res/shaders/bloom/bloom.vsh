$input a_position
$output v_texcoord0


#include "../bgfx/common.shader"


void main()
{
	gl_Position = vec4(a_position, 1.0);
	v_texcoord0 = vec2(a_position.x * 0.5 + 0.5, -a_position.y * 0.5 + 0.5);
}
