$input v_position, v_ndc, v_color0

#include "../bgfx/common.shader"


void main()
{
	ivec2 pixelCoord = ivec2((v_ndc.xy * vec2(0.5, -0.5) + 0.5) * u_viewRect.zw + vec2(0.2285, 0.2285));
	ivec2 actualCoord = ivec2(gl_FragCoord.xy);
	if (actualCoord.x != pixelCoord.x || actualCoord.y != pixelCoord.y)
		discard;
	
	gl_FragColor = v_color0;
}
