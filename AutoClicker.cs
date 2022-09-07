using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace testAutoclicker
{
    class AutoClicker
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);


        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);


        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


        //mouse controls
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;


        //== Delegate ==/
        // Invoke when simulate click
        Action clickEvent;

        // reference to the UI
        private MainWindow MainWindow;


        public bool isRunning;
        Thread threadAutoClicker;

        public int counter = 0; // number of click
        public int delay = 500; // delay betwenn 2 click 

        public int delayInaccuracy = 0; // delay error in %
        public int positionInaccuracy = 0;

        //Screen Axis
        struct Axis
        {
            public int x;
            public int y;
        }

        //Constructor 
        public AutoClicker(MainWindow MainWindow)
        {
            this.MainWindow = MainWindow;
            MainWindow.onPressF6 += Start;
            clickEvent += Click;

        }

        //Destructor
        ~AutoClicker()
        {
            clickEvent -= Click;
        }

        //This function is called when we START the autoclicker
        public void Start()
        {
            // switch subscribe
            MainWindow.onPressF6 -= Start;
            MainWindow.onPressF6 += Stop;

            counter = 0;
            MainWindow.StateAutoclicker.Text = "Press F6 to Stop !";

            isRunning = true;
            CalculDelay();

            delayInaccuracy = Int32.Parse(MainWindow.Freq.Text);
            positionInaccuracy = Int32.Parse(MainWindow.Position.Text);


            threadAutoClicker = new Thread(Run); // create a thread to run without lock
            threadAutoClicker.Start();
        }

        //This function is called when we STOP the autoclicker
        public void Stop()
        {
            // switch subscribe
            MainWindow.onPressF6 -= Stop;
            MainWindow.onPressF6 += Start;

            MainWindow.StateAutoclicker.Text = "Press F6 to Run !";

            UpdateUI();
            isRunning = false;
        }


        // LOOP when autoclicker is active
        public void Run()
        {
            while (isRunning && threadAutoClicker.IsAlive)
            {
                Click();
                Thread.Sleep(GetInaccuracyDelay(delay));
            }


        }


        private void Click()
        {
            MainWindow.Dispatcher.BeginInvoke(() =>
            {
                if (isRunning)
                {
                    MouseLeftClick();
                    UpdateUI();

                }
            });
        }



        private void UpdateUI()
        {
            MainWindow.numberOfClick.Text = counter.ToString();
        }

        public int GetInaccuracyDelay(int initialDelay)
        {
            if (delayInaccuracy == 0) return initialDelay;

            Random rnd = new Random();
            int finalDelay;
            int coeff = (delayInaccuracy / 100);

            int min = initialDelay * (1 - coeff);
            if (min <= 0) min = 0;

            int max = initialDelay * (1 + coeff);

            finalDelay = rnd.Next(rnd.Next(min, max));

            return finalDelay;
        }

        private void MouseLeftClick()
        {
            Axis tmp = GetMousePosition();
            int finalX = tmp.x;
            int finalY = tmp.y;

            if (positionInaccuracy != 0)
            {

                int coeff = (positionInaccuracy / 100);
                

                Random rnd = new Random();
                int min = tmp.x * (1 - coeff);
                if (min <= 0) min = 0;

                int max = tmp.x * (1 + coeff);

                finalX = rnd.Next(rnd.Next(min, max));


                min = tmp.y * (1 - coeff);
                if (min <= 0) min = 0;

                max = tmp.y * (1 + coeff);

                finalY = rnd.Next(rnd.Next(min, max));

            }
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            counter++;
        }


        //get the mouse position from the user32.dll then cast to INT
        private static  Axis GetMousePosition()
        {
            Axis axis;
            Point pt = new Point();
            GetCursorPos(ref pt);
            axis.x = (int)pt.X;
            axis.y = (int)pt.Y;
            return axis;
        }

        //return delay in ms
        public void CalculDelay()
        {
            int totalDelay = 0;
            int millisecond;
            int second;
            int minute;


            if (MainWindow.MilliSecondField.Text == "")
                millisecond = 0;
            else
                millisecond = Int32.Parse(MainWindow.MilliSecondField.Text);


            if (MainWindow.SecondField.Text == "")
                second = 0;
            else
                second = Int32.Parse(MainWindow.SecondField.Text);


            if (MainWindow.MinuteField.Text == "")
                minute = 0;
            else
                minute = Int32.Parse(MainWindow.MinuteField.Text);

            totalDelay += millisecond;
            totalDelay += second * 1000;
            totalDelay += minute * 1000 * 60;

            delay = totalDelay;

            if (delay < 5)
            {
                delay = 5;
            }
        }
    }
}
