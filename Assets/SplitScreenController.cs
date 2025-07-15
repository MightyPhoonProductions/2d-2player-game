using UnityEngine;

public class SplitScreenController : MonoBehaviour
{
    public Transform player1, player2;
    public Camera cam1, cam2;
    public float mergeDistance = 8f;
    public float transitionSpeed = 3f;
    private bool isSplit = false;

    void Update()
    {
        float distance = Vector3.Distance(player1.position, player2.position);
        bool shouldSplit = distance > mergeDistance;

        if (shouldSplit != isSplit)
            isSplit = shouldSplit;

        if (isSplit)
            SetCameraRects(new Rect(0, 0, 0.5f, 1), new Rect(0.5f, 0, 0.5f, 1));
        else
            SetCameraRects(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0)); // hide second camera
    }

    void SetCameraRects(Rect r1, Rect r2)
    {
        cam1.rect = SmoothRect(cam1.rect, r1);
        cam2.rect = SmoothRect(cam2.rect, r2);
    }

    Rect SmoothRect(Rect current, Rect target)
    {
        return new Rect(
            Mathf.Lerp(current.x, target.x, Time.deltaTime * transitionSpeed),
            Mathf.Lerp(current.y, target.y, Time.deltaTime * transitionSpeed),
            Mathf.Lerp(current.width, target.width, Time.deltaTime * transitionSpeed),
            Mathf.Lerp(current.height, target.height, Time.deltaTime * transitionSpeed)
        );
    }
}
