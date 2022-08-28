//=============================================//
// Tcp_comのソース
// Tcp_com /Hでコマンド引数使い方をチェックする
// make N.Tanaka 2024/09/01リリース
// Open souse形式
// Gitのレポジトリにアップする場合
//　　エラーはもちろん、ワーニングもない状態で
//=============================================//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace TCP_communication
{
    //このクラスはぱくってきたのでノーコメント
    class IPProgram
    { 
        static IEnumerable<IPAddress> GetIPAddresses(bool excludeLocalHost = false, bool excludeIPv6 = false)
        {
            var ipaddresses = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .SelectMany(x => x
                .GetIPProperties()
                .UnicastAddresses
                .Select(y => y.Address));

            if (excludeLocalHost)
            {
                ipaddresses = ipaddresses
                .Where(x => !x.Equals(IPAddress.Parse("127.0.0.1")))
                .Where(x => !x.Equals(IPAddress.Parse("::1")));
            }

            if (excludeIPv6)
            {
                ipaddresses = ipaddresses
                .Where(x => x.AddressFamily != AddressFamily.InterNetworkV6);
            }

            return ipaddresses;
        }

        public static string IpGet(char ip_param)
        {
            if (ip_param == 'L'){
                foreach (var x in GetIPAddresses())
                {
                    Console.WriteLine(x);
                    return(x.ToString());
                }

            }

            if (ip_param == 'A')
            {
                Console.WriteLine("ローカルホストを省く");
                foreach (var x in GetIPAddresses(excludeLocalHost: true))
                {
                    Console.WriteLine(x);
                    return(x.ToString());
                }

            }

            if (ip_param == '4')
            {
                Console.WriteLine("IPv6を省く");
                while(true)
                {
                    foreach (var x in GetIPAddresses(excludeIPv6: true))
                    {
                        Console.WriteLine(x);
                        Console.WriteLine("yes = y or no = n");
                        if (Console.ReadLine() == "y")
                        {
                            return(x.ToString());

                        }
                    }

                }

            }   

            Console.WriteLine("Command ERR " + ip_param);
            gVariables.setProgFlg(false);
            return("192.168.1.0");
        }
    }

    //ヘルプ表示クラス
    class TcpHelp {
        //プログラム引数の説明
        public static void programHelp()
        {
            Console.WriteLine("プログラム引数ヘルプ");
            Console.WriteLine("/H:");
            Console.WriteLine("　　プログラムヘルプ一覧を表示する。");
            Console.WriteLine("　　先頭文字がH or hで始まっていれば全てヘルプ。");
            Console.WriteLine("　　例：/help or /Help。");
            Console.WriteLine("　　プログラムヘルプはヘルプ表示後終了します。");
            Console.WriteLine("/FIRST:整数");
            Console.WriteLine("　　Listenerhへの接続タイムアウトms。");
            Console.WriteLine("　　サーバが準備できるまでの時間を設定する。");
            Console.WriteLine("　　クライアントの場合600000をセットするのがおすすめ。");
            Console.WriteLine("/IP:IPアドレス");
            Console.WriteLine("　　Listenerの扱うIPを定義する。");
            Console.WriteLine("　　デフォルト192.168.1.1にしています。");
            Console.WriteLine("/TOUT:整数");
            Console.WriteLine("　　送受信タイムアウトms。");
            Console.WriteLine("　　600000がデフォルト。");
            Console.WriteLine("/USER:名前");
            Console.WriteLine("　　自分の名前を設定する。");
            Console.WriteLine("　　２バイトコード使用も可。コード体系はUTF-8使用。");
            Console.WriteLine("　　この引数は必須項目です。無いと終了します。");
            Console.WriteLine("Input anything Key/n");
            Console.ReadLine();
        }

        //入力制御ヘルプ
        public static void controlHelp()
        {
            Console.WriteLine("入力制御ヘルプ");
            Console.WriteLine("#H");
            Console.WriteLine("　　入力制御ヘルプを表示する。");
            Console.WriteLine("　　先頭文字がH or hで始まっていれば全てヘルプ。");
            Console.WriteLine("　　例：#help or #Help。");
            Console.WriteLine("#SERVER=ON");
            Console.WriteLine("　　サーバ側制御にする。");
            Console.WriteLine("　　同一TCP帯で１台必ずサーバを立てること。");
            Console.WriteLine("E");
            Console.WriteLine("　　サーバ接続待ち終了key。");
            Console.WriteLine("I");
            Console.WriteLine("　　入力許可モードに変更するkey。");
            Console.WriteLine("　　受信timeoutまで入力可。");
            Console.WriteLine("#END");
            Console.WriteLine("　　通信プログラムを終了する。");
            Console.WriteLine("Input anything Key/n");
            Console.ReadLine();
        }

        //入力制御のヘルプ表示
        public static void inpHelpCheck(string inpWord)
        {
            if(inpWord.StartsWith("#H") || inpWord.StartsWith("#h"))
            {
                controlHelp();
                gVariables.addinpCount();
            } else {
                gVariables.clerinpCount();
            }
        }
    }

    //key入力制御クラス
    class typkey{
        //key入力制御関数
        public static string typKeyWord(string stopKey, int timeout, bool inWordFlg, bool inpWordFlg)
        {
            bool okinput = false;
            string inpWord = "";
            var outChar = "-";
            int  tcont = timeout / 1000;
            if(gVariables.getinpCount() == 0 && inpWordFlg)
            {
                //1秒毎に入力確認する。
                for (var i = 0; i < tcont; i++)
                {
                    //最初に待ち時間と入力keyを表示
                    if(i%120 == 0) {
                        Console.Write("{0}秒待ち",tcont - i);
                        Console.Write(" key=" + stopKey);
                        if (gVariables.getServerFlg())
                        {
                            Console.Write("\n$ ");
                        } else {
                            Console.Write("\n> ");
                        }
                    }
                    //キー入力チェック。E又はＩが入力されたら待ち受け終了。
                    if(Console.KeyAvailable){
                        outChar = Console.ReadKey().Key.ToString();
                        if(outChar.ToUpper().StartsWith(stopKey))
                        {
                            okinput = true;
                            break;
                        }
                    }
                    //1秒のディレイ
                    Task.Delay(1000);
                }

            } else {
                okinput = true;
            }
            //timeout前に入力keyを受けたら入力状態にする。
            if (inWordFlg && okinput)
            {
                inpWord = Console.ReadLine();
            }
                    
            return(inpWord);
        }
    }

    //基本的なグローバル変数クラス
    class gVariables {
 
        private static string strUSER = ""; //ユーザ情報

        /// <summary>
        /// ユーザ情報を設定する
        /// </summary>
        /// <param name="strUSER"></param>
        public static void setUSER(string iniUSER)
        {
            strUSER = iniUSER;
        }

        /// <summary>
        /// ユーザ情報を取得する
        /// </summary>
        /// <param name="getUSER"></param>
        public static string getUSER()
        {
            return(strUSER);
        }

        private static System.Net.IPAddress strAdress = 
            System.Net.IPAddress.Parse("192.168.1.1");
        private static System.Net.IPAddress ipAdd = 
            System.Net.IPAddress.Parse("192.168.1.1");
        private static int inpCount = 0;
        private static int ListenerTimeOut = 1000;
        private static int sendTimeOut = 300000;
        private static int receTimeOut = 300000;

        private static int portNo = 2001;
        private static bool progFlg = true;
        private static bool serverFlg = false;
        private static string StartServer = "#SERVER=ON";
        private static string ExitPg = "#END";
        private static string errWord = "#Err";
        /// <summary>
        /// Ｉｐアドレスを設定する
        /// </summary>
        /// <param name="strAdress"></param>
        public static void setLisAdress(string iniAdress)
        {
            strAdress = System.Net.IPAddress.Parse(iniAdress);
        }

        /// <summary>
        /// Ｉｐアドレスを取得する
        /// </summary>
        /// <param name="getAdress"></param>
        public static System.Net.IPAddress getLisAdress()
        {
            return(strAdress);
        }

        /// <summary>
        /// ポート設定フラグ設定する<現在未使用>
        /// </summary>
        /// <param name="setProgFlg"></param>
        public static void setProgFlg(bool n)
        {
            progFlg = n;
        }

        /// <summary>
        /// ポート設定フラグを取得する＜現在未使用＞
        /// </summary>
        /// <param name="getProgFlg"></param>
        public static bool getProgFlg()
        {
            return(progFlg);
        }

        /// <summary>
        /// ポート番号を取得する
        /// </summary>
        /// <param name="getProgNo"></param>
        public static int getPortNo()
        {
            return(portNo);
        }

        /// <summary>
        /// 入力回数を取得する
        /// </summary>
        /// <param name="getinpCount"></param>
        public static int getinpCount()
        {
            return(inpCount);
        }

        /// <summary>
        /// 入力回数を加算する
        /// </summary>
        /// <param name="addtinpCount"></param>
        public static void addinpCount()
        {
            inpCount++;
        }

        /// <summary>
        /// 入力回数を0にする
        /// </summary>
        /// <param name="clerinpCount"></param>
        public static void clerinpCount()
        {
            inpCount = 0;
        }

        /// <summary>
        /// LocalHostIPアドレスを設定する
        /// </summary>
        /// <param name="setConAdress"></param>
        public static void setConAdress()
        {
            ipAdd = System.Net.IPAddress.Parse(IPProgram.IpGet('4'));
        }

        /// <summary>
        /// LocalHostIPアドレスを取得する
        /// </summary>
        /// <param name="getConAdress"></param>
        public static System.Net.IPAddress getConAdress()
        {
            return(ipAdd);
        }

        /// <summary>
        /// サーバ側のListener構築時間を設定する
        /// </summary>
        /// <param name="setListenerTimeOut"></param>
       public static void setListenerTimeOut(string value)
        {
            ListenerTimeOut = Int32.Parse(value);
        }

        /// <summary>
        /// サーバ側のListener構築時間を取得する
        /// </summary>
        /// <param name="getListenerTimeOut"></param>
        public static int getListenerTimeOut()
        {
            return(ListenerTimeOut);
        }

        /// <summary>
        /// 送受信のTimeOutを設定する
        /// </summary>
        /// <param name="setTimeOut"></param>
        public static void setTimeOut(string value)
        {
            sendTimeOut = Int32.Parse(value);
            receTimeOut = Int32.Parse(value);
        }

        /// <summary>
        /// 送信のTimeOutを取得する
        /// </summary>
        /// <param name="getSendTimeOut"></param>
        public static int getSendTimeOut()
        {
            return(sendTimeOut);
        }

        /// <summary>
        /// 受信のTimeOutを取得する
        /// </summary>
        /// <param name="getReceTimeOut"></param>
        public static int getReceTimeOut()
        {
            return(receTimeOut);
        }

        /// <summary>
        /// サーバ側状態を設定する
        /// </summary>
        /// <param name="setServerFlg"></param>
        public static void setServerFlg()
        {
            serverFlg = true;
        }

        /// <summary>
        /// サーバ側状態を取得する
        /// </summary>
        /// <param name="getServerFlg"></param>
        public static bool getServerFlg()
        {
            return(serverFlg);
        }

        /// <summary>
        /// サーバ側開始を取得する
        /// </summary>
        /// <param name="getServerStart"></param>
        public static string getServerStart()
        {
            return(StartServer);
        }

        /// <summary>
        /// プログラム強制終了を取得する
        /// </summary>
        /// <param name="getPgEnd"></param>
        public static string getPgEnd()
        {
            return(ExitPg);
        }

        /// <summary>
        /// エラーを取得する
        /// </summary>
        /// <param name="getErrWord"></param>
        public static string getErrWord()
        {
            return(errWord);
        }
    }

    //引数の意味合いを取得するクラス
    public class NewBaseType
    {
        //区切り記号記号の文字を取得
    	public static string getValue(string param)
    	{
    	  	int len;
    	  	if (param.IndexOf(":") == -1)
    	  	{
    	  	  	Console.WriteLine("引数の文字列に:がありません。");
                gVariables.setProgFlg(false);
    	  	  	return(gVariables.getErrWord());
    	  	}
    	  	  
    	  	len = param.Length - (param.IndexOf(":") + 1);
    	  	  
    	  	return(param.Substring(param.Length - len, len));
    	}

        //”/FIRST:”の解析
        public static void init(string param)
        {
            // アイコンファイルのパス
            //string path = Directory.GetCurrentDirectory() + "/Tcp_com.ico";
            // パスを指定してアイコンのインスタンスを生成
            //Icon icon1 = new Icon(SystemIcons.Exclamation, 40, 40);
            //Icon icon1 = new Icon(path, 40, 40);

            //INIクラスの生成
            if(param.StartsWith("/FIRST:"))
            {
           	    gVariables.setListenerTimeOut(getValue(param));
		    }
            
        }

        //”/IP:”の解析
        public static bool ListenerIpAdrress(string param)
        {
            bool rc = false;

            if(param.StartsWith("/IP:"))
            {
                gVariables.setLisAdress(getValue(param));
                rc = true;
            }

            return(rc);
        }

        //”/H”の解析
        public static bool tcpPGHelp(string param)
        {
            bool rc = false;

            if(param.StartsWith("/H") || param.StartsWith("/h"))
            {
                TcpHelp.programHelp();
                TcpHelp.controlHelp();
                rc = true;
            }

            return(rc);
        }

        //”/TOUT:”の解析
         public static void tcpTimeout(string param)
        {
            if(param.StartsWith("/TOUT:"))
            {
           	  gVariables.setTimeOut(getValue(param));
		    }

        }

        //”/USER:”の解析
        public static bool user(string param)
        {
            bool rc = false;
            //USERクラスの生成
            // 先頭の文字列と一致するかどうかを判断         
            if(param.StartsWith("/USER:"))
            {
           	    gVariables.setUSER(getValue(param));
                rc = true;
		    }

            return(rc);
        }
    }

    //クライアントクラス
    public class Client : NewBaseType
    {
        //クアイアント開始
        public static async Task<string> StartClient(int port, string request)
        {
            var buffer = new byte[1024];
            var response = gVariables.getErrWord();

            // サーバーへ接続
            try
            {
                // クライアント作成
                using (var tcpclient = new TcpClient())
                {
                    // 送受信タイムアウト設定
                    tcpclient.SendTimeout = gVariables.getSendTimeOut();
                    tcpclient.ReceiveTimeout = gVariables.getReceTimeOut();

                    //タイムアウトの時間を設定
                    var timeout = gVariables.getListenerTimeOut(); 
                    //var task = 
                    //    tcpclient.ConnectAsync(gVariables.getConAdress(),
                    //     gVariables.getPortNo());

                    Console.Write("サーバ立ち上げ待ち終了Type E");
                    typkey.typKeyWord("E", timeout, false, true);
                    {
                        //接続後の処理を記述            // サーバーへ接続開始
                        Console.WriteLine("サーバーと通信確立");
                        using (var stream = tcpclient.GetStream())
                        {
                            // サーバーへリクエストを送信
                            buffer = System.Text.Encoding.ASCII.GetBytes(request);
                            await stream.WriteAsync(buffer, 0, buffer.Length);
                            Console.WriteLine("サーバーへ[request]を送信");

                            // サーバーからレスポンスを受信
                            var length = await stream.ReadAsync(buffer, 0, buffer.Length);
                            response = System.Text.Encoding.ASCII.GetString(buffer, 0, length);
                            Console.WriteLine("サーバーから[response]を受信");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return response;
        }   
    }

    //サーバクラス
    public class Server : NewBaseType
    {
        //メイン
        public static void Main(string[] argv)
        {
            bool endPg = false;
            bool inpUser = false;
            bool UserCheck = false;
            bool inpWordFlg = true;

            //引数の取得設定
        	foreach(string stri in argv)
        	{
                NewBaseType.init(stri);
                endPg = NewBaseType.tcpPGHelp(stri);
                if(endPg == true) break;
                NewBaseType.ListenerIpAdrress(stri);
                inpUser = NewBaseType.user(stri);
                if(inpUser == true) UserCheck = true;
                NewBaseType.tcpTimeout(stri);
            }

            if(endPg == true || UserCheck == false) return;
            if(gVariables.getProgFlg() == false) return;
            //ホスト名からIPアドレスを取得する時は、次のようにする
            //string host = "localhost";
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.GetHostEntry(hostは[0];
            gVariables.setConAdress();

            Task<string> sendMsg;
            string sendWord = "";
            string sendText = "";

            //クライアント側の送信
            while(true)
            {
                //System.Text.Encoding enc = System.Text.Encoding.UTF8;

                Console.Write("> ");
                string iniWord = typkey.typKeyWord("I", gVariables.getReceTimeOut(),
                     true, inpWordFlg);

                sendWord = iniWord;
                if(!sendWord.EndsWith("\n"))
                {
                    sendWord += "\n";
                }

                //何も文字が入力されていない処理
                if (sendWord.StartsWith("\n")) inpWordFlg = false;
                else inpWordFlg = true;

                //ヘルプコマンドチェック
                TcpHelp.inpHelpCheck(sendWord);
                
                //終了コマンドチェック
                if (sendWord.StartsWith(gVariables.getPgEnd()))
                {
                    Console.WriteLine("> " + "---TCP 通信終了---\n");

                    //閉じる
                    Console.WriteLine("> " + "サーバとの接続を閉じました。");
                    break;
                }

                //サーバ開始か？
                if (sendWord.StartsWith(gVariables.getServerStart()))
                {
                    //サーバ処理にする
                    sendText = gVariables.getServerStart();
                    gVariables.setServerFlg();
                    Console.WriteLine("> サーバに移行します。");
                    break;

                } else {
                    //送信が制御モードの場合入力に戻す
                    if (sendWord.StartsWith("#") || sendWord.StartsWith("\n"))
                    {
                        continue;
                    }

                    //USER名を追加
                    sendWord = gVariables.getUSER() + " " + sendWord;

                    //文字列をByte型配列に変換
                    //byte[] sendBytes = enc.GetBytes(sendWord + '\n');

                    //クライアントから送信
                    sendMsg = Client.StartClient(gVariables.getPortNo(), sendWord);
                    if (sendMsg.ToString().StartsWith(gVariables.getErrWord()))
                    {
                        Console.WriteLine("> " + "--Send Err--");
                        gVariables.setProgFlg(false);
                        break;
                    }

                }

            }
 
            //サーバ側送信
            if (sendText.StartsWith(gVariables.getServerStart()))
            {
                //送信データがエラーでない
                if (!sendWord.ToString().StartsWith(gVariables.getErrWord()))
                {

                    //TcpListenerオブジェクトを作成する
                    System.Net.Sockets.TcpListener listener =
                        new System.Net.Sockets.TcpListener(gVariables.getConAdress(),
                         gVariables.getPortNo());

                    //Listenを開始する
                    listener.Start();
                    Console.WriteLine("Listenを開始しました({0}:{1})。",
                        ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                        ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

                    while(true)
                    {
                        //接続要求があったら受け入れる
                        System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
                        Console.WriteLine("クライアント({0}:{1})と接続しました。",
                            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
                            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

                        //NetworkStreamを取得
                        System.Net.Sockets.NetworkStream ns = client.GetStream();

                        //読み取り、書き込みのタイムアウトを10秒にする
                        //デフォルトはInfiniteで、タイムアウトしない
                        //(.NET Framework 2.0以上が必要)
                        ns.ReadTimeout = gVariables.getSendTimeOut();
                        ns.WriteTimeout = gVariables.getReceTimeOut();

                        //クライアントから送られたデータを受信する
                        System.Text.Encoding enc = System.Text.Encoding.UTF8;
                        bool disconnected = false;
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        byte[] resBytes = new byte[256];
                        int resSize = 0;

                        do
                        {
                            //データの一部を受信する
                            resSize = ns.Read(resBytes, 0, resBytes.Length);

                            //Readが0を返した時はクライアントが切断したと判断
                            if (resSize == 0)
                            {
                                disconnected = true;
                                Console.WriteLine("クライアントが切断しました。");
                                break;
                            }

                            //受信したデータを蓄積する
                            ms.Write(resBytes, 0, resSize);
                            //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                            // 受信を続ける
                        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
        
                        //受信したデータを文字列に変換
                        string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        ms.Close();

                        //末尾の\nを削除
                        //resMsg = resMsg.TrimEnd('\n');
                        Console.WriteLine("$ " + resMsg);

                        Console.Write("$ ");

                       //クライアントに送信する文字列を作成
                        sendText = typkey.typKeyWord("I", gVariables.getReceTimeOut(),
                             true, inpWordFlg);
                        
                        //末尾の\nを削除
                        //sendText = sendText.TrimEnd('\n');
                        //sendMsg = sendText;
                        if(!sendText.EndsWith("\n"))
                        {
                            sendText += "\n";
                        }

                        //入力がない場合の処理
                        if (sendText.StartsWith("\n")) inpWordFlg = false;
                        else inpWordFlg = true;

                        //ヘルプコマンドチェック
                        TcpHelp.inpHelpCheck(sendText);
                
                        //終了コマンドチェック
                        if (sendText.StartsWith(gVariables.getPgEnd()))
                        {
                            Console.WriteLine("$ " + "---TCP 通信終了---\n");

                            //閉じる
                            ns.Close();
                            client.Close();
                            Console.WriteLine("$ " + "クライアントとの接続を閉じました。");
                            break;
                        }

                        //セッションが繋がっていたら
                        if (!disconnected)
                        {
                            //送信が制御モードの場合入力に戻す
                            if (sendText.StartsWith("#") || sendText.StartsWith("\n"))
                            {
                                continue;
                            }

                            //クライアントにデータを送信する

                            //USER名を追加
                            sendText = gVariables.getUSER() + " " + sendText;

                            //文字列をByte型配列に変換
                            byte[] sendBytes = enc.GetBytes(sendText + '\n');

                            //データを送信する
                            ns.Write(sendBytes, 0, sendBytes.Length);
                            Console.WriteLine(sendText);
                        }

                        //サーバ処理中断？
                        if(!gVariables.getServerFlg()) break;

                    }

                    //リスナを閉じる
                    listener.Stop();
                    Console.WriteLine("$ " + "Listenerを閉じました。");

                    Console.ReadLine();

                    //return;
                }

            }

            //リスナを閉じる
            //slistener.Stop();
            Console.WriteLine("> " + "Tcp_comを閉じました。");

            Console.ReadLine();
            return;
        }
    }
}
