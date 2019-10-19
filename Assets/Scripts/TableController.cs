using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneHandler;

public class TableController : MonoBehaviour
{
	public GameObject Menu;

	public GameObject NumberText;

	public SceneHandler Scene;

	private  GameObject Selected;

	private string mode;

	void Start()
	{
		NumberText.GetComponent<UnityEngine.UI.Text>()
			.text += Table.tableNumber.ToString();
		mode = "select";
	}

    public void ToggleMenu()
	{
		Menu.SetActive(!Menu.activeSelf);
	}

	public void SaveScene()
	{

	}

	public void LeaveTable()
	{
		SceneManager.LoadScene("Join");
	}

	public void SetMode(string mode)
	{
		this.mode = mode;
	}

	public void Click(GameObject obj)
	{
		if (mode.Equals("select"))
		{
			Selected = obj;
		} else
		{
			Element e = Scene.GetElement(obj);
			Scene.AddElement(e.position + Vector3.forward * SceneHandler.SCALE,
				e.rotation, mode);
			Scene.SetChangedTrue();
		}
	}

	public void Left()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.left * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.left;
			Scene.SetChangedTrue();
		}
	}

	public void Right()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.right * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.right;
			Scene.SetChangedTrue();
		}
	}

	public void Forward()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.forward * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.forward;
			Scene.SetChangedTrue();
		}
	}

	public void Back()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.back * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.back;
			Scene.SetChangedTrue();
		}
	}

	public void Up()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.up * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.up;
			Scene.SetChangedTrue();
		}
	}

	public void Down()
	{
		if (Selected != null)
		{
			Selected.transform.localPosition = Selected.transform.localPosition
				+ Vector3.down * SceneHandler.SCALE;
			Element e = Scene.GetElement(Selected);
			e.position = e.position + Vector3.down;
			Scene.SetChangedTrue();
		}
	}

	public void Rotate()
	{
		if (Selected != null)
		{
			Selected.transform.localEulerAngles = Selected.transform.localEulerAngles
				+ Vector3.up * 90;
			Element e = Scene.GetElement(Selected);
			e.rotation = e.rotation + Vector3.up * 90;
			Scene.SetChangedTrue();
		}
	}
}
