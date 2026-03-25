using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;


namespace RePKG_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        static string Temp_file = Environment.GetEnvironmentVariable("TMP");
        static string Desktop_file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string out_file = Desktop_file + @"\RePKG 输出";
        static ArrayList Number_file = new ArrayList();
        static string RePKG_Directory = "";//RePKG.exe 所在目录

        public MainWindow()
        {
            InitializeComponent();
            日志.Text = "[" + DateTime.Now.ToString() + "]: RePKG-WPF 程序启动  Copyright © xcz  2021";
            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 获取临时文件夹：" + Temp_file;
            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 获取桌面路径：" + Desktop_file;

            RePKG_Directory = AppDomain.CurrentDomain.BaseDirectory;
            日志.Text += "\n[" + DateTime.Now.ToString() + "]: RePKG 目录：" + RePKG_Directory;

            if (!Related_functions.Release_file.CheckRePKG(RePKG_Directory))
            {
                日志.Text += "\n[" + DateTime.Now.ToString() + "]: 警告：未找到 RePKG.exe，请确保该文件与程序在同一目录";
                System.Windows.MessageBox.Show("未找到 RePKG.exe！\n请确保 RePKG.exe 与程序在同一目录下。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 程序准备就绪...";
            输出路径.Text = out_file;
        }
        //添加文件
        private void 选择_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PKG文件(*.PKG)|*.pkg";
            ofd.Multiselect = true;
            if (ofd.ShowDialog(this) == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!Number_file.Contains(file))//排除同类项
                    {
                        Number_file.Add(file);//将元素添加到数组末尾
                        文件列表.Items.Add(file);
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 已添加文件：" + file;
                    }
                    else
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 添加失败!原因：已存在此文件";
                    }
                }
            }
            文件下拉框.Header = "已选择" + Number_file.Count + "个对象";
        }
        //清空文件列表
        private void 清空文件_Click(object sender, RoutedEventArgs e)
        {
            Number_file.Clear();
            文件下拉框.Header = "已选择0个对象";
            文件列表.Items.Clear();
            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 清除所选文件列表";
        }
        //清除日志文件
        private void 清除日志_Click(object sender, RoutedEventArgs e)
        {
            日志.Text = null;
            日志.Text = "[" + DateTime.Now.ToString() + "]: 已清空日志...";
        }
        //选择输出目录
        private void 目录_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                if (dialog.FileName != "")
                {
                    输出路径.Text = dialog.FileName;
                    out_file = dialog.FileName;
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 重新定向输出文件夹：" + dialog.FileName;
                }
            }
            catch { }
        }
        //开始输出
        private void 开始_Click(object sender, RoutedEventArgs e)
        {
            if (Number_file.Count == 0)
            {
                日志.Text += "\n[" + DateTime.Now.ToString() + "]: 您似乎木有选择要解压的 PKG 文件 w(ﾟДﾟ)w";
            }
            else
            {
                if (!Related_functions.Release_file.CheckRePKG(RePKG_Directory))
                {
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 错误：找不到 RePKG.exe";
                    System.Windows.MessageBox.Show("找不到 RePKG.exe！\n无法继续执行解压操作。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                日志.Text += "\n[" + DateTime.Now.ToString() + "]: 检查 RePKG.exe 文件...";

                // 确保输出目录存在
                if (!Directory.Exists(out_file))
                {
                    try
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 输出目录不存在，正在创建：" + out_file;
                        Directory.CreateDirectory(out_file);
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: ✓ 输出目录创建成功";
                    }
                    catch (Exception ex)
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: ✗ 错误！无法创建输出目录：" + ex.Message;
                        System.Windows.MessageBox.Show("无法创建输出目录！\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.DoWork += new DoWorkEventHandler(bw_DoWork2);//建立后台
                    bw.RunWorkerAsync();//开始执行
                }

            }
        }
        //后台执行解压
        static string str = "";//存储返回结果
        void bw_DoWork2(object sender, DoWorkEventArgs e)//后台
        {
            string repkgPath = Related_functions.Release_file.GetRePKGPath(RePKG_Directory);

            // 验证 RePKG.exe 是否存在
            if (!System.IO.File.Exists(repkgPath))
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 错误！找不到 RePKG.exe: " + repkgPath;
                }));
                return;
            }

            for (int i = 0; i < Number_file.Count; i++)
            {
                // 使用 Path.Combine 确保路径格式正确
                string outputPath = Path.Combine(out_file, System.IO.Path.GetFileNameWithoutExtension(Number_file[i].ToString()));

                // 确保输出目录存在
                try
                {
                    // 首先确保根输出目录存在
                    if (!Directory.Exists(out_file))
                    {
                        Directory.CreateDirectory(out_file);
                        Dispatcher.Invoke(new Action(delegate
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 创建输出目录：" + out_file;
                        }));
                    }

                    // 然后确保具体的文件输出目录存在
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                        Dispatcher.Invoke(new Action(delegate
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 创建文件输出目录：" + outputPath;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 错误！无法创建输出目录：" + ex.Message;
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 完整错误：" + ex.ToString();
                    }));
                    return;
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    日志.Text += "\n" + new string('-', 80);
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 开始执行第" + (i + 1) + "个文件";
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 源文件：" + Number_file[i];
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 输出路径：" + outputPath;
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 执行命令：\"" + repkgPath + "\" extract \"" + Number_file[i] + "\" -o \"" + outputPath + "\"";
                }));

                // 执行命令 - 使用 RunExe 直接调用 RePKG.exe
                str = Related_functions.CMD.RunExe(repkgPath, "extract \"" + Number_file[i] + "\" -o \"" + outputPath + "\"");

                Dispatcher.Invoke(new Action(delegate
                {
                    日志.Text += "\n[" + DateTime.Now.ToString() + "]: 命令执行完成";
                    if (!string.IsNullOrEmpty(str))
                    {
                        日志.Text += "\n--- RePKG 输出开始 ---";
                        日志.Text += str;
                        日志.Text += "\n--- RePKG 输出结束 ---";

                        // 检查是否真的解压成功
                        bool hasSuccessIndicator = str.Contains("Done") || str.Contains("extracting") || str.Contains("提取") || Directory.Exists(outputPath);
                        bool hasErrorIndicator = str.Contains("错误") || str.Contains("Error") || str.Contains("不正确") || str.Contains("失败") || str.Contains("[错误输出]");

                        if (hasSuccessIndicator && !hasErrorIndicator)
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: ✓ 解压成功";
                        }
                        else if (hasErrorIndicator)
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: ✗ 解压失败，请检查错误信息";
                        }
                        else if (!Directory.Exists(outputPath))
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: ⚠ 警告：输出目录不存在，可能解压失败";
                        }
                        else
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: ? 解压状态未知，请检查输出";
                        }
                    }
                    else
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 警告：RePKG 没有输出任何内容";
                    }

                    // 验证输出目录
                    if (Directory.Exists(outputPath))
                    {
                        var files = Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories);
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: 输出目录包含 " + files.Length + " 个文件";
                        if (files.Length == 0)
                        {
                            日志.Text += "\n[" + DateTime.Now.ToString() + "]: ⚠ 警告：输出目录为空，解压可能未成功";
                        }
                    }
                    else
                    {
                        日志.Text += "\n[" + DateTime.Now.ToString() + "]: ⚠ 警告：输出目录不存在，解压可能失败";
                    }
                }));
            }
        }
        //完成后返回
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            日志.Text += "\n[" + DateTime.Now.ToString() + "]: 全部解压完毕！";
            Number_file.Clear();
            文件列表.Items.Clear();
            文件下拉框.Header = $"已选择0个对象";
        }

        private void 打开目录__Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(out_file) == false)//判断输出目录是否存在
            {
                日志.Text += "\n[" + DateTime.Now.ToString() + "]: 输出目录不存在,重新创建目录";
                Directory.CreateDirectory(out_file);//创建新路径
            }
            Related_functions.CMD.RunCmd("explorer " + out_file + @"\");
        }

        private void 主窗体_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                if (file.ToLower().EndsWith(".pkg"))
                {
                    if (!Number_file.Contains(file))
                    {
                        Number_file.Add(file);//将元素添加到数组末尾
                        文件列表.Items.Add(file);
                        日志.Text += $"\n[{DateTime.Now}]: 已添加文件：{file}";
                    }
                    else
                    {
                        日志.Text += $"\n[{DateTime.Now}]: 重复文件：{file}";
                    }
                }
                else
                {
                    日志.Text += $"\n[{DateTime.Now}]: 无效文件：{file}";
                }
            }
            文件下拉框.Header = $"已选择{Number_file.Count}个对象";
        }

        private void 主窗体_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
