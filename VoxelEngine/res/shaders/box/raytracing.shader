


vec3 intBound(vec3 s, vec3 ds)
{
	s *= sign(ds);
	ds = abs(ds);
	s = fract(s);
	return (1.0 - s) / ds;
}


bool BoxIntersection(vec3 origin, vec3 dir, vec3 offset, vec3 size, out float tmin, out float tmax)
{
	vec3 dirinv = 1.0 / dir;
	
	vec3 t1 = (offset - origin) * dirinv;
	vec3 t2 = (offset + size - origin) * dirinv;
	
	tmin = max(max(min(t1.x, t2.x), min(t1.y, t2.y)), min(t1.z, t2.z));
	tmax = min(min(max(t1.x, t2.x), max(t1.y, t2.y)), max(t1.z, t2.z));
	
	return tmin < tmax && tmax > 0;
}

bool TraceTexture3D(vec3 p, vec3 dir, sampler3D voxels, int mip, ivec3 lower, ivec3 upper, out vec4 result, out float distance)
{
	ivec3 resolution = textureSize(voxels, 0) / pow(2, mip);
	p *= resolution;
	
	ivec3 ip = ivec3(p);
	lower = ip / 2 * 2;
	upper = lower + 2;
	ivec3 step = ivec3(sign(dir));
	vec3 tMax = intBound(p, dir);
	vec3 tDelta = step / dir;
	float t = 0.0;
	
	int maxSteps = 3 * 16;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip + 0.5) / resolution;
		vec4 voxel = texture3DLod(voxels, samplePoint, mip);
		
		if (voxel.w > 0.5)
		{
			result = voxel;
			distance = t / resolution.x; // / sqrt(dot(dir, resolution))
			return true;
		}
		
		bool sx = tMax.x < tMax.y && tMax.x < tMax.z;
		bool sy = !sx && tMax.y < tMax.z;
		bool sz = !sx && !sy;
		ivec3 mask = ivec3(sx, sy, sz);
		
		t = sx ? tMax.x : sy ? tMax.y : tMax.z;
		ip += step * mask;
		tMax += tDelta * mask;
		
		if (ip.x < lower.x || ip.x >= upper.x || ip.y < lower.y || ip.y >= upper.y || ip.z < lower.z || ip.z >= upper.z)
			return false;
	}
	
	distance = t / resolution.x;
	return false;
}

bool RayTraceVoxelGrid(vec3 camera, vec3 dir, vec3 size, sampler3D voxels, ivec3 textureOffset, ivec3 textureDim, out vec3 position, out vec3 color, out vec3 normal)
{
	float tmin, tmax;
	bool intersects = BoxIntersection(camera, dir, vec3_splat(0), size, tmin, tmax);
	
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
	
	int maxMip = 3;
	int mip = maxMip;
	int mipScale = pow(2, mip);
	
	int maxSteps = 16 + 15 + 15;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip / mipScale * mipScale + 0.5 * mipScale) / resolution;
		vec4 voxel = texture3DLod(voxels, samplePoint, mip);
		
		if (voxel.w > 0.5)
		{
			if (mip == 0)
			{
				position = p + t / multiplier * dir;
				color = vec3(1, 0, 1);
				normal = voxel.xyz;
				return true;
			}
			
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
			//ivec3 mask = ivec3(sx, sy, sz);
			
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

bool RayTraceVoxelGrid_(vec3 camera, vec3 dir, vec3 size, sampler3D voxels, ivec3 textureOffset, ivec3 textureDim, out vec3 position, out vec3 color, out vec3 normal)
{
	float tmin, tmax;
	bool intersects = BoxIntersection(camera, dir, vec3_splat(0), size, tmin, tmax);
	
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
