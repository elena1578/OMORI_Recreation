using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
using System.Collections;
#endif


public class OverworldCameraFollow : MonoBehaviour
{
    private Transform target;  // player
    private float camHalfHeight;
    private float camHalfWidth;
    private float minX, maxX, minY, maxY;
    private Vector3 shakeOffset;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InitializeCameraBounds();

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        FindPlayer();
        InitializeCameraBounds();
    }

    public void SetShakeOffset(Vector3 offset) => shakeOffset = offset;

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        StartCoroutine(WaitUntilPlayerExists());
    }
#else
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("[CameraFollow] Player not found");
    }
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
    private IEnumerator WaitUntilPlayerExists()
    {
        while (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[CameraFollow] Player found, starting to follow");
                yield break;
            }
            yield return null;  // wait for next frame
        }
    }
#endif

    private void InitializeCameraBounds()
    {
        GameObject boundsObj = GameObject.Find("CameraBounds");
        if (boundsObj == null)
        {
            Debug.LogWarning("[CameraFollow] CameraBounds object not found in scene");
            return;
        }

        BoxCollider2D box = boundsObj.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning("[CameraFollow] No BoxCollider2D found on CameraBounds object, needed to define camera limits");
            return;
        }

        minX = box.bounds.min.x;
        maxX = box.bounds.max.x;
        minY = box.bounds.min.y;
        maxY = box.bounds.max.y;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x;
        float targetY = target.position.y;

#if !UNITY_WEBGL
        float clampedX = Mathf.Clamp(targetX, minX + camHalfWidth, maxX - camHalfWidth);
        float clampedY = Mathf.Clamp(targetY, minY + camHalfHeight, maxY - camHalfHeight);
#else
        // WebGL has issues w/ regular camera bound sizing, so size down a bit
        float clampedX = Mathf.Clamp(targetX, minX + camHalfWidth, maxX - camHalfWidth) * 0.75f;
        float clampedY = Mathf.Clamp(targetY, minY + camHalfHeight, maxY - camHalfHeight) * 0.75f;
#endif
        transform.position = new Vector3(clampedX, clampedY, transform.position.z) + shakeOffset;
    }
}