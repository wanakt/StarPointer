using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;//스크린 캡춰
using System.IO;

using System.Runtime.InteropServices;//마우스 제어
using System.Threading;

namespace StarPointer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        int[,] brightnessArray = new int[120, 120];

        int maxValue = 0;
        int maxValueXPosition = 0;
        int maxValueYPosition = 0;
        int starXPosition = 0;
        int starYPosition = 0;
        int WindowXPosition = 0;
        int WindowYPosition = 0;

        Boolean register = false;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public MainWindow()
        {
            InitializeComponent();

            string maxData = "MaxValue : " + "\n";
            maxData += "XPosition :        , " + "YPosition : ";
            tbData.Text = maxData;
        }

        private void Button_AutoSelect_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)this.FindName("rectStar");
            if (rectangle != null)
            {
                removeImage();
                return;
            }

            maxValue = 0;

            WindowXPosition = Convert.ToInt32(Application.Current.MainWindow.Left);
            WindowYPosition = Convert.ToInt32(Application.Current.MainWindow.Top);

            // 가로 120, 세로 120의 빈 이미지 파일 생성
            Bitmap bmp = new Bitmap(120, 120, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gr = Graphics.FromImage(bmp);

            // 화면을 카피해서 Bitmap에 저장
            gr.CopyFromScreen(WindowXPosition + 10, WindowYPosition + 50, 0, 0, bmp.Size);

            for (int y = 0; y < 120; y++)
            {
                for (int x = 0; x < 120; x++)
                {
                    System.Drawing.Color color = bmp.GetPixel(x, y);
                    int brightness = color.R;
                    
                    brightnessArray[x, y] = brightness;//흑백이미지인 경우
                    //brightnessArray[x, y] = (int)((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)); //컬러이미지인 경우, 밝기만 취득

                    if (brightness > maxValue)
                    {
                        maxValue = brightness;
                        maxValueXPosition = x;
                        maxValueYPosition = y;
                    }
                }
            }

            starXPosition = WindowXPosition + 10 + maxValueXPosition;
            starYPosition = WindowYPosition + 50 + maxValueYPosition;

            string maxData = "MaxValue : " + maxValue.ToString() + "\n";
            maxData += "XPosition : " + starXPosition.ToString() + ", " + "YPosition : " + starYPosition.ToString();
            tbData.Text = maxData;

            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
 
                imStar.Source = bitmapImage;

                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Name = "rectStar";
                rect.Width = 10;
                rect.Height = 10;
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.Margin = new Thickness(maxValueXPosition-5, maxValueYPosition-5, 0, 0);
                rect.Fill = System.Windows.Media.Brushes.Transparent;
                rect.StrokeThickness = 1;
                rect.Stroke = System.Windows.Media.Brushes.Red;

                Grid.SetRow(rect, 1);
                Grid.SetColumn(rect, 1);
                grImage.Children.Add(rect);
                
                
                if (register == false) RegisterName("rectStar", rect);
                register = true;
            }

            bmp.Dispose();
            gr.Dispose();

            Hide();
            
            System.Drawing.Point mousePosition = new System.Drawing.Point(starXPosition, starYPosition);
            SetCursorPos(starXPosition, starYPosition);
            sendMouseDoubleClick(mousePosition);

            btData.IsEnabled = true;

            Show();
        }

        private void removeImage()
        {
            if (imStar.Source != null) imStar.Source = null;

            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)this.FindName("rectStar");
            if (rectangle != null)
            {
                grImage.Children.Remove(rectangle);
                UnregisterName("rectStar");
                register = false;
            }
        }

        private void Button_Data_Click(object sender, RoutedEventArgs e)
        {
            DataWindow dataWindow = new DataWindow(brightnessArray, maxValue, WindowXPosition + 10, WindowYPosition + 50, maxValueXPosition, maxValueYPosition);
            dataWindow.Show();
        }

        private void Window_Deactivated(object sender, EventArgs e)//다른 프로그램이 전체 화면으로 실행되는 경우에도 항상 위로 오게 설정
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (imStar.Source != null) removeImage();
                btData.IsEnabled = false;
                DragMove();
            }
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        void sendMouseDoubleClick(System.Drawing.Point p)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
