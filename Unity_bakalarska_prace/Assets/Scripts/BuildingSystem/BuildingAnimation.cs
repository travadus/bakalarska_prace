using UnityEngine;

public class BuildingAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float speed = 1f;

    private float time;
    private bool isPlaying = true;

    private void Awake()
    {
        transform.localScale = new Vector3(1, 0, 1);
    }

    private void Update()
    {
        if (!isPlaying) return;

        time += Time.deltaTime * speed;

        float value = animationCurve.Evaluate(time);

        transform.localScale = new Vector3(1, value, 1);

        if (time >= animationCurve.keys[animationCurve.length - 1].time)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isPlaying = false;
            Destroy(this);
        }
    }
}