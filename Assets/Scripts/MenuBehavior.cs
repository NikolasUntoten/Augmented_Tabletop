using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehavior : MonoBehaviour
{
	public const int TABLE_NUM_LEN = 6;

	public GameObject inputField;

	public GameObject joinButton;

    public void JoinTable()
	{
		string input = inputField
			.GetComponentInChildren<UnityEngine.UI.Text>()
			.text;
		if (input.Length == TABLE_NUM_LEN)
		{
			Table.tableNumber = int.Parse(input);
			SceneManager.LoadScene("Table");
		} else
		{
			// alert user of bad input.
		}
	}

	public void ValidateJoinButton()
	{
		string input = inputField
			.GetComponentInChildren<UnityEngine.UI.Text>()
			.text;
		bool valid = input.Length == TABLE_NUM_LEN;
		joinButton
			.GetComponent<UnityEngine.UI.Button>()
			.interactable = valid;
	}

	public void CreateTable()
	{
		SceneManager.LoadScene("Create Table");
	}
}
