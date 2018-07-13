#include "stdafx.h"
#include <windows.h>
#include "botCore.h"

using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

extern "C" __declspec(dllexport) void findGame()
{

}


IntPtr botCore::Win32API::GetForegroundWindow ()
{
    HWND hWnd = ::GetForegroundWindow();
    if (hWnd == NULL)
    return IntPtr(hWnd);
}


void botCore::Win32API::SetForegroundWindow (IntPtr hWnd)
{
    HWND wnd = static_cast<HWND>(hWnd.ToPointer());
    ::SetForegroundWindow(wnd);
}

System::Drawing::Rectangle botCore::Win32API::GetWindowRect (IntPtr hWnd)
{
    HWND wnd = static_cast<HWND>(hWnd.ToPointer());

    RECT rect;
    ::GetWindowRect(wnd, &rect);

    return System::Drawing::Rectangle((int)rect.left, (int)rect.top, (int)rect.right, (int)rect.bottom);
}


int botCore::Win32API::GetCurrentThreadId ()
{
    return ::GetCurrentThreadId();
}


int botCore::Win32API::GetWindowThreadProcessId (IntPtr hWnd)
{
    HWND wnd = static_cast<HWND>(hWnd.ToPointer());
    return ::GetWindowThreadProcessId(wnd, NULL);
}


bool botCore::Win32API::AttachThreadInput (int wndThread, int attachedThread, bool attach)
{
    return ::AttachThreadInput(wndThread, attachedThread, attach) != FALSE;
}


botCore::WndThreadAttacher::WndThreadAttacher(IntPtr hWnd)
    : hWnd(static_cast<HWND>(hWnd.ToPointer()))
{
    wndThread = GetWindowThreadProcessId(this->hWnd, NULL);
}


static void CopyDataToArray(void * data, int size, array<byte, 1> ^ arr)
{
    Marshal::Copy(IntPtr(data), arr, 0, size);
}


array<byte, 1> ^ botCore::Screener::DataToArray(void * data, int size)
{
    array<byte, 1> ^ arr = gcnew array<byte, 1>(size);
    CopyDataToArray(data, size, arr);
    return arr;
}


void botCore::Screener::WriteData(MemoryStream ^ s, void * data, int size)
{
    s->Write(DataToArray(data, size), 0, size);
}


Bitmap ^ botCore::Screener::Execute()
{

    HWND hWnd       = static_cast<HWND>(_hWnd.ToPointer());
    HDC hDC         = GetDC(hWnd);
    HDC hMemDC      = CreateCompatibleDC(hDC);

    RECT rcClient;
    GetClientRect(hWnd, &rcClient);

    HBITMAP hbmScreen = CreateCompatibleBitmap(hDC, rcClient.right - rcClient.left, rcClient.bottom - rcClient.top);
    if (!hbmScreen)
    {
        DeleteObject(hMemDC);
        throw gcnew Exception("create compatible bitmap filed");
    }

    SelectObject(hMemDC, hbmScreen);

    if (!BitBlt(hMemDC, 0, 0, rcClient.right - rcClient.left, rcClient.bottom - rcClient.top,
        hDC, 0, 0, SRCCOPY))
    {
        DeleteObject(hbmScreen);
        DeleteObject(hMemDC);
        throw gcnew Exception("BitBlt filed");
    }

    BITMAP bmpScreen;
    GetObject(hbmScreen, sizeof(BITMAP), &bmpScreen);

    BITMAPINFOHEADER bi;
    ZeroMemory(&bi, sizeof(BITMAPINFOHEADER));
    bi.biSize           = sizeof(BITMAPINFOHEADER);
    bi.biWidth          = bmpScreen.bmWidth;
    bi.biHeight         = bmpScreen.bmHeight;
    bi.biPlanes         = 1;
    bi.biBitCount       = 32;
    bi.biCompression    = BI_RGB;

    INT32 width     = (INT32)bmpScreen.bmWidth;
    INT32 height    = (INT32)bmpScreen.bmHeight;
    INT32 length    = width * 4 * height;

    HANDLE hDIB     = GlobalAlloc(GHND, length);
    LPVOID lpbitmap = GlobalLock(hDIB);

    // get bitmap data
    GetDIBits(hDC, hbmScreen, 0, height, lpbitmap, (BITMAPINFO *)&bi, DIB_PAL_COLORS);

    // free graphic object
    DeleteObject(hbmScreen);
    DeleteObject(hMemDC);
    ReleaseDC(hWnd, hDC);

    BITMAPFILEHEADER bmfHeader;
    bmfHeader.bfType        = 0x4D42;
    bmfHeader.bfOffBits     = (DWORD)(sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER)); 
    bmfHeader.bfSize        = bmfHeader.bfOffBits + length;

    MemoryStream ^ stream = nullptr;
    try
    {
        stream = gcnew MemoryStream();
        WriteData(stream, &bmfHeader, sizeof(BITMAPFILEHEADER));
        WriteData(stream, &bi, sizeof(BITMAPINFOHEADER));
        WriteData(stream, lpbitmap, length);

        // free bitmap buffer
        GlobalUnlock(hDIB);
        GlobalFree(hDIB);

        stream->Seek(0, SeekOrigin::Begin);

        try
        {
            return gcnew Bitmap(stream);
        }
        catch (Exception ^)
        {
          
            return gcnew Bitmap(1, 1);
        }
    }
    catch (Exception ^)
    {
        throw;
    }
    finally
    {
        if (stream != nullptr)
            stream->~MemoryStream();
    }
}


void botCore::IOProvider::KeyboardState(array<bool, 1> ^ buffer)
{
    if (buffer == nullptr)
        throw gcnew ArgumentNullException("buffer");
    if (buffer->Length != 256)
        throw gcnew ArgumentException("buffer length must be 256");

    BYTE keys[256];
    ::GetKeyboardState(keys);

    for (int i = 0; i < 256; ++i) {
        buffer[i] = keys[i] >> 7 == 1;
    }
}


void botCore::IOProvider::KeyboardKeyDown(int code)
{
    keybd_event(code, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
}


void botCore::IOProvider::KeyboardKeyUp(int code)
{
    keybd_event(code, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
}


void botCore::IOProvider::MouseLeftKeyDown()
{
    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
}


void botCore::IOProvider::MouseLeftKeyUp()
{
    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
}


void botCore::IOProvider::MouseRightKeyDown()
{
    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
}


void botCore::IOProvider::MouseRightKeyUp()
{
    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
}


void botCore::IOProvider::MouseMove(System::Drawing::Point offset)
{
    mouse_event(MOUSEEVENTF_MOVE, offset.X, offset.Y, 0, 0);
}


System::Drawing::Point botCore::IOProvider::MousePos()
{
    POINT p;
    GetCursorPos(&p);

    return System::Drawing::Point((int)p.x, (int)p.y);
}
