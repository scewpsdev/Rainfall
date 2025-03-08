#pragma once

#include "SourceFile.h"

#include "ast/File.h"
#include "utils/Log.h"
#include "utils/List.h"

#include <string>


class CGLCompiler
{
public:
	List<SourceFile> sourceFiles;
	List<LinkerFile> linkerFiles;
	List<const char*> linkerPaths;

	List<AST::File*> asts;
	MessageCallback_t msgCallback = nullptr;
	bool disableError = false;


	void init(MessageCallback_t msgCallback);
	void terminate();

	void addFile(const char* filename, const char* name, const char* src);
	void addLinkerFile(const char* filename);
	void addLinkerPath(const char* path);
	bool compile();
	void output(std::string& vertexSrc, std::string& fragmentSrc, std::string& varyings, bool printIR);
	void outputCompute(const char* kernelName, std::string& src, bool printIR);
};
