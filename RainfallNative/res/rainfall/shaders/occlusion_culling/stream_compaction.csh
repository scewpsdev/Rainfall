/*
 * Copyright 2018 Kostas Anagnostou. All rights reserved.
 * License: https://github.com/bkaradzic/bgfx/blob/master/LICENSE
 */

#include "../bgfx/bgfx_compute.shader"

//instance data for all instances (pre culling)
BUFFER_RO(instanceDataIn, vec4, 1);
//per instance visibility (output of culling pass)
BUFFER_RO(instancePredicates, bool, 2);

//how many instances per drawcall
BUFFER_RW(drawcallInstanceCount, uint, 3);
//drawcall data that will drive drawIndirect
BUFFER_RW(drawcallData, uvec4, 4);
//culled instance data
BUFFER_WR(instanceDataOut, vec4, 5);

uniform vec4 u_params;

// Based on Parallel Prefix Sum (Scan) with CUDA by Mark Harris
SHARED uint temp[2048];

NUM_THREADS(1024, 1, 1)
void main()
{
	uint tID = gl_GlobalInvocationID.x;
	int NoofInstancesPowOf2 = int(u_params.y + 0.5);

	int offset = 1;
	bool predicate = instancePredicates[2 * tID];
	temp[2 * tID] = uint(predicate ? 1 : 0);

	predicate = instancePredicates[2 * tID + 1];
	temp[2 * tID + 1] = uint(predicate ? 1 : 0);

	int d;

	//perform reduction
	for (d = NoofInstancesPowOf2 >> 1; d > 0; d >>= 1)
	{
		barrier();

		if (tID < d)
		{
			int ai = int(offset * (2 * tID + 1) - 1);
			int bi = int(offset * (2 * tID + 2) - 1);
			temp[bi] += temp[ai];
		}

		offset *= 2;
	}

	// clear the last element
	if (tID == 0)
	{
		temp[NoofInstancesPowOf2 - 1] = 0;
	}

	// perform downsweep and build scan
	for ( d = 1; d < NoofInstancesPowOf2; d *= 2)
	{
		offset >>= 1;

		barrier();

		if (tID < d)
		{
			int ai = int(offset * (2 * tID + 1) - 1);
			int bi = int(offset * (2 * tID + 2) - 1);
			int t  = int(temp[ai]);
			temp[ai] = temp[bi];
			temp[bi] += t;
		}
	}

	barrier();

	int index = int(2 * tID);

	// scatter results
	predicate = instancePredicates[index];
	if (predicate)
	{
		instanceDataOut[2 * temp[index]    ] = instanceDataIn[2 * index    ];
		instanceDataOut[2 * temp[index] + 1] = instanceDataIn[2 * index + 1];
	}

	index = int(2 * tID + 1);

	predicate = instancePredicates[index];
	if (predicate)
	{
		instanceDataOut[2 * temp[index]    ] = instanceDataIn[2 * index    ];
		instanceDataOut[2 * temp[index] + 1] = instanceDataIn[2 * index + 1];
	}
	
	return;
	if (tID == 0)
	{
		int numIndices = 6 * 6;

		drawIndirect(
			drawcallData,
			0,
			numIndices, 			//number of indices
			drawcallInstanceCount[0], 				//number of instances
			0,			//offset into the vertex buffer
			0							//offset into the instance buffer
			);

		drawcallInstanceCount[0] = 0;
	}

}
