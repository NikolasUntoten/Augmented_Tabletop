
using System.Collections.Generic;
	using Firebase.Database;
	using GoogleARCore;
	using GoogleARCore.CrossPlatform;
	using GoogleARCore.Examples.Common;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.SceneManagement;

#if UNITY_EDITOR
	// Set up touch input propagation while using Instant Preview in the editor.
	using Input = GoogleARCore.InstantPreviewInput;
#endif

public class Table : MonoBehaviour
{
	public static int tableNumber;

	public GameObject Scene;

	public GameObject TableController;

		private string cloudID;

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
					cloudID = (string) snapshot.Child("cloudID").Value;
					
					XPSession.ResolveCloudAnchor(cloudID).ThenAction(result => {
						if (result.Response != CloudServiceResponse.Success)
						{
							_ShowAndroidToastMessage("Sorry, no table by that ID.");
							return;
						}
						

						Transform t = result.Anchor.transform;


						Pose worldPose = _WorldToAnchorPose(new Pose(t.position,
														 t.rotation), t);
						t.SetPositionAndRotation(worldPose.position, worldPose.rotation);

						_ShowAndroidToastMessage("Welcome to the Table!");

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

	/// <summary>
	/// The first-person camera being used to render the passthrough camera image (i.e. AR
	/// background).
	/// </summary>
	public Camera FirstPersonCamera;

	/// <summary>
	/// A prefab to place when a raycast from a user touch hits a vertical plane.
	/// </summary>
	public GameObject GameObjectVerticalPlanePrefab;

	/// <summary>
	/// A prefab to place when a raycast from a user touch hits a horizontal plane.
	/// </summary>
	public GameObject GameObjectHorizontalPlanePrefab;

	/// <summary>
	/// A prefab to place when a raycast from a user touch hits a feature point.
	/// </summary>
	public GameObject GameObjectPointPrefab;

	/// <summary>
	/// The rotation in degrees need to apply to prefab when it is placed.
	/// </summary>
	private const float k_PrefabRotation = 180.0f;

	/// <summary>
	/// True if the app is in the process of quitting due to an ARCore connection error,
	/// otherwise false.
	/// </summary>
	private bool m_IsQuitting = false;

	/// <summary>
	/// The Unity Awake() method.
	/// </summary>
	public void Awake()
	{
		// Enable ARCore to target 60fps camera capture frame rate on supported devices.
		// Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
		Application.targetFrameRate = 60;
	}

	/// <summary>
	/// The Unity Update() method.
	/// </summary>
	public void Update()
	{
		_UpdateApplicationLifecycle();

		// If the player has not touched the screen, we are done with this update.
		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		// Should not handle input if the player is pointing on UI.
		if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
		{
			return;
		}

		// Raycast against the location the player touched to search for planes.
		RaycastHit hit;
		if (Physics.Raycast(Frame.Pose.position, Frame.Pose.forward, out hit, 100.0f))
		{
			//_ShowAndroidToastMessage("HIT! " + hit.collider.gameObject.ToString());
			TableController.GetComponent<TableController>()
				.Click(hit.collider.gameObject.transform.parent.gameObject);
		} else
		{
			//_ShowAndroidToastMessage("MISS");
		}
	}

	/// <summary>
	/// Check and update the application lifecycle.
	/// </summary>
	private void _UpdateApplicationLifecycle()
	{
		// Exit the app when the 'back' button is pressed.
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}

		// Only allow the screen to sleep when not tracking.
		if (Session.Status != SessionStatus.Tracking)
		{
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		} else
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		if (m_IsQuitting)
		{
			return;
		}

		// Quit if ARCore was unable to connect and give Unity some time for the toast to
		// appear.
		if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
		{
			_ShowAndroidToastMessage("Camera permission is needed to run this application.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		} else if (Session.Status.IsError())
		{
			_ShowAndroidToastMessage(
				"ARCore encountered a problem connecting.  Please start the app again.");
			m_IsQuitting = true;
			Invoke("_DoQuit", 0.5f);
		}
	}

	/// <summary>
	/// Actually quit the application.
	/// </summary>
	private void _DoQuit()
	{
		Application.Quit();
	}

	/// <summary>
	/// Show an Android toast message.
	/// </summary>
	/// <param name="message">Message string to show in the toast.</param>
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
