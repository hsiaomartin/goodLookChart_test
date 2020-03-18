﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using LiveCharts;
using LiveCharts.Wpf;
//using myRegularExp;
namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private Queue<double> dataQueue = new Queue<double>(20);
        private bool serialPort_is_open = false;
        private bool time_tick_is_open = false;
        private int curValue = 0;
        private int label_counter = 0;
        private int num = 1;//每次刪除增加幾個點 
        LineSeries myLineSeries;
        ChartValues<double> Values = new ChartValues<double> { };
        // delegate void runCode();
        public Form1()
        {
            InitializeComponent();
            this.timer2.Start();
            InitChart();
            CustomizedLineSeries_Load();
        }
        private void myButtonClicked(object sender, EventArgs e)
        {
            if (!time_tick_is_open)
            {
                this.timer1.Start();
                myBtn.Text = "Time Stop";
                serialPort_textBox.Text += "Time Start！\r\n";
            }
            else
            {
                this.timer1.Stop();
                myBtn.Text = "Time Start";
                serialPort_textBox.Text += "Time Stop！\r\n";
            }
            serialPort_textBox.SelectionStart = serialPort_textBox.Text.Length;
            serialPort_textBox.ScrollToCaret();
            time_tick_is_open = !time_tick_is_open;

        }
        private void labelChange(object sender, EventArgs e) {   //label click event
           // label_counter += 1;
         //  label1. Text = label_counter.ToString();
          //  label1.Text = this.timer1.Interval.ToString();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.timer1.Stop();


            UpdateChart();

            this.timer1.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            label_counter += this.timer1.Interval/100;
            label1.Text = label_counter.ToString() + " s";
            TimeToolStripStatusLabel2.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void InitChart()
        {
            //定義圖表區域
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("ChartArea 1");
            this.chart1.ChartAreas.Add(chartArea1);
            //定義儲存和顯示點的容器
            this.chart1.Series.Clear();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series("Series 1");
            series1.ChartArea = "ChartArea 1";
            series1.ChartType = SeriesChartType.Line;
            series1.IsValueShownAsLabel = false;
            this.chart1.Series.Add(series1);
            //設定圖表顯示樣式
            this.chart1.ChartAreas[0].AxisY.Minimum = 17;
            this.chart1.ChartAreas[0].AxisY.Maximum = 30;
            this.chart1.ChartAreas[0].AxisX.Interval = 1;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //設定標題
            this.chart1.Titles.Clear();
            this.chart1.Titles.Add("S01");
            this.chart1.Titles[0].Text = "顯示";
            this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //設定圖表顯示樣式
            this.chart1.Series[0].Color = Color.Red;

            if (rb1.Checked)
            {
                this.chart1.Titles[0].Text = string.Format(" {0} 顯示", rb1.Text);
                this.chart1.Series[0].ChartType = SeriesChartType.Line;

            }
            if (rb2.Checked)
            {
                this.chart1.Titles[0].Text = string.Format(" {0} 顯示", rb2.Text);
                this.chart1.Series[0].ChartType = SeriesChartType.Spline;
            }
            this.chart1.Series[0].Points.Clear();
        }

        private void UpdateQueueValue()
        {
            
            if (dataQueue.Count > 20)
            {
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue.Dequeue();
                }
            }
            if (rb1.Checked)
            {
                this.chart1.Titles[0].Text = string.Format(" {0} 顯示", rb1.Text);
                
                Random r = new Random();
                for (int i = 0; i < num; i++)
                {
                    dataQueue.Enqueue(r.Next(20, 30));
                }

            }
            if (rb2.Checked)
            {
                this.chart1.Titles[0].Text = string.Format(" {0} 顯示", rb2.Text);
                
                for (int i = 0; i < num; i++)
                {
                    //對curValue只取[0,360]之間的值
                    curValue = curValue % 360;
                    //對得到的正弦值，放大20倍，並上移20
                    dataQueue.Enqueue((5 * Math.Sin(curValue * Math.PI / 180)) + 23);
                    curValue = curValue + 10;
                }
            }




            if (temp_radioButton.Checked) {
                this.chart1.Titles[0].Text = string.Format("{0}顯示","溫度");
               
                string indata = mySerialPort.ReadLine();
                double indata_r=0;
               // backgroundWorker1.RunWorkerAsync(indata);
                Invoke(new Action<string>(
                    (s) =>
                    {

                           if (Regex.IsMatch(s, myRegularExp.RegularExp.UnMinusFloat))
                          {
                            
                            indata_r = Math.Round(double.Parse(s),2);
                            serialPort_textBox.Text += s + " °C\r\n";
                            serialPort_textBox.SelectionStart = serialPort_textBox.Text.Length;
                            serialPort_textBox.ScrollToCaret();
                          }
                    }
                    ), indata);
                    for (int i = 0; i < num; i++)
                    {
                        dataQueue.Enqueue(indata_r);
                    }
                }
            

        }

        private void UpdateChart() {

            UpdateQueueValue();
            this.chart1.Series[0].Points.Clear();
            Values.Clear();
            for (int i = 0; i < dataQueue.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY((i + 1), dataQueue.ElementAt(i));
                Values.Add(dataQueue.ElementAt(i));
            }
            myLineSeries.Values = Values;


        }

        //    private void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        //{


        //    //SerialPort sp = (SerialPort)sender;
        //    //string indata = sp.ReadExisting();
        //    //string indata = mySerialPort.ReadLine() ;
        //    ////string RegularExpressions = "^(([0-9]+\\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\\.[0-9]+)|([0-9]*[1-9][0-9]*))$";
        //    ////textBox1.Text = indata;
        //    //Invoke(new Action<string>(
        //    //        (s) =>
        //    //        {    //if (indata== RegularExpressions)
        //    //            // {
        //    //                    serialPort_textBox.Text += s + " °C\r\n";

        //    //                    serialPort_textBox.SelectionStart = serialPort_textBox.Text.Length;

        //    //                    serialPort_textBox.ScrollToCaret();
        //    //           // }
        //    //        }
        //    //    ), indata);
            




        //}

        private void serialPort_button_Click(object sender, EventArgs e)
        {
            if (!serialPort_is_open)
            {
                serialPort_is_open = true;
                serialPort_button.Text = "Serial Close";
                this.mySerialPort.Open();
                serialPort_textBox.Text += "Serial connected！\r\n";
                serialPort_textBox.SelectionStart = serialPort_textBox.Text.Length;
                serialPort_textBox.ScrollToCaret();
                temp_radioButton.Enabled = true;
            }
            else
            {
                serialPort_is_open = false;
                serialPort_button.Text = "Serial Open";
                this.mySerialPort.Close();
                serialPort_textBox.Text += "Serial disconnected！\r\n";
                serialPort_textBox.SelectionStart = serialPort_textBox.Text.Length;
                serialPort_textBox.ScrollToCaret();
                rb1.Checked = true;
                temp_radioButton.Checked = false;
                temp_radioButton.Enabled = false;
            }
                
        }







        private void CustomizedLineSeries_Load()
        {
            myLineSeries = new LineSeries() { 
                Values = new ChartValues<double> { 5, 3, 5, 7, 3, 9 },
                StrokeThickness = 2,
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 142, 196)),
                Fill = System.Windows.Media.Brushes.Transparent,
                LineSmoothness = 1,
                PointGeometrySize = 8,
                PointForeground =
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 46, 49))
                
            };
            
            cartesianChart1.Series.Add(myLineSeries);
            cartesianChart1.DisableAnimations = true;
            cartesianChart1.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 46, 49));
            //cartesianChart1.AnimationsSpeed = TimeSpan.FromMilliseconds(50);
            cartesianChart1.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                IsMerged = true,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    StrokeDashArray = new System.Windows.Media.DoubleCollection(2),
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
        }


    }



}


