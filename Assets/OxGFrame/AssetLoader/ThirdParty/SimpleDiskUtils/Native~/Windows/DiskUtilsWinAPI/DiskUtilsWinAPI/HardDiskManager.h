// HardDiskManager.h: interface for the CHardDiskManager class.
// LICENSE: http://www.codeproject.com/info/cpol10.aspx
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_HARDDISKMANAGER_H__27F8E542_FA4A_43FF_B29D_59BCD13E31C3__INCLUDED_)
#define AFX_HARDDISKMANAGER_H__27F8E542_FA4A_43FF_B29D_59BCD13E31C3__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <windows.h>

class CHardDiskManager  
{
public:
	CHardDiskManager();
	virtual ~CHardDiskManager();

	bool CheckFreeSpace(LPCTSTR lpDirectoryName);

	DWORD64 GetFreeBytesAvailable(void);
	DWORD64 GetTotalNumberOfBytes(void);
	DWORD64 GetTotalNumberOfFreeBytes(void);

	double GetFreeGBytesAvailable(void);
	double GetTotalNumberOfGBytes(void);
	double GetTotalNumberOfFreeGBytes(void);

private:
	ULARGE_INTEGER m_uliFreeBytesAvailable;     // bytes disponiveis no disco associado a thread de chamada
	ULARGE_INTEGER m_uliTotalNumberOfBytes;     // bytes no disco
	ULARGE_INTEGER m_uliTotalNumberOfFreeBytes; // bytes livres no disco
};

#endif // !defined(AFX_HARDDISKMANAGER_H__27F8E542_FA4A_43FF_B29D_59BCD13E31C3__INCLUDED_)
