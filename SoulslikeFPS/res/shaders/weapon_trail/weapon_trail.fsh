$input v_position, v_texcoord0


#include "../common/common.shader"


SAMPLER2D(s_diffuse, 0);
SAMPLER2D(s_normal, 1);
SAMPLER2D(s_roughness, 2);
SAMPLER2D(s_metallic, 3);
SAMPLER2D(s_emissive, 4);
SAMPLER2D(s_height, 5);

SAMPLER2D(s_blueNoise, 6);

uniform vec4 u_attributeInfo0;
uniform vec4 u_attributeInfo1;

uniform vec4 u_materialData0;
uniform vec4 u_materialData1;
uniform vec4 u_materialData2;
uniform vec4 u_materialData3;

#define u_hasDiffuse u_attributeInfo0[0]
#define u_hasNormal u_attributeInfo0[1]
#define u_hasRoughness u_attributeInfo0[2]
#define u_hasMetallic u_attributeInfo0[3]
#define u_hasEmissive u_attributeInfo1[0]
#define u_hasHeight u_attributeInfo1[1]

#define u_hasTexCoords u_attributeInfo1[3]

uniform vec4 u_cameraPosition;


void main()
{
    vec4 albedo = linearToSRGB(texture2D(s_diffuse, v_texcoord0.xy));
    albedo.a *= v_texcoord0.z;

    vec2 pixelCoord = gl_FragCoord.xy / textureSize(s_blueNoise, 0).xy;
	float noise = texture2D(s_blueNoise, pixelCoord).r;
	if (albedo.a < noise)
		discard;

    gl_FragData[0] = vec4(v_position, 1);
    gl_FragData[1] = vec4(0, 1, 0, 1);
    gl_FragData[2] = vec4(albedo.rgb, 1);
    gl_FragData[3] = vec4(0, 0, 0, 0);
}
