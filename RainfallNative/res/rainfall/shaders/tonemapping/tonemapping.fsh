$input v_texcoord0

#include "../common/common.shader"


SAMPLER2D(s_hdrBuffer, 0);


vec3 ACESCinematic(vec3 color)
{
	mat3 m1 = mat3(
        0.59719, 0.07600, 0.02840,
        0.35458, 0.90834, 0.13383,
        0.04823, 0.01566, 0.83777
	);
	mat3 m2 = mat3(
        1.60475, -0.10208, -0.00327,
        -0.53108,  1.10813, -0.07276,
        -0.07367, -0.00605,  1.07602
	);
	vec3 v = mul(m1, color);
	vec3 a = v * (v + 0.0245786) - 0.000090537;
	vec3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
	return pow(clamp(mul(m2, a / b), 0.0, 1.0), vec3_splat(1.0 / 2.2));
}

vec3 ACESFilmic(vec3 color)
{
	const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return clamp((color * (a * color + b)) / (color * (c * color + d ) + e), 0.0, 1.0);
}

vec3 Tonemap(vec3 color, float exposure)
{
	//color = toFilmic(color);

	color = toAcesFilmic(color * 2);
	color = pow(color, vec3_splat(1.0 / 2.2)); // Gamma correction

	return color;

	//color = vec3(1.0, 1.0, 1.0) - exp(-color * exposure); // Convert to LDR space
	//color = pow(color, vec3_splat(1.0 / 2.2)); // Gamma correction
	
	//return color;
}

vec3 BayerDither(vec3 color, vec2 uv)
{
	int BAYER_PATTERN_16X16[16][16] = {   //  16x16 Bayer Dithering Matrix.  Color levels: 256
		{    0, 191,  48, 239,  12, 203,  60, 251,   3, 194,  51, 242,  15, 206,  63, 254  }, 
		{  127,  64, 175, 112, 139,  76, 187, 124, 130,  67, 178, 115, 142,  79, 190, 127  },
		{   32, 223,  16, 207,  44, 235,  28, 219,  35, 226,  19, 210,  47, 238,  31, 222  },
		{  159,  96, 143,  80, 171, 108, 155,  92, 162,  99, 146,  83, 174, 111, 158,  95  },
		{    8, 199,  56, 247,   4, 195,  52, 243,  11, 202,  59, 250,   7, 198,  55, 246  },
		{  135,  72, 183, 120, 131,  68, 179, 116, 138,  75, 186, 123, 134,  71, 182, 119  },
		{   40, 231,  24, 215,  36, 227,  20, 211,  43, 234,  27, 218,  39, 230,  23, 214  },
		{  167, 104, 151,  88, 163, 100, 147,  84, 170, 107, 154,  91, 166, 103, 150,  87  },
		{    2, 193,  50, 241,  14, 205,  62, 253,   1, 192,  49, 240,  13, 204,  61, 252  },
		{  129,  66, 177, 114, 141,  78, 189, 126, 128,  65, 176, 113, 140,  77, 188, 125  },
		{   34, 225,  18, 209,  46, 237,  30, 221,  33, 224,  17, 208,  45, 236,  29, 220  },
		{  161,  98, 145,  82, 173, 110, 157,  94, 160,  97, 144,  81, 172, 109, 156,  93  },
		{   10, 201,  58, 249,   6, 197,  54, 245,   9, 200,  57, 248,   5, 196,  53, 244  },
		{  137,  74, 185, 122, 133,  70, 181, 118, 136,  73, 184, 121, 132,  69, 180, 117  },
		{   42, 233,  26, 217,  38, 229,  22, 213,  41, 232,  25, 216,  37, 228,  21, 212  },
		{  169, 106, 153,  90, 165, 102, 149,  86, 168, 105, 152,  89, 164, 101, 148,  85  }
	};

	ivec2 pixel = ivec2(uv / u_viewTexel.xy);
	float bayer = (BAYER_PATTERN_16X16[pixel.x % 16][pixel.y % 16]) / 255.0;
	vec3 result = (color * 255 + bayer) / 255;
	return result;
}

void main()
{
	vec3 hdr = texture2D(s_hdrBuffer, v_texcoord0).rgb;
	vec3 tonemapped = Tonemap(hdr, 1.0);
	tonemapped = BayerDither(tonemapped, v_texcoord0);

	gl_FragColor = vec4(tonemapped, 1.0);
	//gl_FragColor = vec4(depthToDistance(texture2DLod(s_hdrBuffer, v_texcoord0, 5).r, 0.025, 1000) / 20, 0, 0, 0);
}
