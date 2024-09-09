using System.Diagnostics;
using System.Runtime.InteropServices;

internal class Program
{
    private static bool action = true;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    private static extern bool ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    private static extern void CloseHandle(IntPtr hHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    public struct Message
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
    }

    //fsModifiers - keys that must be pressed in combination with the key specified by the vk parameter to generate a wm_hotkey message.
    //if the value is 0, the vk key can be pressed without any other key combination.
        
    //vk - virtual hotkey code (tracked key.)

    const int MYACTION_HOTKEY_ID = 1;
    const uint fsModifiers = 0x0001;
    const uint VK_MENU = 0x12;
    
    const uint PROCESS_SUSPEND_RESUME = 0x0002;
    const uint THREAD_SUSPEND_RESUME = 0x0002;
    private static IntPtr hProcess = IntPtr.Zero;
    private static uint processId;

    static void Main()
    {
        RegisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID, fsModifiers, VK_MENU);
        Message message;

        while (GetMessage(out message, IntPtr.Zero, 0, 0))
        {
            if (message.message == 0x0312 && message.wParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                if (action == true)
                {
                    Console.WriteLine("Alt key pressed. Action: true.");
                    action = false;
                    SuspendActiveApplication();
                }
                else
                {
                    Console.WriteLine("Alt key pressed. Action: false.");
                    action = true;
                    ResumeSuspendedApplication();
                }
            }
        }
        UnregisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID);
    }

    private static void SuspendActiveApplication()
    {
        // Get the handle of the active window.
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero)
        {
            Console.WriteLine("No active window found.");
            return;
        }

        // Get the process ID.
        GetWindowThreadProcessId(hWnd, out processId);

        // Open the process with suspend permissions.
        hProcess = OpenProcess(PROCESS_SUSPEND_RESUME, false, processId);
        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("Failed to open process.");
            return;
        }

        // We get all the process threads and suspend them.
        Process process = Process.GetProcessById((int)processId);
        foreach (ProcessThread thread in process.Threads)
        {
            IntPtr hThread = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
            if (hThread != IntPtr.Zero)
            {
                SuspendThread(hThread);
                CloseHandle(hThread);
            }
        }
    }

    private static void ResumeSuspendedApplication()
    {
        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("No process to resume.");
            return;
        }

        // Resuming process flows.
        Process process = Process.GetProcessById((int)processId);
        foreach (ProcessThread thread in process.Threads)
        {
            IntPtr hThread = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
            if (hThread != IntPtr.Zero)
            {
                ResumeThread(hThread);
                CloseHandle(hThread);
            }
        }

        CloseHandle(hProcess);
        hProcess = IntPtr.Zero;
    }
}