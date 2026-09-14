using System;

// One verb's 3 principal parts (bendratis / esamasis laikas / būtasis kartinis
// laikas — e.g. dirbti / dirba / dirbo). The player builds all 3, one at a
// time; a single wrong Check fails the whole block, all 3 correct passes it.
[Serializable]
public class VerbBlock
{
    public string id;
    // Semantic group (movement, food, communication, ...) — lets the block
    // picker chain a few related verbs in a row instead of pure random order,
    // so forms are easier to remember by association.
    public string theme;
    public VerbForm infinitive;
    public VerbForm presentTense;
    public VerbForm pastTense;

    public VerbForm[] Forms => new[] { infinitive, presentTense, pastTense };
}
