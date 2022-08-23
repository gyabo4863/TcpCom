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

    class TcpHelp {
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
            Console.WriteLine("　　クライアントの場合3600000をセットするのがおすすめ。");
            Console.WriteLine("/IP:IPアドレス/サブマスク");
            Console.WriteLine("　　Listenerの扱うIP帯を定義する。");
            Console.WriteLine("　　デフォルト192.168.0.0/16にしています。");
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
            Console.WriteLine("#END");
            Console.WriteLine("　　通信プログラムを終了する。");
            Console.WriteLine("Input anything Key/n");
            Console.ReadLine();
        }

        public static void inpHelpCheck(string inpWord)
        {
            if(inpWord.StartsWith("#H") || inpWord.StartsWith("#h"))
            {
                controlHelp();
            }
        }
    }

    class gVariables {
 
        private static string strUSER = ""; //外部から参照できる文字列

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strUSER"></param>
        public static void setUSER(string iniUSER)
        {
            strUSER = iniUSER;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getUSER"></param>
        public static string getUSER()
        {
            return(strUSER);
        }

        private static string strAdress = "192.168.0.0/16"; //外部から参照できる文字列
        private static System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse("192.168.1.1");
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
        /// 
        /// </summary>
        /// <param name="strAdress"></param>
        public static void setLisAdress(string iniAdress)
        {
            strAdress = iniAdress;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getAdress"></param>
        public static string getLisAdress()
        {
            return(strAdress);
        }

        public static void setProgFlg(bool n)
        {
            progFlg = n;
        }

        public static bool getProgFlg()
        {
            return(progFlg);
        }

        public static int getPortNo()
        {
            return(portNo);
        }

        public static void setConAdress()
        {
            System.Net.IPAddress.Parse(IPProgram.IpGet('4'));
        }

        public static System.Net.IPAddress getConAdress()
        {
            return(ipAdd);
        }

        public static void setListenerTimeOut(string value)
        {
            ListenerTimeOut = Int32.Parse(value);
        }

        public static int getListenerTimeOut()
        {
            return(ListenerTimeOut);
        }

        public static void setTimeOut(string value)
        {
            sendTimeOut = Int32.Parse(value);
            receTimeOut = Int32.Parse(value);
        }

        public static int getSendTimeOut()
        {
            return(sendTimeOut);
        }

        public static int getReceTimeOut()
        {
            return(receTimeOut);
        }

        public static void setServerFlg()
        {
            serverFlg = true;
        }

        public static bool getServerFlg()
        {
            return(serverFlg);
        }

        public static string getServerStart()
        {
            return(StartServer);
        }

        public static string getPgEnd()
        {
            return(ExitPg);
        }

        public static string getErrWord()
        {
            return(errWord);
        }
    }

    public class NewBaseType
    {
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

         public static void tcpTimeout(string param)
        {
            if(param.StartsWith("/TOUT:"))
            {
           	  gVariables.setTimeOut(getValue(param));
		    }

        }

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

    public class Client : NewBaseType
    {
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
                    var task = tcpclient.ConnectAsync(gVariables.getConAdress(), port);
                    
                    if (!task.Wait(timeout))  //ここで画面がフリーズしてしまう。
                    {
                        gVariables.setProgFlg(false);
                        //タイムアウトの例外
                        throw new SocketException(10060);

                    } else
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

    public class Server : NewBaseType
    {

        public static void Main(string[] argv)
        {
            bool endPg = false;
            bool inpUser = false;
            bool UserCheck = false;

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

            //Listenするポート番号
            int port = gVariables.getPortNo();
            Task<string> sendMsg;
            string sendWord = "";
            string sendText = "";
            //bool disconnected = false;

            //クライアント側の送信
            while(true)
            {
                Console.Write("> ");
                string iniWord = Console.ReadLine();

                sendWord = iniWord;
                if(!sendWord.EndsWith("\n"))
                {
                    sendWord += "\n";
                }

                //ヘルプコマンドチェック
                TcpHelp.inpHelpCheck(sendWord);
                
                //終了コマンドチェック
                if (string.Compare(sendWord, gVariables.getPgEnd()) == 0 )
                {
                    Console.WriteLine("> " + "---TCP 通信終了---\n");

                    //閉じる
                    Console.WriteLine("> " + "サーバとの接続を閉じました。");
                    break;
                }

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
                    //byte[] sendBytes = enc.GetBytes(sendMsg + '\n');

                    sendMsg = Client.StartClient(port, sendWord);
                    if (string.Compare(sendMsg.ToString(), gVariables.getErrWord())  == 0)
                    {
                        Console.WriteLine("> " + "--Send Err--");
                        break;
                    }

                }

            }
 
            //サーバ側送信
            if (sendText.StartsWith(gVariables.getServerStart()))
            {
                if (!sendWord.ToString().StartsWith(gVariables.getErrWord()))
                {

                    //TcpListenerオブジェクトを作成する
                    System.Net.Sockets.TcpListener listener =
                        new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(gVariables.getLisAdress()), port);

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
                        sendText = Console.ReadLine();
                        //末尾の\nを削除
                        //sendText = sendText.TrimEnd('\n');
                        //sendMsg = sendText;
                        if(!sendText.EndsWith("\n"))
                        {
                            sendText += "\n";
                        }


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
                        if(gVariables.getServerFlg() == false) break;

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
