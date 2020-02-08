using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneHandler;

public class TableController : MonoBehaviour
{
	// UI references
	// - Pause menu references
	public GameObject Menu;
	public GameObject NumberText;

	// - Mode button refs
	public GameObject SelectButton;
	private GameObject SelectButtonImageOpen;
	private GameObject SelectButtonImageClose;
	public GameObject OptionButton;
	public GameObject OptionSlider;
	public GameObject Option;

	// GameObject references
	public GameObject highlighter;

	private  GameObject Selected;

	// Application state
	public SceneHandler Scene;

	// Either 'paused', 'select', 'pick', 'postselect', 'place', or 'postplace'
	private string mode;

	// Any string that references an existing model.
	private string type;

	private string prevMode;

	void Start()
	{
		if (Menu == null || NumberText == null || SelectButton == null || OptionButton == null || OptionSlider == null
			|| highlighter == null)
		{
			throw new System.Exception("Developer error, set your references!!");
		}
		UnityEngine.UI.Text t = NumberText.GetComponent<UnityEngine.UI.Text>();
		if (t != null)
		{
			t.text += Table.tableNumber.ToString();
		}

		SelectButtonImageOpen = SelectButton.transform.GetChild(0).gameObject;
		SelectButtonImageClose = SelectButton.transform.GetChild(1).gameObject;
		OptionSlider.SetActive(false);

		mode = "select";
		type = "stone_brick_wall";
		OptionButton.transform.GetChild(0).gameObject.GetComponent<Image>()
						.sprite = Table.iconDict[type];

		GameObject content = OptionSlider.transform.GetChild(0).GetChild(0).gameObject;
		int offset = -25;
		foreach (string option in Table.prefabDict.Keys)
		{
			// The local option button, for selecting an option
			GameObject optionButton = UnityEngine.Object.Instantiate(Option);
			// Position button in container
			optionButton.transform.SetParent(content.transform, false);
			RectTransform rect = optionButton.GetComponent<RectTransform>();
			rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, offset);
			offset -= 50;
			// Set up the button's icon
			optionButton.transform.GetChild(0).gameObject.GetComponent<Image>()
					.sprite = Table.iconDict[option];
			// Setup button's function
			Button b = optionButton.GetComponent<Button>();
			b.onClick.AddListener(() => {
				type = option;
				mode = "place";
				OptionButton.transform.GetChild(0).gameObject.GetComponent<Image>()
						.sprite = Table.iconDict[option];
			});
		}
		RectTransform contRect = content.GetComponent<RectTransform>();
		contRect.sizeDelta = new Vector2(contRect.sizeDelta.x, -offset + 25);
	}

	// Need to define the case where user goes from placing to select, make it explicit.
	// Basically, as previously noted, I need an indicator of select vs place
	void Update()
	{
		if (Selected == null)
		{
			highlighter.SetActive(false);
		}
		if ((mode.Equals("postselect") || mode.Equals("postplace")) && SelectButtonImageOpen.activeSelf)
		{
			SelectButtonImageOpen.SetActive(false);
			SelectButtonImageClose.SetActive(true);
		}
		if ((mode.Equals("select") || mode.Equals("pick") || mode.Equals("place")) && !SelectButtonImageOpen.activeSelf)
		{
			SelectButtonImageOpen.SetActive(true);
			SelectButtonImageClose.SetActive(false);
		}
		if (!mode.Equals("pick") && OptionSlider.activeSelf)
		{
			OptionSlider.SetActive(false);
		}
		if (mode.Equals("pick") && !OptionSlider.activeSelf)
		{
			OptionSlider.SetActive(true);
		}
	}

	public void ToggleOptions()
	{
		if (mode.Equals("pick"))
		{
			mode = "select";
		} else
		{
			mode = "pick";
		}
	}

    public void ToggleMenu()
	{
		if (Menu != null)
		{
			if (!Menu.activeSelf)
			{
				prevMode = mode;
				mode = "paused";
			} else
			{
				mode = prevMode;
			}
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
			if (mode.Equals("postselect") || mode.Equals("pick"))
			{
				mode = "select";
			}
			if (mode.Equals("postplace"))
			{
				mode = "place";
			}
			if (highlighter != null)
			{
				highlighter.SetActive(false);
			}
			return;
		}
		if (mode.Equals("select") || mode.Equals("postselect"))
		{
			Selected = obj;
			mode = "postselect";
			//TableUtility.ShowAndroidToastMessage("Selected Object: " + obj);
			if (highlighter != null)
			{
				highlighter.SetActive(true);
			}
			updateHighlighter();
		} else if (mode.Equals("place") || mode.Equals("postplace"))
		{
			// Update this!!!
			Element e = Scene.GetElement(obj);
			mode = "postplace";
			if (e == null)
			{
				Debug.Log("Failed to get selected element from scene.");
				TableUtility.ShowAndroidToastMessage("Failed to get selected object from scene: " + obj);
				return;
			}
			Element eNew = Scene.AddElement(e.position + Vector3.forward,
				e.rotation, mode);
			Selected = eNew.model;
			Scene.SetChangedTrue();
			if (highlighter != null)
			{
				highlighter.SetActive(false);
			}
		} else
		{
			// They've done something silly, reset them and try again.
			mode = "select";
			Click(obj);
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

	public void PickOption(string opt)
	{
		OptionSlider.SetActive(false);
		mode = "place";
		type = opt;
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
