#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::IO;
using namespace System::Reflection;
#include <conio.h>


namespace botCore {

    public ref class Win32API
    {
    public:
        static IntPtr GetForegroundWindow ();
        static void SetForegroundWindow (IntPtr hWnd);

        static System::Drawing::Rectangle GetWindowRect (IntPtr hWnd);

        static int GetWindowThreadProcessId (IntPtr hWnd);
        static int GetCurrentThreadId ();
        static bool AttachThreadInput (int wndThread, int attachedThread, bool attach);
    };

    public ref class WndThreadAttacher
    {
    public:
        WndThreadAttacher(IntPtr hWnd);

    private:
        HWND        hWnd;
        int         wndThread;
        int         attachedTread;
        bool        attached;
    };

    public ref class Screener
    {
    public:
        Screener(IntPtr hWnd)
            : _hWnd(hWnd)
        {
        }

        Bitmap ^ Execute();

    private:
        array<byte, 1> ^ DataToArray(void * data, int size);
        void WriteData(MemoryStream ^ s, void * data, int size);

    private:
        IntPtr _hWnd;
    };

    public ref class IOProvider
    {
    public:
        // output
        static void KeyboardState(array<bool, 1> ^ buffer);
        static System::Drawing::Point MousePos();

        // input
        static void KeyboardKeyDown(int code);
        static void KeyboardKeyUp(int code);

        static void MouseLeftKeyDown();
        static void MouseLeftKeyUp();
        static void MouseRightKeyDown();
        static void MouseRightKeyUp();

        static void MouseMove(System::Drawing::Point offset);
    };

 extern void findGame();

	public ref class Bot {
	public:

		int speed = 1;
		int interval = 120;
		int height, width;


		static bool isRunning() {
			return !GetAsyncKeyState(VK_F12);
		}

		void run() {
			while (true) {
				while (!GetAsyncKeyState(VK_F6)) {
					for (auto i = 0; i < interval; i++) {
						Sleep(1000);
						if (GetAsyncKeyState(VK_F10)) return;
					}
					closeKickMessage();
					leaveQueue();
				}
			};
		};

		Bot() {};
		~Bot() {};

		const int MAXSCREEN = 65535;
		void click() {
			Sleep(200);
			INPUT ip[1] = { 0 };
			ip[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
			ip[0].mi.time = 0;
			SendInput(1, ip, sizeof(ip));
			Sleep(100);
			ip[0].mi.dwFlags = MOUSEEVENTF_LEFTUP;
			Sleep(15);
			SendInput(1, ip, sizeof(ip));
			Sleep(1000);
		};

		void click_short() {
			Sleep(200);
			INPUT ip[1] = { 0 };
			ip[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
			ip[0].mi.time = 0;
			SendInput(1, ip, sizeof(ip));
			Sleep(100);
			ip[0].mi.dwFlags = MOUSEEVENTF_LEFTUP;
			Sleep(15);
			SendInput(1, ip, sizeof(ip));
			Sleep(200);
		};

		void findGame() {
			SetCursorPos(430, 80); //play
			click();
			SetCursorPos(220, 520); //PVE
			click();
			SetCursorPos(220, 430); //easy
			click();
		};

		void leaveQueue() {
			SetCursorPos(960, 780);
			click();
			
		};

		void closeKickMessage() {
			SetCursorPos(960, 920);
			click();
		};

		void pressEsc() {
			INPUT ip = { 0 };
			ip.type = INPUT_KEYBOARD;
			ip.ki.dwFlags = KEYEVENTF_SCANCODE; // hardware scan
			ip.ki.wScan = 0x01; // hardware scan code for key "ESC"
			ip.ki.time = 0;
			SendInput(1, &ip, sizeof(INPUT));
			Sleep(50);
		}

		void leftGame() {
			/*INPUT ip = { 0 };
			ip.type = INPUT_KEYBOARD;
			ip.ki.dwFlags = KEYEVENTF_SCANCODE; // hardware scan
			ip.ki.wScan = 0x01; // hardware scan code for key "ESC"
			ip.ki.time = 0;
			SendInput(1, &ip, sizeof(INPUT));
			Sleep(50);
			*/

			SetCursorPos(1440, 280); //to menu
			click();
			SetCursorPos(960, 990); //agree
			click();
		};

		void leftGameOperations() {
			/*
			INPUT ip = { 0 };
			ip.type = INPUT_KEYBOARD;
			ip.ki.dwFlags = KEYEVENTF_SCANCODE; // hardware scan
			ip.ki.wScan = 0x01; // hardware scan code for key "ESC"
			ip.ki.time = 0;
			SendInput(1, &ip, sizeof(INPUT));
			Sleep(50);
			*/
			SetCursorPos(1440, 340); //to menu
			click();
			SetCursorPos(960, 990); //agree
			click();
		}

		bool isStopped() {
			return (_kbhit() && _getch() == 27);
		}

		void pickOperator(int v) {
			switch (v)
			{
			case 1:
				SetCursorPos(220, 540);
				break;
			case 2:
				SetCursorPos(360, 540);
				break;
			case 3:
				SetCursorPos(490, 540);
				break;
			case 4:
				SetCursorPos(620, 540);
				break;
			case 5:
				SetCursorPos(760, 540);
				break;
			case 6:
				SetCursorPos(890, 540);
				break;
			case 7:
				SetCursorPos(220, 630);
				break;
			case 8:
				SetCursorPos(360, 630);
				break;
			case 9:
				SetCursorPos(490, 630);
				break;
			case 10:
				SetCursorPos(620, 630);
				break;
			case 11:
				SetCursorPos(760, 630);
				break;
			case 12:
				SetCursorPos(890, 630);
				break;
			case 13:
				SetCursorPos(220, 720);
				break;
			case 14:
				SetCursorPos(360, 720);
				break;
			case 15:
				SetCursorPos(490, 720);
				break;
			case 16:
				SetCursorPos(620, 720);
				break;
			case 17:
				SetCursorPos(760, 720);
				break;
			case 18:
				SetCursorPos(890, 720);
				break;
			default: 
				break;
			}
			click_short();
		}

		void endGame() {
			SetCursorPos(1700, 970);
			click();
		}
	};
}
