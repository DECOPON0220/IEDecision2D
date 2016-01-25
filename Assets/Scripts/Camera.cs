using OpenCvSharp;
using OpenCvSharp.CPlusPlus;



class Camera
{

    // 変数宣言

    private VideoCapture video;
    private Mat m_img = new Mat();
    private IplImage i_img = new IplImage();


    //---------------------------------------------------------
    // 関数名 : setDevice
    // 機能   : カメラデバイス選択、設定
    // 引数   : index/カメラデバイス
    // 戻り値 : なし
    //---------------------------------------------------------
    public void setDevice(int index)
    {
        // カメラの設定
        video = new VideoCapture(index);
        video.Set(CaptureProperty.FrameWidth, GlobalVar.CAMERA_WIDTH);
        video.Set(CaptureProperty.FrameHeight, GlobalVar.CAMERA_HEIGHT);
        video.Set(CaptureProperty.Fps, GlobalVar.CAMERA_FPS);
    }

    //---------------------------------------------------------
    // 関数名 : getCameraImage
    // 機能   : カメラ画像の取得
    // 引数   : なし
    // 戻り値 : img/カメラ画像
    //---------------------------------------------------------
    public IplImage getCameraImage()
    {
        video.Read(m_img);
        i_img = convertMatToIplImage(m_img);
        return i_img;
    }

    //---------------------------------------------------------
    // 関数名 : Release                                     
    // 機能   : リソースの破棄
    // 引数   : なし                                 
    // 戻り値 : なし                                           
    //---------------------------------------------------------
    public void Release()
    {
        //video.Release();  何故か終了時エラー
        m_img.Dispose();
        Cv.ReleaseImage(i_img);
    }

    //---------------------------------------------------------
    // 関数名 : convertMatToIplImage
    // 機能   : MatからIplImageへ変換
    // 引数   : なし
    // 戻り値 : img/カメラ画像
    //---------------------------------------------------------
    private IplImage convertMatToIplImage(Mat m_img)
    {
        i_img = m_img.ToIplImage();
        return i_img;
    }
}