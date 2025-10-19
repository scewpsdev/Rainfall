$input a_position, a_normal, a_tangent, a_indices, a_weight, a_texcoord0
$output v_position, v_normal, v_tangent, v_bitangent, v_texcoord0


#include "../common/common.shader"

#define MAX_BONES 128


uniform mat4 u_boneTransforms[128];


void main()
{
	vec4 weights = a_weight / (a_weight[0] + a_weight[1] + a_weight[2] + a_weight[3]);

	mat4 boneTransform = mat4(
		vec4(0.0, 0.0, 0.0, 0.0),
		vec4(0.0, 0.0, 0.0, 0.0),
		vec4(0.0, 0.0, 0.0, 0.0),
		vec4(0.0, 0.0, 0.0, 0.0)
	);
	boneTransform += u_boneTransforms[int(a_indices[0] + 0.5)] * weights[0];
	boneTransform += u_boneTransforms[int(a_indices[1] + 0.5)] * weights[1];
	boneTransform += u_boneTransforms[int(a_indices[2] + 0.5)] * weights[2];
	boneTransform += u_boneTransforms[int(a_indices[3] + 0.5)] * weights[3];

	vec4 animatedPosition = mul(boneTransform, vec4(a_position, 1.0));
	vec4 worldPosition = mul(u_model[0], animatedPosition);

	vec4 animatedNormal = mul(boneTransform, vec4(a_normal, 0.0));
	vec4 worldNormal = mul(u_model[0], animatedNormal);

	vec4 worldTangent = mul(u_model[0], vec4(a_tangent, 0.0));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = worldPosition.xyz;
	v_normal = worldNormal.xyz;
	v_tangent = worldTangent.xyz;
	v_bitangent = cross(v_normal, v_tangent);
	v_texcoord0 = a_texcoord0;
}
