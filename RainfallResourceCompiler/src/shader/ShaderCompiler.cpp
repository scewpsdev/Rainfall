#include "ShaderCompiler.h"

#include "CGLCompiler.h"
#include "SourceFile.h"

#include "parser/Parser.h"

#include <bgfx/bgfx.h>

#include <bx/bx.h>
#include <bx/file.h>
#include <bx/commandline.h>


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

bool CompileBGFXShader(const char* path, const char* out, const char* type)
{
	bgfx::RendererType::Enum renderer = bgfx::RendererType::Direct3D11;
	int shaderType = strcmp(type, "vertex") == 0 ? 0 : strcmp(type, "fragment") == 0 ? 1 : strcmp(type, "compute") == 0 ? 2 : -1;

	const char* platform = platforms[renderer];
	const char* profile = GetShaderProfile(renderer, shaderType);

	const char* argv[] = {
		"shaderc",
		"-f", path,
		"-o", out,
		//"-i", "../Mesa/src/shaders/",
		"--platform", platform,
		"--profile", profile,
		"--type", type,
#if 0 //_DEBUG
		"-O", "0",
		"--debug",
#else
		"-O", GetShaderOpt(renderer, shaderType),
#endif
	};
	int argc = sizeof(argv) / sizeof(const char*);

	return bgfx::compileShader(argc, argv) == 0;
}





static void OnCompilerMessage(CGLCompiler* context, MessageType msgType, const char* filename, int line, int col, const char* msg, ...)
{
	if (context->disableError)
		return;

	static const char* const MSG_TYPE_NAMES[MESSAGE_TYPE_MAX] = {
		"<null>",
		"info",
		"warning",
		"error",
		"fatal error",
	};

	MessageType minMsgType =
#if _DEBUG
		MESSAGE_TYPE_INFO
#else
		MESSAGE_TYPE_WARNING
#endif
		;

	if (msgType >= minMsgType)
	{
		static char message[4192] = {};
		message[0] = 0;

		if (filename)
			sprintf(message + strlen(message), "%s:%d:%d: ", filename, line, col);

		if (msgType != MESSAGE_TYPE_INFO)
			sprintf(message + strlen(message), "%s: ", MSG_TYPE_NAMES[msgType]);

		va_list args;
		va_start(args, msg);
		vsprintf(message + strlen(message), msg, args);
		va_end(args);

		fprintf(stderr, "%s\n", message);
	}
}

bool CompileRainfallShader(const char* path, const char* out)
{
	bx::FileReader reader;
	if (bx::open(&reader, path))
	{
		int64_t size = reader.seek(0, bx::Whence::End);
		reader.seek(0, bx::Whence::Begin);

		char* src = new char[size + 1];
		bx::read(&reader, src, (int32_t)size, nullptr);
		src[size] = 0;

		bx::close(&reader);

		SourceFile sourceFile;
		sourceFile.source = src;
		sourceFile.filename = path;

		CGLCompiler compiler;
		compiler.init(OnCompilerMessage);

		compiler.addFile(path, path, src);
		compiler.compile();
		compiler.output(out, true);

		compiler.terminate();

		delete[] src;
	}
	return false;
}
