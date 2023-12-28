


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

void decodeVoxelData(vec2 voxel, out int value, out vec3 normal, out int material)
{
	int valuenormal = int(voxel.r * 255 + 0.5);
	
	value = valuenormal & 0x03;
	
	int nx = (valuenormal & 0xC0) >> 6;
	int ny = (valuenormal & 0x30) >> 4;
	int nz = (valuenormal & 0x0C) >> 2;
	normal = normalize(vec3(nx + 1, ny + 1, nz + 1)); // TODO optimize normalization
	
	material = int(voxel.g * 255 + 0.5);
}

bool RayTraceVoxelGrid(vec3 camera, vec3 dir, vec3 size, sampler3D voxels, ivec3 textureOffset, ivec3 textureDim, out vec3 out_position, out vec3 out_color, out vec3 out_normal)
{
	vec3 currentNormal;
	
	float tmin, tmax;
	bool intersects = BoxIntersection(camera, dir, vec3_splat(0), size, tmin, tmax, currentNormal);
	
	vec3 p = camera + max(tmin + 0.0001, 0.0) * dir;
	
	ivec3 resolution = textureSize(voxels, 0);
	vec3 multiplier = 1.0 / size * resolution;
	
	p *= multiplier;
	
	ivec3 ip = ivec3(p);
	ivec3 lower = vec3(0, 0, 0);
	ivec3 upper = resolution;
	ivec3 step = ivec3(sign(dir));
	//vec3 tMax = intBound(p, dir);
	vec3 tDelta = step / dir;
	float t = 0.0;
	
	int maxMip = 5;
	int mip = maxMip;
	int mipScale = pow(2, mip);
	
	int maxSteps = 3 * 64;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip / mipScale * mipScale + 0.5 * mipScale) / resolution;
		vec2 voxelData = texture3DLod(voxels, samplePoint, mip).xy;
		
		int value;
		vec3 normal;
		int material;
		decodeVoxelData(voxelData, value, normal, material);
		
		if (value == 2)
		{
			out_position = p + t / multiplier * dir;
			out_color = vec3(1, 0, 1);
			out_normal = normal;
			return true;
		}
		else if (value == 1)
		{
			mip--;
			mipScale /= 2;
		}
		else
		{
			if (mip == maxMip)
			{
				return false;
			}
			
			vec3 tMax = intBound(p / mipScale, dir) * mipScale;
			bool sx = tMax.x < tMax.y && tMax.x < tMax.z;
			bool sy = !sx && tMax.y < tMax.z;
			bool sz = !sx && !sy;
			float t = sx ? tMax.x : sy ? tMax.y : tMax.z;
			ivec3 mask = ivec3(sx, sy, sz);
			currentNormal = -mask * step;
			
			p += t * dir + 0.0001 * dir;
			ivec3 lastip = ip;
			ip = ivec3(p);
			
			if (ip.x / mipScale / 2 != lastip.x / mipScale / 2 ||
				ip.y / mipScale / 2 != lastip.y / mipScale / 2 ||
				ip.z / mipScale / 2 != lastip.z / mipScale / 2)
			{
				mip++;
				mipScale *= 2;
			}
			
			//ip += step * mask;
			//tMax += tDelta * mask;
			
			if (ip.x < lower.x || ip.x >= upper.x || ip.y < lower.y || ip.y >= upper.y || ip.z < lower.z || ip.z >= upper.z)
				return false;
		}
	}
	
	return false;
}

/*
bool RayTraceVoxelGrid_(vec3 camera, vec3 dir, vec3 size, sampler3D voxels, ivec3 textureOffset, ivec3 textureDim, out vec3 position, out vec3 color, out vec3 normal)
{
	vec3 currentNormal;
	float tmin, tmax;
	bool intersects = BoxIntersection(camera, dir, vec3_splat(0), size, tmin, tmax, currentNormal);
	
	vec3 p = camera + max(tmin + 0.0001, 0.0) * dir;
	
	vec3 multiplier = 1.0 / size;
	
	int highestMip = 3;
	int currentMip = highestMip;
	int maxSteps = 3 * 16;
	for (int i = 0; i < maxSteps; i++)
	{
		vec4 result;
		float distance;
		bool hit = TraceTexture3D(p * multiplier, dir, voxels, currentMip, ivec3(0, 0, 0), ivec3(0, 0, 0), result, distance);
		
		if (hit)
		{
			if (currentMip == 2)
			{
				position = p + distance / multiplier.x * dir;
				color = vec3(1, 0, 1);
				normal = result.xyz;
				return true;
			}
			
			p += distance / multiplier.x * dir + 0.0001 * dir;
			currentMip--;
		}
		else
		{
			if (currentMip == highestMip)
			{
				return false;
			}
			
			p += distance / multiplier.x * dir + 0.0001 * dir;
			currentMip++;
		}
	}
	
	return false;
	
	
	
	
	//vec3 multiplier = 1.0 / size;
	
	vec4 result;
	float distance;
	bool hit = TraceTexture3D(p * multiplier, dir, voxels, 2, ivec3(0, 0, 0), ivec3(0, 0, 0), result, distance);
	
	if (hit)
	{
		position = p + distance / multiplier * dir;
		color = vec3(1, 0, 1);
		normal = result.xyz;
		return hit;
		
		p += distance / multiplier * dir + 0.001 * dir;
		hit = TraceTexture3D(p * multiplier, dir, voxels, 2, ivec3(0, 0, 0), ivec3(0, 0, 0), result, distance);
		
		if (hit)
		{
			p += distance / multiplier * dir + 0.001 * dir;
			hit = TraceTexture3D(p * multiplier, dir, voxels, 1, ivec3(0, 0, 0), ivec3(0, 0, 0), result, distance);
			
			if (hit)
			{
				p += distance / multiplier * dir + 0.001 * dir;
				hit = TraceTexture3D(p * multiplier, dir, voxels, 0, ivec3(0, 0, 0), ivec3(0, 0, 0), result, distance);
				
				if (hit)
				{
					position = p + distance / multiplier * dir;
					color = vec3(1, 0, 1);
					normal = result.xyz;
					return hit;
				}
			}
		}
	}
	
	return false;
}
*/
