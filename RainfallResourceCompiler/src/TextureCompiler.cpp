#include "TextureCompiler.h"

#include <vector>
#include <string>
#include <filesystem>

#include <bimg/bimg.h>
#include <bimg/decode.h>
#include <bx/bx.h>
#include <bx/file.h>


namespace fs = std::filesystem;


enum class TextureType
{
	Diffuse,
	Normalmap,
	MaterialAttributes,
	Emissive,
	AmbientOcclusion,
	Heightmap,
	CubemapEquirect,
	CubemapStrip,
	CubemapHDR,
	ColorLUT,
	Other,

	Count
};


static const char* typeFormats[(int)TextureType::Count] = {
	"BC3",
	"BC5",
	"BC3",
	"BC3",
	"BC4",
	"BC4",
	nullptr,
	nullptr,
	nullptr,
	nullptr,
	"BGRA8"
};


namespace bgfx
{
	int compileTexture(int argc, const char* argv[]);
}

template<typename T>
void swap(T& a, T& b)
{
	T tmp = a;
	a = b;
	b = tmp;
}


struct rgb8_t { uint8_t channels[3]; };

static bool CompileTexture(const char* path, const char* out, TextureType type)
{
	if (type == TextureType::ColorLUT)
	{
		bx::Error err;

		bx::FileReader reader;
		if (!bx::open(&reader, path, &err))
			return false;

		uint32_t inputSize = (uint32_t)bx::getSize(&reader);
		if (0 == inputSize)
			return false;

		bx::DefaultAllocator allocator;

		uint8_t* inputData = (uint8_t*)bx::alloc(&allocator, inputSize);

		bx::read(&reader, inputData, inputSize, &err);
		bx::close(&reader);

		if (!err.isOk())
			return false;

		bimg::ImageContainer* image = bimg::imageParse(&allocator, inputData, inputSize, bimg::TextureFormat::Count, &err);

		// Convert to 3D texture and swap Y and Z axis
		image->m_depth *= (image->m_width / image->m_height);
		image->m_width = image->m_height;
		rgb8_t* data = (rgb8_t*)image->m_data;
		for (int z = 0; z < image->m_depth; z++)
		{
			for (int y = 0; y < z; y++)
			{
				for (int x = 0; x < image->m_width; x++)
				{
					rgb8_t& pixel0 = data[x + z * image->m_width + y * image->m_width * image->m_height];
					rgb8_t& pixel1 = data[x + y * image->m_width + z * image->m_width * image->m_height];
					swap(pixel0, pixel1);
				}
			}
		}

		// We convert the lut format to BGRA8 always, because of a BGFX bug in renderer_d3d11.cpp:bgfx::d3d11::TextureD3D11::create at 4456.
		// If the data has any format other than BGRA8 (like RGB8) it will attempt to convert it there.
		// Regardless of whether the texture is 3D or not, it will only convert the first slice, resulting in garbage data for z > 0.
		// The same bug exists in the texturev tool, when loading a 3D ktx that has a format other than BGRA8.
		bimg::ImageContainer* output = bimg::imageConvert(&allocator, bimg::TextureFormat::BGRA8, *image);

		bimg::imageFree(image);
		bx::free(&allocator, inputData);

		bx::FileWriter writer;
		if (bx::open(&writer, out, false, &err))
		{
			bimg::imageWriteKtx(&writer, *output, output->m_data, output->m_size, &err);

			bx::close(&writer);
			bimg::imageFree(output);
			return true;
		}

		bimg::imageFree(output);
		return false;
	}
	else
	{
		const char* format = typeFormats[(int)type];

		std::vector<const char*> args;
		args.push_back("texturec");
		args.push_back("-f");
		args.push_back(path);
		args.push_back("-o");
		args.push_back(out);
		args.push_back("-q");
		//args.push_back("fastest");
		args.push_back("highest");
		args.push_back("--as");
		args.push_back(".ktx");

		//if (linear)
		args.push_back("--linear");

		args.push_back("--validate");
		if (format)
		{
			args.push_back("-t");
			args.push_back(format);
		}

		if (type != TextureType::Other)
			args.push_back("--mips");

		if (type == TextureType::Normalmap)
			args.push_back("--normalmap");
		if (type == TextureType::CubemapEquirect || type == TextureType::CubemapHDR)
			args.push_back("--equirect");
		if (type == TextureType::CubemapStrip)
			args.push_back("--strip");

		return bgfx::compileTexture((int)args.size(), args.data()) == 0;
	}
}

bool CompileTexture(std::string name, std::string extension, const char* path, const char* outpath)
{
	TextureType type = TextureType::Other;

	if (extension == ".png" || extension == ".jpg")
	{
		std::transform(name.begin(), name.end(), name.begin(), [](unsigned char c) {return std::tolower(c); });

		if (name.find("cubemap") != std::string::npos)
		{
			type = name.find("equirect") != std::string::npos ? TextureType::CubemapEquirect : TextureType::CubemapStrip;
		}
		else if (name.find("lut") != std::string::npos)
		{
			type = TextureType::ColorLUT;
		}
		else
		{
			if (name.find("basecolor") != std::string::npos || name.find("albedo") != std::string::npos || name.find("diffuse") != std::string::npos)
				type = TextureType::Diffuse;
			else if (name.find("normal") != std::string::npos)
				type = TextureType::Normalmap;
			else if (name.find("roughnessmetallic") != std::string::npos)
				type = TextureType::MaterialAttributes;
			else if (name.find("roughness") != std::string::npos)
				type = TextureType::MaterialAttributes;
			else if (name.find("metallic") != std::string::npos)
				type = TextureType::MaterialAttributes;
			else if (name.find("height") != std::string::npos || name.find("displacement") != std::string::npos)
				type = TextureType::Heightmap;
			else if (name.find("emissive") != std::string::npos || name.find("emission") != std::string::npos)
				type = TextureType::Emissive;
			else if (name.find("_ao") != std::string::npos || name.find("ambientocclusion") != std::string::npos || name.find("ambient_occlusion") != std::string::npos)
				type = TextureType::AmbientOcclusion;
		}
	}
	else if (extension == ".hdr")
	{
		type = TextureType::CubemapHDR;
	}

	return CompileTexture(path, outpath, type);
}
