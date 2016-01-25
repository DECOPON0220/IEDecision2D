using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
    // 変数宣言
    int[,] ps_arr;                      // データ格納
    IplImage i_img;                     // IplImage;
    IplImage h_img = new IplImage();    // HSV画像
    Camera cam = new Camera();          // カメラ
    Graphic g = new Graphic();          // 画像編集関係
    Texture2D texture;                  // テクスチャ
    IplImage d_img = new IplImage();    // テクスチャ画像
    int[] cg_xy;                        // 重心 - 0:X座標 1:Y座標
    int[] mn_xy;                        // 重心に最も近い点座標 - 0:X座標 1:Y座標
    // 図形用
    int num = 5;
    double[] xdata, ydata;
    // 図形計算用
    ArrayList f_vec, d_vec;
    int f_interval = 3;

    /*     デバッグ用（FPS）     */
    int frameCount;
    float prevTime;
    /* ------------------------- */
    
        

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

        // テクスチャの設定
        d_img = Cv.CreateImage(WINDOW_SIZE, BitDepth.U8, 3);
        texture = new Texture2D(x_window, y_window, TextureFormat.RGB24, false);
        GetComponent<Renderer>().material.mainTexture = texture;

        // 図形格納用配列の初期化
        xdata = new double[num];
        ydata = new double[num];
        f_vec = new ArrayList();
        d_vec  = new ArrayList();

        // 図形の座標を格納
        xdata[0] = 110; ydata[0] = 170;
        xdata[1] = 110; ydata[1] = 70;
        xdata[2] = 210; ydata[2] = 70;
        xdata[3] = 210; ydata[3] = 170;
        xdata[4] = 110; ydata[4] = 170;

        // 図形の重心
        cg_xy = new int[2];
        mn_xy = new int[2];

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
        mn_xy = new int[2];

        // カメラ画像の取得
        i_img = cam.getCameraImage();

        // カメラ画像をBGRからHSVへ変換
        g.convertBgrToHsv(i_img, h_img);

        // 平滑化
        g.convertSmooothing(h_img);

        // カメラ画像の任意の点からデータ取得
        g.getPointData(h_img, ps_arr);

        // 点データを配列に格納
        insertDataToVector1(ps_arr, d_vec);

        // 図形の座標を配列に格納
        insertDataToVector2(xdata, ydata, f_vec);

        // 図形の重心を配列に格納
        getCenterDot(f_vec, cg_xy);

        // 画像を初期化（真っ白に）
        initImg(d_img);

        /*-------------------------------*/
        /*     デバッグ用（重心を表示）  */
        /*-------------------------------*/
        printCenterDot(d_img, cg_xy);

        // 内部に点がある場合
        if (isInsideOrOutside(f_vec, d_vec))
        {
            //Debug.Log("debug");

            // 重心に一番近い手座標を配列に格納
            getNearCenterDot(d_vec, cg_xy, mn_xy);

            // 重心に一番近い手座標を参考にして、図形を動かす
            moveFigure(xdata, ydata, cg_xy, mn_xy);
        }

        // 図形を描画
        printVectorData(d_img, f_vec);

        // 手情報を描画
        printPointData(d_img, ps_arr);

        // 重心に一番近い座標を表示
        printNearCenterDot(d_img, mn_xy);

        /*---------------------------------------*/
        /*     デバッグ用（点情報表示）          */
        /*---------------------------------------*/
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

    // 斜めには動かない
    private void moveFigure(double[] x, double[] y, int[] xy1, int[] xy2)
    {
        int l_x, l_y;
        int f_xy; // 1:x 2:y

        l_x = xy2[0] - xy1[0];
        l_y = xy2[1] - xy1[1];

        // XYどちらに移動するか判定
        if (Mathf.Abs((float)l_x) > Mathf.Abs((float)l_y))
        {
            f_xy = 1;
        }
        else
        {
            f_xy = 2;
        }

        // X方向に移動
        if (f_xy == 1)
        {
            if (l_x > 0)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    x[i] -= 1;
                }
            }
            else
            {
                for (int i = 0; i < x.Length; i++)
                {
                    x[i] += 1;
                }
            }
        }
        // Y方向に移動
        else if (f_xy == 2)
        {
            if (l_y > 0)
            {
                for (int i = 0; i < y.Length; i++)
                {
                    y[i] -= 1;
                }
            }
            else
            {
                for (int i = 0; i < y.Length; i++)
                {
                    y[i] += 1;
                }
            }
        }

        return ;
    }

    private int[] getNearCenterDot(ArrayList v, int[] xy1,int[] xy2)
    {
        float d = 100.0f;
        float t_d = 100.0f;

        //Debug.Log(v.Count);
        for (int i = 0; i < v.Count; i++)
        {
            Complex d_com = (Complex)v[i];

            d = Mathf.Sqrt(Mathf.Pow((float)(d_com.x-xy1[0]),2) + Mathf.Pow((float)(d_com.y-xy1[1]),2));

            if (t_d > d)
            {
                t_d = d;
                xy2[0] = (int)d_com.x;
                xy2[1] = (int)d_com.y;
            }
        }

        //Debug.LogFormat("d:{0} t_d:{1}, xy2[0]:{2} xy2[1]:{3}", d, t_d, xy2[0], xy2[1]);

        return xy2;
    }

    private int[] getCenterDot(ArrayList v, int[] xy)
    {
        double t_x = 0;
        double t_y = 0;

        for (int i = 0; i < v.Count - 1; i++)
        {
            Complex f_com = (Complex)v[i];
            t_x += f_com.x;
            t_y += f_com.y;
        }

        xy[0] = (int)(t_x / (v.Count - 1));
        xy[1] = (int)(t_y / (v.Count - 1));

        return cg_xy;
    }

    private bool isInsideOrOutside(ArrayList f_v, ArrayList d_v)
    {
        for (int i = 0; i < d_v.Count; i++)
        {
            Complex d_com = (Complex)d_v[i];  // 図形

            // 手
            for (int j = 0; j < f_v.Count - 1; j++)
            {
                Complex f_com1 = (Complex)f_v[j];
                Complex f_com2 = (Complex)f_v[j + 1];
                Complex sub1 = f_com1.sub(d_com);
                Complex sub2 = f_com2.sub(f_com1);

                double ch3 = Mathf.Sqrt(Mathf.Pow((float)sub1.x, 2) + Mathf.Pow((float)sub1.y, 2));

                if (ch3 < 20)
                {
                    double ch4 = sub1.x * sub2.y - sub1.y * sub2.x;
                    if (ch4 >= 0)
                    {
                        //近くの外
                        //printSDot(d_img, (int)(com.x) - 1, (int)(com.y) - 1, 1);
                        //flag[k] = 1;
                    }
                    else {
                        //近くの内
                        //printSDot(d_img, (int)(com.x) - 1, (int)(com.y) - 1, 2);
                        //flag[k] = 2;
                        return true;
                    }
                    break;
                }
            }
        }

        return false;
    }

    /*
    private bool isInsideOrOutside(ArrayList f_v, ArrayList d_v)
    {
        for (int i = 0; i < f_v.Count; i++)
        {
            Complex f_com = (Complex)f_v[i];  // 図形
            
            // 手
            for (int j = 0; j < d_v.Count - 1; j++)
            {
                Complex d_com1 = (Complex)d_v[j];
                Complex d_com2 = (Complex)d_v[j+1];
                Complex sub1 = d_com1.sub(f_com);
                Complex sub2 = d_com2.sub(d_com1);

                double ch3 = Mathf.Sqrt(Mathf.Pow((float)sub1.x, 2) + Mathf.Pow((float)sub1.y, 2));

                if (ch3 < 10)
                {
                    double ch4 = sub1.x * sub2.y - sub1.y * sub2.x;
                    if (ch4 >= 0)
                    {
                        //近くの外
                        //printSDot(d_img, (int)(com.x) - 1, (int)(com.y) - 1, 1);
                        //flag[k] = 1;
                    }
                    else {
                        //近くの内
                        //printSDot(d_img, (int)(com.x) - 1, (int)(com.y) - 1, 2);
                        //flag[k] = 2;
                        return true;
                    }
                    break;
                }
            }
        }

        return false;
    }
    */

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

    private ArrayList insertDataToVector2(double[] xdata, double[] ydata, ArrayList v)
    {
        for (int i = 0; i < num - 1; i++)
        {
            // 二点間の距離を取得
            int d = (int)Mathf.Sqrt(Mathf.Pow((float)xdata[i + 1] - (float)xdata[i], 2)
                                  + Mathf.Pow((float)ydata[i + 1] - (float)ydata[i], 2));
            // 各角間の座標生成
            for (int j = 0; j < d / f_interval; j++)
            {
                double gx = f_interval * j * (xdata[i + 1] - xdata[i]) / d + xdata[i];
                double gy = f_interval * j * (ydata[i + 1] - ydata[i]) / d + ydata[i];

                // 配列に格納
                v.Add(new Complex(gx, gy));
            }
        }

        return v;
    }

    /*
    private ArrayList insertDataToVector3(double[] xdata, double[] ydata, ArrayList v)
    {
        // 配列に格納
        for (int i = 0; i < num; i++)
        {
            v.Add(new Complex(xdata[i], ydata[i]));
        }

        return v;
    }
    */

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

    unsafe private IplImage printVectorData(IplImage img, ArrayList v)
    {
        double index;
        byte* pxe = (byte*)img.ImageData;

        for (int i = 0; i < v.Count - 1; i++)
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

                        if (arr[y_tmp, x_tmp] == 1)
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
    unsafe private IplImage printNearCenterDot(IplImage img, int[] arr)
    {
        int index;
        byte* pxe = (byte*)img.ImageData;

        for (int y_ad = 0; y_ad <= 1; y_ad++)
        {
            for (int x_ad = 0; x_ad <= 1; x_ad++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * (arr[1] + y_ad) + ((arr[0] + x_ad) * 3);
                pxe[index] = 0;
                pxe[index + 1] = 255;
                pxe[index + 2] = 0;
            }
        }

        return img;
    }
    
    // デバッグ用
    unsafe private IplImage printCenterDot(IplImage img, int[] arr)
    {
        int index;
        byte* pxe = (byte*)img.ImageData;

        for (int y_ad = 0; y_ad <= 1; y_ad++)
        {
            for (int x_ad = 0; x_ad <= 1; x_ad++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * (arr[1] + y_ad) + ((arr[0] + x_ad) * 3);
                pxe[index] = 0;
                pxe[index + 1] = 0;
                pxe[index + 2] = 255;
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
