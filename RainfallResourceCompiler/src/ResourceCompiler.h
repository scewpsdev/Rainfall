#pragma once

#include <filesystem>


namespace fs = std::filesystem;


struct ResourceTask
{
	fs::path path;
	std::string outpath;
};
