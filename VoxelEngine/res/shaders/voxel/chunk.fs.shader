$input v_position

#include "../bgfx/common.shader"
#include "raytracing.shader"


SAMPLER3D(s_octree, 0);

uniform vec4 u_cameraPosition;
uniform vec4 u_boxSize;


void main()
{
	vec3 camera = u_cameraPosition.xyz;
	vec3 dir = normalize(v_position - camera);
	vec3 size = u_boxSize.xyz;
	
	/*
	RayHit hitData;
	bool hit = TraceChunk(camera, dir, size, s_octree, hitData);
	if (!hit) discard;

	gl_FragColor = vec4(abs(hitData.normal), 1.0);
	*/

	///*
	float tmin, tmax;
	vec3 faceNormal;
	bool intersects = BoxIntersection(camera, dir, vec3(0, 0, 0), size, tmin, tmax, faceNormal);

	vec3 start = camera + max(tmin + 0.001, 0.0) * dir;

	vec3 pos, norm;
	float hit = raycastOctree(start * 16, dir, s_octree, pos, norm);
	if (hit < 0.0) discard;
	
	gl_FragColor = vec4(abs(norm), 1.0);
	//*/

	/*
	float tmin, tmax;
	vec3 faceNormal;
	bool intersects = BoxIntersection(camera, dir, vec3(0, 0, 0), size, tmin, tmax, faceNormal);

	vec3 start = camera + max(tmin + 0.001, 0.0) * dir;

	vec3 pos, norm;
	float hit = raycastOctree_(start * 16, dir, s_octree, pos, norm);
	if (hit < 0.0) discard;
	
	gl_FragColor = vec4(abs(norm), 1.0);
	*/
}
