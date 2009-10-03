using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HDBrowser.Core.Integration.MediaIndex.FileSystem;
using HDBrowser.Core.Model;
using log4net;

namespace HDBrowser.Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly ILog log = LogManager.GetLogger(typeof (MainWindow));
        private FileSystemMediaIndex index;

        public MainWindow() {
            InitializeComponent();
            try {
                index = FileSystemMediaIndex.Intialize();                
                index.CheckForNewMovies(true);
                titleList.ItemsSource = index.Titles;
            } catch (Exception e) {
                throw new Exception("Loafing index failed", e);
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            index.Store();
        }

        private void cover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2) {
                TitleDetailsWindow w = new TitleDetailsWindow();
                w.DataContext = titleList.SelectedItem;
                
                w.ShowDialog();
            }
        }
    }
}