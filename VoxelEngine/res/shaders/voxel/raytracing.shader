#include "../bgfx/bgfx_compute.shader"


vec3 intBound(vec3 s, vec3 ds)
{
	s *= sign(ds);
	ds = abs(ds);
	s = fract(s);
	return (1.0 - s) / ds;
}


bool BoxIntersection(vec3 origin, vec3 dir, vec3 offset, vec3 size, out float tmin, out float tmax, out vec3 normal)
{
	vec3 dirinv = 1.0 / dir;
	
	vec3 t1 = (offset - origin) * dirinv;
	vec3 t2 = (offset + size - origin) * dirinv;
	
	vec3 mn = min(t1, t2);
	vec3 mx = max(t1, t2);
	
	bool sx = mn.x > mn.y && mn.x > mn.z;
	bool sy = !sx && mn.y > mn.z;
	bool sz = !sx && !sy;
	ivec3 mask = ivec3(sx, sy, sz);
	ivec3 step = ivec3(sign(dir));
	normal = -mask * step;
	
	tmin = max(max(mn.x, mn.y), mn.z);
	tmax = min(min(mx.x, mx.y), mx.z);
	
	return tmin < tmax && tmax > 0;
}

bool TraceBrickgrid(vec3 camera, vec3 dir, vec3 position, vec3 size, BgfxUSampler3D brickgrid, BgfxUSampler3D brickgridLod, BgfxUSampler3D brickgridLod2, BgfxUSampler3D brickgridLod3, out vec3 out_position, out vec3 out_color, out vec3 out_normal, out int out_numSteps)
{
	float tmin, tmax;
	vec3 faceNormal;
	bool intersects = BoxIntersection(camera, dir, position, size, tmin, tmax, faceNormal);
	if (!intersects) return false;

	float fragDistance = max(tmin + 0.0001, 0.0);
	vec3 start = camera + fragDistance * dir;
	
	ivec3 resolution = textureSize(brickgrid, 0);
	float multiplier = 1.0 / size.x * resolution.x;
	
	ivec3 ip = ivec3(floor(start * multiplier));
	ivec3 step = ivec3(sign(dir));
	vec3 tDelta = step / dir;
	float t = 0.0;
	
	uint maxMip = 3;
	uint mip = maxMip;
	uint mipScale = pow(4, mip);
	
	int maxSteps = 256 * 3;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip + 0.5) / resolution;
		bool hit = false;
		
		if (mip >= 1)
		{
			uint value = 0;
			if (mip == 1)
				value = texture3DLod(brickgridLod, samplePoint, 0).r;
			else if (mip == 2)
				value = texture3DLod(brickgridLod2, samplePoint, 0).r;
			else if (mip == 3)
				value = texture3DLod(brickgridLod3, samplePoint, 0).r;

			if (value != 0)
			{
				mip--;
				mipScale /= 4;
				hit = true;
			}
		}
		else
		{
			uvec4 value = texture3DLod(brickgrid, samplePoint, 0);
			
			if (value.w != 0)
			{
				out_position = start + t / multiplier * dir;
				out_color = value.rgb / 255.0;
				out_normal = faceNormal;
				out_numSteps = i + 1;
				return true;
			}
		}
		/*
		else if (value == 1)
		{
			mip--;
			mipScale /= 2;
		}
		*/

		if (!hit)
		{
			vec3 p = start * multiplier + t * dir;
			vec3 tMax = intBound(p / mipScale, dir) * mipScale;
			bool sx = tMax.x < tMax.y && tMax.x < tMax.z;
			bool sy = !sx && tMax.y < tMax.z;
			bool sz = !sx && !sy;
			t += sx ? tMax.x : sy ? tMax.y : tMax.z;
			t += 0.0001;
			ivec3 mask = ivec3(sx, sy, sz);
			faceNormal = -mask * step;
			
			p = start * multiplier + t * dir;
			ip = ivec3(floor(p));
			
			/*
			if (ip.x / mipScale / 4 != lastip.x / mipScale / 4 ||
				ip.y / mipScale / 4 != lastip.y / mipScale / 4 ||
				ip.z / mipScale / 4 != lastip.z / mipScale / 4)
			{
				if (mip < maxMip)
				{
					mip++;
					mipScale *= 4;
				}
			}
			*/
			
			if (ip.x < 0 || ip.x >= resolution.x || ip.y < 0 || ip.y >= resolution.y || ip.z < 0 || ip.z >= resolution.z)
			{
				out_color = vec3(0, 1, 0);
				return false;
			}
		}
	}
	
	out_color = vec3(0, 1, 1);
	return false;
}
