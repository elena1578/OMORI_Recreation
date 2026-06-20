using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class BattleDialogManager : MonoBehaviour
{
    public static BattleDialogManager instance;

    [SerializeField] private TextMeshProUGUI text;
#if UNITY_EDITOR
    private float typeSpeed = 0.02f;
#elif  UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
    private float typeSpeed = 0.008f;  // text speed is slower in [pc] build for some reason so increase
#else  // for web builds etc.
    private float typeSpeed = 0.005f;
#endif

    private Coroutine typingRoutine;
    public bool typing { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void Show(string message)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(message));
    }

    public void Show(BattleActionData action, BattleActionResult result)
    {
        string parsed = Parse(action, result);
        typing = true;
        Show(parsed);
    }

    private IEnumerator TypeText(string message)
    {
        text.text = "";

        foreach (char c in message)
        {
            text.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typing = false;
    }

    public void Clear()
    {
        text.text = "";
    }

    /// <summary>
    /// used for when an actor starts their turn/is in process of choosing an action
    /// </summary>
    /// <param name="actor"></param>
    public void ShowPlanningPrompt(BattleActor actor) => Show($"What will {actor.name.ToDisplayName()} do?");


    #region Parsing
    private string Parse(BattleActionData action, BattleActionResult result)
    {
        string text = action.battleLogText;

        text = text.Replace("{actor}", GetActorName(result.actor));
        text = text.Replace("{target}", GetActorName(result.targets));

        text = text.Replace("{damage}", result.damage.ToString());
        text = text.Replace("{heal}", result.heal.ToString());

        text = text.Replace("{statChange}", GetStatusTypeForText(result.statChange ?? action.statChangeType));
        text = text.Replace("{statMultiplier}", result.statMultiplier.ToString());
        text = text.Replace("{statChangeDuration}", result.statChangeDuration.ToString());

        // only add auto result text if base text doesn't already include damage/heal to avoid repetition
        if (!action.battleLogText.Contains("{damage}") && !action.battleLogText.Contains("{heal}"))
            text += GetAutoResultText(action, result);

        if (result.didCrit)
            text = "IT HIT RIGHT IN THE HEART!\n" + text;

        // add toast parsing here

        return text;
    }

    private string GetActorName(BattleActor actor)
    {
        return actor != null ? actor.name.ToDisplayName() : "?????";
    }

    private string GetActorName(List<BattleActor> actors)
    {
        if (actors == null || actors.Count == 0)
            return "?????";

        if (actors.Count == 1)
            return GetActorName(actors[0]);

        return string.Join(", ", actors.ConvertAll(a => GetActorName(a)));
    }

    private string GetAutoResultText(BattleActionData action, BattleActionResult result)
    {
        switch (action.actionType)
        {
            case BattleActionData.ActionType.Attack:
                var target = result.targets[0];
                
                if (result.damage > 0 && (result.statChange != BattleActionData.StatChangeType.None || action.statChangeDuration > 0))
                {
                    string turns = NumberToWords(result.statChangeDuration);  // e.g., "3" -> "three"
                    string changeText = result.statMultiplier == 0.5f ? "halved"  // = 0.5, then "halved"
                        : (result.statMultiplier > 1f ? "increased" : "decreased");  // otherwise just "increased"/"decreased"
                    return $"\n{GetActorName(result.targets)} takes {result.damage} damage and their {GetStatusTypeForText(result.statChange.Value)} was {changeText} for {turns} turn{(result.statChangeDuration > 1 ? "s" : "")}!";
                }

                if (result.damage > 0 && result.emotion.HasValue && action.emotionEffect != EmotionType.Neutral)
                    return $"\n{GetActorName(result.targets)} takes {result.damage} damage and became {GetEmotionText(target)}!";

                if (result.damage > 0)
                    return $"\n{GetActorName(result.targets)} takes {result.damage} damage!";

                return "";

            case BattleActionData.ActionType.Heal:
                target = result.targets[0];

                if (result.heal > 0 && (result.emotion.HasValue && result.emotion.Value != EmotionType.Neutral))
                    return $"\n{GetActorName(result.targets)} recovers {result.heal} HP and became {GetEmotionText(target)}!";
                if (result.heal > 0)
                    return $"\n{GetActorName(result.targets)} recovers {result.heal} HP!";

                return "";

            case BattleActionData.ActionType.Emotion:
                target = result.targets[0];
                EmotionType appliedEmotion = result.emotion ?? action.emotionEffect;

                // tier 1 cases
                // just hit max
                if (result.previousEmotion != target.currentEmotion && target.IsMaxTierOneEmotion())
                    return $"\n{GetActorName(result.targets)} became {GetEmotionText(target)} (now cannot become more {EmotionSystem.GetEmotionGroupingText(target.currentEmotion)})!";

                // already max
                if (result.previousEmotion == target.currentEmotion && target.IsMaxTierOneEmotion())
                    return $"\n{GetActorName(result.targets)} cannot become more {GetEmotionText(target)}!";

                // tier 2+ 
                // just hit max
                if (result.previousEmotion == target.currentEmotion &&
                    target.currentEmotionTier == target.maxEmotionTier &&
                    result.previousEmotionTier < target.maxEmotionTier)
                    return $"\n{GetActorName(result.targets)} became {GetEmotionText(target)} (now cannot become more {EmotionSystem.GetEmotionGroupingText(target.currentEmotion)})!";

                // already maxed
                if (target.IsMaxEmotion())
                    return $"\n{GetActorName(result.targets)} cannot become more {GetEmotionText(target)}!";
                
                // standard parsing
                if (result.emotion.HasValue && action.emotionEffect != EmotionType.Neutral)
                    return $"\n{GetActorName(result.targets)} became {GetEmotionText(target)}!";

                return "";

            case BattleActionData.ActionType.StatChange:
                if (result.statChange.HasValue)
                {
                    string turns = NumberToWords(result.statChangeDuration);  // e.g., "3" -> "three"
                    string changeText = result.statMultiplier == 0.5f ? "halved"  // = 0.5, then "halved"
                        : (result.statMultiplier > 1f ? "increased" : "decreased");  // otherwise just "increased"/"decreased"
                    return $"\n{GetActorName(result.targets)}'s {GetStatusTypeForText(result.statChange.Value)} was {changeText} for {turns} turn{(result.statChangeDuration > 1 ? "s" : "")}!";
                }

                return "";

            default:
                return "";
        }
    }
    #endregion


    #region Text Helpers
    private string GetEmotionText(BattleActor actor)
    {
        return EmotionText.Get(actor.currentEmotion, actor.currentEmotionTier);
    }

    private string GetStatusTypeForText(BattleActionData.StatChangeType statChangeType)
    {
        switch (statChangeType)
        {
            case BattleActionData.StatChangeType.AttackUp:
                return "ATK";
            case BattleActionData.StatChangeType.AttackDown:
                return "ATK";
            case BattleActionData.StatChangeType.DefenseUp:
                return "DEF";
            case BattleActionData.StatChangeType.DefenseDown:
                return "DEF";
            case BattleActionData.StatChangeType.SpeedUp:
                return "SPD";
            case BattleActionData.StatChangeType.SpeedDown:
                return "SPD";
            default:
                return "";
        }
    }

    private string NumberToWords(int number)
    {
        string[] words = { "zero", "one", "two", "three", "four", "five",
                        "six", "seven", "eight", "nine", "ten",
                        "eleven", "twelve", "thirteen", "fourteen", "fifteen",
                        "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };

        if (number >= 0 && number <= 20)
            return words[number];

        return number.ToString();  // fallback for numbers over 20 (even if this'll prob never happen)
    }
    #endregion
}
