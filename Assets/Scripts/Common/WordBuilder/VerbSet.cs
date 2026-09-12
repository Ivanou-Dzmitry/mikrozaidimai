using System.Collections.Generic;
using UnityEngine;

// One lesson's worth of verb blocks. A game's WordBuilderField is configured
// with one of these.
[CreateAssetMenu(fileName = "VerbSet", menuName = "Games/Verb Set")]
public class VerbSet : ScriptableObject
{
    public List<VerbBlock> blocks = new List<VerbBlock>();
}
