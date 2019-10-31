using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using GoogleARCore;
using GoogleARCore.CrossPlatform;

public class FirebaseHandler : MonoBehaviour
{

	private bool _FirebaseReady;

	public class TableEntry
	{
		public int num;
		public string cloudID;
		public TableElement[] array;
	}

	public class TableElement
	{
		public string type;
		public Vector3 position;
		public Vector3 rotation;
	}

    // Start is called before the first frame update
    void Start()
    {
		_FirebaseReady = false;
		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			var dependencyStatus = task.Result;
			if (dependencyStatus == Firebase.DependencyStatus.Available)
			{
				// Create and hold a reference to your FirebaseApp,
				// where app is a Firebase.FirebaseApp property of your application class.
				//   app = Firebase.FirebaseApp.DefaultInstance;

				// Set a flag here to indicate whether Firebase is ready to use by your app.
				_FirebaseReady = true;
			} else
			{
				UnityEngine.Debug.LogError(System.String.Format(
				  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
				// Firebase Unity SDK is not safe to use here.
			}
		});
	}

	// Callback for 'GetTableData'
	public delegate void GetCallback(TableEntry entry);

	// Gets the data of table related to tableNum, and calls callback with it.
	public static void GetTableData(int tableNum, GetCallback callback)
	{
		FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
		DatabaseReference row = database.GetReference(tableNum.ToString());
		row.GetValueAsync().ContinueWith(task => {
			if (task.IsFaulted)
			{
				Debug.Log("Failed, " + task.Exception.ToString());
			} else if (task.IsCompleted)
			{
				DataSnapshot snapshot = task.Result;
				TableEntry table = MarshallTableData(snapshot, tableNum);
				callback(table);
			}
		});
	}

	public static TableEntry MarshallTableData(DataSnapshot snapshot, int tableNum)
	{
		TableEntry table = new TableEntry();
		table.num = tableNum;
		table.cloudID = (string)snapshot.Child("cloudID").Value;
		DataSnapshot arr = snapshot.Child("array");
		long arrLen = arr.ChildrenCount;
		table.array = new TableElement[arrLen];
		for (int i = 0; i < arrLen; i++)
		{
			DataSnapshot el = arr.Child(string.Format("{0}", i));
			TableElement e = new TableElement();
			e.type = (string)el.Child("data").Value;
			e.position = new Vector3();
			e.rotation = new Vector3();
			e.position.x = (float)double.Parse(el.Child("position").Child("0").Value.ToString());
			e.position.y = (float)double.Parse(el.Child("position").Child("1").Value.ToString());
			e.position.z = (float)double.Parse(el.Child("position").Child("2").Value.ToString());
			e.rotation.x = (float)double.Parse(el.Child("rotation").Child("0").Value.ToString());
			e.rotation.y = (float)double.Parse(el.Child("rotation").Child("1").Value.ToString());
			e.rotation.z = (float)double.Parse(el.Child("rotation").Child("2").Value.ToString());
			table.array[i] = e;
		}
		return table;
	}

	public static string MarshallTableObject(TableEntry entry)
	{
		return "";
	}

	// Callback for 'SetTableData'
	public delegate void SetCallback();

	// Sets a table in the database to data.
	// data should be a json string of the form:
	/*
	 *	'
	 *	array:[
	 *		{
	 *			data:"<blocktype>"
	 *			position:[<x>,<y>,<z>]
	 *			rotation:[<x>,<y>,<z>]
	 *		},
	 *		...
	 *	]
	 *	cloudID:"<cloudID>"
	 *	'
	 * 
	 */
	public static void SetTableData(string data, int tableNum, SetCallback callback)
	{
		FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
		DatabaseReference row = database.GetReference(tableNum.ToString());
		row.SetRawJsonValueAsync(data).ContinueWith(task => {
			if (task.IsFaulted)
			{
				TableUtility.ShowAndroidToastMessage("Failed, "
					+ task.Exception.ToString());
			} else if (task.IsCompleted)
			{
				// Table was uploaded. Move to the join scene.
				callback();
			} else
			{
				TableUtility.ShowAndroidToastMessage("?, " + task.Status);
			}
		});
	}

	// Callback for 'CreateCloudAnchor' and 'GetCloudAnchor'
	public delegate void CloudAnchorCallback(CloudAnchorResult result);

	// Creates a cloud anchor based on anchor, and calls callback with result.
	public static void CreateCloudAnchor(Anchor anchor, CloudAnchorCallback callback)
	{
		XPSession.CreateCloudAnchor(anchor).ThenAction(result => {
			if (result.Response != CloudServiceResponse.Success)
			{
				Debug.Log(string.Format("Failed to host Cloud Anchor: {0}",
					result.Response));
				TableUtility.ShowAndroidToastMessage(
					string.Format("Failed to host Cloud Anchor: {0}. Please try again.",
						result.Response));
				return;
			}
			callback(result);
		});
	}

	// Gets a cloud anchor of ID=cloudID, and calls callback with the returned anchor.
	public static void GetCloudAnchor(string cloudID, CloudAnchorCallback callback)
	{
		XPSession.ResolveCloudAnchor(cloudID).ThenAction(result => {
			if (result.Response != CloudServiceResponse.Success)
			{
				TableUtility.ShowAndroidToastMessage("Sorry, no table by that ID.");
				return;
			}
			callback(result);
		});
	}
}
