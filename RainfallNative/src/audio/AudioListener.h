#pragma once

#include "mesa/MesaBase.h"

#include "mesa/systems/scene/Component.h"
#include "mesa/math/Vector.h"


struct MESA_API AudioListener : Component
{
	Vector3 offset = Vector3::Zero;


	virtual void init() override;
	virtual void destroy() override;
};
