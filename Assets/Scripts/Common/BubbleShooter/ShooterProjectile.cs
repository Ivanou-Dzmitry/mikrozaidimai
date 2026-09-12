using TMPro;
using UnityEngine;

// The bubble the player aims and shoots, showing the translation word.
// Sits kinematic (so it can be dragged) until Launch() hands it over to physics.
// Gravity is off so it flies in a straight line like a classic bubble-shooter
// shot rather than arcing — flip useGravity on the prefab if you want an arc instead.
// Z position is frozen — the field is a flat XY board, X stays free so bounces
// off the side walls can actually redirect the shot toward other columns.
[RequireComponent(typeof(Rigidbody))]
public class ShooterProjectile : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] RandomGradientColor randomColor;
    [Tooltip("How far above its normal spot the label jumps while being dragged — a finger/thumb on a phone screen otherwise covers it right where you need to read it.")]
    [SerializeField] float labelLiftOffset = 0.8f;

    Rigidbody rb;
    Vector3 labelRestLocalPosition;

    public WordPair Pair { get; private set; }
    public bool Launched { get; private set; }

    bool resolved;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;
        // fast + small vs a wall can tunnel through with the default Discrete mode
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (randomColor == null) randomColor = GetComponent<RandomGradientColor>();
        if (label != null) labelRestLocalPosition = label.transform.localPosition;
    }

    // true while being dragged: jumps the label up above the ball (and the
    // finger/thumb holding it) so it stays readable; false: snaps back instantly.
    public void SetLabelLifted(bool lifted)
    {
        if (label == null) return;
        label.transform.localPosition = lifted
            ? labelRestLocalPosition + Vector3.up * labelLiftOffset
            : labelRestLocalPosition;
    }

    public void Setup(WordPair pair)
    {
        Pair = pair;
        Launched = false;
        rb.isKinematic = true;
        if (label != null) label.text = pair.GetVariant(Settings.Data.language);
        if (randomColor != null) randomColor.Apply();
    }

    public void Launch(Vector3 velocity)
    {
        Launched = true;
        resolved = false;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!Launched || resolved) return;

        WordBubble target = collision.collider.GetComponentInParent<WordBubble>();
        if (target != null)
        {
            // Destroy() only takes effect at end of frame — guard so a second
            // OnCollisionEnter on the same still-alive object (e.g. grazing
            // another bubble a moment later) can't resolve this shot twice.
            resolved = true;
            BubbleField.Instance.ResolveHit(this, target);
        }
        // hitting anything else (the play field's walls) just bounces naturally via physics
    }
}
