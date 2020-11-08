using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ManageWindow
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public static class AsyncErrorHandler
        {
            public static void HandleException(Exception exception)
            {
                WriteLine(exception);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        //~MainWindow()
        //{
        //    //this.Content = null;
        //} 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/littlegao233/PFShop-CS/releases");
        }
        public static void WriteLine(object content)
        {
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;
            ConsoleColor defaultBackgroundColor = Console.BackgroundColor;
            void ResetConsoleColor()
            {
                Console.ForegroundColor = defaultForegroundColor;
                Console.BackgroundColor = defaultBackgroundColor;
            }
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("PFSHOP");
            Console.ForegroundColor = defaultForegroundColor;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("[WPF] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(content);
            ResetConsoleColor();
        }
        private void MaterialWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.SetWindowSize(Console.WindowWidth, 2);
            WriteLine("窗體加載成功！(窗体打开后控制台会无法操作，关闭窗体即可恢复)");
            Console.Title = "窗体打开后控制台会无法操作，关闭窗体即可恢复";
        }
        private string title = Console.Title;
        private void MaterialWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.Title = title;
            WriteLine("窗體已關閉...");
        }
    }
}
