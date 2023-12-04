#include "TextureCompiler.h"

#include <vector>


namespace bgfx
{
	int compileTexture(int argc, const char* argv[]);
}


bool CompileTexture(const char* path, const char* out, const char* format, bool linear, bool normal, bool mipmaps, bool equirect, bool strip)
{
	std::vector<const char*> args;
	args.push_back("texturec");
	args.push_back("-f");
	args.push_back(path);
	args.push_back("-o");
	args.push_back(out);
	args.push_back("-q");
#ifdef _DEBUG
	args.push_back("fastest");
#else
	args.push_back("highest");
#endif
	args.push_back("--as");
	args.push_back(".ktx");
	if (mipmaps)
		args.push_back("--mips");
	args.push_back("--validate");
	if (format)
	{
		args.push_back("-t");
		args.push_back(format);
	}

	if (linear)
		args.push_back("--linear");
	if (normal)
		args.push_back("--normalmap");
	if (equirect)
		args.push_back("--equirect");
	if (strip)
		args.push_back("--strip");

	return bgfx::compileTexture((int)args.size(), args.data()) == 0;
}
