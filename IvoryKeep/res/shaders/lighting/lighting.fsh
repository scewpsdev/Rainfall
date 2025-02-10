$input v_texcoord0

#include "../bgfx/common.shader"


SAMPLER2D(s_frame, 0);

uniform vec4 u_cameraBounds;
uniform vec4 u_ambientLight;
uniform vec4 u_lightPositions[16];
uniform vec4 u_lightColors[16];


void main()
{
	vec2 worldPosition = vec2(mix(u_cameraBounds.x, u_cameraBounds.y, v_texcoord0.x), mix(u_cameraBounds.z, u_cameraBounds.w, 1 - v_texcoord0.y));
	
	vec3 light = vec3(0, 0, 0);
	for (int i = 0; i < 16; i++)
	{
		vec2 lightPosition = u_lightPositions[i].xy;
		float distance = length(lightPosition - worldPosition);
		float radius = u_lightPositions[i].z;
		float attenuation = 1.0 - clamp(distance / radius, 0.0, 1.0);
		attenuation = pow(attenuation, 3.0);
		vec3 lighting = u_lightColors[i].rgb * attenuation;
		light += lighting;
	}

	light += u_ambientLight.rgb;

	//light += 0.01;
	//light = linearToSRGB(light) * 0.9 + 0.1;
	//light = linearToSRGB(light);
	
	//vec3 color = texture2D(s_frame, v_texcoord0.xy).rgb;
	//vec3 final = color * light;

	gl_FragColor = vec4(light, 1.0);
}
