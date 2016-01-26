using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
    // 変数宣言
    int[,] ps_arr;                      // 物体情報格納
    int[,] ie_arr;                      // 図形に対して内部外部判定を行った結果を格納（0:外部 1:内部）
    IplImage i_img;                     // IplImage;
    IplImage h_img = new IplImage();    // HSV画像
    Camera cam = new Camera();          // カメラ
    Graphic g = new Graphic();          // 画像編集関係
    Texture2D texture;                  // テクスチャ
    IplImage d_img = new IplImage();    // テクスチャ画像

    // 図形用
    int num = 41;            // 角の数
    double[] xdata, ydata;
    // 図形計算用
    ArrayList f_vec, d_vec, m_vec;  // f_vec:図形 d_vec:物体情報 m_vec:監視点 それぞれ座標を格納する
    int[,] io_flag;

    /*     デバッグ用（FPS）     */
    int frameCount;
    float prevTime;
    /* ------------------------- */
    int debug;
    
        

    // Use this for initialization
    void Start () {
        // カメラセットアップ
        cam.setDevice(1);

        // HSV画像の設定
        int x_window = GlobalVar.CAMERA_WIDTH;
        int y_window = GlobalVar.CAMERA_HEIGHT;
        CvSize WINDOW_SIZE = new CvSize(x_window, y_window);
        h_img = Cv.CreateImage(WINDOW_SIZE, BitDepth.U8, 3);

        // データ格納用配列の初期化
        int x_index = x_window / GlobalVar.POINT_INTERVAL;
        int y_index = y_window / GlobalVar.POINT_INTERVAL;
        ps_arr = new int[y_index, x_index];
        ie_arr = new int[y_index, x_index];

        // テクスチャの設定
        d_img = Cv.CreateImage(WINDOW_SIZE, BitDepth.U8, 3);
        texture = new Texture2D(x_window, y_window, TextureFormat.RGB24, false);
        GetComponent<Renderer>().material.mainTexture = texture;

        // 図形格納用配列の初期化
        xdata   = new double[num];
        ydata   = new double[num];
        f_vec   = new ArrayList();
        d_vec   = new ArrayList();
        m_vec   = new ArrayList();
        io_flag = new int[y_index, x_index];    // 外部(遠):0 外部(近):1 内部:2

        // 図形の座標を格納
        xdata[0] = 100; ydata[0] = 170;
        xdata[1] = 92;  ydata[1] = 165;
        xdata[2] = 90;  ydata[2] = 160;
        xdata[3] = 92;  ydata[3] = 155;
        xdata[4] = 100; ydata[4] = 150;

        xdata[5] = 115;  ydata[5] = 150;
        xdata[6] = 135;  ydata[6] = 147;
        xdata[7] = 155;  ydata[7] = 141;
        xdata[8] = 170;  ydata[8] = 132;
        xdata[9] = 175;  ydata[9] = 120;
        xdata[10] = 170; ydata[10] = 108;
        xdata[11] = 155; ydata[11] = 99;
        xdata[12] = 135; ydata[12] = 93;
        xdata[13] = 115; ydata[13] = 90;

        xdata[14] = 100; ydata[14] = 90;
        xdata[15] = 92;  ydata[15] = 85;
        xdata[16] = 90;  ydata[16] = 80;
        xdata[17] = 92;  ydata[17] = 75;
        xdata[18] = 100; ydata[18] = 70;

        xdata[19] = 115; ydata[19] = 63;
        xdata[20] = 130; ydata[20] = 57;
        xdata[21] = 145; ydata[21] = 54;
        xdata[22] = 160; ydata[22] = 53;
        xdata[23] = 175; ydata[23] = 54;
        xdata[24] = 190; ydata[24] = 57;
        xdata[25] = 205; ydata[25] = 65;
        xdata[26] = 220; ydata[26] = 75;

        xdata[27] = 230; ydata[27] = 90;
        xdata[28] = 235; ydata[28] = 107;
        xdata[29] = 237; ydata[29] = 120;
        xdata[30] = 235; ydata[30] = 133;
        xdata[31] = 230; ydata[31] = 150;

        xdata[32] = 220; ydata[32] = 165;
        xdata[33] = 205; ydata[33] = 175;
        xdata[34] = 190; ydata[34] = 183;
        xdata[35] = 175; ydata[35] = 186;
        xdata[36] = 160; ydata[36] = 187;
        xdata[37] = 145; ydata[37] = 186;
        xdata[38] = 130; ydata[38] = 183;
        xdata[39] = 115; ydata[39] = 177;

        xdata[40] = 100; ydata[40] = 170;
      
        // 観測点の座標を配列に格納
        insertDataToVector3(m_vec);

        // 観測点データ初期化
        initMFlag(io_flag);
        insertDataToVector2(xdata, ydata, f_vec);
        getIODMonitoringPoint(f_vec, m_vec, io_flag);
        printIODMonitoringPoint(d_img, io_flag);

        // 初回内部外部判定実行
        f_vec = new ArrayList();
        d_vec = new ArrayList();
        

        /*     デバッグ用（FPS）     */
        frameCount = 0;
        prevTime = 0.0f;
        /* ------------------------  */
	}
	
	// Update is called once per frame
	void Update () {
        /*     デバッグ用（FPS）     */
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;
        /* ------------------------- */

        // 変数初期化
        f_vec = new ArrayList();
        d_vec = new ArrayList();

        // カメラ画像の取得
        i_img = cam.getCameraImage();

        // カメラ画像をBGRからHSVへ変換
        g.convertBgrToHsv(i_img, h_img);

        // 平滑化
        g.convertSmooothing(h_img);

        // カメラ画像の任意の点からデータ取得
        g.getPointData(h_img, ps_arr);

        // 図形の座標を配列に格納
        insertDataToVector2(xdata, ydata, f_vec);

        // 画像を初期化（真っ白に）
        initImg(d_img);

        // 内部に点がある場合
        if (isInsideOrOutside(io_flag, ps_arr))
        {
            // 図形を移動
            overrideXYData(xdata, ydata, io_flag);
            
            // 観測点データ初期化 
            initMFlag(io_flag);

            // 図形と観測点の内部外部判定を行う
            getIODMonitoringPoint(f_vec, m_vec, io_flag);
        }

        // 図形を描画
        printFigureData(d_img, f_vec);

        // 観測点の内部外部判定結果を描写
        printIODMonitoringPoint(d_img, io_flag);

        // 手情報を描画
        printPointData(d_img, ps_arr);

        // テクスチャ表示
        using (var r_img = Cv2.CvArrToMat(d_img))
        {
            texture.LoadImage(r_img.ImEncode(".jpeg"));
            texture.Apply();
        }
        /*---------------------------------------*/
        /*            デバッグ用（FPS）          */
        /*---------------------------------------*/
        //if (time >= 0.5f)
        //{
        //    Debug.LogFormat("{0}fps", frameCount/time);
        //    frameCount = 0;
        //    prevTime = Time.realtimeSinceStartup;
        //}
        /* ------------------------- */
    }

    /*
    private bool isMovableFigure(double[] x, double[] y)
    {
        int m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;
        int m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;

        for (int i = 0; i < m_x; i++)
        {
            if (x[i] < 5 || x[i] > GlobalVar.CAMERA_WIDTH - 5)
            {
                return false;
            }
        }

        for (int i = 0; i < m_y; i++)
        {
            if (y[i] < 5 || y[i] > GlobalVar.CAMERA_HEIGHT - 5)
            {
                return false;
            }
        }

        return true;
    }
    */

    //---------------------------------------------------------
    // 関数名 : initImg
    // 機能   : 画像を初期化
    // 引数   : img/画像
    // 戻り値 : img/画像
    //---------------------------------------------------------
    unsafe private IplImage initImg(IplImage img)
    {
        int index;

        // 画像初期化（すべて白）
        byte* pxe = (byte*)img.ImageData;
        for (int y = 0; y < GlobalVar.CAMERA_HEIGHT; y++)
        {
            for (int x = 0; x < GlobalVar.CAMERA_WIDTH; x++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * y + (x * 3);
                pxe[index] = 255;
                pxe[index + 1] = 255;
                pxe[index + 2] = 255;
            }
        }

        return img;
    }

    //---------------------------------------------------------
    // 関数名 : initMFlag
    // 機能   : 観測点からのデータを初期化する
    // 引数   : flag/観測点の内部外部値
    // 戻り値 : flag/観測点の内部外部値
    //---------------------------------------------------------
    private int[,] initMFlag(int[,] flag)
    {
        int m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;
        int m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;

        for (int y = 0; y < m_y; y++)
        {
            for (int x = 0; x < m_x; x++)
            {
                flag[y, x] = 0;
            }
        }

        return flag;
    }

    //---------------------------------------------------------
    // 関数名 : getIODMonitoringPoint
    // 機能   : 図形に対しての、全ての観測点における内部外部判定値を格納
    // 引数   : f_v/図形座標 m_v/観測点座標 flag/観測点の内部外部値
    // 戻り値 : flag/観測点の内部外部値
    //---------------------------------------------------------
    private int[,] getIODMonitoringPoint(ArrayList f_v, ArrayList m_v, int[,] flag)
    {
        for (int i = 0; i < m_v.Count; i++)
        {
            Complex m_com = (Complex)m_v[i];

            for (int j = 0; j < f_v.Count - 1; j++)
            {
                Complex f_com1 = (Complex)f_v[j];
                Complex f_com2 = (Complex)f_v[j + 1];
                Complex sub1 = f_com1.sub(m_com);
                Complex sub2 = f_com2.sub(f_com1);

                double ch3 = Mathf.Sqrt(Mathf.Pow((float)sub1.x, 2) + Mathf.Pow((float)sub1.y, 2));

                if (ch3 < 20)
                {
                    double ch4 = sub1.x * sub2.y - sub1.y * sub2.x;
                    if (ch4 >= 0)
                    {
                        flag[i / 32, i % 32] = 1;
                    }
                    else {
                        flag[i /32, i % 32] = 2;
                    }
                    break;
                }
            }
        }

        // 補完
        completionIOFlag(flag);


        return flag;
    }

    //---------------------------------------------------------
    // 関数名 : overrideXYData
    // 機能   : 物体情報との内部外部判定結果から、図形のXYデータを書き換える
    // 引数   : x/図形のXデータ y/図形のYデータ flag/観測点の内部外部値
    // 戻り値 : なし
    //---------------------------------------------------------
    private void overrideXYData(double[] x, double[] y, int[,] flag)
    {
        int m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;
        int m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;

        Debug.Log(x[0]);

        for (int t_y = 0; t_y < m_y; t_y++)
        {
            for (int t_x = 0; t_x < m_x; t_x++)
            {
                if (flag[t_y, t_x] == 3 &&
                    t_x != 0 && t_x != m_x &&
                    t_y != 0 && t_y != m_y)
                {
                    // 3の周辺の情報を調べる
                    if (flag[t_y - 1, t_x] == 1)
                    {
                        // 図形が端まで達しているときは移動しない
                        for (int i = 0; i < num; i++)
                        {
                            if (y[i] < 10)
                            {
                                return;
                            }
                        }

                        for (int i = 0; i < num; i++)
                        {
                            y[i] = y[i] - 3;
                        }
                    }
                    else if (flag[t_y + 1, t_x] == 1)
                    {
                        // 図形が端まで達しているときは移動しない
                        for (int i = 0; i < num; i++)
                        {
                            if (y[i] > GlobalVar.CAMERA_HEIGHT - 10)
                            {
                                return;
                            }
                        }

                        for (int i = 0; i < num; i++)
                        {
                            y[i] = y[i] + 3;
                        }
                    }
                    if (flag[t_y, t_x - 1] == 1)
                    {
                        // 図形が端まで達しているときは移動しない
                        for (int i = 0; i < num; i++)
                        {
                            if (x[i] > GlobalVar.CAMERA_WIDTH - 10)
                            {
                                return;
                            }
                        }

                        for (int i = 0; i < num; i++)
                        {
                            x[i] = x[i] + 3;
                        }
                    }
                    else if (flag[t_y, t_x + 1] == 1)
                    {
                        // 図形が端まで達しているときは移動しない
                        for (int i = 0; i < num; i++)
                        {
                            if (x[i] < 10)
                            {
                                return;
                            }
                        }

                        for (int i = 0; i < num; i++)
                        {
                            x[i] = x[i] - 3;
                        }
                    }
                }
            }
        }
    }

    //---------------------------------------------------------
    // 関数名 : completionIOFlag
    // 機能   : 図形に対する内部判定(近)から内部判定(遠)を補完する
    // 引数   : flag/観測点の内部外部値
    // 戻り値 : flag/観測点の内部外部値
    //---------------------------------------------------------
    private int[,] completionIOFlag(int[,] flag)
    {
        int m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;
        int m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;

        for (int y = 0; y < m_y; y++)
        {
            for (int x = 0; x < m_x; x++)
            {
                if (x != 0 && y != 0) {
                    if (flag[y, x] == 0 && flag[y, x - 1] == 2)
                    {
                        flag[y, x] = 2;
                    }
                }
            }
        }

        return flag;
    }

    /*
    private ArrayList insertDataToVector1(int[,] arr, ArrayList v)
    {
        for (int y = 0; y < GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL; y++)
        {
            for (int x = 0; x < GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL; x++)
            {
                if (arr[y, x] == 1)
                {
                    v.Add(new Complex(x * GlobalVar.POINT_INTERVAL, y * GlobalVar.POINT_INTERVAL));
                }
            }
        }

        return v;
    }
    */

    //---------------------------------------------------------
    // 関数名 : insertDataToVector2
    // 機能   : 図形データを一つの配列に統合
    // 引数   : xdata/X座標 ydata/Y座標 v/配列
    // 戻り値 : v/配列
    //---------------------------------------------------------
    private ArrayList insertDataToVector2(double[] xdata, double[] ydata, ArrayList v)
    {
        for (int i = 0; i < num - 1; i++)
        {
            // 配列に格納
            v.Add(new Complex(xdata[i], GlobalVar.CAMERA_HEIGHT - ydata[i]));
        }

        return v;
    }

    //---------------------------------------------------------
    // 関数名 : insertDataToVector3
    // 機能   : 観測点の座標を配列に格納
    // 引数   : v/配列
    // 戻り値 : v/配列
    //---------------------------------------------------------
    private ArrayList insertDataToVector3(ArrayList v)
    {
        int m_x, m_y, t_x, t_y;
        m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;
        m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;

        for (int y = 0; y < m_y; y++)
        {
            for (int x = 0; x < m_x; x++)
            {
                t_x = x * GlobalVar.POINT_INTERVAL + (GlobalVar.POINT_INTERVAL / 2);
                t_y = y * GlobalVar.POINT_INTERVAL + (GlobalVar.POINT_INTERVAL / 2);

                v.Add(new Complex(t_x, t_y));
            }
        }

        return v;
    }

    //---------------------------------------------------------
    // 関数名 : isInsideOrOutside
    // 機能   : 物体情報が内部にあるかチェック
    // 引数   : flag/観測点の内部外部値 arr/物体の情報
    // 戻り値 : true/内部あり false/内部なし
    //---------------------------------------------------------
    private bool isInsideOrOutside(int[,] flag, int[,] arr)
    {
        int m_x = GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL;
        int m_y = GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL;

        for (int y = 0; y < m_y; y++)
        {
            for (int x = 0; x < m_x; x++)
            {
                if (flag[y,x] == 2 && arr[y,x] == 2)
                {
                    flag[y,x] = 3;
                    return true;
                }
            }
        }

        return false;
    }

    //---------------------------------------------------------
    // 関数名 : printIDot
    // 機能   : 図形内部の点を描画
    // 引数   : img/画像 gx/X座標 gy/Y座標
    // 戻り値 : img/画像
    //---------------------------------------------------------
    unsafe private IplImage printIDot(IplImage img, double gx, double gy)
    {
        double index;

        byte* pxe = (byte*)img.ImageData;

        for (int y_ad = -1; y_ad <= 1; y_ad++)
        {
            for (int x_ad = -1; x_ad <= 1; x_ad++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * (int)(gy + y_ad) + ((int)(gx + x_ad) * 3);
                pxe[(int)index] = 0;
                pxe[(int)index + 1] = 100;
                pxe[(int)index + 2] = 255;
            }
        }

        return img;
    }

    /*
    unsafe private IplImage printSDot(IplImage img, double gx, double gy, int flag)
    {
        double index;

        byte* pxe = (byte*)img.ImageData;

        for (int y_ad = -1; y_ad <= 1; y_ad++)
        {
            for (int x_ad = -1; x_ad <= 1; x_ad++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * (int)(gy + y_ad) + ((int)(gx + x_ad) * 3);

                if (flag == 1 && x_ad == 0 && y_ad == 0)
                {

                    pxe[(int)index] = 255;
                    pxe[(int)index + 1] = 255;
                    pxe[(int)index + 2] = 255;
                }
                else
                {
                    pxe[(int)index] = 0;
                    pxe[(int)index + 1] = 255;
                    pxe[(int)index + 2] = 0;
                }
            }
        }
        return img;
    }
    */

    //---------------------------------------------------------
    // 関数名 : printFigureData
    // 機能   : 図形を描画
    // 引数   : img/画像 v/座標配列
    // 戻り値 : img/画像
    //---------------------------------------------------------
    unsafe private IplImage printFigureData(IplImage img, ArrayList v)
    {
        double index;
        byte* pxe = (byte*)img.ImageData;

        for (int i = 0; i < v.Count; i++)
        {
            Complex com = (Complex)v[i];

            for (int y_ad = 0; y_ad <= 1; y_ad++)
            {
                for (int x_ad = 0; x_ad <= 1; x_ad++)
                {
                    index = (GlobalVar.CAMERA_WIDTH * 3) * (int)(com.y + y_ad) + ((int)(com.x + x_ad) * 3);
                    pxe[(int)index] = 0;
                    pxe[(int)index + 1] = 0;
                    pxe[(int)index + 2] = 0;
                }
            }
        }

        return img;
    }

    //---------------------------------------------------------
    // 関数名 : printPointData
    // 機能   : 物体を描画
    // 引数   : img/画像 arr/物体配列
    // 戻り値 : img/画像
    //---------------------------------------------------------
    unsafe private IplImage printPointData(IplImage img, int[,] arr)
    {
        int index, x_tmp, y_tmp;
        byte* pxe = (byte*)img.ImageData;

        for (int y = GlobalVar.POINT_INTERVAL / 2; y < GlobalVar.CAMERA_HEIGHT; y = y + GlobalVar.POINT_INTERVAL)
        {
            for (int x = GlobalVar.POINT_INTERVAL / 2; x < GlobalVar.CAMERA_WIDTH; x = x + GlobalVar.POINT_INTERVAL)
            {
                for (int y_ad = 0; y_ad <= 1; y_ad++)
                {
                    for (int x_ad = 0; x_ad <= 1; x_ad++)
                    {
                        index = (GlobalVar.CAMERA_WIDTH * 3) * (y + y_ad) + ((x + x_ad) * 3);
                        x_tmp = (x - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;
                        y_tmp = (y - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;

                        if (arr[y_tmp, x_tmp] == 2)
                        {
                            pxe[index] = 0;
                            pxe[index + 1] = 0;
                            pxe[index + 2] = 0;
                        }
                    }
                }
            }
        }

        return img;
    }

    // デバッグ用
    unsafe private IplImage printIODMonitoringPoint(IplImage img, int[,] flag)
    {
        int index, x, y;
        byte* pxe = (byte*)img.ImageData;

        for (int py = 0; py < GlobalVar.CAMERA_HEIGHT / GlobalVar.POINT_INTERVAL; py++)
        {
            for (int px = 0; px < GlobalVar.CAMERA_WIDTH / GlobalVar.POINT_INTERVAL; px++)
            {
                x = px * GlobalVar.POINT_INTERVAL + 5;
                y = py * GlobalVar.POINT_INTERVAL + 5;

                for (int y_ad = 0; y_ad <= 1; y_ad++)
                {
                    for (int x_ad = 0; x_ad <= 1; x_ad++)
                    {
                        index = (GlobalVar.CAMERA_WIDTH * 3) * (int)(y + y_ad) + ((int)(x + x_ad) * 3);

                        if (flag[py, px] == 2)
                        {
                            pxe[index] = 0;
                            pxe[index + 1] = 0;
                            pxe[index + 2] = 255;
                        }
                    }
                }
            }
        }
        return img;
    }



    /// <summary>
    /// 終了処理
    /// </summary>
    void OnApplicationQuit()
    {
        // リソースの破棄
        Cv.ReleaseImage(i_img);
        Cv.ReleaseImage(h_img);
        cam.Release();
    }

    class Complex
    {
        public double x;
        public double y;
        bool flag;

        public Complex(double x, double y)
        {
            this.x = x;
            this.y = y;
            this.flag = false;
        }

        Complex add(Complex c2)
        {//複素数足し算
            return new Complex(this.x + c2.x, this.y + c2.y);
        }
        public Complex sub(Complex c2)
        {//複素数引き算
            return new Complex(this.x - c2.x, this.y - c2.y);
        }
        Complex mul(Complex c2)
        {//複素数掛け算
            return new Complex(this.x * c2.x - this.y * c2.y, this.x * c2.y + this.y * c2.x);
        }
        Complex div(Complex c2)
        {//複素数割り算
            double d = (c2.x * c2.x) + (c2.y * c2.y);
            return new Complex(((this.x * c2.x) + (this.y * c2.y)) / d, (-(this.x * c2.y) + (this.y * c2.x)) / d);
        }

        //void print()
        //{
        //    System.out.println(this.x + "+" + this.y + "*i");
        //}

        /*boolean caushy(Complex c1,Complex c2){
        }*/
        double realPart()
        {
            return this.x;
        }
        double realAbs()
        {
            if (this.x >= 0)
            {
                return this.x;
            }
            return this.x * (-1);
        }
        double imaginaryPart()
        {
            return this.y;
        }
        double imaginaryAbs()
        {
            if (this.x >= 0) { return this.y; }
            return this.y * (-1);
        }
    }
}
