using System.Windows.Forms;

namespace WinFormsApp1321
{
    public partial class Form1 : Form
    {
        private bool isOn = false; // 按钮状态
        private int currentCycle = 0; // 当前循环次数
        private int totalCycles = 0; // 总循环次数
        private CancellationTokenSource cancellationTokenSource; // 控制循环停止
        private bool isCalibrationMode = false;

        public Form1()
        {
            InitializeComponent();
        }
        public class CalibrationDataParser
        {
            public static Dictionary<string, object> ParseStandardFile(string filePath)
            {
                var dictionary = new Dictionary<string, object>();

                try
                {
                    string[] lines = File.ReadAllLines(filePath);

                    string barcode = null;
                    float tolerance = 0;
                    List<float> defectPositions = new List<float>();

                    // 解析文件内容
                    bool isInDefectPositionsSection = false;

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        // 跳过空行或注释
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                            continue;

                        // 条码
                        if (barcode == null)
                        {
                            barcode = trimmedLine;
                            dictionary["List"] = barcode;
                        }
                        // wc
                        else if (trimmedLine.StartsWith("[Tolerance]"))
                        {
                            isInDefectPositionsSection = false;
                        }
                        else if (trimmedLine.StartsWith("Value="))
                        {
                            tolerance = float.Parse(trimmedLine.Split('=')[1].Trim());
                            dictionary["Tolerance"] = tolerance;
                        }
                        // 缺陷位置
                        else if (trimmedLine.StartsWith("[DefectPositions]"))
                        {
                            isInDefectPositionsSection = true;
                        }
                        else if (isInDefectPositionsSection && trimmedLine.StartsWith("Post"))
                        {
                            var positionStr = trimmedLine.Split('=')[1].Trim();
                            if (float.TryParse(positionStr, out float position))
                            {
                                defectPositions.Add(position);
                            }
                        }
                    }

                    dictionary["DefectPositions"] = defectPositions;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取标样文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return dictionary;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isOn)  // 当前是开启状态，点击后要关闭
            {
                StopCalibration(true);  // 关闭自校准模式并取消任务
            }
            else
            {
                // 选择文件窗口
                SelectionForm selectionForm = new SelectionForm();
                selectionForm.ShowDialog();

                if (selectionForm.DialogResult == DialogResult.OK)
                {
                    // 放入样棒框
                    DialogResult result = MessageBox.Show(
                        $"系统文件：C:\\system\\system.ini\n" +
                        $"标样文件：{selectionForm.StandardFilePath}\n" +
                        $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                        $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                        "放入样棒后点击确认？",
                        "放入样棒",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question
                    );

                    // “取消”，就直接返回
                    if (result == DialogResult.Cancel)
                    {
                        MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    string selectedStandardFile = selectionForm.StandardFilePath;

                    var calibrationData = CalibrationDataParser.ParseStandardFile(selectedStandardFile);


                    totalCycles = selectionForm.CalibrationCount;
                    currentCycle = 0;

                    // 确认后，创建 循环任务 并启动循环
                    cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken token = cancellationTokenSource.Token;
                    /*  var token = cancellationTokenSource.Token;*/
                    Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));

                    //状态
                    isOn = true;
                    button1.Text = "自校准模式已开启";
                    label1.Text = "当前状态：自校准模式";
                    button2.Enabled = false;
                }
            }
        }

       
        private void button2_Click(object sender, EventArgs e)
        {

            if (isOn)  // 当前是检测模式，点击后要关闭检测模式
            {
                StopDetection();  // 停止检测模式并返回待机状态
            }
            else
            {
                // 进入检测模式
                Form2 detectionForm = new Form2();
                detectionForm.Show();  // 显示检测模式界面

                // 禁用自校准按钮
                button1.Enabled = false;

                // 状态更新
                isOn = true;
                button2.Text = "退出检测模式";  // 修改按钮文本为“退出检测模式”
                label1.Text = "当前状态：检测模式";  // 状态显示为检测模式
            }

            /*
                   label1.Text = "当前状态：检测模式";
               isOn = !isOn; // 切换状态
               button2.Text = isOn ? "检测模式已开启" : "检测模式关闭";
               if (isOn == false)
               {
                   label1.Text = "当前状态：待机状态";
               }
               //MessageBox.Show("检测模式已关闭！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
               button1.Enabled = !isOn;*/

        }
        private void StopDetection()
        {
            // 关闭检测模式界面（Form2）
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form2)
                {
                    form.Close();  // 关闭 Form2
                    break;
                }
            }

            // 启用自校准按钮
            button1.Enabled = true;

            // 状态更新
            isOn = false;
            button2.Text = "进入检测模式";  // 修改按钮文本为“进入检测模式”
            label1.Text = "当前状态：待机";  // 状态显示为待机
        }

        private void button3_Click(object sender, EventArgs e)
        {
           /* // 复位状态
            //isCalibrationMode = false;
            isOn = false;


            label1.Text = "当前状态：待机状态";


            button1.Enabled = true;
            button2.Enabled = true;
            button1.Text = "自校准模式关闭";
            button2.Text = "检测模式关闭";

            MessageBox.Show("系统已恢复为待机状态！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
         //   StopCalibration(true);*/
        }
 
        
        private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
        {
            DateTime lastCycleEndTime = DateTime.Now;
            string iniPath = "C:\\system\\system.ini";
            string sampleFolder = "D:\\标样\\yangguang"; // 样管文件夹路径

            int fileIndex = 1; // 样管文件索引

            while (currentCycle < totalCycles)
            {
                if (token.IsCancellationRequested)
                {
                    MessageBox.Show("自校准任务已停止！", "停止", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    StopCalibration();
                    return;
                }

                currentCycle++;
                UpdateCycleLabel();

                // 生成当前循环的样管文件名
                string sampleFile = Path.Combine(sampleFolder, $"样管{fileIndex}.ini");

                // 检查文件是否存在
                if (!File.Exists(sampleFile))
                {
                    MessageBox.Show($"缺少样管文件: {sampleFile}！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StopCalibration();
                    return;
                }

              

                if (currentCycle >= totalCycles)
                {
                    MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    DateTime validUntil = lastCycleEndTime.AddHours(2); // 计算有效期限
                    WriteDeadlineToIni(iniPath, validUntil); // 写入 system.ini
                    UpdateValidUntilLabel(validUntil); // 更新 UI

                    this.Invoke(new Action(() =>
                    {
                        button2.Enabled = true;  // 只有成功完成才启用检测模式
                    }));

                    StopCalibration(false);
                }

                await Task.Delay(10000, token); // 等待 10 秒，进入下一次循环
            }
        }
    

        private void WriteDeadlineToIni(string iniPath, DateTime deadline)
        {
            try
            {
                List<string> lines = new List<string>();

                if (File.Exists(iniPath))
                {
                    lines = File.ReadAllLines(iniPath).ToList();
                }

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("Deadline="))
                    {
                        lines[i] = $"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"; // 直接更新
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    lines.Add($"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"); // 确保一行
                }

                File.WriteAllLines(iniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void UpdateValidUntilLabel(DateTime validUntil)
        {
            if (label3.InvokeRequired)
            {
                label3.Invoke(new Action(() => UpdateValidUntilLabel(validUntil)));
            }
            else
            {
                label3.Text = $"检测有效期限：{validUntil:yyyy-MM-dd HH:mm:ss}";
            }
        }



        private DateTime ReadDeadlineFromIni(string iniPath)
        {
            try
            {
                if (!File.Exists(iniPath))
                    return DateTime.MinValue;

                string[] lines = File.ReadAllLines(iniPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Deadline="))
                    {
                        string deadlineStr = line.Split('=')[1].Trim();
                        if (DateTime.TryParse(deadlineStr, out DateTime deadline))
                        {
                            return deadline;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DateTime.MinValue;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string iniPath = "C:\\system\\system.ini";
            DateTime deadline = ReadDeadlineFromIni(iniPath);
            if (deadline != DateTime.MinValue)
            {
                UpdateValidUntilLabel(deadline);
            }

           Task.Run(() => CheckDeadline()); // 启动检查任务
        }


        private async void CheckDeadline()
        {
            while (true)
            {
                string iniPath = "C:\\system\\system.ini";
                DateTime deadline = ReadDeadlineFromIni(iniPath);
                DateTime now = DateTime.Now;

                if (deadline != DateTime.MinValue)
                {
                    TimeSpan remaining = deadline - now;

                    if (remaining.TotalMinutes <= 60 && remaining.TotalMinutes > 59)
                    {
                        MessageBox.Show("检测有效期即将到期！剩余不到 1 小时。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (remaining.TotalSeconds <= 0)
                    {
                        MessageBox.Show("检测有效期已过期！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 使用 Invoke 确保 UI 线程操作
                        if (button2.InvokeRequired)
                        {
                            button2.Invoke(new Action(() => button2.Enabled = false));
                        }
                        else
                        {
                            button2.Enabled = false;
                        }
                    }
                   /* else
                    {
                        // 使用 Invoke 确保 UI 线程操作
                        if (button2.InvokeRequired)
                        {
                            button2.Invoke(new Action(() => button2.Enabled = true));
                        }
                        else
                        {
                           button2.Enabled = true;
                        }
                    }*/
                }

                await Task.Delay(1800000); // 每 30fz检查一次
            }
        }

    



    private void UpdateCycleLabel()
        {
            if (label2.InvokeRequired)
            {
                // 如果在非UI线程，使用Invoke来回到UI线程更新
                label2.Invoke(new Action(UpdateCycleLabel));
            }
            else
            {
                label2.Text = $"当前循环次数：{currentCycle} / {totalCycles}";
            }
        }

        private void StopCalibration(bool isManualStop = false)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            bool isCalibrationSuccessful = (currentCycle > 0 && currentCycle >= totalCycles);

            currentCycle = 0;
            totalCycles = 0;
            isOn = false;

            this.Invoke(new Action(() =>
            {
                button1.Text = "自校准模式关闭";
                label1.Text = "当前状态：待机状态";
                label2.Text = "当前循环次数：0";

                // 手动停止 or 异常终止，都应该禁用检测模式
                button2.Enabled = isCalibrationSuccessful && !isManualStop;
            }));
        }


        /*  private void StopCalibration()
          {
              if (cancellationTokenSource != null)
              {
                  cancellationTokenSource.Cancel();  // 取消任务
                  cancellationTokenSource.Dispose(); // 释放资源
                  cancellationTokenSource = null;
              }

              currentCycle = 0;
              totalCycles = 0;
              isOn = false;

              // 在UI线程上更新
              this.Invoke(new Action(() =>
              {
                  button1.Text = "自校准模式关闭";
                  label1.Text = "当前状态：待机状态";
                  label2.Text = "当前循环次数：0";
                  button2.Enabled = true;  // 启用按钮2
              }));
          }*/




        /* private Dictionary<string, int> ReadIniValues(string iniPath, string section)
         {
             Dictionary<string, int> values = new Dictionary<string, int>();

             string[] lines = File.ReadAllLines(iniPath);
             bool inSection = false;

             foreach (string line in lines)
             {
                 string trimmedLine = line.Trim();

                 if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                 {
                     inSection = trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase);
                     continue;
                 }

                 if (inSection && trimmedLine.Contains("="))
                 {
                     string[] parts = trimmedLine.Split('=');
                     if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int value))
                     {
                         values[parts[0].Trim()] = value;
                     }
                 }
             }

             return values;
         }*/


        /*  private int ReadIniTolerance(string iniPath)
          {
              string[] lines = File.ReadAllLines(iniPath);
              foreach (string line in lines)
              {
                  if (line.StartsWith("Value=") && int.TryParse(line.Split('=')[1].Trim(), out int tolerance))
                  {
                      return tolerance;
                  }
              }
              return 10; // 默认误差±10
          }
  */
        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }



       /* private void Form1_Load(object sender, EventArgs e)
        {

        }*/


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
