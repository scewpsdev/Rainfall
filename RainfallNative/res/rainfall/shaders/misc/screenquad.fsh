$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_texture, 0);


void main()
{
	gl_FragColor = vec4(vec3_splat(texture2D(s_texture, v_texcoord0).r), 1);
}
