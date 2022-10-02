using System.Windows;


namespace StarPointer
{
    /// <summary>
    /// DataWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataWindow(int[,] brightnessArray, int maxValue, int imageXPosition, int imageYPosition, int maxValueXPosition, int maxValueYPosition)
        {
            InitializeComponent();
            MakeTable(brightnessArray, maxValue, imageXPosition, imageYPosition, maxValueXPosition, maxValueYPosition);
        }

        private void MakeTable(int[,] brightnessArray, int maxValue, int imageXPosition, int imageYPosition, int maxValueXPosition, int maxValueYPosition)
        {

            string row = "\t" + "\t";

            for (int i = 0; i < 120; i++)
            {
                row += (imageXPosition + i).ToString() + "\t";
            }
            row += "\n";
            row += "\n";

            for (int i = 0; i < 700; i++)
            {
                row += "=";
            }
            row += "\n";

            for (int y = 0; y < 120; y++)
            {
                row += (imageYPosition + y).ToString() + "\t" + "|" + "\t";

                for (int x = 0; x < 120; x++)
                {
                    int brightness = brightnessArray[x, y];
                    row += brightness.ToString() + "\t";
                }
                row += "\n";
            }

            tbViewer.Text = row;

            lbMaxValue.Content = "MaxValue :" + maxValue.ToString();
            lbMaxPositionX.Content = "MaxValue XPosition :" + (imageXPosition + maxValueXPosition).ToString();
            lbMaxPositionY.Content = "MaxValue XPosition :" + (imageYPosition + maxValueYPosition).ToString();
        }
    }
}
