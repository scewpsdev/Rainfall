

#include "ShaderCompiler.h"
#include "TextureCompiler.h"
#include "GeometryCompiler.h"

#include <bx/file.h>

#include <iostream>
#include <filesystem>
#include <fstream>

#include <vector>
#include <map>
#include <future>
#include <mutex>

namespace fs = std::filesystem;


struct ResourceTask
{
	fs::path path;
	std::string outpath;
};


static int assetsCompiled = 0;
static int assetsUpToDate = 0;

static std::vector<ResourceTask> resourcesToCompile;
static std::vector<std::future<void>> resourceFutures;

static std::map<std::string, int64_t> assetTable;
static std::mutex assetTableMutex;

static std::vector<std::pair<fs::path, int64_t>> changedDependencies;


static void TryLoadAssetTable(const char* assetTableFile)
{
	using namespace bx;

	Error err;
	FileReader reader;
	if (open(&reader, assetTableFile))
	{
		int numAssets = 0;
		read(&reader, numAssets, &err);

		for (int i = 0; i < numAssets; i++)
		{
			char path[256] = {};
			read(&reader, path, &err);

			int64_t lastWriteTime;
			read(&reader, lastWriteTime, &err);

			assetTable.emplace(std::string(path), lastWriteTime);
		}

		close(&reader);
	}
}

static void WriteAssetTable(const char* assetTableFile)
{
	using namespace bx;

	Error err;
	FileWriter writer;
	if (open(&writer, assetTableFile))
	{
		write(&writer, (int)assetTable.size(), &err);

		for (const auto& pair : assetTable)
		{
			char path[256] = {};
			strcpy(path, pair.first.c_str());
			write(&writer, path, &err);

			int64_t lastWriteTime = pair.second;
			write(&writer, lastWriteTime, &err);
		}

		close(&writer);
	}
}

static void CreateDirectory(fs::path directory)
{
	if (!fs::is_directory(directory) || !fs::exists(directory))
		fs::create_directories(directory);
}

static bool CopyFile(const char* path, const char* out)
{
	bx::FileReader reader;
	if (bx::open(&reader, path))
	{
		int64_t size = reader.seek(0, bx::Whence::End);
		reader.seek(0, bx::Whence::Begin);

		char* data = new char[size];
		bx::read(&reader, data, (int32_t)size, nullptr);

		bx::close(&reader);

		bx::FileWriter writer;
		if (bx::open(&writer, out, false, nullptr))
		{
			bx::write(&writer, data, (int32_t)size, nullptr);

			bx::close(&writer);

			delete[] data;

			return true;
		}
		else
		{
			delete[] data;
			return false;
		}
	}
	return false;
}

static void CompileFile(const fs::path& file, const std::string& outpath)
{
	std::string filepathStr = file.string();
	std::string extension = file.extension().string();

	fprintf(stderr, "%s\n", filepathStr.c_str());

	bool success = true;

	if (extension == ".glsl" || extension == ".shader")
	{
		std::string name = file.stem().string();
		if (name.size() > 3 && name[name.size() - 3] == '.')
		{
			if (strncmp(&name[name.size() - 2], "vs", 2) == 0)
				success = CompileShader(filepathStr.c_str(), outpath.c_str(), "vertex");
			else if (strncmp(&name[name.size() - 2], "fs", 2) == 0)
				success = CompileShader(filepathStr.c_str(), outpath.c_str(), "fragment") && success;
			else if (strncmp(&name[name.size() - 2], "cs", 2) == 0)
				success = CompileShader(filepathStr.c_str(), outpath.c_str(), "compute") && success;
		}
	}
	else if (extension == ".png")
	{
		std::string name = file.stem().string();
		std::transform(name.begin(), name.end(), name.begin(), [](unsigned char c) {return std::tolower(c); });

		if (name.find("cubemap") != std::string::npos)
		{
			bool equirect = name.find("equirect") != std::string::npos;
			success = CompileTexture(filepathStr.c_str(), outpath.c_str(), nullptr, false, false, true, equirect, !equirect);
		}
		else
		{
			std::string format;
			bool linear = false, normal = false, mipmaps = false;
			if (name.find("basecolor") != std::string::npos || name.find("albedo") != std::string::npos || name.find("diffuse") != std::string::npos)
			{
				format = "BC3";
				//linear = true;
				mipmaps = true;
			}
			else if (name.find("normal") != std::string::npos)
			{
				format = "BC3";
				linear = true;
				normal = true;
				mipmaps = true;
			}
			else if (name.find("roughnessmetallic") != std::string::npos)
			{
				format = "BC3";
				linear = true;
				mipmaps = true;
			}
			else if (name.find("roughness") != std::string::npos)
			{
				format = "BC3";
				linear = true;
				mipmaps = true;
			}
			else if (name.find("metallic") != std::string::npos)
			{
				format = "BC3";
				linear = true;
				mipmaps = true;
			}
			else if (name.find("height") != std::string::npos || name.find("displacement") != std::string::npos)
			{
				format = "BC4";
				linear = true;
				mipmaps = true;
			}
			else if (name.find("emissive") != std::string::npos || name.find("emission") != std::string::npos)
			{
				format = "BC3";
				//linear = true;
				mipmaps = true;
			}
			else if (name.find("_ao") != std::string::npos)
			{
				format = "BC4";
				linear = true;
				mipmaps = true;
			}
			else
			{
				format = "BGRA8";
				linear = true;
			}

			success = CompileTexture(filepathStr.c_str(), outpath.c_str(), format.c_str(), linear, normal, mipmaps);
		}
	}
	else if (extension == ".jpg")
	{
		std::string name = file.stem().string();

		std::string format = "BC3";
		bool linear = false, normal = false, mipmaps = false;

		success = CompileTexture(filepathStr.c_str(), outpath.c_str(), format.c_str(), linear, normal, mipmaps);
	}
	else if (extension == ".hdr")
	{
		std::string name = file.stem().string();

		success = CompileTexture(filepathStr.c_str(), outpath.c_str(), nullptr, true, false, true, true, false);
	}
	else if (extension == ".glb" || extension == ".gltf")
	{
		success = CompileGeometry(filepathStr.c_str(), outpath.c_str());
	}
	else
	{
		success = CopyFile(filepathStr.c_str(), outpath.c_str());
	}

	if (success)
	{
		assetTableMutex.lock();
		assetTable[file.string()] = fs::last_write_time(file).time_since_epoch().count();
		assetTableMutex.unlock();
	}
	else
	{
		printf("Failed to compile resource %s\n", (const char*)file.c_str());
	}
}

void getShaderDependencies(fs::path file, std::vector<std::string>& dependencies)
{
	std::ifstream stream(file);
	std::string line;
	while (std::getline(stream, line))
	{
		if (line.substr(0, 9) == "#include ")
		{
			size_t start = line.find_first_of('"') + 1;
			size_t end = line.find_last_of('"');
			std::string header = line.substr(start, end - start);
			for (size_t i = 0; i < header.length(); i++)
			{
				if (header[i] == '/')
					header[i] = '\\';
			}
			std::string fullHeaderPath = file.parent_path().string() + "\\" + header;
			//printf("%s\n", fullHeaderPath.c_str());
			dependencies.push_back(fullHeaderPath);

			getShaderDependencies(fullHeaderPath, dependencies);
		}
	}
}

static bool FileHasChanged(fs::path file, std::string& outpath, std::string& extension, std::map<std::string, int64_t>& assetTable)
{
	if (assetTable.size() == 0)
		return true;

	if (extension == ".shader")
	{
		std::string name = file.stem().string();
		if (name.size() > 3 && name[name.size() - 3] == '.' &&
			(strncmp(&name[name.size() - 2], "vs", 2) == 0 ||
				strncmp(&name[name.size() - 2], "fs", 2) == 0 ||
				strncmp(&name[name.size() - 2], "cs", 2) == 0))
			;
		else
		{
			return false;
		}

		if (!fs::exists(file))
			printf("error cant find shader %s\n", file.string().c_str());

		std::vector<std::string> shaderDependencies;
		getShaderDependencies(file, shaderDependencies);

		bool recompile = false;
		for (size_t i = 0; i < shaderDependencies.size(); i++)
		{
			fs::path dependencyFile = shaderDependencies[i];

			if (!fs::exists(dependencyFile))
				printf("error cant find dependency %s of shader %s\n", dependencyFile.string().c_str(), file.string().c_str());

			int64_t lastWriteTime = fs::last_write_time(dependencyFile).time_since_epoch().count();

			bool dependencyChanged = false;
			if (assetTable.find(dependencyFile.string()) == assetTable.end())
				dependencyChanged = true;
			else if (lastWriteTime > assetTable[dependencyFile.string()])
				dependencyChanged = true;

			if (dependencyChanged)
			{
				changedDependencies.push_back(std::make_pair(dependencyFile, lastWriteTime));
				recompile = true;
			}
		}

		if (recompile)
			return true;
	}

	auto it = assetTable.find(file.string());
	if (it == assetTable.end())
		return true;

	int64_t lastWriteTime = fs::last_write_time(file).time_since_epoch().count();
	int64_t tableTime = it->second;
	if (lastWriteTime != tableTime)
		return true;

	if (!fs::exists(outpath))
		return true;

	return false;
}

static void ProcessFile(fs::path file, const std::string& outputDirectory, fs::path rootDirectory)
{
	std::string extension = file.extension().string();
	std::string filepathStr = file.string();
	std::string outdir = outputDirectory + file.parent_path().string().substr(rootDirectory.string().size());
	std::string outpath = outdir + "\\" + file.filename().string() + ".bin";

	bool fileOutdated = FileHasChanged(file, outpath, extension, assetTable);
	if (fileOutdated)
	{
		assetsCompiled++;
		CreateDirectory(outdir);
		resourcesToCompile.push_back({ file, outpath });
	}
	else
	{
		assetsUpToDate++;
	}
}

static void ProcessDirectory(fs::path directory, const std::string& outputDirectory, fs::path rootDirectory, const std::vector<std::string>& formats)
{
	for (auto entry : fs::directory_iterator(directory))
	{
		if (fs::is_directory(entry))
		{
			ProcessDirectory(entry, outputDirectory, rootDirectory, formats);
		}
		else
		{
			if (formats.size() > 0)
			{
				if (entry.path().has_extension())
				{
					std::string extension = entry.path().extension().string().substr(1);
					if (std::find(formats.begin(), formats.end(), extension) != formats.end())
						ProcessFile(entry, outputDirectory, rootDirectory);
				}
			}
			else
			{
				ProcessFile(entry, outputDirectory, rootDirectory);
			}
		}
	}
}

int main(int argc, char* argv[])
{
	if (argc >= 3)
	{
		fs::path rootDirectory;
		std::string outputDirectory;
		std::vector<std::string> formats;
		bool singleFile = false;

		int argIndex = 0;
		for (int i = 1; i < argc; i++)
		{
			const char* arg = argv[i];
			if (arg[0] == '-')
			{
				if (strcmp(arg, "-f") == 0)
					singleFile = true;
			}
			else
			{
				if (argIndex == 0)
					rootDirectory = argv[i];
				else if (argIndex == 1)
					outputDirectory = argv[i];
				else if (argIndex >= 2)
				{
					formats.push_back(argv[i]);
				}
				argIndex++;
			}
		}


		if (singleFile)
		{
			CompileFile(rootDirectory, outputDirectory + ".bin");
		}
		else
		{
			if (!fs::exists(rootDirectory))
			{
				std::string rootDirStr = rootDirectory.string();
				fprintf(stderr, "Resource directory '%s' does not exist\n", rootDirStr.c_str());
				return -1;
			}

			std::string assetTableFile = rootDirectory.string() + std::string("\\asset_table");

			TryLoadAssetTable(assetTableFile.c_str());

			ProcessDirectory(rootDirectory, outputDirectory, rootDirectory, formats);

			for (size_t i = 0; i < changedDependencies.size(); i++)
			{
				fs::path dependencyFile = changedDependencies[i].first;
				int64_t lastWriteTime = changedDependencies[i].second;
				assetTable[dependencyFile.string()] = lastWriteTime;
			}

			resourceFutures.resize(resourcesToCompile.size());
			for (size_t i = 0; i < resourcesToCompile.size(); i++)
			{
				ResourceTask task = resourcesToCompile[i];
				resourceFutures[i] = std::async(std::launch::async, CompileFile, task.path, task.outpath);
				//CompileFile(task.path, task.outpath);
			}

			bool allResourcesCompiled = false;
			while (!allResourcesCompiled)
			{
				allResourcesCompiled = true;
				for (size_t i = 0; i < resourceFutures.size(); i++)
				{
					if (!resourceFutures[i]._Is_ready())
					{
						allResourcesCompiled = false;
						break;
					}
				}
			}

			printf("%d assets compiled, %d up to date.\n", assetsCompiled, assetsUpToDate);

			WriteAssetTable(assetTableFile.c_str());
		}

		return 0;
	}

	printf("Usage: ResourceCompiler.exe <res folder> <out folder> [formats...]\n");
	return 0;
}
