using UnityEngine;
using System.Collections.Generic;


public class BattlePartyDataManager : MonoBehaviour
{
    public static BattlePartyDataManager instance;

    [Header("Party Members")]
    public List<CharacterBattleData> partyMembers;

    [HideInInspector] public Dictionary<CharacterName, int> currentHP;  // runtime HP
    // [HideInInspector] public Dictionary<CharacterBattleData, int> currentHP;  // runtime HP
    [HideInInspector] public Dictionary<CharacterName, int> currentJuice;  // runtime juice
    // [HideInInspector] public Dictionary<CharacterBattleData, int> currentJuice;  // runtime juice

    // used for debug sliders in editor
    [HideInInspector] public List<int> initialHP;
    [HideInInspector] public List<int> initialJuice;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRuntimeValues();
    }

    /// <summary>
    /// sets up the runtime HP/juice dictionaries.
    /// uses editor debug values if provided
    /// </summary>
    public void InitializeRuntimeValues()
    {
        // if dictionaries null, create
        if (currentHP == null) currentHP = new Dictionary<CharacterName, int>();
        if (currentJuice == null) currentJuice = new Dictionary<CharacterName, int>();
        if (partyMembers == null) return;

        // make sure debug lists exist
        if (initialHP == null) initialHP = new List<int>();
        if (initialJuice == null) initialJuice = new List<int>();

        // ensure debug lists are same length as party members
        while (initialHP.Count < partyMembers.Count) initialHP.Add(0);
        while (initialJuice.Count < partyMembers.Count) initialJuice.Add(0);

        for (int i = 0; i < partyMembers.Count; i++)
        {
            var member = partyMembers[i];
            if (member == null) continue;

            // only set HP if key doesn't exist yet
            if (!currentHP.ContainsKey(member.characterName))
            {
                // use debug slider val if > 0, otherwise use baseHP
                int hp = Mathf.Clamp(initialHP[i] > 0 ? initialHP[i] : member.baseHP, 0, member.baseHP);
                currentHP[member.characterName] = hp;
            }

            // only set juice if key doesn't exist yet
            if (!currentJuice.ContainsKey(member.characterName))
            {
                // use debug slider value if > 0, otherwise use baseJuice
                int juice = Mathf.Clamp(initialJuice[i] > 0 ? initialJuice[i] : member.baseJuice, 0, member.baseJuice);
                currentJuice[member.characterName] = juice;
            }
        }
    }


    #region HP
    public void SetHP(CharacterName characterName, int hp)
    {
        if (currentHP.ContainsKey(characterName))
            currentHP[characterName] = Mathf.Clamp(hp, 0, partyMembers.Find(c => c.characterName == characterName).baseHP);
    }

    public int GetHP(CharacterName characterName)
    {
        return currentHP.ContainsKey(characterName) ? currentHP[characterName] : 0;
    }
    #endregion


    #region Juice
    public void SetJuice(CharacterName characterName, int juice)
    {
        if (currentJuice.ContainsKey(characterName))
            currentJuice[characterName] = Mathf.Clamp(juice, 0, partyMembers.Find(c => c.characterName == characterName).baseJuice);
    }

    public int GetJuice(CharacterName characterName)
    {
        return currentJuice.ContainsKey(characterName) ? currentJuice[characterName] : 0;
    }
    #endregion


    #region Helpers
    public static void HealEntireParty()
    {
        if (instance == null || instance.partyMembers == null) return;

        foreach (var member in instance.partyMembers)
        {
            if (member == null) continue;
            instance.SetHP(member.characterName, member.baseHP);
            instance.SetJuice(member.characterName, member.baseJuice);
        }
    }
    #endregion
}