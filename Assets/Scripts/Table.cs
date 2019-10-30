using System.Collections.Generic;
using Firebase.Database;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.HelloAR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Table : MonoBehaviour
{
	public static int tableNumber;

	public GameObject Scene;

	public GameObject TableController;

	private string cloudID;

	public void SelectObject()
	{
		GameObject obj = ARCoreController.UnityRaycast();
		if (obj != null)
		{
			TableController.GetComponent<TableController>()
					.Click(obj);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
		DatabaseReference row = database.GetReference(tableNumber.ToString());
		row.GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted)
			{
				Debug.Log("Failed, " + task.Exception.ToString());
			} else if (task.IsCompleted)
			{
				DataSnapshot snapshot = task.Result;
				Debug.Log(snapshot.Value.ToString());
				cloudID = (string)snapshot.Child("cloudID").Value;

				XPSession.ResolveCloudAnchor(cloudID).ThenAction(result => {
					if (result.Response != CloudServiceResponse.Success)
					{
						TableUtility.ShowAndroidToastMessage("Sorry, no table by that ID.");
						return;
					}


					Transform t = result.Anchor.transform;


					Pose worldPose = _WorldToAnchorPose(new Pose(t.position,
													 t.rotation), t);
					t.SetPositionAndRotation(worldPose.position, worldPose.rotation);

					TableUtility.ShowAndroidToastMessage("Welcome to the Table!");

					Scene.GetComponent<SceneHandler>()
					.Initialize(snapshot.Child("array"), result.Anchor.transform.position);
					Scene.transform.parent = t;
					Scene.GetComponent<SceneHandler>().SetChangedTrue();
					Scene.transform.localPosition = new Vector3(0, 0, 0);

				});
			}
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
}
