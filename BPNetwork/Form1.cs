using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPNetwork
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
            this.btnTest.Enabled = false;
            this.btnTrain.Enabled = false;
            this.txtLearnRate.Text = 0.3.ToString();
            //窗口固定大小
            this.MaximizeBox = false;//最大化按钮隐藏
            this.MinimizeBox = false;//最小化按钮隐藏
            this.FormBorderStyle = FormBorderStyle.FixedSingle;//不支持鼠标拖动

            this.lblMessage.Text = "请先载入测试或训练样本";
        }

        /// <summary>
        /// 训练按钮是否已经点击过一次
        /// </summary>
        private static int flag = 0;
        /// <summary>
        /// 训练文件是否已打开
        /// </summary>
        private static int flag2 = 0;
        /// <summary>
        /// 测试文件是否已打开
        /// </summary>
        private static int flag3 = 0;
        private BackgroundWorker bw;

        private void btnTrain_Click(object sender, EventArgs e)
        {
            if (flag2 != 0)//测试文件和训练文件都已经选中
            {
                if (flag == 0)
                {
                    bw = new BackgroundWorker();
                    bw.DoWork += Bw_DoWork;
                    bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                    bw.WorkerSupportsCancellation = true;//1.支持取消操作

                    bw.RunWorkerAsync();
                    this.btnTrain.Text = "停止";
                    flag = 1;
                }
                else
                {
                    this.btnTrain.Text = "训练";
                    bw.CancelAsync();
                    flag = 0;
                }
            }
            else
            {
                MessageBox.Show("请点击文件，选择要训练文件所在的目录！", "文件未载入");
                flag2 = 0;
            }
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Show();//隐藏窗体
            MessageBox.Show("训练成功", "提示");
            this.btnTrain.Text = "训练";
            flag = 0;
        }

        /// <summary>
        /// 训练样本的目录
        /// </summary>
        private static string train_path;
        /// <summary>
        /// 测试样本的目录
        /// </summary>
        private static string test_path;

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                double[] tmp = new double[20];
                //学习率
                double lr = Double.Parse(this.txtLearnRate.Text.Trim());

                //定义BP神经网络类
                BpNet bp = new BpNet(400, 10);

                //数据字典
                Dictionary<string, int> filedictionary = new Dictionary<string, int>();

                for (int i = 0; i < 10; i++)
                {
                    string dir = train_path + @"\" + i + @"\";
                    string[] files = Directory.GetFiles(dir);
                    foreach (string item in files)
                    {
                        filedictionary.Add(item, i);
                    }
                }

                //声明数据存储区域
                double[,] input = new double[filedictionary.Count, 400];
                double[,] output = new double[filedictionary.Count, 10];

                int count = 0;//计数器
                int study = 0;//学习（训练）次数

                //数据装载
                foreach (KeyValuePair<string, int> item in filedictionary)
                {
                    Bitmap bmp = new Bitmap(item.Key);

                    for (int k = 0; k < bmp.Height; k++)
                    {
                        for (int l = 0; l < bmp.Width; l++)
                        {
                            input[count, k * bmp.Width + l] = bmp.GetPixel(l, k).R;
                        }
                    }

                    //交换行，因为位图存储时，先存储最后一行，从图片的底部开始，逐渐向上扫描
                    for (int k = 0; k < bmp.Height / 2; k++)
                    {
                        for (int l = 0; l < bmp.Width; l++)
                        {
                            tmp[l] = input[count, k * bmp.Width + l];
                            input[count, k * bmp.Width + l] = input[count, (bmp.Height - 1 - k) * bmp.Width + l];
                            input[count, (bmp.Height - 1 - k) * bmp.Width + l] = tmp[l];
                        }
                    }

                    output[count, item.Value] = 1;//第j个图片被分为第i类
                    count++;
                }

                do
                {
                    if (!bw.CancellationPending)//2.检测用户是否取消
                    {
                        //训练
                        bp.train(input, output, lr);
                        study++;
                        this.lblMessage.Text = "第" + study + "次训练的误差： " + bp.e;
                    }
                    else
                    {
                        break;//停止训练
                    }
                } while (bp.e > 0.01 && study < 50000);

                bp.saveMatrix(bp.w, "w.txt");
                bp.saveMatrix(bp.v, "v.txt");
                bp.saveMatrix(bp.b1, "b1.txt");
                bp.saveMatrix(bp.b2, "b2.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("出错了" + ex.Message);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (flag3 != 0 && File.Exists("w.txt") && File.Exists("v.txt") && File.Exists("b1.txt") && File.Exists("b2.txt"))
            {
                //清空已有训练结果
                this.lbTestResult.Items.Clear();
                BackgroundWorker bw1 = new BackgroundWorker();

                bw1.DoWork += Bw1_DoWork;

                bw1.RunWorkerAsync();
                flag3 = 1;
            }
            else
            {
                MessageBox.Show("请点击文件，选择要测试文件所在的目录！", "文件未载入");
                flag3 = 0;
            }
        }

        private void Bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //定义BP神经网络类
                BpNet bp = new BpNet(400, 10);

                int right_count = 0;
                string[] files;
                double[] tmp = new double[20];
                //读取文件
                for (int i = 0; i < 10; i++)
                {
                    right_count = 0;
                    string dir = test_path + @"\" + i + @"\";
                    files = Directory.GetFiles(dir);

                    //共files.Length个样本，每个样本数据有400个字节
                    double[] input = new double[400];
                    double[] output = new double[10];

                    for (int j = 0; j < files.Length; j++)
                    {
                        Bitmap bmp = new Bitmap(files[j]);

                        for (int k = 0; k < bmp.Height; k++)
                        {
                            for (int l = 0; l < bmp.Width; l++)
                            {
                                input[k * bmp.Width + l] = bmp.GetPixel(l, k).R;
                            }
                        }

                        //交换行，因为位图存储时，先存储最后一行，从图片的底部开始，逐渐向上扫描
                        for (int k = 0; k < bmp.Height / 2; k++)
                        {
                            for (int l = 0; l < bmp.Width; l++)
                            {
                                tmp[l] = input[k * bmp.Width + l];
                                input[k * bmp.Width + l] = input[(bmp.Height - 1 - k) * bmp.Width + l];
                                input[(bmp.Height - 1 - k) * bmp.Width + l] = tmp[l];
                            }
                        }

                        if (i == bp.test(input))
                        {
                            right_count++;
                        }
                    }

                    this.lbTestResult.Items.Add(files.Length + "个" + i + "样本识别成功率：" + (1.0 * right_count / files.Length * 100).ToString("0.00") + "%");
                }
                this.lblMessage.Text = "训练成功";
            }
            catch (Exception ex)
            {
                MessageBox.Show("出错了" + ex.Message);
            }
        }

        private void menuOpenTrain_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            train_path = path.SelectedPath;
            flag2 = 1;
            this.btnTrain.Enabled = true;
            this.lblMessage.Text = "训练样本载入成功";
        }

        private void menuOpenTest_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            test_path = path.SelectedPath;
            flag3 = 1;
            this.btnTest.Enabled = true;
            this.lblMessage.Text = "测试样本载入成功";
        }

        private static int flag1 = 0;
        private void menuStay_Click(object sender, EventArgs e)
        {
            if (flag1 == 0)
            {
                //窗口置顶
                this.TopMost = true;
                this.menuStay.Text = "取消窗口保持在前";
                flag1 = 1;
            }
            else
            {
                //取消窗口置顶
                this.TopMost = false;
                this.menuStay.Text = "窗口保持在前";
                flag1 = 0;
            }
        }

        private void menuRunBackground_Click(object sender, EventArgs e)
        {
            this.Hide();//隐藏窗体
        }
    }
}
