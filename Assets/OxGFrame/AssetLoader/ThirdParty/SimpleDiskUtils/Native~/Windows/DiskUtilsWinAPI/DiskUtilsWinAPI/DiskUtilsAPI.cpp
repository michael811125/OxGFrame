#include "DiskUtilsAPI.h"



DISKUTILS_API int getAvailableDiskSpace(char* drive)
{
	PULARGE_INTEGER _available = new ULARGE_INTEGER;
	PULARGE_INTEGER _total = new ULARGE_INTEGER;
	PULARGE_INTEGER _free = new ULARGE_INTEGER;

	GetDiskFreeSpaceEx(drive, _available, _total, _free);

	DWORD64 MEGA_BYTE = 1048576;

	DWORD64 free = _available->QuadPart / MEGA_BYTE;

	char s[12];
	sprintf_s(s, "%llu", free);

	int ret;
	sscanf_s(s, "%d", &ret);

	delete(_available);
	delete(_total);
	delete(_free);

	return ret;
}


DISKUTILS_API int getTotalDiskSpace(char* drive)
{
	PULARGE_INTEGER _available = new ULARGE_INTEGER;
	PULARGE_INTEGER _total = new ULARGE_INTEGER;
	PULARGE_INTEGER _free = new ULARGE_INTEGER;

	GetDiskFreeSpaceEx(drive, _available, _total, _free);

	DWORD64 MEGA_BYTE = 1048576;

	DWORD64 total = _total->QuadPart / MEGA_BYTE;

	char s[12];
	sprintf_s(s, "%llu", total);

	int ret;
	sscanf_s(s, "%d", &ret);

	delete(_available);
	delete(_total);
	delete(_free);

	return ret;
}


DISKUTILS_API int getBusyDiskSpace(char* drive)
{
	PULARGE_INTEGER _available = new ULARGE_INTEGER;
	PULARGE_INTEGER _total = new ULARGE_INTEGER;
	PULARGE_INTEGER _free = new ULARGE_INTEGER;

	GetDiskFreeSpaceEx(drive, _available, _total, _free);

	DWORD64 MEGA_BYTE = 1048576;

	DWORD64 busy = (_total->QuadPart - _free->QuadPart) / MEGA_BYTE;

	char s[12];
	sprintf_s(s, "%llu", busy);

	int ret;
	sscanf_s(s, "%d", &ret);

	delete(_available);
	delete(_total);
	delete(_free);

	return ret;
}


DiskUtilsAPI::DiskUtilsAPI()
{
}


DiskUtilsAPI::~DiskUtilsAPI()
{
}
