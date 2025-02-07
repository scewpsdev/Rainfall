#pragma once

#include "ResourceCompiler.h"

#include <vector>


bool PackageResources(const std::vector<ResourceTask>& resources, const std::string packageOut);
