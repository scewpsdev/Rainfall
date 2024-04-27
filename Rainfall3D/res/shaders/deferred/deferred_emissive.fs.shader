$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_gbuffer1, 1);
SAMPLER2D(s_gbuffer3, 3);


void main()
{
    vec4 normalEmissionStrength = texture2D( s_gbuffer1, v_texcoord0);
    vec4 emissiveMetallic = texture2D( s_gbuffer3, v_texcoord0);

    vec3 emissionColor = emissiveMetallic.rgb;
    float emissionStrength = normalEmissionStrength.a;
    vec3 emissive = emissionColor * emissionStrength;

    gl_FragColor = vec4(emissive, 1.0);
}
