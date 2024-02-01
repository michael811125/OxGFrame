#pragma once

#include "HardDiskManager.h"
#include <Windows.h>
#include <cstdio>
using namespace std;

#define DISKUTILS_API __declspec(dllexport)


extern "C"
{
	DISKUTILS_API int getAvailableDiskSpace(char* drive);
	DISKUTILS_API int getTotalDiskSpace(char* drive);
	DISKUTILS_API int getBusyDiskSpace(char* drive);
}

class DiskUtilsAPI
{
public:
	DiskUtilsAPI();
	~DiskUtilsAPI();
};


