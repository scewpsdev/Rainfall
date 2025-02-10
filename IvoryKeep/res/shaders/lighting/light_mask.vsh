$input a_position
$output v_texcoord0


#include "../bgfx/common.shader"


uniform vec4 u_lightMaskRect;
uniform vec4 u_cameraBounds;


void main()
{
	vec2 position = a_position.xy;
	//position = position * 0.5f + 0.5f;
	//position = position * u_lightMaskRect.zw + u_lightMaskRect.xy;
	//position = remap(position, u_cameraBounds.xz, u_cameraBounds.yw, vec2(-1, -1), vec2(1, 1));

	gl_Position = vec4(position, 0.0, 1.0);
	v_texcoord0.xy = a_position.xy * vec2(0.5, -0.5) + 0.5;

	v_texcoord0.xy = vec2(remap(a_position.x, -1, 1, u_cameraBounds.x, u_cameraBounds.y), remap(a_position.y, -1, 1, u_cameraBounds.z, u_cameraBounds.w));
	v_texcoord0.xy = (v_texcoord0.xy - u_lightMaskRect.xy) / u_lightMaskRect.zw;
}
