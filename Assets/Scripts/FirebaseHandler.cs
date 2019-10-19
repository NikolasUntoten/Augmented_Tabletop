using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class FirebaseHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			var dependencyStatus = task.Result;
			if (dependencyStatus == Firebase.DependencyStatus.Available)
			{
				// Create and hold a reference to your FirebaseApp,
				// where app is a Firebase.FirebaseApp property of your application class.
				//   app = Firebase.FirebaseApp.DefaultInstance;

				// Set a flag here to indicate whether Firebase is ready to use by your app.
				GetTableData(123456);
			} else
			{
				UnityEngine.Debug.LogError(System.String.Format(
				  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
				// Firebase Unity SDK is not safe to use here.
			}
		});
	}

	public static string GetTableData(int tableNum)
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
				Debug.Log(snapshot.Value.ToString());
			}
		});
		return "";
	}

	public static void SetTableData(string data, int tableNum)
	{

	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
