using System.Runtime.InteropServices;

internal class Program
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    public struct Message
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point point;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    /*
        fsModifiers - Клавиши, которые необходимо нажать в сочетании с клавишей, указанной параметром vk,чтобы
        сгенерировать WM_HOTKEY сообщение. Если значение равно 0, клавиша vk может нажиматься без сочитаний с другими клавишами.
        
        vk - Код виртуальной горячей клавиши(отслеживаемая клавиша.)
    */
    const int MYACTION_HOTKEY_ID = 1;
    const uint fsModifiers = 0x0001;
    const uint VK_MENU = 0x12;

    static void Main()
    {
        RegisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID, fsModifiers, VK_MENU);
        Message message;

        while (GetMessage(out message, IntPtr.Zero, 0, 0))
        {
            if (message.message == 0x0312 && message.wParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                Console.WriteLine("Alt key pressed");
            }
        }
        UnregisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID);
    }
}