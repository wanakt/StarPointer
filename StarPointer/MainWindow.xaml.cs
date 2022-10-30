using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;//스크린 캡춰
using System.IO;

using System.Runtime.InteropServices;//마우스 제어
using System.Threading;
using Microsoft.Win32;
using System.Windows.Markup;

namespace StarPointer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        int maxValue = 0;
        int maxValueXPosition = 0;
        int maxValueYPosition = 0;
        
        Boolean capturing = false;
        Boolean register = false;

        Bitmap? captured_bmp;

        public static BitmapImage captured_BitmapImage;
        public static decimal scaleFactor = 0;
        public static int starXPosition = 0;
        public static int starYPosition = 0;
        public static int WindowXPosition = 0;
        public static int WindowYPosition = 0;

        int physicalScreenWidth;
        int physicalScreenHeight;
        int logicalScreenWidth;
        int logicalScreenHeight;

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

            //물리적 디스플레이 해상도
            physicalScreenWidth = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
            physicalScreenHeight = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            //논리적 디스플레이 해상도. 따라서 디스플레이 배율이 100%가 아닌 경우 물리적 디스플레이 해상도와 상이함
            logicalScreenWidth = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
            logicalScreenHeight = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);

            scaleFactor = (Decimal)physicalScreenWidth / (Decimal)logicalScreenWidth;
        }

        private void Button_AutoSelect_Click(object sender, RoutedEventArgs e)
        {
            capturing = true;

            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)this.FindName("rectStar");
            
            if (rectangle != null)
            {
                removeImage();
                return;
            }

            maxValue = 0;

            WindowXPosition = Convert.ToInt32(Convert.ToDecimal(Application.Current.MainWindow.Left) * scaleFactor);
            WindowYPosition = Convert.ToInt32(Convert.ToDecimal(Application.Current.MainWindow.Top) * scaleFactor);

            int imageSize = Convert.ToInt32(120 * scaleFactor);
            int imageXPosition = WindowXPosition + Convert.ToInt32(10 * scaleFactor);
            int imageYPosition = WindowYPosition + Convert.ToInt32(50 * scaleFactor);

            // 스케일을 고려하여 가로 120, 세로 120의 빈 이미지 파일 생성
            Bitmap bmp = new Bitmap(imageSize, imageSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gr = Graphics.FromImage(bmp);

            // 화면을 카피해서 Bitmap에 저장
            gr.CopyFromScreen(imageXPosition, imageYPosition, 0, 0, bmp.Size);

            // 가장 밝은 지점을 확인
            int[,] brightnessArray = new int[imageSize, imageSize];
            for (int y = 0; y < imageSize; y++)
            {
                for (int x = 0; x < imageSize; x++)
                {
                    Color color = bmp.GetPixel(x, y);
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

            starXPosition = imageXPosition + maxValueXPosition;
            starYPosition = imageYPosition + maxValueYPosition;

            int rectangleXPosition = Convert.ToInt32(maxValueXPosition * (1 / scaleFactor)) - 5;
            int rectangleYPosition = Convert.ToInt32(maxValueYPosition * (1 / scaleFactor)) - 5;

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
                rect.Margin = new Thickness(rectangleXPosition, rectangleYPosition, 0, 0);
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

            Show();
            capturing = false;
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
            //DataWindow dataWindow = new DataWindow(brightnessArray, maxValue, WindowXPosition + 10, WindowYPosition + 50, maxValueXPosition, maxValueYPosition);
            //dataWindow.Show();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (imStar.Source != null && capturing == false) removeImage();
        }

        private void Button_Manual_Select_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            physicalScreenWidth = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);
            physicalScreenHeight = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            logicalScreenWidth = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
            logicalScreenHeight = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);

            using (Bitmap bmp = new Bitmap(physicalScreenWidth, physicalScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                
                ////화면 밝기 조정 - 간단한지만 속도가 느림
                //for (int y = 0; y < physicalScreenHeight; y++)
                //{
                //    for (int x = 0; x < physicalScreenWidth; x++)
                //    {
                //        Color color = bmp.GetPixel(x, y);

                //        int r = Math.Min(255, color.R + 80);
                //        int g = Math.Min(255, color.G + 80);
                //        int b = Math.Min(255, color.B + 80);

                //        color = Color.FromArgb(r, g, b);
                //        bmp.SetPixel(x, y, color);
                //    }
                //}

                //완성된 Bitmap 파일을 BitmapImage로 변환
                using (MemoryStream memory = new MemoryStream())
                {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    captured_BitmapImage = new BitmapImage();
                    captured_BitmapImage.BeginInit();
                    captured_BitmapImage.StreamSource = memory;
                    captured_BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    captured_BitmapImage.EndInit();
                }
            }

            //Capture_Window에 수정된 BitmapImage를 표시
            Capture_Window capture_window = new Capture_Window();
            capture_window.Width = logicalScreenWidth;
            capture_window.Height = logicalScreenHeight;
            capture_window.ShowDialog();

            if (starXPosition == 0 && starYPosition == 0)
            {
                Show();
                return;
            }
                

            capturing = true;
            int imageSize = Convert.ToInt32(120 * scaleFactor);
            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)this.FindName("rectStar");

            if (rectangle != null)
            {
                removeImage();
                return;
            }

            this.Left = WindowXPosition - 70;
            this.Top = WindowYPosition - 110;

            int imageXPosition = starXPosition - Convert.ToInt32(60 * scaleFactor);
            int imageYPosition = starYPosition - Convert.ToInt32(60 * scaleFactor);

            // 스케일을 고려하여 가로 120, 세로 120의 빈 이미지 파일 생성
            using (Bitmap bmp = new Bitmap(imageSize, imageSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                // 화면을 카피해서 Bitmap에 저장
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CopyFromScreen(imageXPosition, imageYPosition, 0, 0, bmp.Size);
                }
                string maxData = "MaxValue : Manual Select" + "\n";
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

                }
            }

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Name = "rectStar";
            rect.Width = 10;
            rect.Height = 10;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.Margin = new Thickness(55, 55, 0, 0);
            rect.Fill = System.Windows.Media.Brushes.Transparent;
            rect.StrokeThickness = 1;
            rect.Stroke = System.Windows.Media.Brushes.Red;

            Grid.SetRow(rect, 1);
            Grid.SetColumn(rect, 1);
            grImage.Children.Add(rect);

            if (register == false) RegisterName("rectStar", rect);
            register = true;

            System.Drawing.Point mousePosition = new System.Drawing.Point(starXPosition, starYPosition);
            SetCursorPos(starXPosition, starYPosition);
            sendMouseDoubleClick(mousePosition);

            Show();
            capturing = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (imStar.Source != null) removeImage();
                DragMove();
            }
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void sendMouseDoubleClick(System.Drawing.Point p)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
            Thread.Sleep(200);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
