using Microsoft.Win32;
using OpenTK.Graphics.ES10;
using ScottPlot;
using ScottPlot.Palettes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CommTest
{

    // COMポートの　コンボボックス用 
    public class ComPortNameClass
    {
        string _ComPortName;

        public string ComPortName
        {
            get { return _ComPortName; }
            set { _ComPortName = value; }
        }
    }


    // 履歴(ヒストリ)データ　クラス
    // クラス名: HistoryData
    // メンバー:  double  data0
    //            double  data1
    //            double  data2
    //            double  data3
    //            double  data4
    //            double  data5
    //            double  data6
    //            double  data7
    //            double  data8
    //            double  data9
    //            double  data10
    //            double  data11
    //            double  data12
    //            double  dt
    //

    public class HistoryData
    {
        public double data0 { get; set; }       // PV
        public double data1 { get; set; }       // SV
        public double data2 { get; set; }       // MV
        public double data3 { get; set; }       // SLG1_CH0
        public double data4 { get; set; }       // SLG1_CH1
        public double data5 { get; set; }       // SLG1_CH3
        public double data6 { get; set; }       // SLG2_CH0
        public double data7 { get; set; }       // SLG2_CH1
        public double data8 { get; set; }       // SLG3_CH0
        public double data9 { get; set; }       // SLG3_CH1
        public double data10 { get; set; }      // SLG3_CH3
        public double data11 { get; set; }      // SLG4_CH0
        public double data12 { get; set; }      // SLG4_CH1

        public double dt { get; set; }         // 日時 (double型)
    }


    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        
        double ch0_data;          // PV 
        double ch1_data;          // SV
        double ch2_data;          // MV 
        double ch3_data;          // SLG1_CH0
        double ch4_data;          // SLG1_CH1
        double ch5_data;          // SLG1_CH3
        double ch6_data;          // SLG2_CH0
        double ch7_data;          // SLG2_CH1
        double ch8_data;          // SLG3_CH0
        double ch9_data;          // SLG3_CH1
        double ch10_data;         // SLG3_CH3
        double ch11_data;         // SLG4_CH0
        double ch12_data;          // SLG4_CH1


        uint trend_data_item_max;             // 各リアルタイム　トレンドデータの保持数 

        double[] trend_data0;                 // トレンドデータ 0  PV
        double[] trend_data1;                 // トレンドデータ 1  SV            
        double[] trend_data2;                 // トレンドデータ 2  MV
        double[] trend_data3;                 // トレンドデータ 3  SLG1_CH0
        double[] trend_data4;                 // トレンドデータ 4  SLG1_CH1
        double[] trend_data5;                 // トレンドデータ 5  SLG1_CH3
        double[] trend_data6;                 // トレンドデータ 6  SLG2_CH0
        double[] trend_data7;                 // トレンドデータ 7  SLG2_CH1
        double[] trend_data8;                 // トレンドデータ 8  SLG3_CH0
        double[] trend_data9;                 // トレンドデータ 9  SLG3_CH1
        double[] trend_data10;                 // トレンドデータ 10 SLG3_CH3
        double[] trend_data11;                 // トレンドデータ 11 SLG4_CH0
        double[] trend_data12;                 // トレンドデータ 12 SLG4_CH1

        double[] trend_dt;                    // トレンドデータ　収集日時

        ScottPlot.Plottables.Scatter trend_scatter_0; // トレンドデータ0  
        ScottPlot.Plottables.Scatter trend_scatter_1; // トレンドデータ1  
        ScottPlot.Plottables.Scatter trend_scatter_2; // トレンドデータ2  
        ScottPlot.Plottables.Scatter trend_scatter_3; // トレンドデータ3  
        ScottPlot.Plottables.Scatter trend_scatter_4; // トレンドデータ4
        ScottPlot.Plottables.Scatter trend_scatter_5; // トレンドデータ5                                             //
        ScottPlot.Plottables.Scatter trend_scatter_6; // トレンドデータ6  
        ScottPlot.Plottables.Scatter trend_scatter_7; // トレンドデータ7  
        ScottPlot.Plottables.Scatter trend_scatter_8; // トレンドデータ8  
        ScottPlot.Plottables.Scatter trend_scatter_9; // トレンドデータ9
        ScottPlot.Plottables.Scatter trend_scatter_10; // トレンドデータ10                                             //
        ScottPlot.Plottables.Scatter trend_scatter_11; // トレンドデータ11
        ScottPlot.Plottables.Scatter trend_scatter_12; // トレンドデータ12   


        public List<HistoryData> historyData_list;          // ヒストリデータ　データ収集時に使用


        double y_axis_top;                      // Y軸 温度目盛りの上限値
        double y_axis_bottom;                   // Y軸 温度目盛りの下限値

        DispatcherTimer SendIntervalTimer;  // タイマ　モニタ用　電文送信間隔   
        DispatcherTimer RcvWaitTimer;                   　// タイマ　受信待ち用 

        DispatcherTimer WriteWaitTimer;    // タイマ　書き込み送信待ち
        DispatcherTimer ReStartTimer;                    // タイマ データ書き込みコマンド送信後の、モニタ開始用


        public ObservableCollection<ComPortNameClass> ComPortNames;    // 通信ポート(COM1,COM2等)のコレクション 
                                                                       // データバインドするため、ObservableCollection　
        public static SerialPort serialPort;        // シリアルポート

        public static byte[] sendBuf;          // 送信バッファ   
        int sendByteLen;         //　送信データのバイト数

        byte[] rcvBuf;           // 受信バッファ
        int srcv_pt;             // 受信データ格納位置

        DateTime sendDateTime;   // 送信日時
        DateTime receiveDateTime;   // 受信完了日時

        string dot_net_ver;
        public MainWindow()
        {
            InitializeComponent();

            MainWindow.serialPort = new SerialPort();    // シリアルポートのインスタンス生成
            MainWindow.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);  // データ受信時のイベント処理

            ComPortNames = new ObservableCollection<ComPortNameClass>();  // 通信ポートのコレクション　インスタンス生成

            ComPortComboBox.ItemsSource = ComPortNames;       // 通信ポートコンボボックスのアイテムソース指定  

            SetComPortName();                // 通信ポート名をコンボボックスへ設定

            sendBuf = new byte[16];     // 送信バッファ領域  
            rcvBuf = new byte[64];      // 受信バッファ領域   


            SendIntervalTimer = new System.Windows.Threading.DispatcherTimer();　　// タイマーの生成(定周期モニタ用)
            SendIntervalTimer.Tick += new EventHandler(SendIntervalTimer_Tick);  // タイマーイベント
            SendIntervalTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);         // タイマーイベント発生間隔 1sec(コマンド送信周期)

            RcvWaitTimer = new System.Windows.Threading.DispatcherTimer();　 // タイマーの生成(受信待ちタイマ)
            RcvWaitTimer.Tick += new EventHandler(RcvWaitTimer_Tick);        // タイマーイベント
            RcvWaitTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);          // タイマーイベント発生間隔 (受信待ち時間)

            WriteWaitTimer = new System.Windows.Threading.DispatcherTimer();　 // タイマーの生成(書込み待ちタイマ)
            WriteWaitTimer.Tick += new EventHandler(WriteWaitTimer_Tick);        // タイマーイベント
            WriteWaitTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);          // タイマーイベント発生間隔 100[msec](書き込み待ち時間)

            ReStartTimer = new System.Windows.Threading.DispatcherTimer();　　// タイマーの生成(書き込みコマンド送信後のモニタ開始用)
            ReStartTimer.Tick += new EventHandler(ReStartTimer_Tick);         // タイマーイベント
            ReStartTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);           // タイマーイベント発生間隔 (モニタ開始待ち)

            historyData_list = new List<HistoryData>();     // モニタ時のトレンドデータ 記録用　

            Loaded += LoadEvent;      // LoadEvent実行

        }

        //
        // 要素のレイアウトやレンダリングが完了し、操作を受け入れる準備が整ったときに発生
        //
        private void LoadEvent(object sender, EventArgs e)
        {
          
            Chart_Ini();    // チャートの初期表示
        }

        //
        // モニタ開始ボタン
        //
        private void Start_Monitor_Button_Click(object sender, RoutedEventArgs e)
        {
            Monitor_read_cmd();         // モータデータの読み出しコマンドの作成

            SendIntervalTimer.Start();   // 定周期　送信用タイマの開始
        }

        //
        // Test Sendボタンを押した時の処理
        // データの送信
        private void Test_Send_Button_Click(object sender, RoutedEventArgs e)
        {
            Monitor_read_cmd();         // モータデータの 読み出しコマンドの作成

            send_disp_data();   //  送信と送信データの表示
        }

        // 　パラメータ書き込み
        //  Kp, Ti を一括して書き込む
        // Kp(ゲイン):0.1～25.5
        // Ti(積分時間):0.1～25.5[sec]
        //
        //  sendBuf[0]  0x03 :パラメータ書き込みコマンド 
        //         [1]  dummy data 0x00
        //         [2]  dummy data 0x00
        //         [3]  Kp (10倍した値)   Kp=1ならば、10を設定
        //         [4]  Ti (10倍した値)   Ti=1[sec]ならば、10を設定
        //         [5]  dummy data 0x00
        //         [6]  CRC 上位バイト
        //         [7]  CRC 下位バイト
        //
        // 手順: 
        //  1) 入力データの範囲チェック　異常ならばリターン
        //  2) 定周期モニタの停止
        //  3) コマンドと書き込みデータを送信バッファへ格納
        //  4)  WriteWaitTimerを開始
        //  5)  WriteWaitTimerの設定時間を経過 100msec
        //  6)  書き込みコマンド 送信処理
        //  7)  ReStartTimerの開始　1sec待ち (書き込みコマンドのレスポンス待ち)
        //  8) 　定周期モニタの再開
        //
        private void Write_Para_Button_Click(object sender, RoutedEventArgs e)
        {
            bool f_ok = true;

            float.TryParse(Write_Kp_TextBox.Text, out float w_fkp);  // Kp値

            if ((w_fkp < 0.1) || (w_fkp > 25.5))
            {
                f_ok = false;
            }
            Byte w_kp = (Byte)(w_fkp * 10);

            float.TryParse(Write_Ti_TextBox.Text, out float w_fti);  // Ti

            if ((w_fti < 0.1) || (w_fti > 25.5))
            {
                f_ok = false;
            }
            Byte w_ti = (Byte)(w_fti * 10);

            if (f_ok)
            {
                Input_Error_TextBox.Text = "";
            }
            else
            {
                Input_Error_TextBox.Text = "Input Error";
                return;
            }


            SendIntervalTimer.Stop();          //　定周期モニタの停止

            sendBuf[0] = 0x03;     // 送信コマンド 

            sendBuf[1] = 0x00;
            sendBuf[2] = 0x00; 

            sendBuf[3] = w_kp;  // Kp 
            sendBuf[4] = w_ti;  // Ti
            sendBuf[5] = 0x00;

            UInt16 crc_cd = CRC_sendBuf_Cal(6);     // CRC計算

            sendBuf[6] = (Byte)(crc_cd >> 8); // CRCは上位バイト、下位バイトの順に送信
            sendBuf[7] = (Byte)(crc_cd & 0x00ff);

            sendByteLen = 8;               // 送信バイト数

            WriteWaitTimer.Start();             // 送信待ちタイマの起動 100msec待ち
        }


        //
        // 送信データの文字列を得る
        //
        private string get_send_str()
        {
            string send_str = "";

            for (int i = 0; i < sendByteLen; i++)   // 表示用の文字列作成
            {
                if ((i > 0) && (i % 16 == 0))    // 16バイト毎に1行空ける
                {
                    send_str = send_str + "\r\n";
                }

                send_str = send_str + sendBuf[i].ToString("X2") + " ";
            }

            send_str = send_str + "(" + sendDateTime.ToString("HH:mm:ss,fff") + ")";   // 受信データ文字列

            return send_str;
        }


        //
        //  制御データの 読み出しコマンドの作成
        //
        //  sendBuf[0]  0x20 :制御データ読み出しコマンド 
        //         [1]  0x00 : dummy
        //         [2]  0x00 : dummy
        //         [3]  0x00 : dummy
        //         [4]  0x00 : dummy
        //         [5]  0x00 : dummy
        //         [6]  CRC 上位バイト
        //         [7]  CRC 下位バイト
        //
        private void Monitor_read_cmd()
        {
            UInt16 crc_cd;
            
            sendBuf[0] = 0x20;     // 送信コマンド  
            sendBuf[1] = 0x00;
            sendBuf[2] = 0x00;
            sendBuf[3] = 0x00;
            sendBuf[4] = 0x00;
            sendBuf[5] = 0x00;

            crc_cd = CRC_sendBuf_Cal(6);     // CRC計算

            sendBuf[6] = (Byte)(crc_cd >> 8); // CRCは上位バイト、下位バイトの順に送信
            sendBuf[7] = (Byte)(crc_cd & 0x00ff);

            sendByteLen = 8;               // 送信バイト数

        }


        // CRCの計算 (送信バッファ用)
        //  CRC-16 CCITT:
        //  多項式: X^16 + X^12 + X^5 + 1　
        //  初期値: 0xffff
        //  MSBファースト
        //  非反転出力
        // 
        public static UInt16 CRC_sendBuf_Cal(UInt16 size)
        {
            UInt16 crc;

            UInt16 i;

            crc = 0xffff;

            for (i = 0; i < size; i++)
            {
                crc = (UInt16)((crc >> 8) | ((UInt16)((UInt32)crc << 8)));
                crc = (UInt16)(crc ^ (sendBuf[i]));
                crc = (UInt16)(crc ^ (UInt16)((crc & 0xff) >> 4));
                crc = (UInt16)(crc ^ (UInt16)((crc << 8) << 4));
                crc = (UInt16)(crc ^ (((crc & 0xff) << 4) << 1));
            }

            return crc;

        }


        // CRCの計算 (受信バッファ用)
        //  CRC-16 CCITT:
        //  多項式: X^16 + X^12 + X^5 + 1　
        //  初期値: 0xffff
        //  MSBファースト
        //  非反転出力
        // 
        private UInt16 CRC_rcvBuf_Cal(UInt16 size)
        {
            UInt16 crc;

            UInt16 i;

            crc = 0xffff;

            for (i = 0; i < size; i++)
            {
                crc = (UInt16)((crc >> 8) | ((UInt16)((UInt32)crc << 8)));

                crc = (UInt16)(crc ^ rcvBuf[i]);
                crc = (UInt16)(crc ^ (UInt16)((crc & 0xff) >> 4));
                crc = (UInt16)(crc ^ (UInt16)((crc << 8) << 4));
                crc = (UInt16)(crc ^ (((crc & 0xff) << 4) << 1));
            }

            return crc;

        }


        // 定周期モニタ用
        // 
        private void SendIntervalTimer_Tick(object sender, EventArgs e)
        {
            send_disp_data();   //  送信と送信データの表示

        }

        // 
        //  送信と送信データの表示
        // sendBuf[]のデータを、sendByteLenバイト　送信する
        //
        public void send_disp_data()
        {
            if (serialPort.IsOpen == true)
            {
                srcv_pt = 0;                   // 受信データ格納位置クリア

                serialPort.Write(sendBuf, 0, sendByteLen);     // データ送信

                sendDateTime = DateTime.Now;    // 送信日時

                SendTextBox.Text = get_send_str();// 送信データの文字列表示

                RcvWaitTimer.Start();        // 受信監視タイマー　開始
            }

            else
            {
                disp_msg_com_port_closed();     // COM　Port Closedのメッセージボックスの表示

                SendIntervalTimer.Stop();
            }

        }

        //
        // 送信後、1000msec以内に受信文が得られないと、受信エラー
        //  
        private void RcvWaitTimer_Tick(object sender, EventArgs e)
        {
            RcvWaitTimer.Stop();        // 受信監視タイマの停止
            SendIntervalTimer.Stop();   // 定周期モニタ用タイマの停止

            var msg = "Receive time out. \r\n";

            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // メッセージボックスの表示

        }

        // 
        //  定周期モニタが停止した後の待ち時間後の処理
        //  (書き込みコマンドを送信する場合、定周期モニタを停止する。
        // 　定周期モニタ停止後、WriteWaitTimerが開始される。)
        private void WriteWaitTimer_Tick(object sender, EventArgs e)
        {
            WriteWaitTimer.Stop();      //　タイマ停止

            send_disp_data();           // パラメータ書き込みコマンド 送信

            ReStartTimer.Start();        // 定周期モニタ開始待ちタイマの開始　1[sec]待ち　パラメータ書き込みコマンドのレスポンス待ち
        }

        // 書き込みコマンド送信後の、定周期モニタ開始待ち
        private void ReStartTimer_Tick(object sender, EventArgs e)
        {
            ReStartTimer.Stop();                     //タイマ停止

            Monitor_read_cmd();         // モータデータの読み出しコマンドの作成

            SendIntervalTimer.Start();   // 定周期　送信用タイマの開始

        }



        //
        // COM　Port Closedのメッセージボックスの表示
        private void disp_msg_com_port_closed()
        {
            var msg = " COM Port Closed. \r\n";

            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // メッセージボックスの表示
        }

        // モニタ停止ボタン
        private void Stop_Monitor_Button_Click(object sender, RoutedEventArgs e)
        {
            SendIntervalTimer.Stop();     // データ収集用コマンド送信タイマー停止
        }





        // デリゲート関数の宣言
        private delegate void DelegateFn();

        // データ受信時のイベント処理
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            int rd_num = MainWindow.serialPort.BytesToRead;       // 受信データ数

            MainWindow.serialPort.Read(rcvBuf, srcv_pt, rd_num);   // 受信データを読み出して、受信バッファに格納

            srcv_pt = srcv_pt + rd_num;     // 次回の保存位置


            if (((rcvBuf[0] == 0xa0) && (srcv_pt == 46 )) ||  // 読み出しコマンドのレスポンスかつ、受信データ数 = 46 byteの場合　または
                ((rcvBuf[0] == 0x83) && (srcv_pt == 4)))      // 書き込みコマンドのレスポンスかつ、受信データ数 = 4 byteの場合、
            {
                RcvWaitTimer.Stop();        // 受信監視タイマの停止

                Dispatcher.BeginInvoke(new DelegateFn(RcvProc)); // Delegateを生成して、RcvProcを開始   (表示は別スレッドのため)
            }

        }

        //
        // データ受信イベント終了時の処理
        // 受信データの表示
        //
        private void RcvProc()
        {
            RcvTextBox.Text = get_rcv_str();  　   // 受信データ表示

            Disp_monitor_data();   //  モニタ表示とグラフ表示

        }

        //
        // 受信データの文字列を得る
        //
        private string get_rcv_str()
        {
            string rcv_str = "";

            for (int i = 0; i < srcv_pt; i++)   // 表示用の文字列作成
            {
                if ((i > 0) && (i % 16 == 0))    // 16バイト毎に1行空ける
                {
                    rcv_str = rcv_str + "\r\n";
                }

                rcv_str = rcv_str + rcvBuf[i].ToString("X2") + " ";
            }

            receiveDateTime = DateTime.Now;   // 受信完了時刻を得る

            rcv_str = rcv_str + "(" + receiveDateTime.ToString("HH:mm:ss.fff") + ")";   // 受信データ文字列

            return rcv_str;
        }


        // モニタ表示とグラフ表示
        //   受信データ :内容
        //     rcvBuf[0] :  0xa0 (制御データ読み出しコマンドのレスポンス)
        //     rcvBuf[1] :  mode_run_stop (1=Run,0=Stop)
        //     rcvBuf[2] :  Step番号
        //     rcvBuf[3] :  ソーク経過時間[sec]         
        //     rcvBuf[4] :  PV 温度[C]    下位バイト側   (10倍した値 800なら、80.0[℃] ) 
        //     rcvBuf[5] :             :  上位バイト側        
        //     rcvBuf[6] :  SV 温度[C}    下位バイト側   (10倍した値 800なら、80.0[℃] )
        //     rcvBuf[7] :            :   上位バイト側
        //     rcvBuf[8] :　出力(操作量)(MV)(0～100[%]) 
        //     rcvBuf[9] :  比例ゲイン Kp (0.1～25.5) (10倍した値 1ならば0.1)
        //     rcvBuf[10]:  積分時間(I) (0.1～25.5[sec])(10倍した値 1ならば0.1)
        //     rcvBuf[11]:  ダミー(0x00)
        //     rcvBuf[12] :SLG-1 Ch0温度  (下位バイト側) (10倍した値 800なら、80.0[℃])
        //     rcvBuf[13] :        :      (上位バイト側)                            
        //     rcvBuf[14] :SLG-1 Ch1温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )
        //     rcvBuf[15] :               (上位バイト側)
        //     rcvBuf[16] :SLG-1 Ch2温度  (下位バイト側) (未使用) 
        //     rcvBuf[17] :               (上位バイト側)				       
        //     rcvBuf[18] :SLG-1 Ch3温度  (下位バイト側) (10倍した値 250なら、25.0[℃] )     
        //     rcvBuf[19] :  　           (上位バイト側)  
        //     rcvBuf[20] :SLG-2 Ch0温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )    
        //     rcvBuf[21] :               (上位バイト側)
        //     rcvBuf[22] :SLG-2 Ch1温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )    
        //     rcvBuf[23] :               (上位バイト側)	    
        //     rcvBuf[24] :SLG-2 Ch2温度  (下位バイト側) (未使用)       :           
        //     rcvBuf[25] :               (上位バイト側)         
        //     rcvBuf[26] :SLG-2 Ch3温度  (下位バイト側) (未使用)  
        //     rcvBuf[27] :               (上位バイト側)
        //     rcvBuf[28] :SLG-3 Ch0温度  (下位バイト側) (10倍した値 800なら、80.0[℃])
        //     rcvBuf[29] :        :      (上位バイト側)                            
        //     rcvBuf[30] :SLG-3 Ch1温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )
        //     rcvBuf[31] :               (上位バイト側)
        //     rcvBuf[32] :SLG-3 Ch2温度  (下位バイト側) (未使用) 
        //     rcvBuf[33] :               (上位バイト側)				       
        //     rcvBuf[34] :SLG-3 Ch3温度  (下位バイト側) (10倍した値 250なら、25.0[℃] )     
        //     rcvBuf[35] :  　           (上位バイト側)  
        //     rcvBuf[36] :SLG-4 Ch0温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )    
        //     rcvBuf[37] :               (上位バイト側)
        //     rcvBuf[38] :SLG-4 Ch1温度  (下位バイト側) (10倍した値 800なら、80.0[℃] )    
        //     rcvBuf[39] :               (上位バイト側)	    
        //     rcvBuf[40] :SLG-4 Ch2温度  (下位バイト側) (未使用)       :           
        //     rcvBuf[41] :               (上位バイト側)         
        //     rcvBuf[42] :SLG-4 Ch3温度  (下位バイト側) (未使用)    
        //     rcvBuf[43] :               (上位バイト側)
        //     rcvBuf[44] : CRC 上位バイト 
        //     rcvBuf[45] : CRC 下位バイト
        //
        //  ch0_data;  PV 
        //  ch1_data;  SV
        //  ch2_data;  MV 
        //  ch3_data;  SLG1_CH0
        //  ch4_data;  SLG1_CH1
        //  ch5_data;  SLG1_CH3
        //  ch6_data;  SLG2_CH0
        //  ch7_data;  SLG2_CH1
        //  ch8_data;  SLG3_CH0
        //  ch9_data;  SLG3_CH1
        //  ch10_data; SLG3_CH3
        //  ch11_data; SLG4_CH0
        //  ch12_data; SLG4_CH1
        private void Disp_monitor_data()
        {
            UInt16 dt;
            float t_data;

            UInt16 crc_cd = CRC_rcvBuf_Cal(46);         // 全データのCRC計算             

            if (crc_cd != 0)
            {
                AlarmTextBox.Text = "Receive data CRC Err.";
                return;
            }
            else
            {
                AlarmTextBox.Text = "";
            }

            if (rcvBuf[1] == 1)
            {
                Run_Stop_TextBox.Text = "Run";
            }
            else if (rcvBuf[1] == 0)
            {
                Run_Stop_TextBox.Text = "Stop";
            }

            Byte step_n = rcvBuf[2];                    // Step番号
            Step_Number_TextBox.Text = step_n.ToString();
            
            Byte soak_t = rcvBuf[3];                //　ソーク経過時間
            Soak_Time_TextBox.Text = soak_t.ToString();
          
            UInt16 pv_temp = BitConverter.ToUInt16(rcvBuf, 4);    // PV 現在の測定温度[℃]  rcvBuf[4]から uint16へ
            ch0_data = (double)(pv_temp / 10.0);                      
            PV_TextBox.Text = ch0_data.ToString("F1");    // 

            UInt16 sv_temp = BitConverter.ToUInt16(rcvBuf, 6); // SV 目標 設定温度
            ch1_data = (double)(sv_temp / 10.0);
            SV_TextBox.Text = ch1_data.ToString("F1");
          
            Byte t_mv = rcvBuf[8];                  // MV
            ch2_data = (double)t_mv;
            MV_TextBox.Text = ch2_data.ToString("F0");         


            float fl_kp = (float)(rcvBuf[9] / 10.0);      //　比例ゲイン Kp
            Kp_TextBox.Text = fl_kp.ToString("F1");   

            float fl_ti = (float)(rcvBuf[10] / 10.0);     // 積分時間 Ti
            Ti_TextBox.Text = fl_ti.ToString("F1");

            dt = BitConverter.ToUInt16(rcvBuf, 12);    // SLG1 Ch0 rcvBuf[12]から uint16]へ
            ch3_data = (double)(dt / 10.0);
            SLG1_Ch0_TextBox.Text = ch3_data.ToString("F1");

            dt = BitConverter.ToUInt16(rcvBuf, 14);    // SLG1 Ch1
            ch4_data = (double)(dt / 10.0);
            SLG1_Ch1_TextBox.Text = ch4_data.ToString("F1");
            

            // SLG1 Ch2 未使用
            dt = BitConverter.ToUInt16(rcvBuf, 18);    // SLG1 Ch3 基板の熱電対端子付近の温度)
            ch5_data = (double)(dt / 10.0);
            SLG1_Ch3_TextBox.Text = ch5_data.ToString("F1");


            dt = BitConverter.ToUInt16(rcvBuf, 20);    // SLG2 Ch0 rcvBuf[20]から uint16]へ
            ch6_data = (double)(dt / 10.0);
            SLG2_Ch0_TextBox.Text = ch6_data.ToString("F1");

            dt = BitConverter.ToUInt16(rcvBuf, 22);    // SLG2 Ch1
            ch7_data = (double)(dt / 10.0);
            SLG2_Ch1_TextBox.Text = ch7_data.ToString("F1");

            // SLG2 Ch2 未使用
            // SLG2 Ch3 未使用


            dt = BitConverter.ToUInt16(rcvBuf, 28);    // SLG3 Ch0 rcvBuf[28]から uint16]へ
            ch8_data = (double)(dt / 10.0);
            SLG3_Ch0_TextBox.Text = ch8_data.ToString("F1");

            dt = BitConverter.ToUInt16(rcvBuf, 30);    // SLG3 Ch1
            ch9_data = (double)(dt / 10.0);
            SLG3_Ch1_TextBox.Text = ch9_data.ToString("F1");

            // SLG3 Ch2 未使用

            dt = BitConverter.ToUInt16(rcvBuf, 34);    // SLG3 Ch3
            ch10_data = (double)(dt / 10.0);
            SLG3_Ch3_TextBox.Text = ch10_data.ToString("F1");


            dt = BitConverter.ToUInt16(rcvBuf, 36);    // SLG4 Ch0 rcvBuf[36]から uint16]へ
            ch11_data = (double)(dt / 10.0);
            SLG4_Ch0_TextBox.Text = ch11_data.ToString("F1");

            dt = BitConverter.ToUInt16(rcvBuf, 38);    // SLG4 Ch1
            ch12_data = (double)(dt / 10.0);
            SLG4_Ch1_TextBox.Text = ch12_data.ToString("F1");


            Store_History();                // ヒストリデータとして保持

            Chart_update();                 // チャートの更新

        }


        //
        //  ヒストリデータとして保持
        //
        //  ch0_data;  PV 
        //  ch1_data;  SV
        //  ch2_data;  MV 
        //  ch3_data;  SLG1_CH0
        //  ch4_data;  SLG1_CH1
        //  ch5_data;  SLG1_CH3
        //  ch6_data;  SLG2_CH0
        //  ch7_data;  SLG2_CH1
        //  ch8_data;  SLG3_CH0
        //  ch9_data;  SLG3_CH1
        //  ch10_data; SLG3_CH3
        //  ch11_data; SLG4_CH0
        //  ch12_data; SLG4_CH1
        private void Store_History()
        {

            HistoryData historyData = new HistoryData();     // 保存用ヒストリデータ

            historyData.data0 = ch0_data;
            historyData.data1 = ch1_data;
            historyData.data2 = ch2_data;
            historyData.data3 = ch3_data;
            historyData.data4 = ch4_data;
            historyData.data5 = ch5_data;
            historyData.data6 = ch6_data;
            historyData.data7 = ch7_data;
            historyData.data8 = ch8_data;
            historyData.data9 = ch9_data;
            historyData.data10 = ch10_data;
            historyData.data11 = ch11_data;
            historyData.data12 = ch12_data;

            historyData.dt = receiveDateTime.ToOADate();   // 受信日時を deouble型で格納

            historyData_list.Add(historyData);          // Listへ保持

        }


        //
        //   チャートの更新
        private void Chart_update()
        {
            // 1スキャン前のデータを移動後、最新のデータを入れる
            Array.Copy(trend_data0, 1, trend_data0, 0, trend_data_item_max - 1);
            trend_data0[trend_data_item_max - 1] = ch0_data;

            Array.Copy(trend_data1, 1, trend_data1, 0, trend_data_item_max - 1);
            trend_data1[trend_data_item_max - 1] = ch1_data;

            Array.Copy(trend_data2, 1, trend_data2, 0, trend_data_item_max - 1);
            trend_data2[trend_data_item_max - 1] = ch2_data;

            Array.Copy(trend_data3, 1, trend_data3, 0, trend_data_item_max - 1);
            trend_data3[trend_data_item_max - 1] = ch3_data;

            Array.Copy(trend_data4, 1, trend_data4, 0, trend_data_item_max - 1);
            trend_data4[trend_data_item_max - 1] = ch4_data;

            Array.Copy(trend_data5, 1, trend_data5, 0, trend_data_item_max - 1);
            trend_data5[trend_data_item_max - 1] = ch5_data;

            Array.Copy(trend_data6, 1, trend_data6, 0, trend_data_item_max - 1);
            trend_data6[trend_data_item_max - 1] = ch6_data;

            Array.Copy(trend_data7, 1, trend_data7, 0, trend_data_item_max - 1);
            trend_data7[trend_data_item_max - 1] = ch7_data;

            Array.Copy(trend_data8, 1, trend_data8, 0, trend_data_item_max - 1);
            trend_data8[trend_data_item_max - 1] = ch8_data;

            Array.Copy(trend_data9, 1, trend_data9, 0, trend_data_item_max - 1);
            trend_data9[trend_data_item_max - 1] = ch9_data;

            Array.Copy(trend_data10, 1, trend_data10, 0, trend_data_item_max - 1);
            trend_data10[trend_data_item_max - 1] = ch10_data;

            Array.Copy(trend_data11, 1, trend_data11, 0, trend_data_item_max - 1);
            trend_data11[trend_data_item_max - 1] = ch11_data;

            Array.Copy(trend_data12, 1, trend_data12, 0, trend_data_item_max - 1);
            trend_data12[trend_data_item_max - 1] = ch12_data;

            Array.Copy(trend_dt, 1, trend_dt, 0, trend_data_item_max - 1);
            trend_dt[trend_data_item_max - 1] = receiveDateTime.ToOADate();    // 受信日時 double型に変換して、格納

            Axis_make();            // 軸の作成

            wpfPlot_Trend.Refresh();    // リアルタイム グラフの更新 (上のグラフ)
            wpfPlot_Trend_AD.Refresh(); //  リアルタイム グラフの更新 (下のグラフ)

        }


        //
        // 　チャートの初期化(リアルタイム　チャート用)
        //
        //  ch0_data;  PV 
        //  ch1_data;  SV
        //  ch2_data;  MV 
        //  ch3_data;  SLG1_CH0
        //  ch4_data;  SLG1_CH1
        //  ch5_data;  SLG1_CH3
        //  ch6_data;  SLG2_CH0
        //  ch7_data;  SLG2_CH1
        //  ch8_data;  SLG3_CH0
        //  ch9_data;  SLG3_CH1
        //  ch10_data; SLG3_CH3
        //  ch11_data; SLG4_CH0
        //  ch12_data; SLG4_CH1
        //

        private void Chart_Ini()
        {
            trend_data_item_max = 30;             // 各リアルタイム　トレンドデータの保持数(=30 ) 1秒毎に収集すると、30秒分のデータ

            trend_data0 = new double[trend_data_item_max];      // トレンドデータ 0  PV (上のグラフ)
            trend_data1 = new double[trend_data_item_max];      // トレンドデータ 1  SV (上のグラフ)
            trend_data2 = new double[trend_data_item_max];      // トレンドデータ 2  MV (上のグラフ)
            trend_data3 = new double[trend_data_item_max];      // トレンドデータ 3  SLG1のCH0 (上のグラフ)
            trend_data4 = new double[trend_data_item_max];      // トレンドデータ 4  SLG1のCH1 (上のグラフ)  
            trend_data5 = new double[trend_data_item_max];      // トレンドデータ 5  SLG1のCH3 (上のグラフ)
            trend_data6 = new double[trend_data_item_max];      // トレンドデータ 6  SLG2のCH0 (上のグラフ)
            trend_data7 = new double[trend_data_item_max];      // トレンドデータ 7  SLG2のCH1 (上のグラフ)  

            trend_data8 = new double[trend_data_item_max];      // トレンドデータ 8  SLG3のCH0 (下のグラフ)
            trend_data9 = new double[trend_data_item_max];      // トレンドデータ 9  SLG3のCH1 (下のグラフ)  
            trend_data10 = new double[trend_data_item_max];     // トレンドデータ 10  SLG4のCH3 (下のグラフ)
            trend_data11 = new double[trend_data_item_max];     // トレンドデータ 11 SLG4のCH0 (下のグラフ)
            trend_data12 = new double[trend_data_item_max];     // トレンドデータ 12 SLG4のCH1 (下のグラフ)  

            trend_dt = new double[trend_data_item_max];

            DateTime datetime = DateTime.Now;   // 現在の日時

            DateTime[] myDates = new DateTime[trend_data_item_max];  // 日時型

            for (int i = 0; i < trend_data_item_max; i++)  // 初期値の設定
            {
                trend_data0[i] = 20 + (i * 3);            // PV
                trend_data1[i] = 140;                     // SV
                trend_data2[i] = 50;                      // MV
                trend_data3[i] = 20 + i;                  // SLG1 CH0
                trend_data4[i] = 30 + i;                  // SLG1 CH1
                trend_data5[i] = 40 + i;                  // SLG1 CH3
                trend_data6[i] = 50 + i;                  // SLG2 CH0
                trend_data7[i] = 60 + i;                  // SLG2 CH1

                trend_data8[i] = 120 + i;                  // SLG3 CH0
                trend_data9[i] = 130 + i;                  // SLG3 CH1
                trend_data10[i] = 140 + i;                 // SLG3 CH3
                trend_data11[i] = 150 + i;                 // SLG4 CH0
                trend_data12[i] = 160 + i;                 // SLG4 CH1

                myDates[i] = datetime + new TimeSpan(0, 0, i);  // i秒増やす

                trend_dt[i] = myDates[i].ToOADate();   // (現在の日時 + i 秒)をdouble型に変換
            }


            trend_scatter_0 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data0, ScottPlot.Colors.Blue); //  PV  (上のグラフ)
            trend_scatter_1 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data1, ScottPlot.Colors.DarkGray); // SV (上のグラフ)
            trend_scatter_2 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data2, ScottPlot.Colors.Red);  // MV (上のグラフ)

            trend_scatter_3 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data3, ScottPlot.Colors.DarkCyan);  // SLG1 CH0 (上のグラフ)
            trend_scatter_4 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data4, ScottPlot.Colors.DarkRed);   // SLG1 CH1 (上のグラフ)
            trend_scatter_5 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data5, ScottPlot.Colors.Green);  // SLG1 CH3 (上のグラフ)
            
            trend_scatter_6 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data6, ScottPlot.Colors.DarkOrange);      //  SLG2 CH0 (上のグラフ)
            trend_scatter_7 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data7, ScottPlot.Colors.DarkMagenta);  // SLG2 CH1 (上のグラフ)


            trend_scatter_8 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data8, ScottPlot.Colors.DarkCyan);  // SLG3 CH0 (上のグラフ)
            trend_scatter_9 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data9, ScottPlot.Colors.DarkRed);   // SLG3 CH1 (上のグラフ)
            trend_scatter_10 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data10, ScottPlot.Colors.Green);  // SLG3 CH3 (上のグラフ)

            trend_scatter_11 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data11, ScottPlot.Colors.DarkOrange);      //  SLG4 CH0 (上のグラフ)
            trend_scatter_12 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data12, ScottPlot.Colors.DarkMagenta);  // SLG4 CH1 (上のグラフ)


            trend_scatter_0.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;   // 上のグラフ Y軸 左側 (PVは、左のY軸を使用)
            trend_scatter_1.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;   // 上のグラフ Y軸 左側 (SVは、左のY軸を使用)
            trend_scatter_2.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Right; // 上のグラフ Y軸 右側 (MV値は、右のY軸)
            trend_scatter_3.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH0は、左のY軸)
            trend_scatter_4.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH1は、左のY軸を使用)
            trend_scatter_5.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH3は、左のY軸を使用)
            trend_scatter_6.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG2 CH0は、左のY軸を使用)
            trend_scatter_7.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG2 CH1は、左のY軸を使用)

            trend_scatter_8.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH0は、左のY軸)
            trend_scatter_9.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH1は、左のY軸)
            trend_scatter_10.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH3は、左のY軸)
            trend_scatter_11.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG4 CH0は、左のY軸)
            trend_scatter_12.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG4 CH1は、左のY軸)

            wpfPlot_Trend.UserInputProcessor.IsEnabled = false;     // マウスによるパン(グラフの移動)、ズーム(グラフの拡大、縮小)の操作禁止
            wpfPlot_Trend_AD.UserInputProcessor.IsEnabled = false;


            Axis_make();            // 軸の作成 
         
            // 凡例の表示
            // 参考:scottplot.net/cookbook/5.0/Legend/
            //
            wpfPlot_Trend.Plot.Legend.FontSize = 24;
            wpfPlot_Trend_AD.Plot.Legend.FontSize = 24;

            trend_scatter_0.LegendText = "PV";      
            trend_scatter_1.LegendText = "SV";     
            trend_scatter_2.LegendText = "MV";      
            trend_scatter_3.LegendText = "SLG1-CH0";     
            trend_scatter_4.LegendText = "SLG1-CH1";    
            trend_scatter_5.LegendText = "SLG1-CH3";
            trend_scatter_6.LegendText = "SLG2-CH0";
            trend_scatter_7.LegendText = "SLG2-CH1";

            trend_scatter_8.LegendText = "SLG3-CH0";
            trend_scatter_9.LegendText = "SLG3-CH1";
            trend_scatter_10.LegendText = "SLG3-CH3";
            trend_scatter_11.LegendText = "SLG4-CH0";
            trend_scatter_12.LegendText = "SLG4-CH1";

            wpfPlot_Trend.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Vertical);
            wpfPlot_Trend_AD.Plot.ShowLegend(Alignment.UpperLeft,ScottPlot.Orientation.Vertical);   

            wpfPlot_Trend.Refresh();        // データ変更後のリフレッシュ (上のグラフ用)
            wpfPlot_Trend_AD.Refresh();     // データ変更後のリフレッシュ (下のグラフ用)

        }

        //
        // 　軸の作成 
        //　上のグラフ: PV,SV,MV
        //  下のグラフ: 

        private void Axis_make()
        {
        
            // X軸の日時リミットを、最終日時+1秒にする
            DateTime dt_end = DateTime.FromOADate(trend_dt[trend_data_item_max - 1]); // double型を　DateTime型に変換
            TimeSpan dt_sec = new TimeSpan(0, 0, 1);    // 1 秒
            DateTime dt_limit = dt_end + dt_sec;      // DateTime型(最終日時+ 1秒) 
            double dt_ax_limt = dt_limit.ToOADate();   // double型(最終日時+ 1秒) 

            //wpfPlot_Trend.Plot.Axes.SetLimits(trend_dt[0], dt_ax_limt, y_axis_bottom, y_axis_top);  // 上のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒,Y軸下限=0, Y軸上限=2000
            
            wpfPlot_Trend.Plot.Axes.SetLimitsX(trend_dt[0],dt_ax_limt);     // // 上のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒
            wpfPlot_Trend.Plot.Axes.SetLimitsY(0, 250, yAxis: wpfPlot_Trend.Plot.Axes.Left);      // PV,SV 上のグラフ Y軸 (左側)  下限=0, 上限=250[℃]
            wpfPlot_Trend.Plot.Axes.SetLimitsY(0, 110, yAxis: wpfPlot_Trend.Plot.Axes.Right);       // MV    上のグラフ Y軸 (右側)  下限=0, 上限=100[%]

            //wpfPlot_Trend_AD.Plot.Axes.SetLimits(trend_dt[0], dt_ax_limt, 0, 4095);  // 下のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒,Y軸下限=0, Y軸上限=4095
            wpfPlot_Trend_AD.Plot.Axes.SetLimitsX(trend_dt[0], dt_ax_limt);            // 下のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒
            wpfPlot_Trend_AD.Plot.Axes.SetLimitsY(0, 250, yAxis: wpfPlot_Trend_AD.Plot.Axes.Left);      // 下のグラフ Y軸 (左側)  下限=0, 上限=250

            custom_ticks();                             // X軸の目盛りのカスタマイズ
            set_y_axes_label();                         // 上のグラフ用 Y軸(左側、右側)のラベル.
            set_y_axes_label_ad();                      // 下のグラフ用 Y軸
        }


        // 上のグラフ用
        //  Y軸(左側、右側)のラベル
        //　左|                |右
        //   C|                |MV %
        //    |                |
        //    +----------------+
        //
        private void set_y_axes_label()
        {
            wpfPlot_Trend.Plot.Axes.Left.Label.FontName = "Meyrio UI";      // Y軸(左側) ラベルのフォント名
            wpfPlot_Trend.Plot.Axes.Left.Label.FontSize = 24;               // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_Trend.Plot.Axes.Left.Label.Text = "C";                // Y軸(左側) ラベル (scottplot.net/cookbook/5.0/Styling/AxisCustom/)

            wpfPlot_Trend.Plot.Axes.Right.Label.FontName = "Meyrio UI";     // Y軸(右側) ラベルのフォント名
            wpfPlot_Trend.Plot.Axes.Right.Label.FontSize = 24;              // Y軸(右側) ラベルのフォントサイズ変更  :
            wpfPlot_Trend.Plot.Axes.Right.Label.Text = "MV %";                 // Y軸(右側) ラベル 

        }

        // 下のグラフ用
        //  Y軸(左側)のラベル
        //　左|                |
        //  C |                |
        //    |                |
        //    +----------------+
        //
        private void set_y_axes_label_ad()
        {
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.FontName = "Meyrio UI";      // Y軸(左側) ラベルのフォント名
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.FontSize = 24;               // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.Text = "C";           // Y軸(左側) ラベル (scottplot.net/cookbook/5.0/Styling/AxisCustom/)

        }


        //
        //  目盛りのカスタマイズ 
        // 参考: scottplot.net/cookbook/5.0/CustomizingTicks/
        //
        //       Custom Tick DateTimes
        // Users may define custom ticks using DateTime units
        // 
        private void custom_ticks()
        {
            DateTime dt;
            string label;

            // create a manual DateTime tick generator and add ticks
            ScottPlot.TickGenerators.DateTimeManual ticks = new ScottPlot.TickGenerators.DateTimeManual();

            //for (int i = 0; i < trend_data_item_max; i++)  // 1秒毎に目盛りのラベル表示
            //{
            //    DateTime dt = DateTime.FromOADate(trend_dt[i]);
            //    string label = dt.ToString("HH:mm:ss");
            //    ticks.AddMajor(dt, label);
            //}


            dt = DateTime.FromOADate(trend_dt[1]);  // 先頭 + 1の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);

            UInt16 t = (ushort)(trend_data_item_max / 2);
            dt = DateTime.FromOADate(trend_dt[t]);  // 中間の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);

            dt = DateTime.FromOADate(trend_dt[trend_data_item_max - 1]);  // 最後の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);
                                                                             // 上のグラフ用
            wpfPlot_Trend.Plot.Axes.Bottom.TickGenerator = ticks;    　　　　// tell the horizontal axis to use the custom tick generator
            wpfPlot_Trend.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;     //  X軸　目盛りのフォントサイズ
            wpfPlot_Trend.Plot.Axes.Left.TickLabelStyle.FontSize = 24;       //  Y軸(左側)　目盛りのフォントサイズ
            wpfPlot_Trend.Plot.Axes.Right.TickLabelStyle.FontSize = 24;      //  Y軸(右側)  目盛りのフォントサイズ

                                                                             // 下のグラフ用
            wpfPlot_Trend_AD.Plot.Axes.Bottom.TickGenerator = ticks;    　　 // tell the horizontal axis to use the custom tick generator
            wpfPlot_Trend_AD.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;  //  X軸　目盛りのフォントサイズ
            wpfPlot_Trend_AD.Plot.Axes.Left.TickLabelStyle.FontSize = 24;    //  Y軸(左側)　目盛りのフォントサイズ
            wpfPlot_Trend_AD.Plot.Axes.Right.TickLabelStyle.FontSize = 24;    //  Y軸(左側)　目盛りのフォントサイズ

        }


        // チェックボックスによるトレンド線の表示 
        //
        //  ch0_data;  PV 
        //  ch1_data;  SV
        //  ch2_data;  MV 
        //  ch3_data;  SLG1_CH0
        //  ch4_data;  SLG1_CH1
        //  ch5_data;  SLG1_CH3
        //  ch6_data;  SLG2_CH0
        //  ch7_data;  SLG2_CH1
        //  ch8_data;  SLG3_CH0
        //  ch9_data;  SLG3_CH1
        //  ch10_data; SLG3_CH3
        //  ch11_data; SLG4_CH0
        //  ch12_data; SLG4_CH1
        private void CH_N_Show(object sender, RoutedEventArgs e)
        {

            if (trend_scatter_0 is null) return;
            if (trend_scatter_1 is null) return;
            if (trend_scatter_2 is null) return;
            if (trend_scatter_3 is null) return;
            if (trend_scatter_4 is null) return;
            if (trend_scatter_5 is null) return;
            if (trend_scatter_6 is null) return;
            if (trend_scatter_7 is null) return;
            if (trend_scatter_8 is null) return;
            if (trend_scatter_9 is null) return;
            if (trend_scatter_10 is null) return;
            if (trend_scatter_11 is null) return;
            if (trend_scatter_12 is null) return;

            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "PV_CheckBox")
            {
                trend_scatter_0.IsVisible = true;
            }
            else if (checkBox.Name == "SV_CheckBox")
            {
                trend_scatter_1.IsVisible = true;
            }
            else if (checkBox.Name == "MV_CheckBox")
            {
                trend_scatter_2.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                trend_scatter_3.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                trend_scatter_4.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                trend_scatter_5.IsVisible = true;
            }
            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                trend_scatter_6.IsVisible = true;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                trend_scatter_7.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                trend_scatter_8.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                trend_scatter_9.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                trend_scatter_10.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                trend_scatter_11.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                trend_scatter_12.IsVisible = true;
            }


            wpfPlot_Trend.Refresh();   // グラフの更新 (上のグラフ)
            wpfPlot_Trend_AD.Refresh();  // グラフの更新 (下のグラフ)
        }

        // チェックボックスによるトレンド線の非表示
        private void CH_N_Hide(object sender, RoutedEventArgs e)
        {
            if (trend_scatter_0 is null) return;
            if (trend_scatter_1 is null) return;
            if (trend_scatter_2 is null) return;
            if (trend_scatter_3 is null) return;
            if (trend_scatter_4 is null) return;
            if (trend_scatter_5 is null) return;

            if (trend_scatter_6 is null) return;
            if (trend_scatter_7 is null) return;
            if (trend_scatter_8 is null) return;
            if (trend_scatter_9 is null) return;
            if (trend_scatter_10 is null) return;
            if (trend_scatter_11 is null) return;
            if (trend_scatter_12 is null) return;

            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "PV_CheckBox")
            {
                trend_scatter_0.IsVisible = false;
            }
            else if (checkBox.Name == "SV_CheckBox")
            {
                trend_scatter_1.IsVisible = false;
            }
            else if (checkBox.Name == "MV_CheckBox")
            {
                trend_scatter_2.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                trend_scatter_3.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                trend_scatter_4.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                trend_scatter_5.IsVisible = false;
            }
            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                trend_scatter_6.IsVisible = false;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                trend_scatter_7.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                trend_scatter_8.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                trend_scatter_9.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                trend_scatter_10.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                trend_scatter_11.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                trend_scatter_12.IsVisible = false;
            }


            wpfPlot_Trend.Refresh();   // グラフの更新 (上のグラフ)
            wpfPlot_Trend_AD.Refresh();  // グラフの更新 (下のグラフ)

        }


        //
        // 保持しているデータをファイルへ保存
        //  ch0_data;  PV 
        //  ch1_data;  SV
        //  ch2_data;  MV 
        //  ch3_data;  SLG1_CH0
        //  ch4_data;  SLG1_CH1
        //  ch5_data;  SLG1_CH3
        //  ch6_data;  SLG2_CH0
        //  ch7_data;  SLG2_CH1
        //  ch8_data;  SLG3_CH0
        //  ch9_data;  SLG3_CH1
        //  ch10_data; SLG3_CH3
        //  ch11_data; SLG4_CH0
        //  ch12_data; SLG4_CH1
        //

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            string path;

            string str_one_line;

            SaveFileDialog sfd = new SaveFileDialog();           //　SaveFileDialogクラスのインスタンスを作成 

            sfd.FileName = "temp_trend.csv";                              //「ファイル名」で表示される文字列を指定する

            sfd.Title = "保存先のファイルを選択してください。";        //タイトルを設定する 

            sfd.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする

            if (sfd.ShowDialog() == true)            //ダイアログを表示する
            {
                path = sfd.FileName;

                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);

                    str_one_line = DataMemoTextBox.Text; // メモ欄
                    sw.WriteLine(str_one_line);         // 1行保存


                    str_one_line = "DateTime" + "," + "PV " + "," + "SV" + "," + "MV" + "," +
                                    "SLG1_CH0" + "," + "SLG1_CH1" + "," + "SLG1_CH3" + "," +
                                    "SLG2_CH0" + "," + "SLG2_CH1" + "," +
                                    "SLG3_CH0" + "," + "SLG3_CH1" + "," + "SLG3_CH3" + "," +
                                    "SLG4_CH0" + "," + "SLG4_CH1";

                    sw.WriteLine(str_one_line);         // 1行保存

                    foreach (HistoryData historyData in historyData_list)         // historyData_listの内容を保存
                    {
                        DateTime dateTime = DateTime.FromOADate(historyData.dt); // 記録されている日時(double型)を　DateTime型に変換

                        string st_dateTime = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");             // DateTime型を文字型に変換　（2021/10/22 11:09:06.125 )

                        string st_dt0 = historyData.data0.ToString();       // PV 
                        string st_dt1 = historyData.data1.ToString();       // SV
                        string st_dt2 = historyData.data2.ToString();       // MV
                        string st_dt3 = historyData.data3.ToString();       // SLG1_CH0
                        string st_dt4 = historyData.data4.ToString();       // SLG1_CH1
                        string st_dt5 = historyData.data5.ToString();       // SLG1_CH3
                        string st_dt6 = historyData.data6.ToString();       // SLG2_CH0
                        string st_dt7 = historyData.data7.ToString();       // SLG2_CH1
                        string st_dt8 = historyData.data8.ToString();       // SLG3_CH0
                        string st_dt9 = historyData.data9.ToString();       // SLG3_CH1
                        string st_dt10 = historyData.data10.ToString();       // SLG3_CH3
                        string st_dt11 = historyData.data11.ToString();       // SLG4_CH0
                        string st_dt12 = historyData.data12.ToString();       // SLG4_CH1


                        str_one_line = st_dateTime + "," + st_dt0 + "," + st_dt1 + "," + st_dt2 + "," + st_dt3 + "," + st_dt4 + "," + st_dt5 + "," +
                                       st_dt6 + "," + st_dt7 + "," + st_dt8 + "," + st_dt9 + "," + st_dt10 + "," + st_dt11 + "," + st_dt12;

                        sw.WriteLine(str_one_line);         // 1行保存
                    }

                    sw.Close();
                }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }


        // 収集済みのデータをクリアの確認
        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "収集済みのデータがクリアされます。";
            string caption = "Check clear";

            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

            switch (result)
            {
                case MessageBoxResult.Yes:      // Yesを押した場合
                    historyData_list.Clear();   // 収集済みのデータのクリア
                    break;

                case MessageBoxResult.No:
                    break;

                case MessageBoxResult.Cancel:
                    break;
            }
        }

        // トレンド 履歴画面
        private void History_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new HistoryWindow();      // 注意メッセージのダイアログを開く
            window.Owner = this;
            window.Show();
        }




        // 通信ポート名をコンボボックスへ設定
        private void SetComPortName()
        {
            ComPortNames.Clear();           // 通信ポートのコレクション　クリア

            string[] PortList = SerialPort.GetPortNames();              // 存在するシリアルポート名が配列の要素として得られる。

            foreach (string PortName in PortList)
            {
                ComPortNames.Add(new ComPortNameClass { ComPortName = PortName }); // シリアルポート名の配列を、コレクションへコピー
            }

            if (ComPortNames.Count > 0)
            {
                ComPortComboBox.SelectedIndex = 0;   // 最初のポートを選択
                ComPortOpenButton.IsEnabled = true;  // ポートOPENボタンを「有効」にする。

                //OpenInfoTextBox.Text = "(" + serialPort.PortName + ") is opened.";
                //ComPortOpenButton.Content = "Close";      // ボタン表示 Close

            }
            else
            {
                ComPortOpenButton.IsEnabled = false;  // ポートOPENボタンを「無効」にする。
                OpenInfoTextBox.Text = "COM port is not found.";
            }


        }


        // Findボタンを押した時の処理
        // 通信ポートの検索ボタン
        //
        private void ComPortSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SetComPortName();
        }


        // Openボタンを押した時の処理
        // 通信ポートのオープン
        //
        //  SerialPort.ReadBufferSize = 4096 byte (デフォルト)
        //             WriteBufferSize =2048 byte
        //
        private void ComPortOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort.IsOpen == true)    // 既に Openしている場合
            {
                try
                {
                    serialPort.Close();

                    OpenInfoTextBox.Text = "Close(" + serialPort.PortName + ")";

                    ComPortComboBox.IsEnabled = true;        // 通信条件等を選択できるようにする。
                   // ComPortSearchButton.IsEnabled = true;    // 通信ポート検索ボタンを有効とする。
                    ComPortOpenButton.Content = "Open"; 　　 // ボタン表示を Closeから Openへ

                }
                catch (Exception ex)
                {
                    OpenInfoTextBox.Text = ex.Message;
                }

            }
            else                      // Close状態からOpenする場合
            {
                serialPort.PortName = ComPortComboBox.Text;    // 選択したシリアルポート


                //  serialPort.BaudRate = 750000;           // ボーレート 750[Kbps]
                
                serialPort.BaudRate = 1000000;             // ボーレート 1[Mbps]
                                                         
                //  serialPort.BaudRate = 1500000;         // ボーレート 1.5[Mbps]


                BaudrateTextBox.Text = serialPort.BaudRate.ToString();  // ボーレート表示


                serialPort.Parity = Parity.None;       // パリティ無し
                serialPort.StopBits = StopBits.One;    //  1 ストップビット

                try
                {
                    serialPort.Open();             // シリアルポートをオープンする
                    serialPort.DiscardInBuffer();  // 受信バッファのクリア


                    ComPortComboBox.IsEnabled = false;        // 通信条件等を選択不可にする。

                 //   ComPortSearchButton.IsEnabled = false;    // 通信ポート検索ボタンを無効とする。

                    OpenInfoTextBox.Text = " Open (" + serialPort.PortName + ")";

                    ComPortOpenButton.Content = "Close";      // ボタン表示を OpenからCloseへ
                }
                catch
                {
                    OpenInfoTextBox.Text = "(" + serialPort.PortName + ") is opend by another.";
                }
            }
        }

    }
}
