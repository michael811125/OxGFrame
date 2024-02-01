// HardDiskManager.cpp: implementation of the CHardDiskManager class.
// LICENSE: http://www.codeproject.com/info/cpol10.aspx
//////////////////////////////////////////////////////////////////////

#include "HardDiskManager.h"

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CHardDiskManager::CHardDiskManager()
{
    // bytes available to caller
	m_uliFreeBytesAvailable.QuadPart     = 0L;
	// bytes on disk
	m_uliTotalNumberOfBytes.QuadPart     = 0L;
	// free bytes on disk
	m_uliTotalNumberOfFreeBytes.QuadPart = 0L;
}

CHardDiskManager::~CHardDiskManager()
{
}

bool CHardDiskManager::CheckFreeSpace(LPCTSTR lpDirectoryName)
{
	if( !GetDiskFreeSpaceEx(
		lpDirectoryName,                  // directory name
		&m_uliFreeBytesAvailable,         // bytes available to caller
		&m_uliTotalNumberOfBytes,         // bytes on disk
		&m_uliTotalNumberOfFreeBytes) )   // free bytes on disk
		return false;

	return true;
}

DWORD64 CHardDiskManager::GetFreeBytesAvailable(void)
{ 
	return m_uliFreeBytesAvailable.QuadPart;
}

DWORD64 CHardDiskManager::GetTotalNumberOfBytes(void)
{ 
	return m_uliTotalNumberOfBytes.QuadPart;
}

DWORD64 CHardDiskManager::GetTotalNumberOfFreeBytes(void)
{ 
	return m_uliTotalNumberOfFreeBytes.QuadPart;
}

double CHardDiskManager::GetFreeGBytesAvailable(void)
{ 
	return (double)( (signed __int64)(m_uliFreeBytesAvailable.QuadPart)/1.0e9 );
}

double CHardDiskManager::GetTotalNumberOfGBytes(void)
{ 
	return (double)( (signed __int64)(m_uliTotalNumberOfBytes.QuadPart)/1.0e9 );     
}

double CHardDiskManager::GetTotalNumberOfFreeGBytes(void)
{ 
	return (double)( (signed __int64)(m_uliTotalNumberOfFreeBytes.QuadPart)/1.0e9 ); 
}
