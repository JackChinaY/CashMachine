using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace CameraCapture
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 定义一个公共变量
        /// </summary>
        Capture capture = null;
        private volatile bool captureFlag = false;//判断摄像头的状态
        private static Tesseract _ocr;//创建识别对象
        private volatile int margin_left = 100;//左边距
        private volatile int margin_top = 120;//右边距
        private volatile int object_width = 420;//宽度
        private volatile int object_height = 240;//高度
        private volatile int count = 4;//采集区域一行能够显示数字的个数
        private delegate void SetMarginEdgeEventHandler(Margin_Edge margin_edge);//声明一个委托，其实就是个“命令”
        /////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();//前台页面控件初始化
            this.ResizeMode = ResizeMode.CanMinimize;//禁用“最大化”按钮
            button2.IsEnabled = false;//截取按钮在摄像头未打开的状态下不可用
            readConfiguration("型号01");//加载用户上次最后设置的参数
            //margin_left = Convert.ToInt32(left.Text);
            //margin_top = Convert.ToInt32(top.Text);
            //object_width = Convert.ToInt32(width.Text);
            //object_height = Convert.ToInt32(height.Text);

            //Identification(new Image<Bgr, Byte>(@"wan.jpg"));

        }
        /////////////////////////////////////////////////////////////////////////////////   
        #region 一些基础工作
        /// <summary>
        /// 打开或关闭摄像头
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (button1.Content.Equals("打开"))
            {
                if (radioButton_local.IsChecked == true)
                {
                    capture = new Capture(0);  //初始化摄像头
                }
                else
                {
                    try
                    {
                        capture = new Capture(1);  //初始化摄像头
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("外接USB摄像头打不开，请检查是否可用！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                Thread RgbThread = new Thread(Capture_ImageGrabbed);//创建一个线程
                RgbThread.Start(capture);//开启线程
                captureFlag = true;//设置摄像头的状态为开启
                button1.Content = "关闭";
                textblock1.Text = capture.QueryFrame().Width.ToString() + "*" + capture.QueryFrame().Height.ToString();
                button2.IsEnabled = true;
            }
            else
            {
                captureFlag = false;//设置摄像头的状态为关闭
                capture.Dispose();
                imageBox1.Image = capture.QueryFrame();
                capture = null;
                button1.Content = "打开";
                button2.IsEnabled = false;
            }
        }
        /// <summary>
        /// 显示摄像头拍摄的画面
        /// </summary>
        /// <param name="obj"></param>
        private void Capture_ImageGrabbed(object obj)
        {
            Capture capture = obj as Capture;
            capture.Start();//开启摄像头
            while (captureFlag == true && capture != null)
            {
                Image<Bgr, Byte> frame = capture.QueryFrame();  //接收数据,一帧画面
                //目标矩形框大小
                Rectangle rectangle = new Rectangle(new System.Drawing.Point(margin_left, margin_top), new System.Drawing.Size(object_width, object_height));
                //在二值图像中画框
                if (frame != null)
                {
                    frame.Draw(rectangle, new Bgr(220, 20, 60), 2);
                }
                imageBox1.Image = frame;   //显示图像
            }
        }
        /// <summary>
        /// 截取视频流中的一张图片并识别里面的数字
        /// </summary>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (captureFlag == true)
            {
                Image<Bgr, Byte> image = capture.QueryFrame();  //接收数据
                imageBox2.Image = image;   //显示图像
                Identification(image);
            }
        }
        /// <summary>
        /// 设置左右边距和目标物体的宽度和高度 提交按钮
        /// </summary>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //设置目标区域最小宽度为50
            if (Convert.ToInt32(width.Text) < 150)
            {
                MessageBox.Show("目标区域的宽度不能低于150像素！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //设置目标区域最小高度为50
            if (Convert.ToInt32(height.Text) < 50)
            {
                MessageBox.Show("目标区域的高度不能低于50像素！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //Button btn = sender as Button;//将sender进行转换，转换成按钮对象。  
            //btn.Content = "提交...";//设置按钮的文本为“提交...”  
            //Thread.Sleep(3000);
            Margin_Edge margin_edge = new Margin_Edge();
            margin_edge.margin_left = Convert.ToInt32(left.Text);
            margin_edge.margin_top = Convert.ToInt32(top.Text);
            margin_edge.object_width = Convert.ToInt32(width.Text);
            margin_edge.object_height = Convert.ToInt32(height.Text);
            margin_edge.count = Convert.ToInt32(comboBox1.SelectionBoxItem.ToString());
            //声明一个委托,这里就是具体阐述这个命令是干什么的
            SetMarginEdgeEventHandler myDelegate = new SetMarginEdgeEventHandler(SetMarginEdge);
            //执行委托，并附上参数
            myDelegate(margin_edge);
            //Identification(new Image<Bgr, Byte>(@"wan.jpg"));
        }
        /// <summary>
        /// 利用委托改变全局变量，边框距，因为当摄像头开启的线程中引用了全局变量，在多线程的环境中要改变全局变量必须使用委托
        /// </summary>
        private void SetMarginEdge(Margin_Edge margin_edge)
        {
            margin_left = margin_edge.margin_left;
            margin_top = margin_edge.margin_top;
            object_width = margin_edge.object_width;
            object_height = margin_edge.object_height;
            count = margin_edge.count;
        }

        ///// <summary>
        ///// 读取配置文件，该文件保存的是用户上次设置的参数
        ///// </summary>
        //private void readConfiguration()
        //{
        //    // 创建一个XmlDocument类的对象
        //    XmlDocument doc = new XmlDocument();
        //    // 加载要读取的xml文档
        //    doc.Load("conf.xml");
        //    // 读取root根节点,如果加了//表示整个文档中所有的item元素
        //    XmlNode node = doc.SelectSingleNode("root");
        //    //获取item节点
        //    XmlNode childNodesList = node.SelectSingleNode("item");
        //    //遍历所有子节点
        //    foreach (XmlElement xmlelement in childNodesList)
        //    {
        //        //将默认参数输出到页面和后台
        //        if (xmlelement.Name == "marginleft")
        //        {
        //            margin_left = Convert.ToInt32(xmlelement.InnerText);
        //            left.Text = margin_left.ToString();
        //        }
        //        else if (xmlelement.Name == "margintop")
        //        {
        //            margin_top = Convert.ToInt32(xmlelement.InnerText);
        //            top.Text = margin_top.ToString();
        //        }
        //        else if (xmlelement.Name == "width")
        //        {
        //            object_width = Convert.ToInt32(xmlelement.InnerText);
        //            //设置最小宽度为50
        //            if (object_width < 50)
        //            {
        //                object_width = 50;
        //            }
        //            width.Text = object_width.ToString();
        //        }
        //        else if (xmlelement.Name == "height")
        //        {
        //            object_height = Convert.ToInt32(xmlelement.InnerText);
        //            //设置最小高度为50
        //            if (object_height < 50)
        //            {
        //                object_height = 50;
        //            }
        //            height.Text = object_height.ToString();
        //        }
        //        else if (xmlelement.Name == "numbercount")
        //        {
        //            count = Convert.ToInt32(xmlelement.InnerText);
        //            comboBox1.SelectedValue = count.ToString();
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            //Console.WriteLine(comboBox2.SelectionBoxItem.ToString());
            readConfiguration(comboBox2.SelectionBoxItem.ToString());
        }
        /// <summary>
        /// 读取配置文件，该文件保存的是用户上次设置的参数
        /// </summary>
        private void readConfiguration(string id)
        {
            // 创建一个XmlDocument类的对象
            XmlDocument doc = new XmlDocument();
            // 加载要读取的xml文档
            doc.Load("conf.xml");
            // 读取root根节点,如果加了//表示整个文档中所有的item元素
            XmlNode node = doc.SelectSingleNode("root");
            //获取item节点
            XmlNodeList childNodesList = node.ChildNodes;
            //遍历所有item节点
            foreach (XmlElement xmlelementNode in childNodesList)
            {
                //Console.WriteLine(xmlelementNode.GetAttribute("id"));
                if (xmlelementNode.GetAttribute("id") == id)
                {
                    XmlNode childNode = (XmlNode)xmlelementNode;
                    //遍历所有item子节点
                    foreach (XmlElement xmlelement in childNode)
                    {

                        //将默认参数输出到页面和后台
                        if (xmlelement.Name == "marginleft")
                        {
                            margin_left = Convert.ToInt32(xmlelement.InnerText);
                            left.Text = margin_left.ToString();
                        }
                        else if (xmlelement.Name == "margintop")
                        {
                            margin_top = Convert.ToInt32(xmlelement.InnerText);
                            top.Text = margin_top.ToString();
                        }
                        else if (xmlelement.Name == "width")
                        {
                            object_width = Convert.ToInt32(xmlelement.InnerText);
                            //设置最小宽度为50
                            if (object_width < 50)
                            {
                                object_width = 50;
                            }
                            width.Text = object_width.ToString();
                        }
                        else if (xmlelement.Name == "height")
                        {
                            object_height = Convert.ToInt32(xmlelement.InnerText);
                            //设置最小高度为50
                            if (object_height < 50)
                            {
                                object_height = 50;
                            }
                            height.Text = object_height.ToString();
                        }
                        else if (xmlelement.Name == "numbercount")
                        {
                            count = Convert.ToInt32(xmlelement.InnerText);
                            comboBox1.SelectedValue = count.ToString();
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 修改配置文件，该文件保存的是用户最后一次设置的参数
        /// </summary>
        private void updateConfiguration(Margin_Edge margin_edge)
        {
            // 创建一个XmlDocument类的对象
            XmlDocument doc = new XmlDocument();
            // 加载要读取的xml文档
            doc.Load("conf.xml");
            // 读取root根节点,如果加了//表示整个文档中所有的item元素
            XmlNode node = doc.SelectSingleNode("root");
            //获取所有item节点
            XmlNodeList childNodesList = node.ChildNodes;
            //遍历所有item节点
            foreach (XmlElement xmlelementNode in childNodesList)
            {
                //Console.WriteLine(xmlelementNode.GetAttribute("id"));
                if (xmlelementNode.GetAttribute("id") == margin_edge.typeId)
                {
                    XmlNode childNode = (XmlNode)xmlelementNode;
                    //遍历所有item子节点
                    foreach (XmlElement xmlelement in childNode)
                    {
                        //将默认参数输出到页面和后台
                        if (xmlelement.Name == "marginleft")
                        {
                            xmlelement.InnerText = margin_edge.margin_left_s;
                        }
                        else if (xmlelement.Name == "margintop")
                        {
                            xmlelement.InnerText = margin_edge.margin_top_s;
                        }
                        else if (xmlelement.Name == "width")
                        {
                            xmlelement.InnerText = margin_edge.object_width_s;
                        }
                        else if (xmlelement.Name == "height")
                        {
                            xmlelement.InnerText = margin_edge.object_height_s;
                        }
                        else if (xmlelement.Name == "numbercount")
                        {
                            xmlelement.InnerText = margin_edge.count_s;
                        }
                    }
                }
            }
            //保存
            doc.Save("conf.xml");

        }
        /// <summary>
        /// 关闭窗口时检查摄像头是否被关闭
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //如果摄像头未关闭则将摄像头关闭
            if (capture != null)
            {
                captureFlag = false;//设置摄像头的状态为关闭
                capture.Dispose();
                capture = null;
            }
            //保存用户最后一次输入的参数值，便于下次打开软件时加载最后一次设置的参数
            Margin_Edge margin_edge = new Margin_Edge();
            margin_edge.margin_left_s = left.Text;
            margin_edge.margin_top_s = top.Text;
            margin_edge.typeId = comboBox2.SelectionBoxItem.ToString();
            if (Convert.ToInt32(width.Text) < 150)
            {
                margin_edge.object_width_s = "150";
            }
            else
            {
                margin_edge.object_width_s = width.Text;
            }
            if (Convert.ToInt32(height.Text) < 50)
            {
                margin_edge.object_height_s = "50";
            }
            else
            {
                margin_edge.object_height_s = height.Text;
            }
            margin_edge.count_s = comboBox1.SelectionBoxItem.ToString();
            updateConfiguration(margin_edge);
        }
        /// <summary>
        /// 实体：左边距、上边距、目标物体宽度、目标物体高度
        /// </summary>
        public class Margin_Edge
        {
            public int margin_left { get; set; }//左边距
            public int margin_top { get; set; }//上边距
            public int object_width { get; set; }//目标物体宽度
            public int object_height { get; set; }//目标物体高度
            public int count { get; set; }//目标物体中数字个数
            //设置string类型主要用于向配置文件中写数据，尤其当页面数据为空时，上面的int类型就会报错，所以用string类型进行操作
            public string margin_left_s { get; set; }//左边距
            public string margin_top_s { get; set; }//上边距
            public string object_width_s { get; set; }//目标物体宽度
            public string object_height_s { get; set; }//目标物体高度
            public string count_s { get; set; }//目标物体中数字个数
            public string typeId { get; set; }//目标物体中数字个数
            //无参构造函数
            public Margin_Edge()
            {
            }
            //有参构造函数
            public Margin_Edge(double left, double top)
            {
                margin_left = Convert.ToInt32(left);
                margin_top = Convert.ToInt32(top);
            }
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region 主要识别的算法
        /// <summary>
        /// 识别图像里的内容
        /// </summary>  
        private void Identification(Image<Bgr, Byte> image)
        {
            //try
            //{
            imageBox2.Image = image;
            Console.WriteLine("原始图片的长度和高度：" + image.Size);
            //目标矩形框大小
            Rectangle rectangle = new Rectangle(new System.Drawing.Point(margin_left, margin_top), new System.Drawing.Size(object_width, object_height)); //创建一个矩形，左上角坐标为（80，80）大小为40*40

            //灰度化，将彩色图像灰度化
            var grayImage = image.Convert<Gray, Byte>();

            //二值化，将灰度图像采用高斯自适应阈值法二值化,得到二值图像
            var binaryImage = grayImage.ThresholdAdaptive(new Gray(255), ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_GAUSSIAN_C, THRESH.CV_THRESH_BINARY, 121, new Gray(5));

            //截取目标区域
            var objectImage = binaryImage.Copy(rectangle);
            //截取目标区域 复制
            var objectImage_copy = objectImage.Copy();

            imageBox3.Image = grayImage;
            imageBox4.Image = binaryImage;
            imageBox5.Image = objectImage;

            //在二值图像中画框
            binaryImage.Draw(rectangle, new Gray(0), 1);

            //检测结果
            string result = "";
            //将采集区域的数字裁剪成单个数字
            List<Image<Gray, Byte>> list = new List<Image<Gray, Byte>>();
            for (int i = 0; i < count; i++)
            {
                //目标矩形框大小
                Rectangle rect = new Rectangle(new System.Drawing.Point(Convert.ToInt32(object_width / count) * i, 0), new System.Drawing.Size(Convert.ToInt32(object_width / count), object_height));
                //list.Add(objectImage.Copy(rect));
                result += IdentificationNumber(objectImage.Copy(rect), Convert.ToInt32(object_width / count), object_height);
                //在二值图像中画框
                objectImage_copy.Draw(rect, new Gray(0), 1);

            }
            imageBox6.Image = objectImage_copy;
            textBox1.Text = result;
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}
        }

        /// <summary>  
        /// 测的7段线每段线中5*5区域的灰度值
        /// </summary>  
        public string IdentificationNumber(Image<Gray, Byte> image, int image_width, int image_height)
        {
            //字符库0123456789.
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("00000011", "0");
            dictionary.Add("11110011", "1");
            dictionary.Add("01001011", "2");
            dictionary.Add("01100001", "3");
            dictionary.Add("10110001", "4");
            dictionary.Add("00100101", "5");
            dictionary.Add("00000101", "6");
            dictionary.Add("01110011", "7");
            dictionary.Add("00000001", "8");
            dictionary.Add("00110001", "9");
            dictionary.Add("00000010", "0.");//带小数点的
            dictionary.Add("11110010", "1.");
            dictionary.Add("01001010", "2.");
            dictionary.Add("01100000", "3.");
            dictionary.Add("10110000", "4.");
            dictionary.Add("00100100", "5.");
            dictionary.Add("00000100", "6.");
            dictionary.Add("01110010", "7.");
            dictionary.Add("00000000", "8.");
            dictionary.Add("00110000", "9.");

            //七段码每段码的位置 日，顶部为1，逆时针转：1 2 3 4 5 6，中间一横为7
            List<Margin_Edge> list = new List<Margin_Edge>();
            list.Add(new Margin_Edge(image_width * 0.43, 6));//1
            list.Add(new Margin_Edge(8, image_height * 0.25));//2
            list.Add(new Margin_Edge(5, image_height * 0.66));//3
            list.Add(new Margin_Edge(image_width * 0.37, image_height - 17));//4
            list.Add(new Margin_Edge(image_width - 20, image_height * 0.66));//5
            list.Add(new Margin_Edge(image_width - 15, image_height * 0.25));//6
            list.Add(new Margin_Edge(image_width * 0.4, image_height * 0.45));//7
            list.Add(new Margin_Edge(image_width - 10, image_height - 12));//7
            //检测结果
            string result = "";

            for (int i = 0; i < 8; i++)
            {
                //检测7段线
                if (i < 7)
                {
                    //目标矩形框大小
                    Rectangle rectangle = new Rectangle(new System.Drawing.Point(list[i].margin_left, list[i].margin_top), new System.Drawing.Size(15, 15));
                    //截取目标区域
                    var objectImage = image.Copy(rectangle);
                    //获取该区域的灰度均值
                    string temp1 = objectImage.GetAverage().ToString().Substring(1);
                    string temp2 = temp1.Substring(0, temp1.Length - 1);
                    //Console.WriteLine("temp2:" + temp2);
                    double temp3 = Convert.ToDouble(temp2);
                    //判断该区域的平均值
                    if (temp3 < 200)
                    {
                        result += "0";
                    }
                    else
                    {
                        result += "1";
                    }
                    //绘制白色方块
                    //for (int j = 0; j < 15; j++)
                    //{
                    //    for (int k = 0; k < 15; k++)
                    //    {
                    //        image[list[i].margin_top + j, list[i].margin_left + k] = new Gray(255);
                    //    }
                    //}
                }
                //检测小数点
                else
                {
                    //目标矩形框大小
                    Rectangle rectangle = new Rectangle(new System.Drawing.Point(list[i].margin_left, list[i].margin_top), new System.Drawing.Size(9, 9));
                    //截取目标区域
                    var objectImage = image.Copy(rectangle);
                    //获取该区域的灰度均值
                    string temp1 = objectImage.GetAverage().ToString().Substring(1);
                    string temp2 = temp1.Substring(0, temp1.Length - 1);
                    //Console.WriteLine("temp2:" + temp2);
                    double temp3 = Convert.ToDouble(temp2);
                    //判断该区域的平均值
                    if (temp3 < 120)
                    {
                        result += "0";
                    }
                    else
                    {
                        result += "1";
                    }
                    //绘制白色方块
                    //for (int j = 0; j < 9; j++)
                    //{
                    //    for (int k = 0; k < 9; k++)
                    //    {
                    //        image[list[i].margin_top + j, list[i].margin_left + k] = new Gray(255);
                    //    }
                    //}
                }
            }
            //imageBox6.Image = image;
            //Console.WriteLine("本次检测结果：" + result);
            //textBox1.Text = dictionary[result];
            //返回结果
            try
            {
                return dictionary[result];
            }
            catch (Exception)
            {
                return "0";
            }
        }

        #endregion
        /////////////////////////////////////////////////////////////////////////////////
        #region 参考用代码
        //传入图片进行识别
        public string ORC_(Bitmap img)
        {
            //""标示OCR识别调用失败
            string re = "";
            if (img == null)
                return re;
            else
            {
                Bgr drawColor = new Bgr(Color.Blue);
                try
                {
                    Image<Bgr, Byte> image = new Image<Bgr, byte>(img);


                    using (Image<Gray, byte> gray = image.Convert<Gray, Byte>())
                    {
                        _ocr.Recognize(gray);
                        Tesseract.Charactor[] charactors = _ocr.GetCharactors();
                        foreach (Tesseract.Charactor c in charactors)
                        {
                            image.Draw(c.Region, drawColor, 1);
                        }
                        //imageBox9.Image = image;
                        re = _ocr.GetText();
                    }
                    return re;
                }
                catch (Exception ex)
                {

                    return re;
                }
            }
        }

        ///// <summary>
        ///// 识别图像里的内容
        ///// </summary>  
        //private void Identification(Image<Bgr, Byte> image)
        //{
        //    try
        //    {
        //        //fontyp eng E:\大学课程\图像识别\jTessBoxEditor-1.7.3\jTessBoxEditor\tesseract-ocr\tessdata   E:\大学课程\图像识别\libemgucv-windows-universal-cuda-2.4.10.1940\bin\tessdata
        //        //_ocr = new Tesseract(@"E:\大学课程\图像识别\libemgucv-windows-universal-cuda-2.4.10.1940\bin\tessdata", "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_ONLY);//方法第一个参数可为""表示通过环境变量调用字库，第二个参数表示字库的文件，第三个表示识别方式，可看文档与资料查找。
        //        //_ocr.SetVariable("tessedit_pageseg_mode", "10");
        //        //_ocr.SetVariable("tessedit_char_whitelist", "0123456789");//此方法表示只识别1234567890与x字母abcdefghijklmnopqrstuvwxyz
        //        string result = "";
        //        //Bitmap bitmap = new Bitmap(image.ToBitmap());
        //        Bitmap bitmap = new Bitmap(Gray2(image).ToBitmap());
        //        //bitmap = BrightnessP(bitmap, );//图片加亮处理
        //        //bitmap = KiContrast(bitmap, );//调整对比对
        //        //this.pictureBox3.Image = bitmap;
        //        imageBox2.Image = image;
        //        //result = ORC_(bitmap);
        //        Console.WriteLine("图片的长度和宽度：" + bitmap.Size);
        //        //Console.WriteLine("识别内容：" + result);
        //        //textBox1.Text = result.Trim();
        //        //_ocr.Dispose();
        //        //Gray(image);
        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show(exception.Message);
        //    }
        //}

        /// <summary>  
        /// 图像处理,灰度化、二值化、裁剪、腐蚀、膨胀
        /// </summary>  
        public Image<Gray, Byte> Gray2(Image<Bgr, Byte> image)
        {
            //Image<Bgr, Byte> threshImage = new Image<Bgr, Byte>();
            //var threshImage = image.CopyBlank();

            //var grayImage = new Image<Gray, Byte>(image.Width, image.Height);

            ////图像灰度化
            //IntPtr gray_ptr = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(image), Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_8U, 1);
            //CvInvoke.cvCvtColor(image, gray_ptr, COLOR_CONVERSION.BGR2GRAY);
            //CvInvoke.cvThreshold(gray_ptr, gray_ptr, 20, 255, THRESH.CV_THRESH_BINARY);

            ////图像显示
            //MIplImage mipimage1 = (MIplImage)Marshal.PtrToStructure(gray_ptr, typeof(MIplImage));
            //Image<Gray, Byte> result = new Image<Gray, Byte>(mipimage1.width, mipimage1.height, mipimage1.widthStep, mipimage1.imageData);

            ////灰度图片二值化
            //IntPtr byte_ptr = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(gray_ptr), Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_8U, 1);
            //CvInvoke.cvAdaptiveThreshold(gray_ptr, byte_ptr, 255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_GAUSSIAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, 23, 5);

            ////边缘检测和
            ////CvInvoke.cvCanny(byte_ptr, byte_ptr, 90, 120, 3);

            ////腐蚀
            //CvInvoke.cvErode(byte_ptr, byte_ptr, IntPtr.Zero, 1);
            ////图像显示
            //MIplImage mipimage1 = (MIplImage)Marshal.PtrToStructure(byte_ptr, typeof(MIplImage));
            //Image<Gray, Byte> result = new Image<Gray, Byte>(mipimage1.width, mipimage1.height, mipimage1.widthStep, mipimage1.imageData);
            //imageBox3.Image = result;

            ////膨胀
            //CvInvoke.cvDilate(byte_ptr, byte_ptr, IntPtr.Zero, 1);
            ////CvInvoke.bl(img, dst, 10, 30, 15);
            ////图像显示
            //mipimage1 = (MIplImage)Marshal.PtrToStructure(byte_ptr, typeof(MIplImage));
            //Image<Gray, Byte> result2 = new Image<Gray, Byte>(mipimage1.width, mipimage1.height, mipimage1.widthStep, mipimage1.imageData);
            //imageBox4.Image = result2.SmoothMedian(5);

            //目标矩形框大小
            Rectangle rectangle = new Rectangle(new System.Drawing.Point(margin_left, margin_top), new System.Drawing.Size(object_width, object_height)); //创建一个矩形，左上角坐标为（80，80）大小为40*40

            //灰度化，将彩色图像灰度化
            var grayImage = image.Convert<Gray, Byte>();

            //二值化，将灰度图像采用高斯自适应阈值法二值化,得到二值图像
            var binaryImage = grayImage.ThresholdAdaptive(new Gray(255), ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_GAUSSIAN_C, THRESH.CV_THRESH_BINARY, 199, new Gray(5));

            //截取目标区域
            var objectImage = binaryImage.Copy(rectangle);

            ////目标矩形框大小
            //Rectangle rectangle_test = new Rectangle(new System.Drawing.Point(margin_left, margin_top), new System.Drawing.Size(5, 5)); //创建一个矩形，左上角坐标为（80，80）大小为5*5


            //Console.WriteLine("1点的灰度值：" + objectImage[10, 32]); objectImage[10, 32] = new Gray(255);

            //Console.WriteLine("2点的灰度值：" + objectImage[40, 13]); objectImage[40, 13] = new Gray(255);

            //Console.WriteLine("3点的灰度值：" + objectImage[80, 10]); objectImage[80, 10] = new Gray(255);

            //Console.WriteLine("4点的灰度值：" + objectImage[103, 32]); objectImage[103, 32] = new Gray(255);

            //Console.WriteLine("5点的灰度值：" + objectImage[80, 50]); objectImage[80, 50] = new Gray(255);

            //Console.WriteLine("6点的灰度值：" + objectImage[40, 55]); objectImage[40, 55] = new Gray(0);

            //Console.WriteLine("7点的灰度值：" + objectImage[57, 32]); objectImage[57, 32] = new Gray(255);

            //中值滤波，采用中值滤波法去噪声
            //var smoothBinaryImage = binaryImage.SmoothGaussian(3);
            //边缘检测
            //var smoothBinaryImageCanny = smoothBinaryImage.Canny(100, 300);

            //颜色取反,目标物体为白色
            //var smoothBinaryImageNot = objectImage.Not();

            //膨胀，膨胀是把白色目标给膨胀的
            //var dilate = smoothBinaryImageNot.Dilate(4);

            //腐蚀，腐蚀是把白色目标给腐蚀的
            //var erode = dilate.Dilate(2);
            //取反
            //var erode2 = dilate.Not();

            imageBox3.Image = grayImage;
            imageBox4.Image = binaryImage;
            imageBox5.Image = objectImage;
            //objectImage.ToBitmap().Save(@"E:\temp.jpg");
            //imageBox6.Image = smoothBinaryImageNot;
            binaryImage.Draw(rectangle, new Gray(0), 2);
            //imageBox7.Image = dilate;
            //imageBox8.Image = erode;
            //imageBox9.Image = erode2;
            //imageBox10.Image = erode2.ConvertScale<Byte>(1, 0);
            //imageBox10.Image = erode2.PyrDown();
            //imageBox10.Image = erode2.Resize(Convert.ToInt32(object_width *0.3), Convert.ToInt32(object_height * 0.3), INTER.CV_INTER_AREA);
            return binaryImage;
        }



        /// <summary>  
        /// 增加图像亮度  
        /// </summary>  
        /// <param name="a"></param>

        /// <param name="v"></param>  
        /// <returns></returns>  
        //public static Bitmap BrightnessP(Bitmap a, int v)
        //{
        //    System.Drawing.Imaging.BitmapData bmpData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    int bytes = a.Width * a.Height * 3;
        //    IntPtr ptr = bmpData.Scan0;
        //    int stride = bmpData.Stride;
        //    unsafe
        //    {
        //        byte* p = (byte*)ptr;
        //        int temp;
        //        for (int j = 0; j < a.Height; j++)
        //        {
        //            for (int i = 0; i < a.Width * 3; i++, p++)
        //            {
        //                temp = (int)(p[0] + v);
        //                temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
        //                p[0] = (byte)temp;
        //            }
        //            p += stride - a.Width * 3;
        //        }
        //    }
        //    a.UnlockBits(bmpData);
        //    return a;
        //}
        ///<summary>
        ///图像对比度调整
        ///</summary>
        ///<param name="b">原始图</param>
        ///<param name="degree">对比度[-100, 100]</param>
        ///<returns></returns>
        //public static Bitmap KiContrast(Bitmap b, int degree)
        //{
        //    if (b == null)
        //    {
        //        return null;
        //    }
        //    if (degree < -100) degree = -100;
        //    if (degree > 100) degree = 100;
        //    try
        //    {
        //        double pixel = 0;
        //        double contrast = (100.0 + degree) / 100.0;
        //        contrast *= contrast;
        //        int width = b.Width;
        //        int height = b.Height;
        //        BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //        unsafe
        //        {
        //            byte* p = (byte*)data.Scan0;
        //            int offset = data.Stride - width * 3;
        //            for (int y = 0; y < height; y++)
        //            {
        //                for (int x = 0; x < width; x++)
        //                {
        //                    // 处理指定位置像素的对比度
        //                    for (int i = 0; i < 3; i++)
        //                    {
        //                        pixel = ((p / 255.0 - 0.5) * contrast + 0.5) * 255;
        //                        if (pixel < 0) pixel = 0;
        //                        if (pixel > 255) pixel = 255;
        //                        p = (byte)pixel;
        //                    } // i
        //                    p += 3;
        //                } // x
        //                p += offset;
        //            } // y
        //        }
        //        b.UnlockBits(data);
        //        return b;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        /*
         0 = Orientation and script detection (OSD) only.
    1 = Automatic page segmentation with OSD.
    2 = Automatic page segmentation, but no OSD, or OCR.
    3 = Fully automatic page segmentation, but no OSD. (Default)
    4 = Assume a single column of text of variable sizes.
    5 = Assume a single uniform block of vertically aligned text.
    6 = Assume a single uniform block of text.
    7 = Treat the image as a single text line.
    8 = Treat the image as a single word.
    9 = Treat the image as a single word in a circle.
    10 = Treat the image as a single character.
         * 
         */
        #endregion
        /////////////////////////////////////////////////////////////////////////////////
    }
}
