using System.Collections.Generic;
using Firebase.Database;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.HelloAR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreateTable : MonoBehaviour
{

	public GameObject tablePrefab;

	// Used for the callback function in ARCoreController
	// Creates and uploads a table on touch, then loads the join scene.
	public void MakeTable()
	{
		try
		{
			TrackableHit hit = ARCoreController.ARCoreRayCast();

			// Instantiate prefab at the hit pose.
			var gameObject = Instantiate(tablePrefab, hit.Pose.position, hit.Pose.rotation);

			// Compensate for the hitPose rotation facing away from the raycast (i.e.
			// camera).
			gameObject.transform.Rotate(0, 180.0f, 0, Space.Self);

			// Create an anchor to allow ARCore to track the hitpoint as understanding of
			// the physical world evolves.
			var anchor = hit.Trackable.CreateAnchor(hit.Pose);

			// Make game object a child of the anchor.
			gameObject.transform.parent = anchor.transform;

			_DoUploadAndMoveOn(anchor);
		} catch (System.Exception e)
		{
			TableUtility.ShowAndroidToastMessage(e.Message);
		}
	}

	// Creates the anchor, then kicks to _AfterAnchorMade
	private void _DoUploadAndMoveOn(Anchor anchor)
	{
		TableUtility.ShowAndroidToastMessage(
			"Creating cloud anchor: " + anchor.ToString());
		FirebaseHandler.CreateCloudAnchor(anchor, _AfterAnchorMade);
	}

	// Uploads Anchor Data, then loads join scene.
	private void _AfterAnchorMade(CloudAnchorResult result)
	{
		TableUtility.ShowAndroidToastMessage(
				string.Format("Cloud Anchor {0} was created and saved.",
				result.Anchor.CloudId));
		Debug.Log(string.Format("Cloud Anchor {0} was created and saved.",
			result.Anchor.CloudId));

		string cloudID = result.Anchor.CloudId;
		int tablenum = Random.Range(100000, 999999);
		string json = "{\"cloudID\":\"" + cloudID + "\""
			+ ",\"array\":["
			+ "{\"data\":\"floor\",\"position\":[0,0,0],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[1,0,0],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[2,0,0],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[3,0,0],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[0,0,1],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[1,0,1],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[2,0,1],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[3,0,1],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[0,0,2],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[1,0,2],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[2,0,2],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"floor\",\"position\":[3,0,2],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"wall\",\"position\":[0,1,-1],\"rotation\":[0,-90,0]},"
			+ "{\"data\":\"wall\",\"position\":[1,1,-1],\"rotation\":[0,-90,0]},"
			+ "{\"data\":\"wall\",\"position\":[2,1,-1],\"rotation\":[0,-90,0]},"
			+ "{\"data\":\"wall\",\"position\":[3,1,-1],\"rotation\":[0,-90,0]},"
			+ "{\"data\":\"wall\",\"position\":[0,1,3],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"wall\",\"position\":[1,1,3],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"wall\",\"position\":[2,1,3],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"wall\",\"position\":[3,1,3],\"rotation\":[0,90,0]},"
			+ "{\"data\":\"wall\",\"position\":[-1,1,0],\"rotation\":[0,0,0]},"
			+ "{\"data\":\"wall\",\"position\":[-1,1,1],\"rotation\":[0,0,0]},"
			+ "{\"data\":\"wall\",\"position\":[-1,1,2],\"rotation\":[0,0,0]},"
			+ "{\"data\":\"wall\",\"position\":[4,1,0],\"rotation\":[0,180,0]},"
			+ "{\"data\":\"wall\",\"position\":[4,1,1],\"rotation\":[0,180,0]},"
			+ "{\"data\":\"wall\",\"position\":[4,1,2],\"rotation\":[0,180,0]}"
			+ "]}";
		string jsonFloor = "{\"cloudID\":\"" + cloudID + "\", \"array\":[";
		int width = 5;
		for (int x = -width; x < width; x++)
		{
			for (int z = -width; z < width; z++)
			{
				jsonFloor += "{\"data\":\"stone_brick_floor\",\"position\":[" + x + ",-1," + z + "],\"rotation\":[0,90,0]}";
				if (z != width - 1 || x != width - 1)
				{
					jsonFloor += ",";
				}
			}
		}
		jsonFloor += "]}";

		TableUtility.ShowAndroidToastMessage("saving cloud data");
		FirebaseHandler.SetTableData(jsonFloor, tablenum, () => {
			Table.tableNumber = tablenum;
			SceneManager.LoadScene("Table");
		});
	}
}
