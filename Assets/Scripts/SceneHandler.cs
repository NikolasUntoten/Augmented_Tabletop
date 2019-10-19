using Firebase.Database;
using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{

	public GameObject WallPrefab;

	public GameObject FloorPrefab;


	public class Element
	{
		public Vector3 position;
		public Vector3 rotation;
		public string data;
		public GameObject model;
	}
	private List<Element> elements;

	private bool _changed;

	private static bool _saved;

	private Vector3 _position;

	public void Initialize(DataSnapshot data, Vector3 position)
	{
		_position = position;
		_RemoveAllModels();
		elements = new List<Element>();
		_changed = false;
		//gameObject.transform.position = _position;
		foreach (DataSnapshot child in data.Children)
		{
			Element e = new Element();
			e.position.x = (float) double.Parse(child.Child("position").Child("0").Value.ToString());
			e.position.y = (float)double.Parse(child.Child("position").Child("1").Value.ToString());
			e.position.z = (float)double.Parse(child.Child("position").Child("2").Value.ToString());
			e.rotation.x = (float)double.Parse(child.Child("rotation").Child("0").Value.ToString());
			e.rotation.y = (float)double.Parse(child.Child("rotation").Child("1").Value.ToString());
			e.rotation.z = (float)double.Parse(child.Child("rotation").Child("2").Value.ToString());
			e.data = (string)child.Child("data").Value;
			_AttachModel(e);
			elements.Add(e);
		}
		gameObject.transform.localPosition = new Vector3(0, 0, 0);
	}

	public void Save()
	{
		//_ShowAndroidToastMessage("Saving data...");
		string json = "{";

		for (int i = 0; i < elements.Count; i++)
		{
			Element e = elements[i];
			json += "\"" + i + "\":{\"data\":\"" + e.data + "\","
				+ "\"position\":[" + e.position[0] + "," 
				+ e.position[1] + "," + e.position[2] + "],"
				+ "\"rotation\":[" + e.rotation[0] + ","
				+ e.rotation[1] + "," + e.rotation[2] + "]}";
			if (i + 1 != elements.Count)
			{
				json += ",";
			}
		}
		_saved = true;

		json += "}";
		FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
		DatabaseReference row = database.GetReference(
			Table.tableNumber.ToString()).Child("array");
		row.SetRawJsonValueAsync(json).ContinueWith(task => {
			if (task.IsFaulted)
			{
				//_ShowAndroidToastMessage("Save failed! " + task.Exception);
			} else
			{
				//_ShowAndroidToastMessage("Save Success");
			}
		});
	}

	public Element AddElement(Vector3 pos, Vector3 rot, string data)
	{
		_changed = true;
		Element e = new Element();
		e.position = pos;
		e.rotation = rot;
		e.data = data;
		_AttachModel(e);
		elements.Add(e);
		return e;
	}

	public Element GetElement(Vector3 pos)
	{
		foreach (Element e in elements)
		{
			if (e.position.Equals(pos))
			{
				return e;
			}
		}
		return null;
	}

	public Element GetElement(GameObject obj)
	{
		foreach (Element e in elements)
		{
			if (e.model == obj)
			{
				return e;
			}
		}
		return null;
	}

	public void SetChangedTrue()
	{
		_changed = true;
	}

	public void RemoveElement(Element e)
	{
		_changed = true;
		elements.Remove(e);
	}

    // Start is called before the first frame update
    void Start()
    {
		elements = new List<Element>();
		_changed = false;
		_RefreshModels();
		FirebaseDatabase.DefaultInstance.GetReference(Table.tableNumber.ToString())
			.Child("array")
			.ValueChanged += HandleChange;
		_saved = false;
    }

	void HandleChange(object sender, ValueChangedEventArgs args)
	{
		if (_saved)
		{
			_saved = false;
			return;
		}
		if (args.DatabaseError != null)
		{
			return;
		}
		Initialize(args.Snapshot, _position);
	}

    // Update is called once per frame
    void Update()
    {
        if (_changed)
		{
			_changed = false;
			Save();
			//_RefreshModels();
		}
    }

	private void _RefreshModels()
	{
		_RemoveAllModels();
		_AddAllModels();
	}

	private void _RemoveAllModels()
	{
		foreach (Element e in elements)
		{
			if (e.model != null)
			{
				Destroy(e.model);
				e.model = null;
			}
		}
	}

	public static float SCALE = 0.1f;

	private void _AddAllModels()
	{
		foreach (Element e in elements) {
			_AttachModel(e);
		}
	}

	private void _AttachModel(Element e)
	{
		e.model = Object.Instantiate(_GetModel(e.data));
		e.model.transform.parent = gameObject.transform;
		e.model.transform.localScale = Vector3.one * SCALE;
		e.model.transform.localPosition = e.position * SCALE;
		e.model.transform.localEulerAngles = e.rotation;
	}

	private GameObject _GetModel(string data)
	{
		switch (data)
		{
			case "wall":
				return WallPrefab;
			case "floor":
				return FloorPrefab;
			default:
				return FloorPrefab;
		}
	}

	private void _ShowAndroidToastMessage(string message)
	{
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject unityActivity =
			unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		if (unityActivity != null)
		{
			AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				AndroidJavaObject toastObject =
					toastClass.CallStatic<AndroidJavaObject>(
						"makeText", unityActivity, message, 0);
				toastObject.Call("show");
			}));
		}
	}
}
