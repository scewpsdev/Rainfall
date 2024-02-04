#include "ShaderCompiler.h"

#include <bgfx/bgfx.h>


static const char* rendererNames[] =
{
	nullptr,       //!< No rendering.
	nullptr,       //!< AGC
	"dx9",     //!< Direct3D 9.0
	"dx11",     //!< Direct3D 11.0
	"dx12",     //!< Direct3D 12.0
	nullptr,       //!< GNM
	"metal",         //!< Metal
	nullptr,       //!< NVN
	"glsl",     //!< OpenGL ES 2.0+
	"glsl",       //!< OpenGL 2.1+
	"spirv",       //!< Vulkan
	nullptr,       //!< WebGPU
};

static const char* platforms[] =
{
	nullptr,       //!< No rendering.
	nullptr,       //!< AGC
	"windows",     //!< Direct3D 9.0
	"windows",     //!< Direct3D 11.0
	"windows",     //!< Direct3D 12.0
	nullptr,       //!< GNM
	"osx",         //!< Metal
	nullptr,       //!< NVN
	"android",     //!< OpenGL ES 2.0+
	"windows",       //!< OpenGL 2.1+
	"windows",       //!< Vulkan
	nullptr,       //!< WebGPU
};


namespace bgfx
{
	int compileShader(int _argc, const char* _argv[]);
}

static const char* GetShaderProfile(bgfx::RendererType::Enum renderer, int shaderType)
{
	switch (renderer)
	{
		/*
	case bgfx::RendererType::Direct3D9:
		switch (shaderType)
		{
		case 0: return "vs_3_0";
		case 1: return "ps_3_0";
		default: return nullptr;
		}
		*/
	case bgfx::RendererType::Direct3D11:
	case bgfx::RendererType::Direct3D12:
		return "s_5_0";
	case bgfx::RendererType::OpenGL:
		switch (shaderType)
		{
		case 0: return "120";
		case 1: return "120";
		case 2: return "430";
		default: return nullptr;
		}
	case bgfx::RendererType::OpenGLES:
		return "320_es";
	case bgfx::RendererType::Metal:
		return "metal";
	case bgfx::RendererType::Vulkan:
		return "spirv";
	default:
		return nullptr;
	}
}

static const char* GetShaderOpt(bgfx::RendererType::Enum renderer, int shaderType)
{
	switch (renderer)
	{
	//case bgfx::RendererType::Direct3D9:
	//	return "3";
	case bgfx::RendererType::Direct3D11:
		switch (shaderType)
		{
		case 0: return "3";
		case 1: return "3";
		case 2: return "1";
		default: return nullptr;
		}
	case bgfx::RendererType::Direct3D12:
		switch (shaderType)
		{
		case 0: return "3";
		case 1: return "3";
		case 2: return "1";
		default: return nullptr;
		}
	case bgfx::RendererType::OpenGL:
		return "3";
	case bgfx::RendererType::Metal:
		return "3";
	case bgfx::RendererType::Vulkan:
		return "3";
	default:
		return nullptr;
	}
}

bool CompileShader(const char* path, const char* out, const char* type)
{
	bgfx::RendererType::Enum renderer = bgfx::RendererType::Direct3D11;
	int shaderType = strcmp(type, "vertex") == 0 ? 0 : strcmp(type, "fragment") == 0 ? 1 : strcmp(type, "compute") == 0 ? 2 : -1;

	const char* platform = platforms[renderer];
	const char* profile = GetShaderProfile(renderer, shaderType);
	const char* opt = GetShaderOpt(renderer, shaderType);

	const char* argv[] = {
		"shaderc",
		"-f", path,
		"-o", out,
		//"-i", "../Mesa/src/shaders/",
		"--platform", platform,
		"--profile", profile,
		"--type", type,
		"-O", opt,
	};
	int argc = sizeof(argv) / sizeof(const char*);

	return bgfx::compileShader(argc, argv) == 0;
}
