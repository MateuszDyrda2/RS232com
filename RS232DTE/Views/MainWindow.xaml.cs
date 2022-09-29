using RS232DTE.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RS232DTE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Window> otherWindows = new List<Window>();

        /// <summary>
        /// Main program window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainPageLoaded;
            Closing += StopAllOther;
        }

        /// <summary>
        /// Fills the main window with a page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ags"></param>
        /// </summary>
        /// <param name="target"></param>
        private void MainPageLoaded(object sender, RoutedEventArgs e) => NewWindow(Frame1);

        /// <summary>
        /// Navigate to a new page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ags"></param>
        /// </summary>
        /// <param name="target"></param>
        private static void NewWindow(Frame target) => target.Navigate(new RSWindow());

        /// <summary>
        /// Add a new window on a separate thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewWindow(object sender, RoutedEventArgs e)
        {
            var newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        /// <summary>
        /// Thread starting point
        /// </summary>
        private void ThreadStartingPoint()
        {
            var tempWindow = new Window();
            var f = new Frame();
            tempWindow.Content = f;
            NewWindow(f);
            tempWindow.Show();
            otherWindows.Add(tempWindow);
            Dispatcher.Run();
        }

        /// <summary>
        /// As a response to program closing, ends all other used threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopAllOther(object? sender, EventArgs e)
        {
            foreach (var window in otherWindows)
            {
                window.Dispatcher.Invoke(
                    DispatcherPriority.Normal, new ThreadStart(window.Close));
            }
        }
    }
}
