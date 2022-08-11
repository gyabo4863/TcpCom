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
            return("192.168.1.0");
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

        private static string strAdress = ""; //外部から参照できる文字列
        private static System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse("192.168.1.1");
        private static int ListenerTimeOut = 1000;
        private static int sendTimeOut = 300000;
        private static int receTimeOut = 300000;
        private static string StartServer = "#SERVER=ON";
        private static string ExitPg = "#END";
        private static string errWord = "#Err";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strAdress"></param>
        public static void setAdress(string iniAdress)
        {
            strAdress = iniAdress;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getAdress"></param>
        public static string getAdress()
        {
            return(strAdress);
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

         public static void tcpTimeout(string param)
        {
            if(param.StartsWith("/TOUT:"))
            {
           	  gVariables.setTimeOut(getValue(param));
		    }

        }

        public static void user(string param)
        {
            //USERクラスの生成
            // 先頭の文字列と一致するかどうかを判断         
            if(param.StartsWith("/USER:"))
            {
           	  gVariables.setUSER(getValue(param));
		    }
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
            //引数の取得設定
        	foreach(string stri in argv)
        	{
                NewBaseType.init(stri);
                NewBaseType.user(stri);
                NewBaseType.tcpTimeout(stri);
            }
            //ホスト名からIPアドレスを取得する時は、次のようにする
            //string host = "localhost";
            //System.Net.IPAddress ipAdd =
            //    System.Net.Dns.GetHostEntry(hostは[0];
            gVariables.setConAdress();

            //Listenするポート番号
            int port = 2001;
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

                //終了コマンドチェック
                if (string.Compare(sendWord, gVariables.getPgEnd()) == 0 )
                {
                    Console.WriteLine("> " + "---TCP 通信終了---\n");

                    //閉じる
                    Console.WriteLine("> " + "サーバとの接続を閉じました。");
                    break;
                }

                if (string.Compare(sendWord, gVariables.getServerStart()) == 0 )
                {
                    //サーバ処理にする
                    sendText = gVariables.getServerStart();
                    break;
                } else {
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
            if (string.Compare(sendText,gVariables.getServerStart()) == 0 )
            {
                if (string.Compare(sendWord.ToString(), gVariables.getErrWord())  != 0)
                {

                    //TcpListenerオブジェクトを作成する
                    System.Net.Sockets.TcpListener listener =
                        new System.Net.Sockets.TcpListener(gVariables.getConAdress(), port);

                    while(true)
                    {
                        //Listenを開始する
                        listener.Start();
                        Console.WriteLine("Listenを開始しました({0}:{1})。",
                            ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                            ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

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

                        //サーバから送られたデータを受信する
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

                        //終了コマンドチェック
                        if (string.Compare(sendWord, gVariables.getPgEnd()) == 0 )
                        {
                            Console.WriteLine("$ " + "---TCP 通信終了---\n");

                            //閉じる
                            ns.Close();
                            client.Close();
                            Console.WriteLine("$ " + "サーバとの接続を閉じました。");
                            break;
                        }

                        if (!disconnected)
                        {
                            //クライアントにデータを送信する

                            //USER名を追加
                            sendText = gVariables.getUSER() + " " + sendText;

                            //文字列をByte型配列に変換
                            byte[] sendBytes = enc.GetBytes(sendText + '\n');

                            //データを送信する
                            ns.Write(sendBytes, 0, sendBytes.Length);
                            Console.WriteLine(sendText);
                        }

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
            Console.WriteLine("$ " + "Listenerを閉じました。");

            Console.ReadLine();
            return;
        }
    }
}
