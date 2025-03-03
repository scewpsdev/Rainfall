
#include "log.h"

#include "../CGLCompiler.h"


MessageCallback_t GetMsgCallback(CGLCompiler* context)
{
	return context->msgCallback;
}
