$input v_texcoord0


#include "../bgfx/common.shader"


SAMPLER3D(s_octree, 0);

uniform vec4 u_cameraPosition;
uniform vec3

uniform vec4 iResolution;
#define iTime iResolution.z

#define mindetail 0
#define maxdetail 7
#define steps 300
#define time iTime*0.5
#define maxdistance 300.0
#define tree 2.0

#define ambient 0.03
//#define softshadow //really bad soft shadowing


#define rot(spin) mat2(cos(spin),sin(spin),-sin(spin),cos(spin))

//random function from https://www.shadertoy.com/view/MlsXDf
float rnd(vec4 v) { return fract(4e4*sin(dot(v,vec4(13.46,41.74,-73.36,14.24))+17.34)); }

//0 is empty, 1 is subdivide and 2 is full
int getvoxel(vec3 p, float size) {
    if ((p.x==0.0||p.x==2.0)&&p.y==0.0) {
        return 0;
    }

    float val = rnd(vec4(p,size));
    
    if (val < 0.4+size*0.25) {
        return 0;
    } else if (val < 0.95+size*0.03) {
        return 1;
    } else {
        return 2;
    }
    
    //return int(val*val*3.0);
}

//ray-cube intersection, on the inside of the cube
vec3 voxel(vec3 ro, vec3 rd, vec3 ird, float size)
{
    size *= 0.5;
    
    vec3 hit = -(sign(rd)*(ro-size)-size)*ird;
    
    return hit;
}

vec4 octreeray(vec3 ro, vec3 rd, float maxdist,
               out vec3 lro, out vec3 fro,
               out float size, out float proxim) {
    size = pow(tree,float(-mindetail));
    lro = mod(ro,size);
    fro = ro-lro;
    vec3 srd = sign(rd);
    vec3 ird = 1.0/max(rd*srd,0.001);
    vec3 mask;
    bool exitoct = false;
    int recursions = mindetail;
    float dist = 0.0;
    int i;
    float edge = 1.0;
    proxim = 1.0;
    float lastsize = size;
    vec3 hit = voxel(lro, rd, ird, size);
    
    //the octree traverser loop
    //each iteration i either:
    // - check if i need to go up a level
    // - check if i need to go down a level
    // - check if i hit a cube
    // - go one step forward if cube is empty
    for (i = 0; i < steps; i++)
    {
        if (dist > maxdist) break;
        int voxelstate = getvoxel(fro*size,size);
        //int voxelstate = getvoxel(floor(fro/size+0.5)*size,size);
        
        //i go up a level
        if (exitoct)
        {
            vec3 newfro = floor(fro/size/tree+0.5/tree)*size*tree;
            
            lro += fro-newfro;
            fro = newfro;
            
            recursions--;
            size *= tree;
            exitoct = (recursions > mindetail) && (((mod(dot(fro/size,mask)+0.5,tree)-tree*0.5)*dot(srd,mask)) < -tree*0.5+0.75);
            
            if (!exitoct) {
            	hit = voxel(lro, rd, ird, size);
            }
        }
        //subdivide
        else if(voxelstate == 1&&recursions<maxdetail)
        {
            
            recursions++;
            size /= tree;
            
            vec3 mask2 = clamp(floor(lro/size),0.0,tree-1.0);
            fro += mask2*size;
            lro -= mask2*size;
            hit = voxel(lro, rd, ird, size);
        }
        //move forward
        else if (voxelstate == 0 || (voxelstate == 1 && recursions == maxdetail))
        {
#ifdef softshadow
            if (lastsize >= size) {
                lastsize = size;
            } else {
                proxim = lastsize;
            }
#endif
            
            //raycast and find distance to nearest voxel surface in ray direction
            if (hit.x < min(hit.y,hit.z)) {
                mask = vec3(1,0,0);
            } else if (hit.y < hit.z) {
                mask = vec3(0,1,0);
            } else {
                mask = vec3(0,0,1);
            }
            float len = dot(hit,mask);
            
            //moving forward in ray direction
            lro += rd*len-mask*srd*size;
            dist += len;
            hit -= len;
            hit += mask*ird*size;
            
            vec3 newfro = fro+mask*srd*size;
            //this line is a bit ugly, this checks if i've gone out of octree bounds
            vec3 x = floor(newfro/size/tree+0.5/tree);
            vec3 y = floor(fro/size/tree+0.5/tree);
            exitoct = (x.x != y.x || x.y != y.y || x.z != y.z)&&(recursions>0);
            fro = newfro;
        }
        else
        {
            break;
        }
    }
    return vec4(dist, mask);
}

void main()
{
    gl_FragColor = vec4_splat(0.0);
    vec2 uv = (v_texcoord0.xy * iResolution * 2.0 - iResolution.xy) /iResolution.y;
    
    vec3 ro = vec3(0.5+sin(time)*0.4,0.5+cos(time)*0.4,time);
    vec3 rd = normalize(vec3(uv,1.0));
    
    vec3 lro;
    vec3 fro;
    float size;
    float dummy;
    vec4 len = octreeray(ro, rd, maxdistance, lro, fro, size,dummy);
    
    vec3 hit = ro+rd*len.x;

    float val = fract(dot(fro,vec3(15.23,754.345,3.454)));
    vec3 normal = -len.yzw*sign(rd);
    hit += normal*0.001;
    vec3 color = (sin(val*vec3(39.896,57.3225,48.25))*0.5+0.5)*0.4+0.2;
    
    color /= hit.x*hit.x*0.01+1.0;
    gl_FragColor.xyz = color;
}