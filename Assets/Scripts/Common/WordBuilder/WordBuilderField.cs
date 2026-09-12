using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// Main gameplay loop for the letter-building game. Blocks (a verb's 3 forms)
// are drawn randomly from a pending pool; the player builds one form at a
// time out of shuffled letters (target's own letters + random decoys). Check
// on a correct form advances to the next form in the block; a wrong Check
// fails the WHOLE block (sends it to the fail queue, to be retried later) and
// jumps to a new random block. A block only leaves the session for good once
// all 3 of its forms are answered correctly in a row.
public class WordBuilderField : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] VerbSet verbSet;
    [SerializeField] GameSession session;

    [Header("UI")]
    [SerializeField] TMP_Text translationText;
    [Tooltip("Shows the current block's forms as they're answered correctly (e.g. \"dirbti - dirba\"), so they stay visible together for recall. Empty until the first one lands; cleared on a new block (including after a fail).")]
    [SerializeField] TMP_Text progressText;
    [SerializeField] AnswerZone answerZone;
    [SerializeField] Transform letterButtonParent;
    [SerializeField] LetterTileButton letterButtonPrefab;
    [SerializeField] Button backButton;
    [SerializeField] Button checkButton;
    [SerializeField] Button nextButton;
    [Tooltip("Blinks the letter tile for whatever letter comes next in the word, without revealing anything else.")]
    [SerializeField] Button helpButton;
    [Tooltip("How many Help presses are allowed per BLOCK (all 3 forms share this budget, not one each). Resets on the next block.")]
    [SerializeField] int maxHelpUses = 3;
    [SerializeField] TMP_Text helpCountText;
    [Tooltip("Shown in the HelperPanel when a block's all 3 forms are answered correctly, before moving on.")]
    [SerializeField] LocalizedString successMessage;
    [Tooltip("Prefix shown above the player's wrong attempt in the HelperPanel's example line, with the incorrect letters underlined.")]
    [SerializeField] LocalizedString mistakesLabel;

    [Header("Letters")]
    [SerializeField] int extraLetterCount = 3;
    [SerializeField] string decoyAlphabet = "aąbcčdeęėfghiįyjklmnoprsštuųūvzž";
    [Tooltip("Delay between each letter button appearing, so the panel fills in instead of popping in all at once. 0 = instant.")]
    [SerializeField] float letterSpawnDelay = 0.05f;

    readonly List<VerbBlock> pending = new List<VerbBlock>();
    readonly List<VerbBlock> failQueue = new List<VerbBlock>();
    readonly List<LetterTileButton> spawnedButtons = new List<LetterTileButton>();

    readonly List<string> answeredForms = new List<string>();

    VerbBlock currentBlock;
    int currentFormIndex;
    Coroutine spawnRoutine;
    bool inputEnabled = true;
    int helpUsesLeft;

    void Awake()
    {
        if (session == null) session = GetComponentInParent<GameSession>();
    }

    void OnEnable()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBack);
        if (checkButton != null) checkButton.onClick.AddListener(OnCheck);
        if (nextButton != null) nextButton.onClick.AddListener(OnNext);
        if (helpButton != null) helpButton.onClick.AddListener(OnHelp);
    }

    void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(OnBack);
        if (checkButton != null) checkButton.onClick.RemoveListener(OnCheck);
        if (nextButton != null) nextButton.onClick.RemoveListener(OnNext);
        if (helpButton != null) helpButton.onClick.RemoveListener(OnHelp);
        if (HelperPanel.Instance != null) HelperPanel.Instance.Hidden -= HandleHintHidden;
    }

    void Start()
    {
        pending.AddRange(verbSet.blocks);
        Shuffle(pending);
        NextBlock();
    }

    void NextBlock()
    {
        if (pending.Count == 0)
        {
            if (failQueue.Count == 0)
            {
                session?.Win();
                return;
            }

            pending.AddRange(failQueue);
            failQueue.Clear();
            Shuffle(pending);
        }

        int lastIndex = pending.Count - 1;
        currentBlock = pending[lastIndex];
        pending.RemoveAt(lastIndex);
        currentFormIndex = 0;

        answeredForms.Clear();
        UpdateProgressText();

        helpUsesLeft = maxHelpUses;
        UpdateHelpCountText();

        ShowForm();
    }

    void UpdateHelpCountText()
    {
        if (helpCountText != null) helpCountText.text = helpUsesLeft.ToString();
    }

    void UpdateProgressText()
    {
        if (progressText != null) progressText.text = string.Join(" - ", answeredForms);
    }

    void ShowForm()
    {
        VerbForm form = currentBlock.Forms[currentFormIndex];

        if (translationText != null)
        {
            translationText.text = form.GetVariant(Settings.Data.language);
        }

        answerZone.Clear();
        SetInputEnabled(true);
        BuildLetterButtons(form.target);
    }

    void UpdateCheckInteractable()
    {
        if (checkButton != null) checkButton.interactable = inputEnabled && answerZone.CurrentText.Length > 0;
    }

    // Disabled while waiting for the HelperPanel hint to finish showing after a
    // wrong answer/skip, so the player can't queue up more input against a form
    // that's about to change out from under them.
    void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (backButton != null) backButton.interactable = enabled;
        if (nextButton != null) nextButton.interactable = enabled;
        if (helpButton != null) helpButton.interactable = enabled && helpUsesLeft > 0;
        UpdateCheckInteractable();
    }

    void BuildLetterButtons(string target)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        foreach (var button in spawnedButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        spawnedButtons.Clear();

        var letters = new List<string>();
        var seen = new HashSet<char>();
        foreach (char c in target)
        {
            if (seen.Add(c)) letters.Add(c.ToString());
        }

        // Decoys must not repeat a letter already in the word (and not repeat
        // each other either) — pick from the alphabet minus what's taken so far.
        var availableDecoys = new List<char>();
        foreach (char c in decoyAlphabet)
        {
            if (!seen.Contains(c)) availableDecoys.Add(c);
        }

        int decoyCount = Mathf.Min(extraLetterCount, availableDecoys.Count);
        for (int i = 0; i < decoyCount; i++)
        {
            int index = Random.Range(0, availableDecoys.Count);
            letters.Add(availableDecoys[index].ToString());
            availableDecoys.RemoveAt(index);
        }

        Shuffle(letters);

        spawnRoutine = StartCoroutine(SpawnLetterButtons(letters));
    }

    IEnumerator SpawnLetterButtons(List<string> letters)
    {
        for (int i = 0; i < letters.Count; i++)
        {
            string letter = letters[i];
            LetterTileButton button = Instantiate(letterButtonPrefab, letterButtonParent);
            button.Setup(letter, OnLetterClicked);
            button.name = $"LetterButton_{i + 1:00}_{letter}";
            spawnedButtons.Add(button);

            if (letterSpawnDelay > 0f) yield return new WaitForSeconds(letterSpawnDelay);
        }

        spawnRoutine = null;
    }

    void OnLetterClicked(string letter)
    {
        answerZone.Append(letter);
        UpdateCheckInteractable();
    }

    void OnBack()
    {
        answerZone.RemoveLast();
        UpdateCheckInteractable();
    }

    // Blinks the tile for whichever letter comes next (by position in the
    // target word, based on how much has been typed so far) — doesn't reveal
    // anything about later letters or how long the word is.
    void OnHelp()
    {
        if (helpUsesLeft <= 0) return;

        string target = currentBlock.Forms[currentFormIndex].target;
        int nextIndex = answerZone.CurrentText.Length;
        if (nextIndex >= target.Length) return;

        string nextLetter = target[nextIndex].ToString();
        foreach (var button in spawnedButtons)
        {
            if (button != null && string.Equals(button.Letter, nextLetter, System.StringComparison.OrdinalIgnoreCase))
            {
                button.PlayHint();
                break;
            }
        }

        helpUsesLeft--;
        if (helpButton != null) helpButton.interactable = helpUsesLeft > 0;
        UpdateHelpCountText();
    }

    void OnCheck()
    {
        VerbForm form = currentBlock.Forms[currentFormIndex];
        bool correct = string.Equals(answerZone.CurrentText, form.target, System.StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            session?.AddScore(1);
            answeredForms.Add(form.target);
            UpdateProgressText();
            currentFormIndex++;

            if (currentFormIndex >= currentBlock.Forms.Length)
            {
                CompleteBlock();
            }
            else
            {
                ShowForm();
            }
        }
        else
        {
            FailCurrentBlock(form, answerZone.CurrentText);
        }
    }

    // All 3 forms answered correctly — leave the finished trio visible in
    // progressText and show a quick "Great" in the HelperPanel before the
    // next block replaces both.
    void CompleteBlock()
    {
        SetInputEnabled(false);

        string message = LocalizationUtil.Resolve(successMessage, "Great");
        string forms = string.Join(" - ", answeredForms);
        bool messageShowing = HelperPanel.Instance != null && HelperPanel.Instance.ShowMessage(message, forms);

        if (messageShowing)
        {
            HelperPanel.Instance.Hidden += HandleHintHidden;
        }
        else
        {
            NextBlock();
        }
    }

    // Player gives up on the current form/block instead of answering — treated
    // the same as a wrong Check: the correct answer is shown and the whole
    // block goes to the fail queue to be retried later.
    void OnNext()
    {
        FailCurrentBlock(currentBlock.Forms[currentFormIndex]);
    }

    // wrongAttempt is optional — passed from a failed Check (shows which
    // letters were wrong), omitted from a Next/skip (nothing was submitted).
    void FailCurrentBlock(VerbForm form, string wrongAttempt = null)
    {
        failQueue.Add(currentBlock);
        SetInputEnabled(false);

        string mistakes = !string.IsNullOrEmpty(wrongAttempt) ? BuildMistakesText(wrongAttempt, form.target) : null;

        bool hintShowing = HelperPanel.Instance != null
            && HelperPanel.Instance.ShowHint(form.GetVariant(Settings.Data.language), form.target, mistakes);

        if (hintShowing)
        {
            HelperPanel.Instance.Hidden += HandleHintHidden;
        }
        else
        {
            NextBlock();
        }
    }

    // The submitted attempt with any letter that doesn't match the correct
    // word at that position underlined/colored red (extra letters past the
    // correct word's length count as wrong too).
    string BuildMistakesText(string attempt, string target)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(LocalizationUtil.Resolve(mistakesLabel, "Mistakes:")).Append(' ');

        for (int i = 0; i < attempt.Length; i++)
        {
            char c = attempt[i];
            bool wrong = i >= target.Length || char.ToLowerInvariant(c) != char.ToLowerInvariant(target[i]);

            if (wrong) sb.Append("<color=#FF5555><u>").Append(c).Append("</u></color>");
            else sb.Append(c);
        }

        return sb.ToString();
    }

    void HandleHintHidden()
    {
        HelperPanel.Instance.Hidden -= HandleHintHidden;
        NextBlock();
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
