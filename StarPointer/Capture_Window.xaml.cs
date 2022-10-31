using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;

using System.Runtime.InteropServices;//마우스 제어
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace StarPointer
{
    /// <summary>
    /// Capture_Window.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Capture_Window : Window
    {

        BitmapImage magnifying_BitmapImage;
        int currentTargetXPosition = MainWindow.WindowXPosition + 70;
        int currentTargetYPosition = MainWindow.WindowYPosition + 110;

        Decimal scaleFactor = MainWindow.scaleFactor;

        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send, Application.Current.Dispatcher);
        Stopwatch stopwatch = new Stopwatch();//DispatcherTimer는 경과시간을 확인할 수 없음. 경과시간 확인위해서는 stopwatch를 별도로 돌려야 함

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public Capture_Window()
        {
            InitializeComponent();
            CapturedImage.Source = MainWindow.captured_BitmapImage;

            int centerOfScreenWidth = (int)SystemParameters.PrimaryScreenWidth / 2;
            int centerOfScreenHeight = (int)SystemParameters.PrimaryScreenHeight / 2;

            Target_Move(currentTargetXPosition, currentTargetYPosition);

            using (Bitmap magnifying_Bmp = new Bitmap(Convert.ToInt32(50 * scaleFactor), Convert.ToInt32(50 * scaleFactor), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(magnifying_Bmp))
                {
                    //현재의 Taget 위치 주변 화면 또는 중앙 화면을 캡춰해서 Bitmap 메모리에 저장
                    gr.CopyFromScreen(Convert.ToInt32((currentTargetXPosition - 25) * scaleFactor), Convert.ToInt32((currentTargetYPosition - 25) * scaleFactor), 0, 0, magnifying_Bmp.Size);
                }

                using (MemoryStream memory = new MemoryStream())
                {
                    magnifying_Bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    magnifying_BitmapImage = new BitmapImage();
                    magnifying_BitmapImage.BeginInit();
                    magnifying_BitmapImage.StreamSource = memory;
                    magnifying_BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    magnifying_BitmapImage.EndInit();
                }
                MagnifyingImage.Source = magnifying_BitmapImage;
            }
        }

        public void Target_Move(int xPosition, int yPosition)
        {
            lineTargetHorizontal.Margin = new Thickness(xPosition - 25, yPosition - 25, 0, 0);
            lineTargetVertical.Margin = new Thickness(xPosition - 25, yPosition - 25, 0, 0);
            rtTarget.Margin = new Thickness(xPosition - 25, yPosition - 25, 0, 0);
        }

        public void magnify(int currentTargetXPositon, int currentTargetYposition)
        {
            using (Bitmap magnifying_Bmp = new Bitmap(Convert.ToInt32(50 * scaleFactor), Convert.ToInt32(50 * scaleFactor), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(magnifying_Bmp))
                {
                    gr.CopyFromScreen(Convert.ToInt32((currentTargetXPositon - 25) * scaleFactor), Convert.ToInt32((currentTargetYposition - 25) * scaleFactor), 0, 0, magnifying_Bmp.Size);
                }

                using (MemoryStream memory = new MemoryStream())
                {
                    magnifying_Bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    magnifying_BitmapImage = new BitmapImage();//BeginInit를 여러차레 반복하려면, 이런식으로 새로운 개체를 만들어 주어야 함
                    magnifying_BitmapImage.BeginInit();
                    magnifying_BitmapImage.StreamSource = memory;
                    magnifying_BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    magnifying_BitmapImage.EndInit();
                }
                MagnifyingImage.Source = magnifying_BitmapImage;
            }
        }

        private void Button_Click_Target_Up(object sender, MouseEventArgs e)
        {
            //DispatchrTimer는 경과시간 확인방법이 없음. 경과시간에 따라서 달리 작동하도록 만드려면 Stopwatch를 함께 이용해야 함
            stopwatch = new Stopwatch();
            stopwatch.Start();

            currentTargetYPosition -= 1;
            Target_Move(currentTargetXPosition, currentTargetYPosition);

            //timer와 stopwatch를 리셋
            //확대 이미지 렌더링 속도가 너무 느려서, timer로 반복할 때에는 이미지 렌더링 생략함. Bitmap 이미지 처리 속도가 느린 것으로 추측됨
            timer = new DispatcherTimer(DispatcherPriority.Send, Application.Current.Dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(80);
            timer.Tick += new EventHandler(Target_Up);

            magnify(currentTargetXPosition, currentTargetYPosition);

            timer.Start();
        }

        private void Target_Up(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > 400 && stopwatch.ElapsedMilliseconds < 1000)
            {
                currentTargetYPosition -= 1;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 1000 && stopwatch.ElapsedMilliseconds < 2000)
            {
                currentTargetYPosition -= 3;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 2000 && stopwatch.ElapsedMilliseconds < 3000)
                {
                currentTargetYPosition -= 10;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 3000)
            {
                currentTargetYPosition -= 20;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
        }

        private void Button_Click_Target_Down(object sender, RoutedEventArgs e)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            currentTargetYPosition += 1;
            Target_Move(currentTargetXPosition, currentTargetYPosition);

            //timer와 stopwatch를 리셋
            //확대 이미지 렌더링 속도가 너무 느려서, timer로 반복할 때에는 이미지 렌더링 생략함. Bitmap 이미지 처리 속도가 느린 것으로 추측됨
            timer = new DispatcherTimer(DispatcherPriority.Send, Application.Current.Dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(80);
            timer.Tick += new EventHandler(Target_Down);

            magnify(currentTargetXPosition, currentTargetYPosition);

            timer.Start();
        }

        private void Target_Down(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > 400 && stopwatch.ElapsedMilliseconds < 1000)
            {
                currentTargetYPosition += 1;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 1000 && stopwatch.ElapsedMilliseconds < 2000)
            {
                currentTargetYPosition += 3;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 2000 && stopwatch.ElapsedMilliseconds < 3000)
            {
                currentTargetYPosition += 10;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 3000)
            {
                currentTargetYPosition += 20;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
        }

        private void Button_Click_Target_Left(object sender, RoutedEventArgs e)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            currentTargetXPosition -= 1;
            Target_Move(currentTargetXPosition, currentTargetYPosition);

            //timer와 stopwatch를 리셋
            //확대 이미지 렌더링 속도가 너무 느려서, timer로 반복할 때에는 이미지 렌더링 생략함. Bitmap 이미지 처리 속도가 느린 것으로 추측됨
            timer = new DispatcherTimer(DispatcherPriority.Send, Application.Current.Dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(80);
            timer.Tick += new EventHandler(Target_Left);

            magnify(currentTargetXPosition, currentTargetYPosition);

            timer.Start();
        }

        private void Target_Left(object sender, EventArgs e)
        {            

            if (stopwatch.ElapsedMilliseconds > 400 && stopwatch.ElapsedMilliseconds < 1000)
            {
                currentTargetXPosition -= 1;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 1000 && stopwatch.ElapsedMilliseconds < 2000)
            {
                currentTargetXPosition -= 3;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 2000 && stopwatch.ElapsedMilliseconds < 3000)
            {
                currentTargetXPosition -= 10;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 3000)
            {
                currentTargetXPosition -= 20;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
        }

        private void Button_Click_Target_Right(object sender, RoutedEventArgs e)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            currentTargetXPosition += 1;
            Target_Move(currentTargetXPosition, currentTargetYPosition);

            //timer와 stopwatch를 리셋
            //확대 이미지 렌더링 속도가 너무 느려서, timer로 반복할 때에는 이미지 렌더링 생략함. Bitmap 이미지 처리 속도가 느린 것으로 추측됨
            timer = new DispatcherTimer(DispatcherPriority.Send, Application.Current.Dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(80);
            timer.Tick += new EventHandler(Target_Right);

            magnify(currentTargetXPosition, currentTargetYPosition);

            timer.Start();
        }

        private void Target_Right(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > 400 && stopwatch.ElapsedMilliseconds < 1000)
            {
                currentTargetXPosition += 1;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 1000 && stopwatch.ElapsedMilliseconds < 2000)
            {
                currentTargetXPosition += 3;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 2000 && stopwatch.ElapsedMilliseconds < 3000)
            {
                currentTargetXPosition += 10;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
            else if (stopwatch.ElapsedMilliseconds >= 3000)
            {
                currentTargetXPosition += 20;
                Target_Move(currentTargetXPosition, currentTargetYPosition);
            }
        }

        private void Repetition_Stop(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            stopwatch.Stop();

            magnify(currentTargetXPosition, currentTargetYPosition);
        }

        private void Button_Select(object sender, RoutedEventArgs e)
        {
            //WinForm의 Control들과 Class들은 물리적 해상도를 사용함. 따라서 디스플레이 비율이 100%가 아니면 scaleFactor를 곱해주어야 제 위치 표시
            MainWindow.starXPosition = Convert.ToInt32(currentTargetXPosition * scaleFactor);
            MainWindow.starYPosition = Convert.ToInt32(currentTargetYPosition * scaleFactor);

            //반면에 WPF의 Control들은 디스플레이 해상도를 그대로 사용함
            MainWindow.WindowXPosition = currentTargetXPosition;
            MainWindow.WindowYPosition = currentTargetYPosition;

            Close();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.starXPosition = 0;
            MainWindow.starYPosition = 0;

            Close();
        }
    }
}
