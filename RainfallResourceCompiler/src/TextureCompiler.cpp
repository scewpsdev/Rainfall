#include "TextureCompiler.h"

#include <vector>
#include <string>
#include <filesystem>


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
	"BGRA8"
};


namespace bgfx
{
	int compileTexture(int argc, const char* argv[]);
}

static bool CompileTexture(const char* path, const char* out, TextureType type)
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
