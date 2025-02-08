#include "ResourcePackager.h"

#include <bx/bx.h>
#include <bx/readerwriter.h>
#include <bx/file.h>

#include <zlib/zlib.h>

#include <vector>


struct ResourceHeaderElement
{
	char path[256];
	int offset;
	int size;
	int decompressedSize;
	int padding;
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

	int64_t numResources = (int64_t)resources.size();
	bx::write(writer, numResources, &err);

	int currentMemoryBlockOffset = 0;
	std::vector<Resource> files;
	for (int i = 0; i < numResources; i++)
	{
		ResourceHeaderElement header = {};

		std::string file = resources[i].path.string();
		file = file.substr(rootDirectory.parent_path().string().length() + 1);
		BX_ASSUME(file.length() < 256);
		strcpy(header.path, file.c_str());

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

		int paddedSize = (compressedSize + 7) / 8 * 8;
		currentMemoryBlockOffset += paddedSize;

		Resource r;
		r.header = header;
		r.data = buffer;
		files.push_back(r);
	}

	for (int i = 0; i < numResources; i++)
	{
		Resource r = files[i];
		bx::write(writer, r.header, &err);
	}

	for (int i = 0; i < numResources; i++)
	{
		Resource r = files[i];
		bx::write(writer, r.data, r.header.size, &err);

		int paddedSize = (r.header.size + 7) / 8 * 8;
		int padding = paddedSize - r.header.size;
		for (int i = 0; i < padding; i++)
		{
			unsigned char c = 0;
			bx::write(writer, c, &err);
		}
	}

	bx::close(writer);
	delete writer;

	return true;
}
