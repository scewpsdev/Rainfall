#include "ShaderCompiler.h"

#include "CGLCompiler.h"
#include "SourceFile.h"

#include "parser/Parser.h"

#include <bgfx/bgfx.h>

#include <bx/bx.h>
#include <bx/file.h>
#include <bx/commandline.h>

#include <vector>
#include <string>


namespace bgfx
{
	struct Options
	{
		Options();

		void dump();

		char shaderType;
		std::string platform;
		std::string profile;

		std::string	inputFilePath;
		std::string	outputFilePath;

		std::vector<std::string> includeDirs;
		std::vector<std::string> defines;
		std::vector<std::string> dependencies;

		bool disasm;
		bool raw;
		bool preprocessOnly;
		bool depends;

		bool debugInformation;

		bool avoidFlowControl;
		bool noPreshader;
		bool partialPrecision;
		bool preferFlowControl;
		bool backwardsCompatibility;
		bool warningsAreErrors;
		bool keepIntermediate;

		bool optimize;
		uint32_t optimizationLevel;
	};
}


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
	bool compileShader(const char* _varying, const char* _comment, char* _shader, uint32_t _shaderLen, const Options& _options, bx::WriterI* _shaderWriter, bx::WriterI* _messageWriter);
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

static int GetShaderOpt(bgfx::RendererType::Enum renderer, int shaderType)
{
	switch (renderer)
	{
		//case bgfx::RendererType::Direct3D9:
		//	return 3;
	case bgfx::RendererType::Direct3D11:
		switch (shaderType)
		{
		case 0: return 3;
		case 1: return 3;
		case 2: return 1;
		default: return 0;
		}
	case bgfx::RendererType::Direct3D12:
		switch (shaderType)
		{
		case 0: return 3;
		case 1: return 3;
		case 2: return 1;
		default: return 0;
		}
	case bgfx::RendererType::OpenGL:
		return 3;
	case bgfx::RendererType::Metal:
		return 3;
	case bgfx::RendererType::Vulkan:
		return 3;
	default:
		return 0;
	}
}

bool CompileBGFXShader(const char* path, const char* out, const char* type)
{
	bgfx::RendererType::Enum renderer = bgfx::RendererType::Direct3D11;
	int shaderType = strcmp(type, "vertex") == 0 ? 0 : strcmp(type, "fragment") == 0 ? 1 : strcmp(type, "compute") == 0 ? 2 : -1;

	const char* platform = platforms[renderer];
	const char* profile = GetShaderProfile(renderer, shaderType);
	char opt[2];
	_itoa(GetShaderOpt(renderer, shaderType), opt, 10);

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
		"-O", opt,
#endif
	};
	int argc = sizeof(argv) / sizeof(const char*);

	return bgfx::compileShader(argc, argv) == 0;
}


static bool CompileBGFXShaderFromSrc(const char* src, const char* out, const char* type, const char* varyings)
{
	const char* outFilePath = out;

	bgfx::Options options;
	//options.inputFilePath = nullptr;
	options.outputFilePath = outFilePath;
	options.shaderType = bx::toLower(type[0]);

	bgfx::RendererType::Enum renderer = bgfx::RendererType::Direct3D11;
	int shaderType = strcmp(type, "vertex") == 0 ? 0 : strcmp(type, "fragment") == 0 ? 1 : strcmp(type, "compute") == 0 ? 2 : -1;
	const char* platform = platforms[renderer];
	const char* profile = GetShaderProfile(renderer, shaderType);
	int opt = GetShaderOpt(renderer, shaderType);

	options.platform = platform;
	options.profile = profile;

	options.debugInformation = false;
	options.avoidFlowControl = false;
	options.noPreshader = false;
	options.partialPrecision = false;
	options.preferFlowControl = false;
	options.backwardsCompatibility = false;
	options.warningsAreErrors = false;
	options.keepIntermediate = false;

	uint32_t optimization = opt;
	if (optimization > 0)
	{
		options.optimize = true;
		options.optimizationLevel = optimization;
	}

	options.depends = false;
	options.preprocessOnly = false;
	const char* includeDir = nullptr;

	BX_TRACE("depends: %d", options.depends);
	BX_TRACE("preprocessOnly: %d", options.preprocessOnly);
	BX_TRACE("includeDir: %s", includeDir);

	/*
	std::string dir;
	{
		bx::FilePath fp(filePath);
		bx::StringView path(fp.getPath());

		dir.assign(path.getPtr(), path.getTerm());
		options.includeDirs.push_back(dir);
	}
	*/

	bool compiled = false;

	std::string varying = "";

	if ('c' != options.shaderType)
	{
		varying = R"(
vec3 a_position  : POSITION;
vec3 a_normal    : NORMAL;
vec3 a_tangent   : TANGENT;
vec4 a_indices   : BLENDINDICES;
vec4 a_weight    : BLENDWEIGHT;
vec3 a_texcoord0 : TEXCOORD0;
vec4 a_texcoord1 : TEXCOORD1;
vec4 a_texcoord2 : TEXCOORD2;
vec4 a_color0    : COLOR0;

vec4 i_data0     : TEXCOORD7;
vec4 i_data1     : TEXCOORD6;
vec4 i_data2     : TEXCOORD5;
vec4 i_data3     : TEXCOORD4;
vec4 i_data4     : TEXCOORD3;

)" + std::string(varyings);
	}

	int32_t size = (int32_t)strlen(src);
	const int32_t total = size + 16384;
	char* data = new char[total];
	memcpy(data, src, size);

	// Trim UTF-8 BOM
	if (data[0] == '\xef'
		&& data[1] == '\xbb'
		&& data[2] == '\xbf')
	{
		bx::memMove(data, &data[3], size - 3);
		size -= 3;
	}

	const char ch = data[0];
	if (false // https://en.wikipedia.org/wiki/Byte_order_mark#Byte_order_marks_by_encoding
		|| '\x00' == ch
		|| '\x0e' == ch
		|| '\x2b' == ch
		|| '\x84' == ch
		|| '\xdd' == ch
		|| '\xf7' == ch
		|| '\xfb' == ch
		|| '\xfe' == ch
		|| '\xff' == ch
		)
	{
		bx::printf("Shader input file has unsupported BOM.\n");
		return bx::kExitFailure;
	}

	// Compiler generates "error X3000: syntax error: unexpected end of file"
	// if input doesn't have empty line at EOF.
	data[size] = '\n';
	bx::memSet(&data[size + 1], 0, total - size - 1);

	{
		bx::FileWriter* writer = new bx::FileWriter;
		if (!bx::open(writer, outFilePath))
		{
			bx::printf("Unable to open output file '%s'.\n", outFilePath);
			return false;
		}

		printf("%s\n", varying.c_str());

		printf("%s\n", data);

		std::string commandLineComment = "// shaderc command line:\n//";
		//for (int32_t ii = 0, num = cmdLine.getNum(); ii < num; ++ii)
		//{
		//	commandLineComment += " ";
		//	commandLineComment += cmdLine.get(ii);
		//}
		commandLineComment += "\n\n";

		compiled = bgfx::compileShader(
			varying != "" ? varying.c_str() : nullptr
			, commandLineComment.c_str()
			, data
			, size
			, options
			, writer
			, bx::getStdOut()
		);

		bx::close(writer);
		delete writer;
	}

	if (compiled)
	{
		return true;
	}

	bx::remove(outFilePath);

	bx::printf("Failed to build shader.\n");
	return false;
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

		std::string vertexSrc, fragmentSrc;
		std::string varyings;
		compiler.output(vertexSrc, fragmentSrc, varyings, true);

		compiler.terminate();

		delete[] src;

		vertexSrc = R"(
$input a_position, a_normal
$output v_position, v_texcoord0


//#include "../common/common.shader"


void main()
{
	vec4 worldPosition = mul(u_model[0], vec4(a_position, 1));

	gl_Position = mul(u_viewProj, worldPosition);

	v_position = a_position;
	v_texcoord0 = a_normal;
}

)";

		varyings = R"(
vec3 v_texcoord0 : TEXCOORD0 = vec3(0.0, 0.0, 0.0);
vec3 v_position  : TEXCOORD1 = vec3(0.0, 0.0, 0.0);
vec3 v_view      : TEXCOORD2 = vec3(0.0, 0.0, 0.0);
vec3 v_normal    : NORMAL    = vec3(0.0, 0.0, 1.0);
vec3 v_tangent   : TANGENT   = vec3(1.0, 0.0, 0.0);
vec3 v_bitangent : BINORMAL  = vec3(0.0, 1.0, 0.0);
vec4 v_color0    : COLOR     = vec4(0.8, 0.8, 0.8, 1.0);
)";

		std::string vertexOutPath = std::string(out).substr(0, strrchr(out, '.') - out) + ".vertex.bin";
		std::string fragmentOutPath = std::string(out).substr(0, strrchr(out, '.') - out) + ".fragment.bin";

		CompileBGFXShaderFromSrc(vertexSrc.c_str(), vertexOutPath.c_str(), "vertex", varyings.c_str());
		CompileBGFXShaderFromSrc(fragmentSrc.c_str(), fragmentOutPath.c_str(), "fragment", varyings.c_str());
	}
	return false;
}
