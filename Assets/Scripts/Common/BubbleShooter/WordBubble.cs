using TMPro;
using UnityEngine;

// A target bubble sitting in the field, showing one Lithuanian word. Static —
// no Rigidbody, just a collider for the projectile to hit.
[RequireComponent(typeof(Collider))]
public class WordBubble : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] RandomGradientColor randomColor;

    public WordPair Pair { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    void Awake()
    {
        if (randomColor == null) randomColor = GetComponent<RandomGradientColor>();
    }

    public void Setup(WordPair pair, int row, int column, Gradient gradientOverride = null)
    {
        Pair = pair;
        Row = row;
        Column = column;
        if (label != null) label.text = pair.target;

        if (randomColor != null)
        {
            if (gradientOverride != null) randomColor.SetGradient(gradientOverride);
            randomColor.Apply();
        }
    }
}
