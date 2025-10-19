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
	
	uint maxMip = 4;
	uint mip = maxMip;
	uint mipScale = pow(2, mip);
	
	int maxSteps = 256 * 3;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip + 0.5) / resolution;
		bool hit = false;
		
		if (mip >= 1)
		{
			uint value = 0;
			if (mip == 1)
			{
				uint bitmask = texture3DLod(brickgridLod, samplePoint, 0).r;
				uint idx = (ip.x / 2 % 2) + (ip.y / 2 % 2 * 2) + (ip.z / 2 % 2 * 2 * 2);
				value = bitmask & (1 << idx);
			}
			else if (mip == 2)
			{
				value = texture3DLod(brickgridLod, samplePoint, 0).r; 
			}
			else if (mip == 3)
			{
				uint bitmask = texture3DLod(brickgridLod2, samplePoint, 0).r;
				uint idx = (ip.x / 8 % 2) + (ip.y / 8 % 2 * 2) + (ip.z / 8 % 2 * 2 * 2);
				value = bitmask & (1 << idx);
			}
			else if (mip == 4)
			{
				value = texture3DLod(brickgridLod2, samplePoint, 0).r;
			}
			else if (mip == 5)
			{
				uint bitmask = texture3DLod(brickgridLod3, samplePoint, 0).r;
				uint idx = (ip.x / 32 % 2) + (ip.y / 32 % 2 * 2) + (ip.z / 32 % 2 * 2 * 2);
				value = bitmask & (1 << idx);
			}
			else if (mip == 6)
			{
				value = texture3DLod(brickgridLod3, samplePoint, 0).r;
			}

			if (value != 0)
			{
				mip--;
				mipScale /= 2;
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
			
			p = start * multiplier + (t + 0.0001) * dir;
			ivec3 lastip = ip;
			ip = ivec3(floor(p));
			
			/*
			if (ip.x / mipScale / 2 != lastip.x / mipScale / 2 ||
				ip.y / mipScale / 2 != lastip.y / mipScale / 2 ||
				ip.z / mipScale / 2 != lastip.z / mipScale / 2)
			{
				if (mip < maxMip)
				{
					mip++;
					mipScale *= 2;
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

float raycast( in vec3 ro, in vec3 rd, sampler3D octree, out vec3 oVos, out vec3 oDir )
{
	ivec3 pos = floor(ro);
	vec3 ri = 1.0/rd;
	vec3 rs = sign(rd);
	vec3 dis = (pos-ro + 0.5 + rs*0.5) * ri;
	vec3 resolution = textureSize(octree, 0);
	
	float res = -1.0;
	vec3 mm = vec3_splat(0);
	for( int i=0; i<128; i++ ) 
	{
		float value = texelFetch(octree, pos, 0).r * 255;
		if( value>0.5 ) { res=1.0; break; }
		if (pos.x < 0 || pos.y < 0 || pos.z < 0 || pos.x > resolution.x || pos.y > resolution.y || pos.z > resolution.z) {break;}
		mm = step(dis.xyz, dis.yzx) * step(dis.xyz, dis.zxy);
		dis += mm * rs * ri;
        pos += mm * rs;
	}

	vec3 nor = -mm*rs;
	vec3 vos = pos;
	
    // intersect the cube	
	vec3 mini = (pos-ro + 0.5 - 0.5*vec3_splat(rs))*ri;
	float t = max ( mini.x, max ( mini.y, mini.z ) );
	
	oDir = nor;
	oVos = vos;

	return t*res;
}

float raycastOctree( in vec3 ro, in vec3 rd, sampler3D octree, out vec3 oVos, out vec3 oDir )
{
	uint maxMip = 1;
	uint mip = maxMip;
	uint mipScale = pow(2, mip);

	ivec3 pos = floor(ro);
	vec3 ri = 1.0/rd;
	vec3 rs = sign(rd);
	vec3 dis = (pos/mipScale-ro/mipScale + 0.5 + rs*0.5) * ri * mipScale;
	vec3 resolution = textureSize(octree, 0);

	float res = -1.0;
	vec3 mm = vec3_splat(0);
	for (int i = 0; i < 128; i++) 
	{
		uint value = uint(texelFetch(octree, pos / mipScale, mip).r * 255 + 0.5);
		if (value != 0 && mip == 0) { res=1.0; break; }
		else if (value != 0) { mip--; mipScale /= 2; continue; }
		mm = step(dis.xyz, dis.yzx) * step(dis.xyz, dis.zxy);
		dis += mm * rs * ri * mipScale;
        pos += mm * rs * mipScale;
	}

	vec3 nor = -mm*rs;
	vec3 vos = pos;
	
    // intersect the cube	
	vec3 mini = (pos-ro + 0.5 - 0.5*rs)*ri;
	float t = max ( mini.x, max ( mini.y, mini.z ) );
	
	oDir = nor;
	oVos = vos;

	return t*res;
}

//ray-cube intersection, on the inside of the cube
vec3 voxel(vec3 ro, vec3 rd, vec3 ird, float size)
{
    size *= 0.5;
    
    vec3 hit = -(sign(rd)*(ro-size)-size)*ird;
    
    return hit;
}

float raycastOctree_(vec3 ro, vec3 rd, sampler3D octree, out vec3 out_position, out vec3 out_normal)
{
#define detail 1
#define steps 300
#define maxdistance 30.0

	float size = pow(2, 7);
    vec3 lro = mod(ro,size);
    vec3 fro = ro-lro;
    vec3 ird = 1.0/max(abs(rd),0.001);
    vec3 mask;
    bool exitoct = false;
    int recursions = 0;
    float dist = 0.0;
    float fdist = 0.0;
    int i;
    float edge = 1.0;
    vec3 lastmask = vec3(1, 0, 0);
    vec3 normal = vec3_splat(0.0);

    //the octree traverser loop
    //each iteration i either:
    // - check if i need to go up a level
    // - check if i need to go down a level
    // - check if i hit a cube
    // - go one step forward if octree cell is empty
    // - repeat if i did not hit a cube
    for (i = 0; i < steps; i++)
    {
        if (dist > maxdistance) break;
        
        //i go up a level
        if (exitoct)
        {
            
            vec3 newfro = floor(fro/(size*2.0))*(size*2.0);
            
            lro += fro-newfro;
            fro = newfro;
            
            recursions--;
            size *= 2.0;
            
            exitoct = (recursions > 0) && (abs(dot(mod(fro/size+0.5,2.0)-1.0+mask*sign(rd)*0.5,mask))<0.1);
        }
        else
        {
            //checking what type of cell it is: empty, full or subdivide
			float mip = log2(size);
			ivec3 resolution = textureSize(octree, 0);
			int voxelstate = int(texture3DLod(octree, (fro + 0.5) / resolution, mip).r * 255 + 0.5);
            //int voxelstate = getvoxel(fro, size);
            if (voxelstate == 1 && recursions > detail)
            {
                voxelstate = 0;
            }
            
            if(voxelstate == 1&&recursions<=detail)
            {
                //if(recursions>detail) break;

                recursions++;
                size *= 0.5;

                //find which of the 8 voxels i will enter
                vec3 mask2 = step(vec3_splat(size),lro);
                fro += mask2*size;
                lro -= mask2*size;
            }
            //move forward
            else if (voxelstate == 0||voxelstate == 2)
            {
                //raycast and find distance to nearest voxel surface in ray direction
                //i don't need to use voxel() every time, but i do anyway
                vec3 hit = voxel(lro, rd, ird, size);

                /*if (hit.x < min(hit.y,hit.z)) {
                    mask = vec3(1,0,0);
                } else if (hit.y < hit.z) {
                    mask = vec3(0,1,0);
                } else {
                    mask = vec3(0,0,1);
                }*/
                mask = vec3(lessThan(hit,min(hit.yzx,hit.zxy)));
                float len = dot(hit,mask);
    //#ifdef objects
                if (voxelstate == 2) {
                    break;
                }
    //#endif

                //moving forward in ray direction, and checking if i need to go up a level
                dist += len;
                fdist += len;
                lro += rd*len-mask*sign(rd)*size;
                vec3 newfro = fro+mask*sign(rd)*size;
                exitoct = (floor(newfro/size*0.5+0.25)!=floor(fro/size*0.5+0.25))&&(recursions>0);
                fro = newfro;
                lastmask = mask;
            }
        }
		/*
#ifdef drawgrid
        vec3 q = abs(lro/size-0.5)*(1.0-lastmask);
        edge = min(edge,-(max(max(q.x,q.y),q.z)-0.5)*80.0*size);
#endif
		*/
    }
    ro += rd*dist;
    if(i < steps && dist < maxdistance)
    {
    	float val = fract(dot(fro,vec3(15.23,754.345,3.454)));
//#ifndef raymarchhybrid
        vec3 normal = -lastmask*sign(rd);
//#endif
		out_position = ro;
		out_normal = normal;
		return true;

        //vec3 color = sin(val*vec3(39.896,57.3225,48.25))*0.5+0.5;
    	//fragColor = vec4(color*(normal*0.25+0.75),1.0);
        
		/*
#ifdef borders
        vec3 q = abs(lro/size-0.5)*(1.0-lastmask);
        edge = clamp(-(max(max(q.x,q.y),q.z)-0.5)*20.0*size,0.0,edge);
#endif
		*/
//#ifdef blackborders
        //fragColor *= edge;
//#else
        //fragColor = 1.0-(1.0-fragColor)*edge;
//#endif
    } else {
        //#ifdef blackborders
                //fragColor = vec4(edge);
        //#else
        //        fragColor = vec4(1.0-edge);
        //#endif
		return false;
    }
//#ifdef fog
//    fragColor *= 1.0-dist/maxdistance;
//#endif
}

struct RayHit
{
	vec3 position;
	vec3 normal;
	vec3 albedo;
};

bool TraceChunk(vec3 camera, vec3 dir, vec3 size, sampler3D octree, out RayHit hit)
{
	float tmin, tmax;
	vec3 faceNormal;
	bool intersects = BoxIntersection(camera, dir, vec3(0, 0, 0), size, tmin, tmax, faceNormal);

	float fragDistance = max(tmin + 0.0001, 0.0);
	vec3 start = camera + fragDistance * dir;
	
	ivec3 resolution = textureSize(octree, 0);
	float multiplier = 1.0 / size.x * resolution.x;

	uint maxMip = 0;
	uint mip = maxMip;
	uint mipScale = pow(2, mip);
	
	ivec3 ip = ivec3(floor(start * multiplier));
	ivec3 step = ivec3(sign(dir));
	vec3 tMax = intBound(start * multiplier / mipScale, dir) * mipScale;
	vec3 tDelta = step / dir;
	float t = 0.0;
	
	int maxSteps = 128;
	for (int i = 0; i < maxSteps; i++)
	{
		vec3 samplePoint = (ip + 0.5) / resolution;
		
		uint value = uint(texture3DLod(octree, samplePoint, mip).r * 255 + 0.5);

		if (value == 2)
		{
			hit.position = start + t / multiplier * dir;
			hit.normal = faceNormal;
			hit.albedo = vec3(1, 0, 1);
			return true;
		}
		else if (value == 1)
		{
			mip--;
			mipScale /= 2;
		}
		else
		{
			//vec3 p = start * multiplier + t * dir;
			bool sx = tMax.x < tMax.y && tMax.x < tMax.z;
			bool sy = !sx && tMax.y < tMax.z;
			bool sz = !sx && !sy;
			//t += sx ? tMax.x : sy ? tMax.y : tMax.z;
			//t += 0.0001;
			ivec3 mask = ivec3(sx, sy, sz);
			ivec3 face = mask * step;
			faceNormal = -face;
			ip += face * mipScale;
			tMax += tDelta * mask * mipScale;
			
			//p = start * multiplier + (t + 0.0001) * dir;
			//ivec3 lastip = ip;
			//ip = ivec3(floor(p));
			
			/*
			if (ip.x / mipScale / 2 != lastip.x / mipScale / 2 ||
				ip.y / mipScale / 2 != lastip.y / mipScale / 2 ||
				ip.z / mipScale / 2 != lastip.z / mipScale / 2)
			{
				if (mip < maxMip)
				{
					mip++;
					mipScale *= 2;
				}
			}
			*/
			
			if (ip.x < 0 || ip.x >= resolution.x || ip.y < 0 || ip.y >= resolution.y || ip.z < 0 || ip.z >= resolution.z)
			{
				//out_color = vec3(0, 1, 0);
				return false;
			}
		}
	}
	
	//out_color = vec3(0, 1, 1);
	return false;
}
