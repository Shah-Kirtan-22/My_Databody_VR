using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class Assist : MonoBehaviour
{
	private Text messageText;
	[SerializeField] private string filePath = "Assets/1000mails.csv";

	[SerializeField] private float timePerChar = 0.1f; // speed at which the character is written

	private void Awake()
	{
		// refer to the text object in the game
		messageText = transform.Find("Message").GetComponent<Text>();
	}

	void Start()
	{
		StartCoroutine(PrintLineByLine());
	}

	IEnumerator PrintLineByLine()
	{
		StreamReader strReader = new StreamReader(filePath);
		bool endofFile = false;

		string appended_data_string = "";

		while (!endofFile)
		{
			string data_string = strReader.ReadLine();
			Debug.Log("Value is:" + data_string);
			if (data_string == null)
			{
				Debug.Log("End of File !!!!!");
				endofFile = true;
				break;
			}
			for (int i = 1; i < data_string.Length; i++)
			{
				messageText.text = appended_data_string + Environment.NewLine + data_string.Substring(0, i + 1);
				yield return new WaitForSeconds(timePerChar);
			}

			appended_data_string += Environment.NewLine;
			appended_data_string += data_string;

		}
	}
}
