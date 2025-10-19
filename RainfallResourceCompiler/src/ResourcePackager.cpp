#include "ResourcePackager.h"

#include <bx/bx.h>
#include <bx/readerwriter.h>
#include <bx/file.h>

#include <zlib/zlib.h>

#include <vector>


struct ResourceHeaderElement
{
	int pathLen;
	char* path;
	int offset;
	int size;
	int decompressedSize;
};

struct Resource
{
	ResourceHeaderElement header;
	char* data;
};

static char* ReadFile(const char* path, int* size)
{
	FILE* fp = fopen(path, "rb");
	if (!fp)
		return nullptr;

	fseek(fp, 0L, SEEK_END);
	long bufsize = ftell(fp);
	fseek(fp, 0L, SEEK_SET);
	if (bufsize == -1)
		return nullptr;

	char* buffer = (char*)malloc(bufsize);
	memset(buffer, 0, bufsize);
	size_t readBytes = fread(buffer, sizeof(char), bufsize, fp);
	*size = (int)bufsize;

	fclose(fp);

	return buffer;
}

bool PackageResources(const std::vector<ResourceTask>& resources, const std::string packageOut, fs::path rootDirectory, bool compressData)
{
	bx::FileWriter* writer = new bx::FileWriter();
	if (!bx::open(writer, packageOut.c_str()))
	{
		return false;
	}

	bx::ErrorAssert err;

	int numResources = (int)resources.size();
	bx::write(writer, numResources, &err);

	int currentMemoryBlockOffset = 0;
	std::vector<Resource> files;
	for (int i = 0; i < numResources; i++)
	{
		ResourceHeaderElement header = {};

		std::string file = resources[i].path.string();
		file = file.substr(rootDirectory.parent_path().string().length() + 1);
		header.path = _strdup(file.c_str());
		header.pathLen = (int)strlen(header.path);

		int size;
		char* buffer = ReadFile(resources[i].outpath.c_str(), &size);

		int compressedSize = size;

		if (compressData)
		{
			compressedSize = compressBound(size);
			Bytef* compressedData = new Bytef[compressedSize];
			if (compress(compressedData, (uLong*)&compressedSize, (const Bytef*)buffer, size) != Z_OK)
			{
				printf("Compression failed: %s\n", file.c_str());
				delete[] compressedData;
				continue;
			}
			free(buffer);
			buffer = (char*)compressedData;
		}

		header.offset = currentMemoryBlockOffset;
		header.size = compressedSize;
		header.decompressedSize = size;

		currentMemoryBlockOffset += compressedSize;

		Resource r;
		r.header = header;
		r.data = buffer;
		files.push_back(r);
	}

	for (int i = 0; i < numResources; i++)
	{
		Resource r = files[i];
		bx::write(writer, r.header.pathLen, &err);
		bx::write(writer, r.header.path, r.header.pathLen, &err);
		bx::write(writer, r.header.offset, &err);
		bx::write(writer, r.header.size, &err);
		bx::write(writer, r.header.decompressedSize, &err);
	}

	for (int i = 0; i < numResources; i++)
	{
		Resource r = files[i];
		bx::write(writer, r.data, r.header.size, &err);
	}

	bx::close(writer);
	delete writer;

	return true;
}
