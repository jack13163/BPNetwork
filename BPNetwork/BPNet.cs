using System;
using System.IO;
using System.Text;

namespace BPNetwork
{
    /// <summary>  
    /// BpNet 的摘要说明。  
    /// </summary>  
    public class BpNet
    {
        /// <summary>
        /// 输入节点数
        /// </summary>
        public int inNum;  
        /// <summary>
        /// 隐层节点数
        /// </summary>
        int hideNum;  
        /// <summary>
        /// 输出层节点数  
        /// </summary>
        public int outNum;
        /// <summary>
        /// 样本总数
        /// </summary>
        public int sampleNum;  

        Random R;
        /// <summary>
        /// 输入节点的输入(输出)数据  
        /// </summary>
        double[] x;
        /// <summary>
        /// 隐层节点的输出  
        /// </summary>
        double[] x1;
        /// <summary>
        /// 输出节点的输出  
        /// </summary>
        double[] x2;
        /// <summary>
        /// 隐层的输入  
        /// </summary>
        double[] o1;
        /// <summary>
        /// 输出层的输入  
        /// </summary>
        double[] o2;
        /// <summary>
        /// 权值矩阵w  
        /// </summary>
        public double[,] w;
        /// <summary>
        /// 权值矩阵V  
        /// </summary>
        public double[,] v;
        /// <summary>
        /// 权值矩阵w  
        /// </summary>
        public double[,] dw;
        /// <summary>
        /// 权值矩阵V  
        /// </summary>
        public double[,] dv;
        
        /// <summary>
        /// 隐层阈值矩阵  
        /// </summary>
        public double[] b1;
        /// <summary>
        /// 输出层阈值矩阵  
        /// </summary>
        public double[] b2;
        /// <summary>
        /// 隐层阈值矩阵  
        /// </summary>
        public double[] db1;
        /// <summary>
        /// 输出层阈值矩阵
        /// </summary>
        public double[] db2;

        /// <summary>
        /// 隐层的误差
        /// </summary>
        double[] pp;
        /// <summary>
        /// 输出层的误差
        /// </summary>
        double[] qq;
        /// <summary>
        /// 输出层的教师数据
        /// </summary>
        double[] yd;
        /// <summary>
        /// 均方误差
        /// </summary>
        public double e;
        /// <summary>
        /// 归一化比例系数
        /// </summary>
        double in_rate;

        /// <summary>
        /// 计算隐藏层节点数
        /// </summary>
        /// <param name="m">输入层节点数</param>
        /// <param name="n">输出层节点数</param>
        /// <returns></returns>
        public int computeHideNum(int m, int n)
        {
            double s = Math.Sqrt(0.43 * m * n + 0.12 * n * n + 2.54 * m + 0.77 * n + 0.35) + 0.51;
            int ss = Convert.ToInt32(s);
            return ((s - (double)ss) > 0.5) ? ss + 1 : ss;
        }

        /// <summary>
        /// 初始化神经网络
        /// </summary>
        /// <param name="innum">输入节点数</param>
        /// <param name="outnum">输出节点数</param>
        public BpNet(int innum, int outnum)
        {
            // 构造函数逻辑  
            R = new Random();

            this.inNum = innum; //数组第二维大小 为 输入节点数  
            this.outNum = outnum; //输出节点数  
            this.hideNum = computeHideNum(inNum, outNum); //隐藏节点数

            x = new double[inNum];
            x1 = new double[hideNum];
            x2 = new double[outNum];

            o1 = new double[hideNum];
            o2 = new double[outNum];

            w = new double[inNum, hideNum];
            v = new double[hideNum, outNum];
            dw = new double[inNum, hideNum];
            dv = new double[hideNum, outNum];

            b1 = new double[hideNum];
            b2 = new double[outNum];
            db1 = new double[hideNum];
            db2 = new double[outNum];

            pp = new double[hideNum];
            qq = new double[outNum];
            yd = new double[outNum];

            //初始化w  
            for (int i = 0; i < inNum; i++)
            {
                for (int j = 0; j < hideNum; j++)
                {
                    w[i, j] = (R.NextDouble() * 2 - 1.0) / 2;
                }
            }

            //初始化v  
            for (int i = 0; i < hideNum; i++)
            {
                for (int j = 0; j < outNum; j++)
                {
                    v[i, j] = (R.NextDouble() * 2 - 1.0) / 2;
                }
            }
            
            e = 0.0;
            in_rate = 1.0;
        }

        /// <summary>
        /// 训练函数
        /// </summary>
        /// <param name="p">训练样本集合</param>
        /// <param name="t">训练样本结果集合</param>
        /// <param name="rate">学习率</param>
        public void train(double[,] p, double[,] t, double rate)
        {
            //获取样本数量
            this.sampleNum = p.GetLength(0);
            e = 0.0;
            //求p，t中的最大值  
            double pMax = 0.0;
            for (int isamp = 0; isamp < sampleNum; isamp++)
            {
                for (int i = 0; i < inNum; i++)
                {
                    if (Math.Abs(p[isamp, i]) > pMax)
                    {
                        pMax = Math.Abs(p[isamp, i]);
                    }
                }

                for (int j = 0; j < outNum; j++)
                {
                    if (Math.Abs(t[isamp, j]) > pMax)
                    {
                        pMax = Math.Abs(t[isamp, j]);
                    }
                }
                in_rate = pMax;
            }

            for (int isamp = 0; isamp < sampleNum; isamp++)
            {
                //数据归一化  
                for (int i = 0; i < inNum; i++)
                {
                    x[i] = p[isamp, i] / in_rate;
                }
                for (int i = 0; i < outNum; i++)
                {
                    yd[i] = t[isamp, i] / in_rate;
                }

                //计算隐层的输入和输出  
                for (int j = 0; j < hideNum; j++)
                {
                    o1[j] = 0.0;
                    for (int i = 0; i < inNum; i++)
                    {
                        o1[j] += w[i, j] * x[i];
                    }
                    x1[j] = 1.0 / (1.0 + Math.Exp(-o1[j] - b1[j]));
                }

                //计算输出层的输入和输出  
                for (int k = 0; k < outNum; k++)
                {
                    o2[k] = 0.0;
                    for (int j = 0; j < hideNum; j++)
                    {
                        o2[k] += v[j, k] * x1[j];
                    }
                    x2[k] = 1.0 / (1.0 + Math.Exp(-o2[k] - b2[k]));
                }

                //计算输出层误差和均方差  
                for (int k = 0; k < outNum; k++)
                {
                    qq[k] = (yd[k] - x2[k]) * x2[k] * (1.0 - x2[k]);
                    e += (yd[k] - x2[k]) * (yd[k] - x2[k]);
                    //更新V  
                    for (int j = 0; j < hideNum; j++)
                    {
                        v[j, k] += rate * qq[k] * x1[j];
                    }
                }

                //计算隐层误差  
                for (int j = 0; j < hideNum; j++)
                {
                    pp[j] = 0.0;
                    for (int k = 0; k < outNum; k++)
                    {
                        pp[j] += qq[k] * v[j, k];
                    }
                    pp[j] = pp[j] * x1[j] * (1 - x1[j]);

                    //更新W  
                    for (int i = 0; i < inNum; i++)
                    {
                        w[i, j] += rate * pp[j] * x[i];
                    }
                }

                //更新b2  
                for (int k = 0; k < outNum; k++)
                {
                    b2[k] += rate * qq[k];
                }

                //更新b1  
                for (int j = 0; j < hideNum; j++)
                {
                    b1[j] += rate * pp[j];
                }
            }
            e = Math.Sqrt(e);
            //adjustWV(w,dw);  
            //adjustWV(v,dv);  
        }
        
        /// <summary>
        /// 测试函数(单个数据测试)
        /// </summary>
        /// <param name="p">待测试样本</param>
        /// <returns>识别结果</returns>
        public int test(double[] p)
        {
            double[,] w = new double[inNum, hideNum];
            double[,] v = new double[hideNum, outNum];
            double[] b1 = new double[hideNum];
            double[] b2 = new double[outNum];
            //1.读取权值矩阵系数
            readMatrixW(w, "w.txt");
            readMatrixW(v, "v.txt");
            readMatrixB(b1, "b1.txt");
            readMatrixB(b2, "b2.txt");

            //2.数据归一化  
            double pMax = 0.0;
            for (int i = 0; i < inNum; i++)
            {
                if (Math.Abs(p[i]) > pMax)
                {
                    pMax = Math.Abs(p[i]);
                }
            }
            in_rate = pMax;//归一化系数
            for (int i = 0; i < inNum; i++)
            {
                x[i] = p[i] / in_rate;
            }

            //3.计算隐层的输入和输出  
            for (int j = 0; j < hideNum; j++)
            {
                o1[j] = 0.0;
                for (int i = 0; i < inNum; i++)
                {
                    o1[j] += w[i, j] * x[i];
                }

                x1[j] = 1.0 / (1.0 + Math.Exp(-o1[j] - b1[j]));
            }

            //4.计算输出层的输入和输出  
            for (int k = 0; k < outNum; k++)
            {
                o2[k] = 0.0;
                for (int j = 0; j < hideNum; j++)
                {
                    o2[k] += v[j, k] * x1[j];
                }
                x2[k] = 1.0 / (1.0 + Math.Exp(-o2[k] - b2[k]));
            }

            //5.判断是否正确
            double max = x2[0];
            int maxi = 0;
            for(int i = 0; i < outNum; i++)
            {
                if(x2[i] > max)
                {
                    max = x2[i];
                    maxi = i;
                }
            }
            return maxi;
        }

        public void adjustWV(double[,] w, double[,] dw)
        {
            for (int i = 0; i < w.GetLength(0); i++)
            {
                for (int j = 0; j < w.GetLength(1); j++)
                {
                    w[i, j] += dw[i, j];
                }
            }
        }

        public void adjustWV(double[] w, double[] dw)
        {
            for (int i = 0; i < w.Length; i++)
            {
                w[i] += dw[i];
            }
        }

        /// <summary>
        /// 保存矩阵w,v  
        /// </summary>
        /// <param name="w">要保存的矩阵</param>
        /// <param name="filename">文件名</param>
        public void saveMatrix(double[,] w, string filename)
        {
            StreamWriter sw = File.CreateText(filename);
            for (int i = 0; i < w.GetLength(1); i++)
            {
                for (int j = 0; j < w.GetLength(0); j++)
                {
                    sw.Write(w[j, i].ToString("0.000000000000000") + " ");
                }
                sw.WriteLine();
            }
            sw.Close();

        }

        /// <summary>
        /// 保存矩阵b1,b2  
        /// </summary>
        /// <param name="b">要保存的阀值矩阵</param>
        /// <param name="filename">文件名</param>
        public void saveMatrix(double[] b, string filename)
        {
            StreamWriter sw = File.CreateText(filename);
            for (int i = 0; i < b.Length; i++)
            {
                sw.Write(b[i] + " ");
            }
            sw.Close();
        }

        /// <summary>
        /// 读取矩阵W,V  
        /// </summary>
        /// <param name="w">要读取到的那个矩阵</param>
        /// <param name="filename">文件所在位置</param>
        public void readMatrixW(double[,] w, string filename)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(filename, Encoding.GetEncoding("gb2312"));

                String line;
                int i = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] s1 = line.Trim().Split(' ');
                    for (int j = 0; j < s1.Length; j++)
                    {
                        w[j, i] = Convert.ToDouble(s1[j]);
                    }
                    i++;
                }
                sr.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 读取矩阵b1,b2  
        /// </summary>
        /// <param name="b">要读取的阀值矩阵</param>
        /// <param name="filename">文件所在位置</param>
        public void readMatrixB(double[] b, string filename)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(filename, Encoding.GetEncoding("gb2312"));

                String line;
                if ((line = sr.ReadLine()) != null)
                {
                    string[] strs = line.Trim().Split(' ');
                    for (int i = 0; i < strs.Length; i++)
                    {
                        b[i] = Convert.ToDouble(strs[i]);
                    }
                }
                sr.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}