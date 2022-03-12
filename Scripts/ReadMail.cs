using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ReadMail : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StreamReader strReader = new StreamReader("C:/Users/Kirtan/Desktop/Internship/Body_VR_Version_3.0/Assets/Resources/1000mails.csv"); //path to the file
        bool endofFile = false;
        while (!endofFile)
        {
            string data_string = strReader.ReadLine();
            if (data_string == null)
            {
                Debug.Log("End of File !!!!!");
                endofFile = true;
                break;
            }
            var data_values = data_string.Split(',');

            for (int i = 0; i < data_values.Length; i++)
            {
                //TextWriter.AddWriter(messageText, data_values[i].ToString(), .1f);
                Debug.Log("Value:" + i.ToString() + " " + data_values[i].ToString());
                //yield return new WaitForSeconds(0.1f);
            }

            //messageText.text = "Hello World!";
            // call the textwriter and pass the arguments
            //TextWriter.AddWriter(messageText, "Hey just here looking for the latest episode of seven deadly sins in VR!", .1f);

        }
    }

 
}
