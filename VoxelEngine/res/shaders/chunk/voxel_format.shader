


vec3 decodeNormal(vec2 value)
{
	int r = (int)(value.r * 255 + 0.5);
	int g = (int)(value.g * 255 + 0.5);
	return vec3(((r & 0x7C) >> 2) / 32.0, 0, 0);
	int ivalue = (g << 8) | r;
	float x = ((ivalue & 0x7C00) >> 10 - 16) / 15.0;
	float y = ((ivalue & 0x3E0) >> 5 - 16) / 15.0;
	float z = ((ivalue & 0x1F) - 16) / 15.0;
	return normalize(vec3(x, y, z));
}

void decodeVoxelData(vec2 voxel, out int value, out vec3 normal, out int material)
{
	int valuenormal = int(voxel.r * 255 + 0.5);
	
	value = valuenormal & 0x03;
	
	int nx = (valuenormal & 0xC0) >> 6;
	int ny = (valuenormal & 0x30) >> 4;
	int nz = (valuenormal & 0x0C) >> 2;
	normal = normalize(vec3(nx - 1, ny - 1, nz - 1)); // TODO optimize normalization
	
	material = int(voxel.g * 255 + 0.5);
}

vec2 encodeVoxelData(int value, vec3 normal, int material)
{
	int nx = normal.x < -0.38 ? 0 : normal.x > 0.38 ? 2 : 1;
	int ny = normal.y < -0.38 ? 0 : normal.y > 0.38 ? 2 : 1;
	int nz = normal.z < -0.38 ? 0 : normal.z > 0.38 ? 2 : 1;
	int valuenormal = (nx << 6) | (ny << 4) | (nz << 2) | value;
	float r = valuenormal / 255.0;
	float g = material / 255.0;
	return vec2(r, g);
}
