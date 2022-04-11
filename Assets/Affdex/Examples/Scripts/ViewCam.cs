using UnityEngine;
using Affdex;
using System.IO;
using System.Collections;

public class ViewCam : MonoBehaviour {

    public Affdex.CameraInput cameraInput;
    public Affdex.VideoFileInput movieInput;

    //debugFeature, to get access to face points
    public Affdex.DebugFeatureViewer dfv;

    //image name to save
    private string imageName;

    //size
    private float width;
    private float height;

    //main controller
    private MainController mainController;

    //eyesCTRL
    private EyeCTRL eyeCtrl;

    private void Awake()
    {
        mainController = GameObject.Find("MainController").GetComponent<MainController>();
        eyeCtrl = GameObject.Find("EyeCTRL").GetComponent<EyeCTRL>();
    }

    // Use this for initialization
    void Start () {
		if (! AffdexUnityUtils.ValidPlatform ())
			return;

        Texture texture = movieInput != null ? movieInput.Texture : cameraInput.Texture;

        if (texture == null)
            return;

        this.GetComponent<MeshRenderer>().material.mainTexture = texture;

        // rotate the image to be upright on the display
#if UNITY_STANDALONE_WIN
        if (cameraInput != null) {
			float videoRotationAngle = -cameraInput.videoRotationAngle;
			transform.rotation = transform.rotation * Quaternion.AngleAxis (videoRotationAngle, Vector3.forward);
		}
#endif

        width = texture.width;
        height = texture.height;

        float wscale = (float)texture.width / (float)texture.height;
       
        transform.localScale = new Vector3((transform.localScale.y * wscale)*-1, transform.localScale.y, 1);

        dfv = gameObject.GetComponent<DebugFeatureViewer>();
	}

    private void Update()
    {
        if (dfv.face != null && !mainController.isSleeping)
        {
            //Debug.Log(width + " - " + height);
            //Debug.Log(dfv.face.FeaturePoints[12].x + " - " + dfv.face.FeaturePoints[12].y);

            //convert the image point to valid point to look at
            //max: 15 degrees, 7.5 each side (x axis)
            float normX = dfv.face.FeaturePoints[12].x / width;

            //Debug.Log(dfv.face.FeaturePoints.Length);

            if(normX < 0.5f)
            {
                normX *= -15f;
            }
            else
            {
                normX *= 15f;
            }

            /*mainController.FollowFace(new Vector3(dfv.face.FeaturePoints[12].x - (width/2),
                (height / 2) - dfv.face.FeaturePoints[12].y, 0));*/
            /*mainController.FollowFace(new Vector3(dfv.face.FeaturePoints[12].x - (width / 2),
                3, 0));*/
            //eyeCtrl.followPoint = new Vector3(dfv.face.FeaturePoints[12].x - (width / 2), 3, 0);

            eyeCtrl.followPoint = new Vector3(dfv.face.FeaturePoints[12].x - (width / 2), (height / 2) - dfv.face.FeaturePoints[12].y, 0);

            //face shape
            //FaceShape(dfv.face.FeaturePoints);

            StartSaveImageCoRo(mainController.absPath+"camImage.png");
        }
    }

    public void StartSaveImageCoRo(string imageName)
    {
        this.imageName = imageName;

        StartCoroutine(SaveImage());
    }

    //define the face shape
    //as defined in Waheed et al. 2017
    /*
     * Oval FH > EE, FL > CB --> Balanced and diplomats
     * Round FH ≈EE < CB ≈FL --> Sensitive and caring
     * Square FH ≈EE≈CB --> Bold, decisive mind, intelligent and analytical
     * Diamond FL > CB > FH > EE --> Dominant with less force
     * Oblong FL > AND CB ≈FH ≈EE --> Practical and methodical
     * Triangular EE > CB > FH --> Creative and fiery temperament
     * 
     * Where FH= Forehead (5 and 10), EE= Ear Distance, FL= Face length, CB= Cheek bone
     * 
     * Face features calculated with euclidean distance --> sqrt( (𝑎1−𝑎2)^2+(𝑏1−𝑏2)^2 )
     * */
    private void FaceShape(FeaturePoint[] dfv)
    {
        //Debug.Log(dfv[16].x + " - " + dfv[17].x + " - " + dfv[30].x + " - " + dfv[31].x);
        //Debug.Break();
        float FH = Vector2.Distance(new Vector2(dfv[10].x, dfv[10].y), new Vector2(dfv[5].x, dfv[5].y));
        float EE = Vector2.Distance(new Vector2(dfv[0].x, dfv[0].y), new Vector2(dfv[4].x, dfv[4].y));
        //affectiva does not have the topmost facial landmark (hair line), using nose root instead
        float FL = Vector2.Distance(new Vector2(dfv[2].x, dfv[2].y), new Vector2(dfv[11].x, dfv[11].y));
        float CB = Vector2.Distance(new Vector2(dfv[16].x, dfv[16].y), new Vector2(dfv[19].x, dfv[19].y));
        //Debug.Log("Forehead = " + FH);
        //Debug.Log("Ear to Ear = " + EE);
        //Debug.Log("Face Lenght = " + FL);
        //Debug.Log("Cheek Bone = " + CB);
        //oval
        if (FH > EE && FL > CB)
        {
            Debug.Log("Oval!");
        }
        //Round
        else if (EE < CB && Similar(FH, EE, 0.1f) && Similar(CB, FL, 0.1f))
        {
            Debug.Log("Round!");
        }
        //Square
        else if (Similar(FH, EE, 0.1f) && Similar(FH, CB, 0.1f))
        {
            Debug.Log("Square!");
        }
        //Diamond
        else if (FL > CB && CB > FH && FH > EE)
        {
            Debug.Log("Diamond!");
        }
        //Oblong
        else if (Similar(FH, CB, 0.1f) && Similar(FH, EE, 0.1f) && FL > CB)
        {
            Debug.Log("Oblong!");
        }
        //Triangular
        else if (EE > CB && CB > FH)
        {
            Debug.Log("Triangular!");
        }
        else
        {
            Debug.Log("Not Found!");
        }
    }

    //check if two numbers are similar each other, giving a certain percentage value
    private bool Similar(float val1, float val2, float percentage)
    {
        bool sim = false;

        float div = val1 / val2;

        if(div > 1 && div - 1 <= percentage)
        {
            sim = true;
        } else if (div < 1 && 1 - div <= percentage)
        {
            sim = true;
        }

        return sim;
    }

    private IEnumerator SaveImage()
    {
        yield return new WaitForEndOfFrame();

        //Debug.Log("Material - " + this.GetComponent<MeshRenderer>().materials[0].mainTexture);
        //Debug.Break();

        //save image
        Texture txtr = this.GetComponent<MeshRenderer>().materials[0].mainTexture;
        Texture2D image = new Texture2D(txtr.width, txtr.height, TextureFormat.RGB24, false);

        RenderTexture rt = new RenderTexture(txtr.width, txtr.height, 0);
        RenderTexture.active = rt;
        // Copy your texture ref to the render texture
        Graphics.Blit(txtr, rt);

        Destroy(rt);

        image.ReadPixels(new Rect(0, 0, txtr.width, txtr.height), 0, 0);
        image.Apply();

        byte[] _bytes = image.EncodeToPNG();
        //Debug.Log(_bytes);
        FileStream newImage = File.Create(imageName);
        newImage.Close();
        File.WriteAllBytes(imageName, _bytes);

        Destroy(image);

        //ScreenCapture.CaptureScreenshot("camImage.png");
    }
}