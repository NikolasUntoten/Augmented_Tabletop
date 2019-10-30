namespace GoogleARCore.Examples.HelloAR
{
	using System.Collections.Generic;
	using Firebase.Database;
	using GoogleARCore;
	using GoogleARCore.CrossPlatform;
	using GoogleARCore.Examples.Common;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
	using UnityEngine.SceneManagement;
	using UnityEngine.Serialization;

#if UNITY_EDITOR
	// Set up touch input propagation while using Instant Preview in the editor.
	using Input = InstantPreviewInput;
#endif

	/// <summary>
	/// Controls the HelloAR example.
	/// </summary>
	public class ARCoreController : MonoBehaviour
	{

		public delegate void OnClick(Touch touch);

		[System.Serializable]
		public class WorldTouchedEvent : UnityEvent { }

		[SerializeField]
		private WorldTouchedEvent m_worldTouched = new WorldTouchedEvent();
		public WorldTouchedEvent onTouch
		{
			get { return m_worldTouched; }
			set { m_worldTouched = value; }
		}

		/// <summary>
		/// The first-person camera being used to render the passthrough camera image (i.e. AR
		/// background).
		/// </summary>
		public Camera FirstPersonCamera;

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

		public static TrackableHit ARCoreRayCast()
		{
			Touch touch = Input.GetTouch(0);
			// Raycast against the location the player touched to search for planes.
			TrackableHit hit;
			TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
				TrackableHitFlags.FeaturePointWithSurfaceNormal;

			if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
			{
				return hit;
			}

			throw new System.Exception("No ARCore object detected.");
		}

		public static GameObject UnityRaycast()
		{
			Touch touch = Input.GetTouch(0);
			// Raycast against the location the player touched to search for planes.
			RaycastHit hit;
			if (Physics.Raycast(Frame.Pose.position, Frame.Pose.forward, out hit, 100.0f))
			{
				//_ShowAndroidToastMessage("HIT! " + hit.collider.gameObject.ToString());
				return hit.collider.gameObject.transform.parent.gameObject;
			} else
			{
				//_ShowAndroidToastMessage("MISS");
				return null;
			}
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

			m_worldTouched.Invoke();

			//foreach (OnClick onClick in OnClickActions)
			//{
			//	onClick(touch);
			//}
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
				TableUtility.ShowAndroidToastMessage("Camera permission is needed to run this application.");
				m_IsQuitting = true;
				Invoke("_DoQuit", 0.5f);
			} else if (Session.Status.IsError())
			{
				TableUtility.ShowAndroidToastMessage(
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
	}
}
