#include "ResourcePackager.h"

#include <bx/bx.h>
#include <bx/readerwriter.h>
#include <bx/file.h>


struct ResourceHeaderElement
{
	char path[256];
	int offset;
	int size;
};

static char* ReadFile(const char* path, int* size)
{
	FILE* fp = fopen(path, "r");
	if (!fp)
		return nullptr;

	fseek(fp, 0L, SEEK_END);
	long bufsize = ftell(fp);
	fseek(fp, 0L, SEEK_SET);
	if (bufsize == -1)
		return nullptr;

	char* buffer = (char*)malloc(bufsize);
	memset(buffer, 0, bufsize);
	fread(buffer, sizeof(char), bufsize, fp);
	*size = (int)bufsize;

	fclose(fp);

	return buffer;
}

bool PackageResources(const std::vector<ResourceTask>& resources, const std::string packageOut)
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

	for (int i = 0; i < numResources; i++)
	{
		ResourceHeaderElement header = {};

		std::string file = resources[i].path.string();
		if (file.length() >= 256)
			__debugbreak();
		strcpy(header.path, file.c_str());

		int size = (int)std::filesystem::file_size(resources[i].outpath);

		header.offset = currentMemoryBlockOffset;
		header.size = size;
		currentMemoryBlockOffset += size;

		bx::write(writer, header, &err);
	}

	for (int i = 0; i < numResources; i++)
	{
		int size;
		char* buffer = ReadFile(resources[i].outpath.c_str(), &size);
		bx::write(writer, buffer, size, &err);
		delete buffer;
	}

	bx::close(writer);
	delete writer;

	return true;
}
