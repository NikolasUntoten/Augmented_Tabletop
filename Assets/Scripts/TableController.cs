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

	public GameObject highlighter;

	private  GameObject Selected;

	private string mode;

	void Start()
	{
		if (NumberText != null)
		{
			UnityEngine.UI.Text t = NumberText.GetComponent<UnityEngine.UI.Text>();
			if (t != null)
			{
				t.text += Table.tableNumber.ToString();
			}
		}
		mode = "select";
	}

	void Update()
	{
		if (Selected == null)
		{
			highlighter.SetActive(false);
		}
	}

    public void ToggleMenu()
	{
		if (Menu != null)
		{
			Menu.SetActive(!Menu.activeSelf);
		}
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
		if (obj == null)
		{
			//TableUtility.ShowAndroidToastMessage("Clicked on Null! try again.");
			Selected = null;
			if (highlighter != null)
			{
				highlighter.SetActive(false);
			}
			return;
		}
		if (mode.Equals("select"))
		{
			Selected = obj;
			//TableUtility.ShowAndroidToastMessage("Selected Object: " + obj);
			if (highlighter != null)
			{
				highlighter.SetActive(true);
			}
			updateHighlighter();
		} else
		{
			// Update this!!!
			Element e = Scene.GetElement(obj);
			if (e == null)
			{
				Debug.Log("Failed to get selected element from scene.");
				TableUtility.ShowAndroidToastMessage("Failed to get selected object from scene: " + obj);
				return;
			}
			Scene.AddElement(e.position + Vector3.forward,
				e.rotation, mode);
			Scene.SetChangedTrue();
			if (highlighter != null)
			{
				highlighter.SetActive(false);
			}
		}
	}

	private void updateHighlighter()
	{
		if (highlighter != null && Selected != null)
		{
			highlighter.transform.position = Selected.transform.position;
			highlighter.transform.rotation = Selected.transform.rotation;
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
			updateHighlighter();
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
			updateHighlighter();
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
			updateHighlighter();
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
			updateHighlighter();
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
			updateHighlighter();
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
			updateHighlighter();
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
