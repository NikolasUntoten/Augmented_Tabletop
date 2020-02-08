using System.Collections.Generic;
using Firebase.Database;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.HelloAR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class Table : MonoBehaviour
{
	public static int tableNumber;

	public GameObject Scene;

	public GameObject TableController;

	[Serializable]
	public struct NamedPrefab
	{
		public string name;
		public GameObject obj;
		public Sprite icon;
	}

	public NamedPrefab[] Prefabs;

	public static Dictionary<string, GameObject> prefabDict;

	public static Dictionary<string, Sprite> iconDict;

	public static GameObject ErrorPrefab;

	private string cloudID;

	public void SelectObject()
	{
		GameObject obj = ARCoreController.UnityRaycast(_ShowAndroidToastMessage);
		TableController.GetComponent<TableController>()
					.Click(obj);
	}

	public void Awake()
	{
		prefabDict = new Dictionary<string, GameObject>();
		iconDict = new Dictionary<string, Sprite>();
		foreach (NamedPrefab p in Prefabs)
		{
			prefabDict.Add(p.name, p.obj);
			iconDict.Add(p.name, p.icon);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		FirebaseHandler.GetTableData(tableNumber, (entry) => {
			FirebaseHandler.GetCloudAnchor(entry.cloudID, (result => {
				if (result.Anchor == null)
				{
					_ShowAndroidToastMessage("Cloud Anchor not Viable.");
					return;
				}
				Transform t = result.Anchor.transform;

				Pose worldPose = _WorldToAnchorPose(new Pose(t.position,
												 t.rotation), t);
				if (worldPose == null)
				{
					_ShowAndroidToastMessage("Error converting cloud anchor to world pose.");
					return;
				}
				t.SetPositionAndRotation(worldPose.position, worldPose.rotation);

				TableUtility.ShowAndroidToastMessage("Welcome to the Table!");

				SceneHandler sceneHandler = Scene.GetComponent<SceneHandler>();
				if (sceneHandler == null)
				{
					_ShowAndroidToastMessage("Scene Handler failed to initialize");
					return;
				}
				Scene.SetActive(true);
				sceneHandler.Initialize(entry, result.Anchor.transform.position);
				Scene.transform.parent = t;
				sceneHandler.SetChangedTrue();
				Scene.transform.localPosition = new Vector3(0, 0, 0);
			}));
		});
	}

	private Pose _WorldToAnchorPose(Pose pose, Transform m_AnchorTransform)
	{

		Matrix4x4 anchorTWorld = Matrix4x4.TRS(
			m_AnchorTransform.position, m_AnchorTransform.rotation, Vector3.one).inverse;

		Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
		Quaternion rotation = pose.rotation * Quaternion.LookRotation(
			anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

		return new Pose(position, rotation);
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
