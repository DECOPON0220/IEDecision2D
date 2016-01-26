using OpenCvSharp;



class Graphic
{
    //---------------------------------------------------------
    // 関数名 : convertBgrToHsv
    // 機能   : 画像をBGRからHSVに変換
    // 引数   : img/BGR画像
    // 戻り値 : img/HSV画像
    //---------------------------------------------------------
    public IplImage convertBgrToHsv(IplImage i_img, IplImage h_img)
    {
        Cv.CvtColor(i_img, h_img, ColorConversion.BgrToHsv);
        return h_img;
    }

    //---------------------------------------------------------
    // 関数名 : convertSmooothing
    // 機能   : 平滑化
    // 引数   : img/平滑化前画像
    // 戻り値 : img/平滑化後画像
    //---------------------------------------------------------
    public IplImage convertSmooothing(IplImage img)
    {
        Cv.Smooth(img, img, SmoothType.Median, 5, 0, 0, 0);
        return img;
    }

    //---------------------------------------------------------
    // 関数名 : getPointData
    // 機能   : 指定された点からの情報を取得
    // 引数   : img/カメラ画像 arr/情報
    // 戻り値 : arr/情報
    //---------------------------------------------------------
    unsafe public int[,] getPointData(IplImage img, int[,] arr)
    {
        byte* pxe = (byte*)img.ImageData;

        // 縦横一定間隔ごとのデータ取得
        for (int y = GlobalVar.POINT_INTERVAL / 2; y < GlobalVar.CAMERA_HEIGHT; y = y + GlobalVar.POINT_INTERVAL)
        {
            for (int x = GlobalVar.POINT_INTERVAL / 2; x < GlobalVar.CAMERA_WIDTH; x = x + GlobalVar.POINT_INTERVAL)
            {
                // 配列に点の情報を格納
                setPointData(pxe, x, y, arr, isBluePxe(pxe, x, y));
            }
        }

        return arr;
    }

    // 保留
    //---------------------------------------------------------
    // 関数名 : delDataNoise
    // 機能   : 点からの情報を格納した配列からノイズを除去
    // 引数   : arr/情報
    // 戻り値 : arr/ノイズを除去した情報
    //---------------------------------------------------------
    public int[,] delDataNoise(int[,] arr)
    {
        // 

        return arr;
    }

    //---------------------------------------------------------
    // 関数名 : isBluePxe
    // 機能   : ピクセルが青かどうか判別
    // 引数   : pxe/ピクセル情報 x/横ピクセル y/縦ピクセル
    // 戻り値 : true/青 false/青以外
    //---------------------------------------------------------
    unsafe private bool isBluePxe(byte* pxe, int x, int y)
    {
        // インデックス計算
        int index = (GlobalVar.CAMERA_WIDTH * 3) * y + (x * 3);

        int h = pxe[index];
        int s = pxe[index];
        int v = pxe[index];

        // 青の場合
        if (h >= 85 && h <= 110
         && s >= 5 && s <= 255
         && v >= 5 && v <= 255)
        {
            return true;
        }

        return false;
    }

    //---------------------------------------------------------
    // 関数名 : setPointData
    // 機能   : データを取得、配列に保存
    // 引数   : pxe/ピクセル情報 x/インデックス y/インデックス arr/データ
    // 戻り値 : なし
    //---------------------------------------------------------
    unsafe private void setPointData(byte* pxe, int x, int y, int[,] arr, bool flag)
    {
        // 青
        if (flag)
        {
            arr[(y / GlobalVar.POINT_INTERVAL), (x / GlobalVar.POINT_INTERVAL)] = 0;
        }
        // 青以外
        else
        {
            arr[(y / GlobalVar.POINT_INTERVAL), (x / GlobalVar.POINT_INTERVAL)] = 2;
        }
    }



    /*                        */
    /*     デバッグ用関数     */
    /*                        */

    //---------------------------------------------------------
    // 関数名 : getPointImage1
    // 機能   : 配列に格納されている情報を元に画像を生成、取得
    // 引数   : img/画像 arr/点情報配列
    // 戻り値 : img/生成画像
    //---------------------------------------------------------
    unsafe public IplImage getPointImage1(IplImage img, int[,] arr)
    {
        // 変数宣言
        int index, x_tmp, y_tmp;

        // 画像初期化（すべて黒）
        byte* pxe = (byte*)img.ImageData;
        for (int y = 0; y < GlobalVar.CAMERA_HEIGHT; y++)
        {
            for (int x = 0; x < GlobalVar.CAMERA_WIDTH; x++)
            {
                index = (GlobalVar.CAMERA_WIDTH * 3) * y + (x * 3);
                pxe[index] = 0;
                pxe[index + 1] = 0;
                pxe[index + 2] = 0;
            }
        }

        // 青以外の点を白
        for (int y = GlobalVar.POINT_INTERVAL / 2; y < GlobalVar.CAMERA_HEIGHT; y = y + GlobalVar.POINT_INTERVAL)
        {
            for (int x = GlobalVar.POINT_INTERVAL / 2; x < GlobalVar.CAMERA_WIDTH; x = x + GlobalVar.POINT_INTERVAL)
            {
                for (int y_ad = -1; y_ad <= 1; y_ad++)
                {
                    for (int x_ad = -1; x_ad <= 1; x_ad++)
                    {
                        // インデックス計算
                        index = (GlobalVar.CAMERA_WIDTH * 3) * (y + y_ad) + ((x + x_ad) * 3);
                        x_tmp = (x - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;
                        y_tmp = (y - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;

                        // 青以外の場合，白
                        if (arr[y_tmp, x_tmp] == 1)
                        {
                            pxe[index] = 255;
                            pxe[index + 1] = 255;
                            pxe[index + 2] = 255;
                        }

                        /* デバッグ */
                        //if (x_tmp == 0 && y_tmp == 0)
                        //{
                        //    pxe[index] = 0;
                        //    pxe[index + 1] = 255;
                        //    pxe[index + 2] = 0;
                        //}
                        /*----------*/
                    }

                }
            }
        }

        return img;
    }

    //---------------------------------------------------------
    // 関数名 : getPointImage2
    // 機能   : 配列に格納されている情報を元に画像を生成、取得
    // 引数   : img/画像 arr1/点情報配列1 arr/点情報配列2
    // 戻り値 : img/生成画像
    //---------------------------------------------------------
    unsafe public IplImage getPointImage2(IplImage img, int[,] arr1, int[,] arr2)
    {
        // 変数宣言
        int index;
        int x_tmp = 0;
        int y_tmp = 0;

        // 画像初期化（すべて黒）
        byte* pxe = (byte*)img.ImageData;
        for (int y = 0; y < GlobalVar.CAMERA_HEIGHT; y++)
        {
            for (int x = 0; x < GlobalVar.CAMERA_WIDTH * 2; x++)
            {
                index = ((GlobalVar.CAMERA_WIDTH * 2) * 3) * y + (x * 3);
                pxe[index] = 0;
                pxe[index + 1] = 0;
                pxe[index + 2] = 0;
            }
        }
        // 青以外の点を白
        for (int y = GlobalVar.POINT_INTERVAL / 2; y < GlobalVar.CAMERA_HEIGHT; y = y + GlobalVar.POINT_INTERVAL)
        {
            for (int x = GlobalVar.POINT_INTERVAL / 2; x < GlobalVar.CAMERA_WIDTH * 2; x = x + GlobalVar.POINT_INTERVAL)
            {
                for (int y_ad = -1; y_ad <= 1; y_ad++)
                {
                    for (int x_ad = -1; x_ad <= 1; x_ad++)
                    {
                        // インデックス計算
                        index = ((GlobalVar.CAMERA_WIDTH * 2) * 3) * (y + y_ad) + ((x + x_ad) * 3);

                        y_tmp = (y - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;
                        if (x < GlobalVar.CAMERA_WIDTH)
                        {
                            x_tmp = (x - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;

                            //Debug.Log(x_tmp);
                            if (arr1[y_tmp, x_tmp] == 1)
                            {
                                pxe[index] = 255;
                                pxe[index + 1] = 255;
                                pxe[index + 2] = 255;
                            }

                        }
                        else
                        {
                            x_tmp = ((x - GlobalVar.CAMERA_WIDTH) - (GlobalVar.POINT_INTERVAL / 2)) / GlobalVar.POINT_INTERVAL;

                            if (arr2[y_tmp, x_tmp] == 1)
                            {
                                pxe[index] = 255;
                                pxe[index + 1] = 255;
                                pxe[index + 2] = 255;
                            }
                        }
                    }

                }
            }
        }

        return img;
    }
}