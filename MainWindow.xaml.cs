using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;


namespace testAutoclicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // IMPORTATIONS des fonctions voulu.
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        // define  hotkey & key
        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        //CAPS LOCK:

        private const uint VK_F6 = 0x75; //F6
        private const uint VK_CAPITAL = 0x14;


        // attribut
        private IntPtr _windowHandle;
        private HwndSource _source;
        private AutoClicker autoClicker;


        // event
        public Action onCapsLock;
        public Action onPressF6;



        public MainWindow()
        {
            InitializeComponent();
            autoClicker = new AutoClicker(this);
          
        }


      

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //override to unregister HOTKEY listener
        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        //override to register HOTKEY listener
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_CAPITAL); //CTRL + CAPS_LOCK
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F6); //NONE + F6

        }

        // HOTKEY listener
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            switch (msg)
            {
                case WM_HOTKEY:

                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_CAPITAL)
                            {
                                onCapsLock?.Invoke();

                            }
                            if (vkey == VK_F6)
                            {
                                onPressF6?.Invoke();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnLostFocus_numericField(object sender, RoutedEventArgs e)
        {
            //get the source form the event
            TextBox textBox = (TextBox)e.Source;

            //if the field is empty we set 0
            if (textBox.Text == "")
                textBox.Text = "0";


        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            //get the source form the event
            TextBox textBox = (TextBox)e.Source;

            //if the field is 0 we set empty
            if (textBox.Text == "0")
                textBox.Text = "";


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}