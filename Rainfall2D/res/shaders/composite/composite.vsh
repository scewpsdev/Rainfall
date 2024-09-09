$input a_position
$output v_texcoord0


#include "../bgfx/common.shader"


uniform vec4 u_cameraSettings;
#define frameSize u_cameraSettings.xy
#define cameraFractPos u_cameraSettings.zw


void main()
{
	vec2 fractOffset = cameraFractPos / frameSize * 2;
	gl_Position = vec4(a_position.xy - fractOffset, a_position.z, 1.0);
	v_texcoord0 = a_position.xy * vec2(0.5, -0.5) + 0.5;
}
