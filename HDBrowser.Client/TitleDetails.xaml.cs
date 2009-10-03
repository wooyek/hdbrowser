using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HDBrowser.Core.Integration.MediaIndex.FileSystem;
using HDBrowser.Core.Integration.MediaNfoSources.Imdb;
using HDBrowser.Core.Model;

namespace HDBrowser.Client
{
    /// <summary>
    /// Interaction logic for TitleDetails.xaml
    /// </summary>
    public partial class TitleDetails : UserControl
    {
        public TitleDetails()
        {
            InitializeComponent();
        }

        private void search_Click(object sender, RoutedEventArgs e) {
            FileEntry title = (FileEntry) DataContext;
            string q = title.GetInfoBasedOnFile().Name;
            List<TitleInfo> infos = Imdb.SearchDirty(q);
            list.ItemsSource = infos;
        }

        private void List_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            FileEntry title = (FileEntry) DataContext;
            TitleInfo info = (TitleInfo) list.SelectedItem;
            title.Info.Url = info.Url;
            title.Update(true);
        }
    }
}
