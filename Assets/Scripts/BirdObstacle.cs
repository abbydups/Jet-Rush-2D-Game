using UnityEngine;

/*
 * Extends the base Obstacle class to add wing flapping animation.
 * The bird flies straight left at scroll speed and bobs up and down
 * using a sine wave to simulate wing flapping motion.
 */
public class BirdObstacle : Obstacle
{
    [Header("Bird Settings")]
    [Tooltip("How far up and down the bird bobs to simulate wing flapping.")]
    public float flapAmplitude = 0.3f;

    [Tooltip("How fast the bird flaps its wings.")]
    public float flapSpeed = 3f;

    private float _startY;

    /*
     * Stores the starting Y position for the bobbing calculation.
     */
    protected override void Start()
    {
        _startY = transform.position.y;
    }

    /*
     * Calls the base Obstacle Update for leftward movement
     * then adds the wing flapping bob on top.
     */
    protected override void Update()
    {
        if (GameManager.Instance == null) return;

        // Only bob during Playing state
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            base.Update();
            return;
        }

        // Call base class Update for leftward movement and destroy check
        base.Update();

        // Bob up and down using sine wave to simulate wing flapping
        float newY = _startY + Mathf.Sin(Time.time * flapSpeed) * flapAmplitude;
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
}