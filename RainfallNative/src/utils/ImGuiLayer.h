#pragma once

#include "Event.h"


void ImGuiLayerInit();
void ImGuiLayerDestroy();
void ImGuiLayerBeginFrame();
void ImGuiLayerEndFrame();
bool ImGuiLayerProcessEvent(const Event* ev);
