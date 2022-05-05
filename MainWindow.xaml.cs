using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;
using segyViewer.segyReader;

namespace segyViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFile(object sender, RoutedEventArgs e)
        {
            string[] filePaths;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择文件";
            openFileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "(*.sgy)|*.sgy";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                filePaths = openFileDialog.FileNames;
                foreach (var filePath in filePaths)
                {
                    TreeViewItem treeViewItem = new TreeViewItem();
                    treeViewItem.Header = filePath.Split('\\')[filePath.Split('\\').Length - 1];
                    treeViewItem.ToolTip = filePath;
                    fileTree.Items.Add(treeViewItem);
                }
            }
            else
            {
                filePaths = null;
            }
        }

        private void fileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (fileTree != null)
            {
                TreeViewItem treeViewItem = (TreeViewItem)fileTree.SelectedItem;
                if (treeViewItem != null)
                {
                    string? filePath = treeViewItem.ToolTip.ToString();
                    scottePlot(filePath);
                }
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileTree != null)
            {
                TreeViewItem treeViewItem = (TreeViewItem)fileTree.SelectedItem;
                if (treeViewItem != null)
                {
                    string? filePath = treeViewItem.ToolTip.ToString();
                    scottePlot(filePath);
                }
            }
        }

        private void scottePlot(string filePath)
        {
            this.wpfPlot.Plot.Clear();

            SegyReader segyReader = new SegyReader(filePath);

            double m = 0;

            foreach (Trace trace in segyReader.traces)
            {
                wpfPlot.Plot.AddSignal(mapmaxmin(trace.values, m + 2, m), color: System.Drawing.Color.MediumBlue);
                m += 3;
            }

            this.wpfPlot.Refresh();
        }

        private double[] mapmaxmin(double[] data, double max, double min)
        {
            double[] result = new double[data.Length];
            double Dmin = data.Min();
            double Dmax = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                double temp = (max - min) * (data[i] - Dmin) / (Dmax - Dmin) + min;
                if (double.IsNaN(temp))
                {
                    result[i] = (max + min) / 2;
                }
                else
                {
                    result[i] = temp;
                }
            }
            return result;
        }

        private void fileTreeClearBtn_Click(object sender, RoutedEventArgs e)
        {
            this.fileTree.Items.Clear();
        }
    }
}