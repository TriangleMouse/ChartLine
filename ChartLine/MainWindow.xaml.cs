using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Forms.DataVisualization.Charting;
using System.ComponentModel;


namespace ChartLine
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public class XY
    {
        
        public double X { get; set; }
        public double Y { get; set; }

        public static XY FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(';');
            XY xy = new XY();
            xy.X = Convert.ToDouble(values[0]);
            xy.Y = Convert.ToDouble(values[1]);
            return xy;
        }
    }

    public partial class MainWindow : Window
    {
        public bool Work = false;
        public ObservableCollection<XY> xylist = new ObservableCollection<XY> { 
            new XY {X=0, Y=0}
        };
        public ObservableCollection<XY> xylist1 = new ObservableCollection<XY> {
            new XY {X=0, Y=0}
        };
        public ObservableCollection<XY> xyReversList = new ObservableCollection<XY>();
        public ObservableCollection<XY> xyReversList1 = new ObservableCollection<XY>();
        public bool RevrsVbl = false, RevrsVsbl1 = false;
        public MainWindow()
        {
            InitializeComponent();
            XYGrid.ColumnWidth = 125;
            XYGrid.ItemsSource = xylist;
            

        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

            if (txtX.Text != "" & txtY.Text != "")
            {
                Reverse();
                XY xy = new XY();
                xy.X = Convert.ToDouble(txtX.Text);
                xy.Y = Convert.ToDouble(txtY.Text);
                if (SwitchFunc.IsChecked == false)
                {
                    xylist.Add(xy);
                    XYGrid.ItemsSource = null;
                    XYGrid.ItemsSource = xylist;
                }
                else
                {
                    xylist1.Add(xy);
                    XYGrid.ItemsSource = null;
                    XYGrid.ItemsSource = xylist1;
                }
                
                txtX.Text = "";
                txtY.Text = "";
                Work = false;
            }
            else
            {
              System.Windows.Forms.MessageBox.Show("Значение Х или У не введено");
            }
           
       
        }
        //Обновление графика(рисование по точкам)
        private void UpdChartBtn_Click(object sender, RoutedEventArgs e)
        {
            int n;
          
            if (SwitchFunc.IsChecked == false)
                n = 0;
            else
                n = 2;

            ((LineSeries)mcChart.Series[n]).ItemsSource = null;
            ((LineSeries)mcChart.Series[n]).ItemsSource = XYGrid.ItemsSource;
            
            
            

        }


        //движение окна
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        
        //Закрыть окно
        private void BtnExit(object sender, RoutedEventArgs e)
        {
            if (Work == false) {
                System.Windows.Forms.MessageBox.Show("Вы не сохранили результат своей работы");
                MessageBoxResult result = System.Windows.MessageBox.Show("Вы хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            else
                this.Close();
        }
        //Свернуть окно
        private void BtnMinimized(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }



        public string ExportDataGrid(bool withHeaders, System.Windows.Controls.DataGrid grid)
        {
            string colPath;
            System.Reflection.PropertyInfo propInfo;
            System.Windows.Data.Binding binding;
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder(Convert.ToString((char)65279));
            System.Collections.IList source = (grid.ItemsSource as System.Collections.IList);
            if (source == null)
                return "";

            foreach (Object data in source)
            {
                List<string> csvRow = new List<string>();
                foreach (DataGridColumn col in grid.Columns)
                {
                    if (col is DataGridBoundColumn)
                    {
                        binding = (System.Windows.Data.Binding)(col as DataGridBoundColumn).Binding;
                        colPath = binding.Path.Path;
                        propInfo = data.GetType().GetProperty(colPath);
                        if (propInfo != null)
                        {
                            csvRow.Add(propInfo.GetValue(data, null).ToString());
                        }
                    }
                }
                strBuilder
                    .Append(String.Join(";", csvRow.ToArray()))
                    .Append("\r\n");
            }


            return strBuilder.ToString();
        }

        //Соххранение данных
        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            string data = ExportDataGrid(true, XYGrid);
            SaveFileDialog sfd = new SaveFileDialog()
            {
                DefaultExt = "csv",
                Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
                FilterIndex = 1
            };
            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = sfd.OpenFile())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(data);
                        writer.Close();
                        
                    }
                    stream.Close();
                    Work = true;
                }
            }
        }


        public List<XY> xes = new List<XY>();
        //Загрузка данных
        private void Load_BtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "CSV documents (.csv)|*.csv";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                xes.Clear();
                string filename = openFileDlg.FileName;
                XYGrid.ItemsSource = null;
                xes = File.ReadAllLines(filename).Select(v => XY.FromCsv(v)).ToList();
                if (SwitchFunc.IsChecked==false)
                {
                    xylist.Clear();
                    foreach (XY xy in xes)
                    {
                        xylist.Add(xy);
                    }
                    XYGrid.ItemsSource = xylist;
                }
                else
                {
                    xylist1.Clear();
                    foreach (XY xy in xes)
                    {
                        xylist1.Add(xy);
                    }
                    XYGrid.ItemsSource = xylist1;
                }
                
                Reverse();
                Work = false;
            }
            
        }

        //обратная функция
        public void Reverse()
        {
            if (SwitchFunc.IsChecked == false)
            {
                xyReversList.Clear();
                foreach (XY xy in xylist)
                {
                    xyReversList.Add(new XY { X = xy.Y, Y = xy.X });
                   
                } 
                if (RevrsVbl==true) {((LineSeries)mcChart.Series[1]).ItemsSource = null; ((LineSeries)mcChart.Series[1]).ItemsSource = xyReversList;}
            }
            else
            {
                xyReversList1.Clear();
                foreach (XY xy in xylist1)
                {
                    xyReversList1.Add(new XY { X = xy.Y, Y = xy.X });
                }
                if (RevrsVsbl1 == true) { ((LineSeries)mcChart.Series[3]).ItemsSource = null; ((LineSeries)mcChart.Series[3]).ItemsSource = xyReversList; }
            }
            
        }
        //
        private void ReverseFuncBTN_Click(object sender, RoutedEventArgs e)
        {

            Reverse();
            if (SwitchFunc.IsChecked == false)
            {
                ((LineSeries)mcChart.Series[1]).ItemsSource = null;
                ((LineSeries)mcChart.Series[1]).ItemsSource = xyReversList;
                RevrsVbl = true;
            }
            else
            {
                ((LineSeries)mcChart.Series[3]).ItemsSource = null;
                ((LineSeries)mcChart.Series[3]).ItemsSource = xyReversList1;
                RevrsVsbl1 = true;
            }
        }

        //ограничения на ввод
        private void txtY_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ",") && (!txtY.Text.Contains(",") && txtY.Text.Length != 0) || (e.Text == "-") && (!txtY.Text.Contains("-") && txtY.Text.Length == 0)))
            {
                e.Handled = true;
            }
        }

        private void txtX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ",") && (!txtX.Text.Contains(",") && txtX.Text.Length != 0) || (e.Text == "-") && (!txtX.Text.Contains("-") && txtX.Text.Length == 0)))
            {
                e.Handled = true;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ",")  || (e.Text == "-") ))
            {
                e.Handled = true;
                
            }
        }


        private void SwitchFunc_Click(object sender, RoutedEventArgs e)
        {
            XYGrid.ItemsSource = null;
            if (SwitchFunc.IsChecked == false)
                XYGrid.ItemsSource = xylist;
            else
                XYGrid.ItemsSource = xylist1;
        }


      
    }
}
