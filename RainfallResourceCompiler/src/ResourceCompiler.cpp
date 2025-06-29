#include "ResourceCompiler.h"

#include "shader/ShaderCompiler.h"
#include "TextureCompiler.h"
#include "GeometryCompiler.h"
#include "ResourcePackager.h"

#include <bx/file.h>

#include <iostream>
#include <fstream>

#include <vector>
#include <map>
#include <future>
#include <mutex>


static bool optimizeSceneGraph = true;

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

static bool CompileOtherResource(const char* path, const char* out)
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

static bool IsShader(const std::string& extension)
{
	return extension == ".glsl" || extension == ".shader" || extension == ".vsh" || extension == ".fsh" || extension == ".csh" || extension == ".shd";
}

static bool IsTexture(const std::string& extension)
{
	return extension == ".png" || extension == ".jpg" || extension == ".hdr";
}

static bool IsGeometry(const std::string& extension)
{
	return extension == ".glb" || extension == ".gltf";
}

static bool IsSound(const std::string& extension)
{
	return extension == ".ogg" || extension == ".wav";
}

static void CompileFile(const fs::path& file, const std::string& outpathStr)
{
	const char* outpath = outpathStr.c_str();

	std::string filepathStr = file.string();
	std::string name = file.stem().string();
	std::string extension = file.extension().string();

	fprintf(stderr, "%s\n", filepathStr.c_str());

	bool success = true;

	if (IsShader(extension))
	{
		bool vertex = extension == ".vsh" || name.size() >= 4 && strncmp(&name[name.size() - 4], ".vsh", 4) == 0;
		bool fragment = extension == ".fsh" || name.size() >= 4 && strncmp(&name[name.size() - 4], ".fsh", 4) == 0;
		bool compute = extension == ".csh" || name.size() >= 4 && strncmp(&name[name.size() - 4], ".csh", 4) == 0;

		if (vertex)
			success = CompileBGFXShader(filepathStr.c_str(), outpath, "vertex") && success;
		if (fragment)
			success = CompileBGFXShader(filepathStr.c_str(), outpath, "fragment") && success;
		if (compute)
			success = CompileBGFXShader(filepathStr.c_str(), outpath, "compute") && success;

		bool rainfallShader = extension == ".shd";
		if (rainfallShader)
			success = CompileRainfallShader(filepathStr.c_str(), outpath) && success;
	}
	else if (IsTexture(extension))
	{
		success = CompileTexture(name, extension, filepathStr.c_str(), outpath);
	}
	else if (IsGeometry(extension))
	{
		bool isLevel = name.find("_level") != std::string::npos;

		success = CompileGeometry(filepathStr.c_str(), outpath, optimizeSceneGraph && !isLevel);
	}
	else
	{
		success = CompileOtherResource(filepathStr.c_str(), outpath);
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

	if (IsShader(extension))
	{
		std::string name = file.stem().string();
		if (extension.size() >= 2 && (
			strncmp(&extension[extension.size() - 3], "vsh", 2) == 0 ||
			strncmp(&extension[extension.size() - 3], "fsh", 2) == 0 ||
			strncmp(&extension[extension.size() - 3], "csh", 2) == 0
			) ||
			name.size() > 4 && name[name.size() - 4] == '.' &&
			(strncmp(&name[name.size() - 3], "vsh", 3) == 0 ||
				strncmp(&name[name.size() - 3], "fsh", 3) == 0 ||
				strncmp(&name[name.size() - 3], "csh", 3) == 0) ||
			//extension == ".shader" ||
			extension == ".shd")
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

static void ProcessFile(fs::path file, const std::string& outputDirectory, fs::path rootDirectory, bool package)
{
	std::string outdir = outputDirectory + file.parent_path().string().substr(rootDirectory.string().size());
	std::string outpath = outdir + "\\" + file.filename().string();

	if (package)
	{
		std::string file = outpath.substr(0, outpath.length() - 4);
		resourcesToCompile.push_back({ file, outpath });
	}
	else
	{
		outpath = outpath + ".bin";

		std::string extension = file.extension().string();
		std::string filepathStr = file.string();

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
}

static void ProcessDirectory(fs::path directory, const std::string& outputDirectory, fs::path rootDirectory, const std::vector<std::string>& formats, bool package)
{
	for (auto entry : fs::directory_iterator(directory))
	{
		if (fs::is_directory(entry))
		{
			ProcessDirectory(entry, outputDirectory, rootDirectory, formats, package);
		}
		else
		{
			std::string name = entry.path().stem().string();
			std::string extension = entry.path().extension().string();
			bool packageFile = extension == ".dat" && name.substr(0, 4) == "data";
			if (!packageFile)
			{
				if (formats.size() > 0)
				{
					if (entry.path().has_extension())
					{
						std::string extension = entry.path().extension().string().substr(1);
						if (std::find(formats.begin(), formats.end(), extension) != formats.end())
							ProcessFile(entry, outputDirectory, rootDirectory, package);
					}
				}
				else
				{
					ProcessFile(entry, outputDirectory, rootDirectory, package);
				}
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
		bool package = false;
		bool packageCompress = false;

		int argIndex = 0;
		for (int i = 1; i < argc; i++)
		{
			const char* arg = argv[i];
			if (arg[0] == '-')
			{
				if (strcmp(arg, "-f") == 0)
					singleFile = true;
				else if (strcmp(arg, "--preserve-scenegraph") == 0)
					optimizeSceneGraph = false;
				else if (strcmp(arg, "--package") == 0)
					package = true;
				else if (strcmp(arg, "--compress") == 0)
					packageCompress = true;
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


		if (package)
		{
			ProcessDirectory(rootDirectory, rootDirectory.string(), rootDirectory, {}, true);

			std::vector<ResourceTask> shaders;
			std::vector<ResourceTask> textures;
			std::vector<ResourceTask> geometries;
			std::vector<ResourceTask> sounds;
			std::vector<ResourceTask> misc;

			for (int i = 0; i < (int)resourcesToCompile.size(); i++)
			{
				fs::path file = resourcesToCompile[i].path;
				std::string extension = file.extension().string();
				if (IsShader(extension))
					shaders.push_back(resourcesToCompile[i]);
				else if (IsTexture(extension))
					textures.push_back(resourcesToCompile[i]);
				else if (IsGeometry(extension))
					geometries.push_back(resourcesToCompile[i]);
				else if (IsSound(extension))
					sounds.push_back(resourcesToCompile[i]);
				else
					misc.push_back(resourcesToCompile[i]);
			}

			if (shaders.size() > 0)
				PackageResources(shaders, rootDirectory.string() + "\\datas.dat", rootDirectory, packageCompress);
			if (textures.size() > 0)
				PackageResources(textures, rootDirectory.string() + "\\datat.dat", rootDirectory, packageCompress);
			if (geometries.size() > 0)
				PackageResources(geometries, rootDirectory.string() + "\\datag.dat", rootDirectory, packageCompress);
			if (sounds.size() > 0)
				PackageResources(sounds, rootDirectory.string() + "\\dataa.dat", rootDirectory, packageCompress);
			if (misc.size() > 0)
				PackageResources(misc, rootDirectory.string() + "\\datam.dat", rootDirectory, packageCompress);

			printf("%d assets packaged.\n", (int)resourcesToCompile.size());
		}
		else if (singleFile)
		{
			std::string outpath = outputDirectory + ".bin";
			CompileFile(rootDirectory, outpath.c_str());
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

			ProcessDirectory(rootDirectory, outputDirectory, rootDirectory, formats, false);

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
				//CompileFile(task.path, task.outpath.c_str());
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
