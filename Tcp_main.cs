//=============================================//
// Tcp_com.exeのソース
// このソフトはローカル環境のみで通信をやり取り
// 現在確認済は、Windows11,Linux(lubuntu),
//     Mac(os10),Android(os9),iPad(iOS12)
// ファイヤーウォール、ルーティングは使用者責任
// Tcp_com /Hでコマンド引数使い方をチェックする
// *_run.bat,*_run.shは編集してお使い下さい
// --------------------------------------------
// make N.Tanaka 2024/09/10リリース
// 端末１台に付き１ライセンス必要です
// 販売サイト：https://diskcobo77.theshop.jp/
// Open souse形式
// Git: https://github.com/gyabo4863/TcpCom.git
// 他言語で販売したい方は相談にのります。
// mail: gyabo4863@gmail.com
//=============================================//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace TcpComWindows
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
            if (ip_param == 'L')
            {
                foreach (var x in GetIPAddresses())
                {
                    Console.WriteLine(x);
                    return (x.ToString());
                }
            }

            if (ip_param == 'A')
            {
                Console.WriteLine("ローカルホストを省く");
                foreach (var x in GetIPAddresses(excludeLocalHost: true))
                {
                    Console.WriteLine(x);
                    return (x.ToString());
                }
            }

            if (ip_param == '4')
            {
                Console.WriteLine("IPv6を省く");
                while (true)
                {
                    foreach (var x in GetIPAddresses(excludeIPv6: true))
                    {
                        Console.WriteLine(x);
                        Console.WriteLine("yes = y or no = n");
                        if (Console.ReadLine() == "y")
                        {
                            return (x.ToString());
                        }
                    }
                }
            }

            Console.WriteLine("Command ERR " + ip_param);
            gVariables.setProgFlg(false);
            return ("192.168.1.1");
        }
    }

    //ヘルプ表示クラス
    class TcpHelp
    {
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
            Console.WriteLine("/DEBAG:ON");
            Console.WriteLine("　　デバグを開始状態出始める。");
            Console.WriteLine("　　デバグ停止状態がデフォルト。");
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
            Console.WriteLine("　　サーバ状態の時使用できません。");
            Console.WriteLine("#CLIENT=ON");
            Console.WriteLine("　　クライアント接続開始。");
            Console.WriteLine("　　Listen開始後にクライアント接続開始する。");
            Console.WriteLine("　　クライアント状態に移行後に設定しない。");
            Console.WriteLine("　　サーバ状態の時使用出来ません。");
            Console.WriteLine("#DEBAG=ON");
            Console.WriteLine("　　デバック情報表示。");
            Console.WriteLine("#DEBAG=OFF");
            Console.WriteLine("　　デバッグ情報非表示。");
            Console.WriteLine("　　通常はこのモードになっています。");
            Console.WriteLine("#END");
            Console.WriteLine("　　通信プログラムを終了する。");
            Console.WriteLine("Input anything Key/n");
            Console.ReadLine();
        }

        //入力制御のヘルプ表示
        public static void inpHelpCheck(string inpWord)
        {
            if (inpWord.StartsWith("#H") || inpWord.StartsWith("#h"))
            {
                controlHelp();
                gVariables.addinpCount();
            }
            else
            {
                gVariables.clerinpCount();
            }
        }
    }

    //key入力制御クラス
    class typkey
    {
        //入力の処理
        public static void keyCommon(string sendWord)
        {
            //System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //ヘルプコマンド？
            TcpHelp.inpHelpCheck(sendWord);

            //デバッグ情報ON？
            if (sendWord.StartsWith(gVariables.getOnDeBagCom()))
            {
                gVariables.setOnDeBag();
            }

            //デバッグ情報OFF？
            if (sendWord.StartsWith(gVariables.getOffDeBagCom()))
            {
                gVariables.setOffDeBag();
            }

            return;
        }
    }

    //基本的なグローバル変数クラス
    class gVariables
    {

        //ユーザの値は大切なので別格扱い
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
            return (strUSER);
        }

        //グローバル変数の保存場所
        private static System.Net.IPAddress ipAdd =
            System.Net.IPAddress.Parse("192.168.1.1");
        private static int inpCount = 0;
        private static int ListenerTimeOut = 1000;
        private static int sendTimeOut = 300000;
        private static int receTimeOut = 300000;
        private static int portNo = 2001;
        private static bool progFlg = true;
        private static bool serverFlg = false;
        private static bool onlyonFlg = true;
        private static bool debagFlg = false;

        //本当はconstでよい値だがなんとなく編集可能にした。
        private static string StartServer = "#SERVER=ON";
        private static string StartClient = "#CLIENT=ON";
        private static string DeBagOn = "#DEBAG=ON";
        private static string DeBagOff = "#DEBAG=OFF";
        private static string ExitPg = "#END";
        private static string errWord = "#Err";

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
            return (progFlg);
        }

        /// <summary>
        /// ポート番号を取得する
        /// </summary>
        /// <param name="getProgNo"></param>
        public static int getPortNo()
        {
            return (portNo);
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
            return (ipAdd);
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
            return (ListenerTimeOut);
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
            return (sendTimeOut);
        }

        /// <summary>
        /// 受信のTimeOutを取得する
        /// </summary>
        /// <param name="getReceTimeOut"></param>
        public static int getReceTimeOut()
        {
            return (receTimeOut);
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
            return (serverFlg);
        }

        /// <summary>
        /// サーバ側開始を取得する
        /// </summary>
        /// <param name="getServerStart"></param>
        public static string getServerStart()
        {
            return (StartServer);
        }

        /// <summary>
        /// クライアント側開始を取得する
        /// </summary>
        /// <param name="getClientStart"></param>
        public static string getClientStart()
        {
            return (StartClient);
        }

        /// <summary>
        /// マルチ運用にする
        /// </summary>
        /// <param name="setMulti"></param>
        public static void setMulti()
        {
            onlyonFlg = false;
        }

        /// <summary>
        /// 単一実行中かどうかを取得
        /// </summary>
        /// <param name="getOnlyOnFlg"></param>
        public static bool getOnlyOnFlg()
        {
            return(onlyonFlg);
        }

        /// <summary>
        /// DeBagをONにする
        /// </summary>
        /// <param name="setOnDeBag"></param>
        public static void setOnDeBag()
        {
            debagFlg = true;
        }

        /// <summary>
        /// DeBagをOFFにする
        /// </summary>
        /// <param name="seOffDeBag"></param>
        public static void setOffDeBag()
        {
            debagFlg = false;
        }

        /// <summary>
        /// DeBag情報を取得する
        /// </summary>
        /// <param name="getDeBagFlg"></param>
        public static bool getDeBagFlg()
        {
            return(debagFlg);
        }

        /// <summary>
        /// DeBagをONのコマンド取得する
        /// </summary>
        /// <param name="getOnDeBagCom"></param>
        public static string getOnDeBagCom()
        {
            return(DeBagOn);
        }

        /// <summary>
        /// DeBagをOFFのコマンド取得する
        /// </summary>
        /// <param name="getOffDeBagCom"></param>
        public static string getOffDeBagCom()
        {
            return (DeBagOff);
        }

        /// <summary>
        /// プログラム強制終了を取得する
        /// </summary>
        /// <param name="getPgEnd"></param>
        public static string getPgEnd()
        {
            return (ExitPg);
        }

        /// <summary>
        /// エラーを取得する
        /// </summary>
        /// <param name="getErrWord"></param>
        public static string getErrWord()
        {
            return (errWord);
        }
    }

    //引数の意味合いを取得するクラス
    public class NewBaseType
    {
        //区切り記号記号の文字を取得
        public static string getValue(string param)
        {
            int len;

            //区切り記号がなかったら
            if (param.IndexOf(":") == -1)
            {
                if (gVariables.getDeBagFlg())
                    Console.WriteLine("引数の文字列に:がありません。");
                gVariables.setProgFlg(false);
                return (gVariables.getErrWord());
            }

            //区切り記号位置取得  
            len = param.Length - (param.IndexOf(":") + 1);

            return (param.Substring(param.Length - len, len));
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
            if (param.StartsWith("/FIRST:"))
            {
                gVariables.setListenerTimeOut(getValue(param));
            }

        }

        //”/H”の解析
        public static bool tcpPGHelp(string param)
        {
            bool rc = false;

            //ヘルプのキーワード？
            if (param.StartsWith("/H") || param.StartsWith("/h"))
            {
                TcpHelp.programHelp();
                TcpHelp.controlHelp();
                rc = true;
            }

            return (rc);
        }

        //”/TOUT:”の解析
        public static void tcpTimeout(string param)
        {
            if (param.StartsWith("/TOUT:"))
            {
                gVariables.setTimeOut(getValue(param));
            }
        }

        //”/USER:”の解析
        public static bool user(string param)
        {
            bool rc = false;
            //USERクラスの生成        
            if (param.StartsWith("/USER:"))
            {
                gVariables.setUSER(getValue(param));
                rc = true;
            }

            return (rc);
        }
   
        public static void debag(string param)
        {
            //デバッグをON状態で始める
            if (param.StartsWith("/DEBAG:ON"))
            {
                gVariables.setOnDeBag();
            }
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

                    //Listenerタイムアウトの時間を設定
                    //var timeout = gVariables.getListenerTimeOut();
                    Console.WriteLine("> サーバーへ接続します。Input anyKey");
                    Console.ReadLine();
                    {
                        //接続後の処理を記述            // サーバーへ接続開始
                        if (gVariables.getDeBagFlg())
                            Console.WriteLine("サーバーと通信確立");
                        using (var stream = tcpclient.GetStream())
                        {
                            // サーバーへリクエストを送信
                            buffer = System.Text.Encoding.ASCII.GetBytes(request);
                            await stream.WriteAsync(buffer, 0, buffer.Length);
                            if (gVariables.getDeBagFlg())
                                Console.WriteLine("サーバーへ[request]を送信");

                            // サーバーからレスポンスを受信
                            var length = await stream.ReadAsync(buffer, 0, buffer.Length);
                            response = System.Text.Encoding.ASCII.GetString(buffer, 0, length);
                            if (gVariables.getDeBagFlg())
                                Console.WriteLine("サーバーから[response]を受信");
                            gVariables.setMulti();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (gVariables.getDeBagFlg())
                    Console.WriteLine(ex.ToString());
            }

            return response;
        }
    }

    //サーバクラス
    internal class Tcp_main : NewBaseType
    {
        //メイン
        public static void Main(string[] argv)
        {
            bool endPg = false;
            bool inpUser = false;
            bool UserCheck = false;
            int joinCount = 0;

            // 標準入力のエンコーディングにUTF-8を用いる
            Console.InputEncoding = Encoding.UTF8;

            // 標準出力のエンコーディングにUTF-8を用いる
            Console.OutputEncoding = Encoding.UTF8;

            //引数の取得設定
            foreach (string stri in argv)
            {
                NewBaseType.init(stri);
                endPg = NewBaseType.tcpPGHelp(stri);
                if (endPg == true) break;
                inpUser = NewBaseType.user(stri);
                if (inpUser == true) UserCheck = true;
                NewBaseType.tcpTimeout(stri);
                NewBaseType.debag(stri);
            }

            //強制終了の処理
            if (endPg == true || UserCheck == false) return;
            if (gVariables.getProgFlg() == false) return;

            //自分(localHost)のIPアドレスを取得する。
            gVariables.setConAdress();

            //メーセージの保存場所
            Task<string> sendMsg;
            string sendWord = "";
            string sendText = "";

            //クライアント側の送信
            while (true)
            {
                //クライアント文字入力処理
                Console.WriteLine("> ");
                sendText = Console.ReadLine();
                if (!sendText.EndsWith("\n"))
                {
                    sendText += "\n";
                }

                //入力が共通コマンドの処理
                typkey.keyCommon(sendWord);

                if (gVariables.getDeBagFlg())
                    Console.WriteLine(">  [[" + sendText);

                //終了コマンド？
                if (sendText.StartsWith(gVariables.getPgEnd()))
                {
                    if (gVariables.getDeBagFlg())
                        Console.WriteLine("> " + "---TCP 通信終了---\n");

                    //閉じる
                    if (gVariables.getDeBagFlg())
                        Console.WriteLine("> " + "サーバとの接続を閉じました。");
                    break;
                }

                //クライアント接続開始？
                if (sendText.StartsWith(gVariables.getClientStart()))
                {
                    //最初の1回のみ有効
                    if ( joinCount == 0)
                    {
                        //クライアント接続開始する
                        sendText = "==>接続\n";
                    }
                    joinCount++;
                }

                //サーバ開始か？
                if (sendText.StartsWith(gVariables.getServerStart()))
                {
                    //サーバ処理にする
                    sendText = gVariables.getServerStart();
                    gVariables.setServerFlg();
                    gVariables.setMulti();
                    Console.WriteLine("> サーバに移行します。");
                    break;
                }
                else
                {
                    //送信が制御モードの場合入力に戻す
                    if (sendText.StartsWith("#") || sendText.StartsWith("\n"))
                    {
                        continue;
                    }

                    //USER名を追加
                    sendText = gVariables.getUSER() + " " + sendText;

                    //クライアントから送信
                    sendMsg = Client.StartClient(gVariables.getPortNo(), sendText);
                    if (sendMsg.ToString().StartsWith(gVariables.getErrWord()))
                    {
                        if (gVariables.getDeBagFlg())
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
                if (!sendText.ToString().StartsWith(gVariables.getErrWord()))
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

                    while (true)
                    {
                        //接続要求があったら受け入れる
                        System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
                        if (gVariables.getDeBagFlg())
                            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
                            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

                        //NetworkStreamを取得
                        System.Net.Sockets.NetworkStream ns = client.GetStream();

                        //読み取り、書き込みのタイムアウトを設定タイムアウトにする
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
                                if (gVariables.getDeBagFlg())
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

                        //サーバにメッセージ表示
                        Console.WriteLine("$ " + resMsg);
                        Console.Write("$ ");

                        //クライアントに送信する文字列を作成
                        sendText = Console.ReadLine();

                        //末尾に\nを追加
                        if (!sendText.EndsWith("\n"))
                        {
                            sendText += "\n";
                        }

                        //入力が共通コマンドの処理
                        typkey.keyCommon(sendText);

                        //終了コマンド？
                        if (sendText.StartsWith(gVariables.getPgEnd()))
                        {
                            if (gVariables.getDeBagFlg())
                                Console.WriteLine("$ " + "---TCP 通信終了---\n");

                            //閉じる
                            ns.Close();
                            client.Close();
                            if (gVariables.getDeBagFlg())
                                Console.WriteLine("$ " + "クライアントとの接続を閉じました。");
                            break;
                        }

                        //クライアント接続開始？
                        if (sendText.StartsWith(gVariables.getClientStart()))
                        {
                            //クライアント接続しない
                            Console.WriteLine("$ クライアントに移行できません。");
                            continue;
                        }

                        //サーバ開始か？
                        if (sendText.StartsWith(gVariables.getServerStart()))
                        {
                            //サーバ状態の為処理なし
                            Console.WriteLine("$ サーバ状態です。");
                            continue;
                        }

                        //セッションが繋がっていたら
                        if (!disconnected)
                        {
                            //送信が制御モードの場合入力に戻す
                            if (sendText.StartsWith("#") || sendText.EndsWith("\n"))
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
                        if (!gVariables.getServerFlg()) break;

                    }

                    //リスナを閉じる
                    listener.Stop();
                    if (gVariables.getDeBagFlg())
                        Console.WriteLine("$ " + "Listenerを閉じました。");
                    Console.ReadLine();
                }

            }

            //プログラムを終わらす。
            Console.WriteLine("> " + "Tcp_comを終了ました。");
            Console.ReadLine();
            return;
        }
    }
}
