//#include "material.shader"

//#pragma vertex vertex
//#pragma fragment fragment

struct appdata
{
	vec3 position : POSITION;
	vec3 uv : NORMAL;
}

struct v2f
{
	vec3 position : POSITION;
	vec3 uv : NORMAL;
}

void vertex(in appdata in, out v2f out, out vec4 outPosition)
{
	//vec4 worldPosition = u_model[0] * vec4(in.position, 1);
	//outPosition = u_viewProj * worldPosition;

	//out.position = in.position;
	//out.uv = in.uv;

	outPosition = vec4(0, 0, 0, 1);
	out.position = in.position;
	out.uv = in.uv;
}

void fragment(in v2f in, out vec4 outData0, out vec4 outData1, out vec4 outData2, out vec4 outData3)
{
	//vec4 albedo = linearToSRGB(texture2D(s_diffuse, in.uv.xy));
	//albedo.a *= in.uv.z;

	//vec2 pixelCoord = gl_FragCoord.xy / textureSize(s_blueNoise, 0).xy;
	//float noise = texture2D(s_blueNoise, pixelCoord).r;
	//clip(albedo.a - noise);

	outData0 = vec4(in.position, 1);
	outData1 = vec4(0, 1, 0, 1);
	//outData2 = vec4(albedo.rgb, 1);
	outData3 = vec4(0, 0, 0, 0);
}
