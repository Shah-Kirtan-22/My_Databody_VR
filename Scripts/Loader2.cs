using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;



public struct MHDHeader2
{
    public int[] dims;
    public float[] spacing;
    public string datafile;
    public string datatype;
};


public class Loader2 : MonoBehaviour
{

    public string fname = "T00.raw";
    public string imageDir = "Assets/Resources/CT_demo/";
    public int[] dim = new int[3] { 512, 512, 128 };
    public bool mipmap;

    public string xmlname = "Assets/Resources/color-transfer-functions/demo2.xml";
    //private string xmlname = "D:\\Dropbox\\SVCC\\Development\\unity3d_projects\\MHD\\CT_demo\\settings\\2014-02-12-15-12-26.xml";

    public MHDHeader2 mhdheader;
    private float[,] cTable;

    public int NumberOfFrames = 1;

    List<Texture3D> _volumeBuffer = new List<Texture3D>();
    //private Texture3D[] _volumeBuffer = new Texture3D[NumberOfFrames];
    private int frameNumber = 0;

    public KeyCode increaseAlpha;
    public KeyCode decreaseAlhpa;
    public float alpha = 0.5f;
    
    
    [SerializeField]
    [Range(0.01f, 5f)]
    public float volScale = 1f;

    [SerializeField]
    [Range(0.02f, 5f)]
    public float brightness = 1f;

    [SerializeField]
    [Range(0.02f, 5f)]
    public float red = 1f;

    [SerializeField]
    [Range(0.01f, 2f)]
    public float timeInterval = 0.05f;
    private float timeMeas = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim1Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim1Max = 1f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim2Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim2Max = 1f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim3Min = 0f;

    [SerializeField]
    [Range(0f,1f)]
    float _ClipDim3Max = 1f;

    bool clipForward = false;
    bool isPlay = true;
     
    void Start()
    {

        
        cTable = ParseSettingsXML(xmlname);
        for (int i = 0; i < cTable.GetLength(0); i++)
            //Debug.Log(i.ToString() + ") " + cTable[i, 0].ToString() + "," + cTable[i, 1].ToString() + "," + cTable[i, 2].ToString() + "," + cTable[i, 3].ToString() + "," + cTable[i, 4].ToString());

        Read_MHD_Header(Path.Combine(imageDir, "T00.mhd"));

        for (int k = 0; k < NumberOfFrames; k++)
        {
            string fname_ = "T" + k.ToString("D2");
            Color[] colors = LoadData(Path.Combine(imageDir, fname_ + ".raw"));
            _volumeBuffer.Add(new Texture3D(dim[0], dim[1], dim[2], TextureFormat.ARGB32, mipmap));

            _volumeBuffer[k].SetPixels(colors);
            _volumeBuffer[k].Apply();
        }

    }


    public void Update()
    {
        if (isPlay)
        {
            timeMeas += Time.deltaTime;

            if (timeMeas > timeInterval)
            {
                frameNumber++;
                timeMeas = 0f;
                if (frameNumber >= NumberOfFrames)
                {
                    frameNumber = 0;
                }
            }
            GetComponent<Renderer>().material.SetTexture("_Data", _volumeBuffer[frameNumber % NumberOfFrames]);

        }

        if(Input.GetKeyDown(increaseAlpha))
        {
            alpha += 0.5f;
        }

        if(Input.GetKeyDown(decreaseAlhpa))
        {
            alpha -= 0.5f;
        }

        var getColor = this.GetComponent<MeshRenderer>().material.GetColor("_Color");
        getColor.a = alpha;

        //Sould fix: the dimensions might affect the transform.localscale
        this.transform.localScale = new Vector3(mhdheader.spacing[0] * volScale, mhdheader.spacing[1] * volScale * dim[1]/dim[0], mhdheader.spacing[2] * volScale * dim[2]/dim[0]);
        GetComponent<Renderer>().material.SetFloat("_Brightness", brightness);
        GetComponent<Renderer>().material.SetFloat("col", red);
        GetComponent<Renderer>().material.SetVector("_ClipDimMin", new Vector4(_ClipDim1Min, _ClipDim2Min, _ClipDim3Min, 1f));
        GetComponent<Renderer>().material.SetVector("_ClipDimMax", new Vector4(_ClipDim1Max, _ClipDim2Max, _ClipDim3Max, 1f));



        GameObject clipPlane = GameObject.FindGameObjectWithTag("ClipPlane");
        GameObject cubeParent = GameObject.FindGameObjectWithTag("CubeParent");

        //clipPlane.transform.localScale = new Vector3(0.2f * this.transform.localScale[0], 0.2f * this.transform.localScale[1], 0.2f * this.transform.localScale[2]);
        clipPlane.transform.localScale = new Vector3(1.2f * this.transform.localScale[0], 1.2f * this.transform.localScale[1], 1.2f * this.transform.localScale[2]);

        Plane p = new Plane(this.transform.InverseTransformDirection(clipPlane.transform.forward), this.transform.InverseTransformPoint(clipPlane.transform.position));


        GetComponent<Renderer>().material.SetVector("_ClipPlane", new Vector4(p.normal.x, this.transform.localScale[1] / this.transform.localScale[0] * p.normal.y, this.transform.localScale[2] / this.transform.localScale[0] * p.normal.z, p.distance/Mathf.Sqrt(3f)));    

        //Plane p = new Plane(this.transform.InverseTransformDirection(clipPlane.transform.up), this.transform.InverseTransformPoint(clipPlane.transform.position));
        //GetComponent<Renderer>().material.SetVector("_ClipPlane", new Vector4(-p.normal.x, -p.normal.y, -p.normal.z, -p.distance)); 
    
    
    }

    
    private void OnDestroy()
    {
        foreach (Texture3D _volbufferchild in _volumeBuffer)
        {
            if (_volbufferchild != null)
            {
                Destroy(_volbufferchild);
            }
        }
    }


    private Color[] LoadData(string fname)
    {
        Color[] colors;
        FileStream file = new FileStream(fname, FileMode.Open);

        BinaryReader reader = new BinaryReader(file);
        byte[] buffer_byte = new byte[2 * dim[0] * dim[1] * dim[2]];
        reader.Read(buffer_byte, 0, sizeof(byte) * buffer_byte.Length);
        reader.Close();

        short[] buffer = new short[buffer_byte.Length / 2];
        Buffer.BlockCopy(buffer_byte, 0, buffer, 0, buffer_byte.Length);


        colors = new Color[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            colors[i] = findColor(buffer[i]);
        }



        return colors;
    }

    private Color findColor(int pixel_value)
    {
        if (pixel_value <= (int)cTable[0, 0])
            return new Color(cTable[0, 1], cTable[0, 2], cTable[0, 3], cTable[0, 4]);
        if (pixel_value >= (int)cTable[cTable.GetLength(0) - 1, 0])
            return new Color(cTable[cTable.GetLength(0) - 1, 1], cTable[cTable.GetLength(0) - 1, 2], cTable[cTable.GetLength(0) - 1, 3], cTable[cTable.GetLength(0) - 1, 4]);

        for (int i = 0; i < cTable.GetLength(0) - 1; i++)
        {
            if ((pixel_value >= (int)cTable[i, 0]) & (pixel_value <= (int)cTable[i + 1, 0]))
            {
                float npixel_val = ((float)pixel_value - cTable[i, 0]) / (cTable[i + 1, 0] - cTable[i, 0]);
                return Color.Lerp(new Color(cTable[i, 1], cTable[i, 2], cTable[i, 3], cTable[i, 4]), new Color(cTable[i + 1, 1], cTable[i + 1, 2], cTable[i + 1, 3], cTable[i + 1, 4]), npixel_val);
            }
        }

        return new Color(0, 0, 0, 0);
    }


    private bool Read_MHD_Header(string filename)
    {

        try
        {
            string line;
            StreamReader theReader = new StreamReader(filename, Encoding.Default);
            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();

                    if (line != null)
                    {
                        string[] entries = line.Split(' ');
                        if (entries.Length > 0)
                        {
                            if (entries[0] == "DimSize")
                            {
                                mhdheader.dims = new int[3];
                                for (int i = 2; i < entries.Length; i++)
                                    mhdheader.dims[i - 2] = Int32.Parse(entries[i]);
                            }
                            else if (entries[0] == "ElementSpacing")
                            {
                                mhdheader.spacing = new float[3];
                                for (int i = 2; i < entries.Length; i++)
                                    mhdheader.spacing[i - 2] = float.Parse(entries[i]);
                            }
                            else if (entries[0] == "ElementType")
                            {
                                mhdheader.datatype = entries[2];
                            }
                            else if (entries[0] == "ElementDataFile")
                            {
                                mhdheader.datafile = Path.Combine(Path.GetDirectoryName(filename), entries[2]);
                            }

                        }
                    }
                } while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }


    float[,] ParseSettingsXML(string xmlname)
    {
        try
        {
            StreamReader theReader = new StreamReader(xmlname, Encoding.Default);
            XmlDocument xmlSettings = new XmlDocument();
            xmlSettings.Load(theReader);

            XmlNodeList rgbsettings = xmlSettings.GetElementsByTagName("rgbfuncsettings");
            var npoints_rgb = Int32.Parse(rgbsettings[0].Attributes["NumberOfPoints"].Value);

            float[,] rgb = new float[npoints_rgb, 4];

            for (int i = 0; i < npoints_rgb; i++)
            {
                string[] ptvalues = rgbsettings[0].Attributes["pt" + i].Value.Split(',');
                if (ptvalues.Length > 3)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        rgb[i, j] = float.Parse(ptvalues[j]);
                    }
                }
            }

            XmlNodeList opacitysettings = xmlSettings.GetElementsByTagName("scalarfuncsettings");
            var npoints_alpha = Int32.Parse(opacitysettings[0].Attributes["NumberOfPoints"].Value);

            float[,] alpha = new float[npoints_alpha, 2];

            for (int i = 0; i < npoints_alpha; i++)
            {
                string[] ptvalues = opacitysettings[0].Attributes["pt" + i].Value.Split(',');
                if (ptvalues.Length > 2)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        alpha[i, j] = float.Parse(ptvalues[j]);
                    }
                }
            }

            float[] intensityvals = merge_arrays(rgb, alpha);
            float[,] pixelTable = new float[intensityvals.Length, 5];


            for (int i = 0; i < intensityvals.Length; i++)
            {
                pixelTable[i, 0] = intensityvals[i];

                for (int j = 0; j < 4; j++) pixelTable[i, j + 1] = 1.0f; //Initialize RGBA values

                //RGB values
                if (pixelTable[i, 0] <= rgb[0, 0])
                {
                    pixelTable[i, 1] = rgb[0, 1];
                    pixelTable[i, 2] = rgb[0, 2];
                    pixelTable[i, 3] = rgb[0, 3];
                }
                else if (pixelTable[i, 0] >= rgb[rgb.GetLength(0) - 1, 0])
                {
                    pixelTable[i, 1] = rgb[rgb.GetLength(0) - 1, 1];
                    pixelTable[i, 2] = rgb[rgb.GetLength(0) - 1, 2];
                    pixelTable[i, 3] = rgb[rgb.GetLength(0) - 1, 3];
                }
                else
                {
                    for (int kc = 0; kc < alpha.GetLength(0) - 1; kc++)
                    {
                        if ((pixelTable[i, 0] >= rgb[kc, 0]) & (pixelTable[i, 0] <= rgb[kc + 1, 0]))
                        {
                            float npixel_val = ((float)pixelTable[i, 0] - rgb[kc, 0]) / (rgb[kc + 1, 0] - rgb[kc, 0]);
                            Color rgbtemp = Color.Lerp(new Color(rgb[kc, 1], rgb[kc, 2], rgb[kc, 3], 1f), new Color(rgb[kc + 1, 1], rgb[kc + 1, 2], rgb[kc + 1, 3], 1f), npixel_val);
                            pixelTable[i, 1] = rgbtemp[0];
                            pixelTable[i, 2] = rgbtemp[1];
                            pixelTable[i, 3] = rgbtemp[2];
                            break;
                        }
                    }
                }


                if (pixelTable[i, 0] <= alpha[0, 0])
                {
                    pixelTable[i, 4] = alpha[0, 1];
                }
                else if (pixelTable[i, 0] >= alpha[alpha.GetLength(0) - 1, 0])
                {
                    pixelTable[i, 4] = alpha[alpha.GetLength(0) - 1, 1];
                }
                else
                {
                    for (int ka = 0; ka < alpha.GetLength(0) - 1; ka++)
                    {
                        if ((pixelTable[i, 0] >= alpha[ka, 0]) & (pixelTable[i, 0] <= alpha[ka + 1, 0]))
                        {
                            float npixel_val = ((float)pixelTable[i, 0] - alpha[ka, 0]) / (alpha[ka + 1, 0] - alpha[ka, 0]);
                            pixelTable[i, 4] = Mathf.Lerp(alpha[ka, 1], alpha[ka + 1, 1], npixel_val);
                            break;
                        }
                    }
                }
                //Debug.Log(i.ToString() + ") " + alpha[k, 0].ToString()+","+alpha[k+1, 0].ToString()+","+pixelTable[i,0].ToString());
            }



            return pixelTable;

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }

    }

    // Concatenate unique intensity values from color and opacity tables
    float[] merge_arrays(float[,] rgb, float[,] alpha)
    {
        float[] res = new float[rgb.GetLength(0) + alpha.GetLength(0)];
        int i = 0, j = 0, k = 0;

        while (i < rgb.GetLength(0) && j < alpha.GetLength(0))
        {
            if (rgb[i, 0] < alpha[j, 0])
            {
                res[k] = rgb[i, 0];
                i++;
            }
            else if ((rgb[i, 0] > alpha[j, 0]))
            {
                res[k] = alpha[j, 0];
                j++;
            }
            else
            {
                res[k] = rgb[i, 0];
                i++;
                j++;
            }
            k++;
        }

        while (i < rgb.GetLength(0))
        {
            res[k] = rgb[i, 0];
            i++;
            k++;
        }

        while (j < alpha.GetLength(0))
        {
            res[k] = alpha[j, 0];
            j++;
            k++;
        }

        float[] merged_array = new float[k];

        for (int ii = 0; ii < k; ii++)
            merged_array[ii] = res[ii];

        return merged_array;
    }


    public void changeScale(float scale)
    {
        volScale = scale;
    }

    public void increaseScale()
    {
        volScale = Mathf.Clamp(1.01f * volScale, 0.1f, 5.0f);
    }

    public void decreaseScale()
    {
        volScale = Mathf.Clamp(volScale/1.01f, 0.1f, 5.0f);
    }

    public void increaseBrightness()
    {
        brightness = Mathf.Clamp(1.01f * brightness, 0.002f, 5.0f);
    }

    public void decreaseBrightness()
    {
        brightness = Mathf.Clamp(brightness / 1.01f, 0.002f, 5.0f);
    }

    public void increaseRed()
    {
        red = Mathf.Clamp(1.01f * red, 0.002f, 5.0f);
    }

    public void decreaseRed()
    {
        red = Mathf.Clamp(red / 1.01f, 0.002f, 5.0f);
    }

    public void increaseClipDim1()
    {
        if (clipForward){
            _ClipDim1Min = Mathf.Clamp(_ClipDim1Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim1Max = Mathf.Clamp(_ClipDim1Max - 0.01f, 0f, 1f);
        }
    }


    public void decreaseClipDim1()
    {
        if (clipForward)
        {
            _ClipDim1Min = Mathf.Clamp(_ClipDim1Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim1Max = Mathf.Clamp(_ClipDim1Max + 0.01f, 0f, 1f);
        }
    }


    public void increaseClipDim2()
    {
        if (clipForward)
        {
            _ClipDim2Min = Mathf.Clamp(_ClipDim2Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim2Max = Mathf.Clamp(_ClipDim2Max - 0.01f, 0f, 1f);
        }
    }


    public void decreaseClipDim2()
    {
        if (clipForward)
        {
            _ClipDim2Min = Mathf.Clamp(_ClipDim2Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim2Max = Mathf.Clamp(_ClipDim2Max + 0.01f, 0f, 1f);
        }
    }

    public void increaseClipDim3()
    {
        if (clipForward)
        {
            _ClipDim3Min = Mathf.Clamp(_ClipDim3Min + 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim3Max = Mathf.Clamp(_ClipDim3Max - 0.01f, 0f, 1f);
        }
    }


    public void decreaseClipDim3()
    {
        if (clipForward)
        {
            _ClipDim3Min = Mathf.Clamp(_ClipDim3Min - 0.01f, 0f, 1f);
        }
        else
        {
            _ClipDim3Max = Mathf.Clamp(_ClipDim3Max + 0.01f, 0f, 1f);
        }
    }

    public void changeClipDir()
    {
        if (clipForward)
        {
            clipForward = false;
        }else{
            clipForward = true;
        }

    }

    public void playOnOff()
    {
        if (isPlay)
        {
            isPlay = false;
        }
        else
        {
            isPlay = true;
        }
		Debug.Log("Reset");
	}

    public void resetSceneCT()
	{
		Debug.Log("Reset");
	}

	
}
