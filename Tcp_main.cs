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
        //key入力時の表示制御
        public static void dispKey(int tcont, int i, string stopKey)
        {
            Console.Write("{0}秒待ち", tcont - i);
            Console.Write(" key=" + stopKey);

            if (gVariables.getServerFlg())
            {
                Console.Write("\n$ ");
            }
            else
            {
                Console.Write("\n> ");
            }
        }

        //key入力制御関数
        public static async Task<string> typKeyWord(string stopKey, int timeout, bool inWordFlg, bool inpWordFlg)
        {
            bool okinput = false;
            bool skipFlg = true;
            string inpWord = "";
            var outChar = "-";
            int tcont = timeout / 1000;

            //入力初回と表示許可フラグがtrueの場合
            if (gVariables.getinpCount() == 0 && inpWordFlg)
            {
                //1秒毎に入力確認する。
                for (var i = 0; i < tcont; i++)
                {
                    //最初と２分毎に待ち時間と入力keyを表示
                    if (i % 120 == 0 && skipFlg)
                    {
                        if (i < 120) skipFlg = false;
                        dispKey(tcont, i, stopKey);
                    }
                    else
                    {
                        if (i >= 120) skipFlg = true;
                        //dispKey(tcont, i, stopKey);
                    }

                    //キー入力チェック。E又はＩが入力されたら待ち受け終了。
                    if (Console.KeyAvailable)
                    {
                        outChar = Console.ReadKey().Key.ToString();
                        if (outChar.ToUpper().StartsWith(stopKey))
                        {
                            okinput = true;
                            break;
                        }
                    }

                    //1秒のディレイ
                    await Task.Delay(1000);
                }

            }
            else
            {
                //とりあえず、入力許可にする。
                okinput = true;
            }

            //timeout前に入力keyを受けたら入力状態にする。
            if (inWordFlg && okinput)
            {
                inpWord = Console.ReadLine();
            }

            return (inpWord);
        }

        //入力の処理
        public static Task<string> keyInput()
        {
            string sendWord ="";
            //System.Text.Encoding enc = System.Text.Encoding.UTF8;
            //コマンド受付
            Console.Write("> ");
            Task<string> iniWord = typkey.typKeyWord("I", gVariables.getReceTimeOut(),
                true, gVariables.getKeyInpFlg());

            //とりあえず\nで終了させる。
            sendWord = iniWord.ToString();
            if (!sendWord.EndsWith("\n"))
            {
                sendWord += "\n";
            }

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

            return(iniWord);
        }
    
        //マルチ非対応の処理
        public static async void keyDelay()
        {
            while(true)
            {
                //オンリーワンが偽になるまでループする
                if (!gVariables.getOnlyOnFlg()) break;
                //1秒のディレイ
                await Task.Delay(1000);
            }
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
        private static bool onlyonFlg = true;
        private static bool keyinpFlg = true;
        private static bool debagFlg = false;

        //本当はconstでよい値だがなんとなく編集可能にした。
        private static string StartServer = "#SERVER=ON";
        private static string StartClient = "#CLIENT=ON";
        private static string DeBagOn = "#DEBAG=ON";
        private static string DeBagOff = "#DEBAG=OFF";
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
            return (strAdress);
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
        /// 入力回数を取得する
        /// </summary>
        /// <param name="getinpCount"></param>
        public static int getinpCount()
        {
            return (inpCount);
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
        /// Key入力時タイマーをONにする
        /// </summary>
        /// <param name="setOnKeyFlg"></param>
        public static void setOnKeyFlg()
        {
            keyinpFlg = true;
        }

        /// <summary>
        /// Key入力時タイマーをOFFにする
        /// </summary>
        /// <param name="setOffKeyFlg"></param>
        public static void setOffKeyFlg()
        {
            keyinpFlg = false;
        }
        
        /// <summary>
        /// Key入力時タイマー状態を取得する
        /// </summary>
        /// <param name="getKeyInpFlg"></param>
        public static bool getKeyInpFlg()
        {
            return(keyinpFlg);
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

        //”/IP:”の解析
        public static bool ListenerIpAdrress(string param)
        {
            bool rc = false;

            //IPアドレス？
            if (param.StartsWith("/IP:"))
            {
                gVariables.setLisAdress(getValue(param));
                rc = true;
            }

            return (rc);
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
                    var timeout = gVariables.getListenerTimeOut();

                    if (gVariables.getDeBagFlg())
                        Console.Write("サーバ立ち上げ待ち終了Type E");
                    await typkey.typKeyWord("E", timeout, false, true);
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
            bool inpWordFlg = true;
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
                NewBaseType.ListenerIpAdrress(stri);
                inpUser = NewBaseType.user(stri);
                if (inpUser == true) UserCheck = true;
                NewBaseType.tcpTimeout(stri);
            }

            //強制終了の処理
            if (endPg == true || UserCheck == false) return;
            if (gVariables.getProgFlg() == false) return;

            //自分(localHost)のIPアドレスを取得する。
            gVariables.setConAdress();

            //メーセージの保存場所
            Task<string> sendMsg;
            Task<string> sendWord;
            string sendText = "";

            //クライアント側の送信
            while (true)
            {
                //クライアント文字入力処理
                Task<string> inpWord = typkey.keyInput();
                sendText = inpWord.ToString();

                //何も文字が入力されていない処理
                if (sendText.StartsWith("\n")) inpWordFlg = false;
                else  inpWordFlg = true;

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
                        inpWordFlg = true;
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
                    if (sendText.StartsWith("#") || !inpWordFlg)
                    {
                        //マルチ非対応の処理
                        if (gVariables.getOnlyOnFlg())
                        {
                            gVariables.setOffKeyFlg();
                        }
                        else
                        {
                            gVariables.setOnKeyFlg();
                        }       
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

                //マルチ非対応の処理
                if (gVariables.getOnlyOnFlg())
                {
                    gVariables.setOffKeyFlg();
                }
                else
                {
                    gVariables.setOnKeyFlg();
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
                        sendWord = typkey.keyInput();
                        sendText = sendWord.ToString();

                        //末尾に\nを追加
                        if (!sendText.EndsWith("\n"))
                        {
                            sendText += "\n";
                        }

                        //入力がない場合の処理
                        if (sendText.StartsWith("\n")) inpWordFlg = false;
                        else inpWordFlg = true;

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
                            if (sendText.StartsWith("#") || !inpWordFlg)
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
