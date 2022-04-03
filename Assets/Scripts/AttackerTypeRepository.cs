using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackerType { Rider, Infantry, Piker };

[System.Serializable]
public class AttackerEditorEntry
{
  public   AttackerType typeAttack;
  public  AttackerType typeDefense;
  public  float modifier;
}

public class AttackerTypeRepository : MonoBehaviour
{
    public class AttackerMatch
    {
        public AttackerMatch(AttackerType attacker, AttackerType defender)
        {
            typeAttack = attacker;
            typeDefense = defender;
        }
        public override bool Equals(object other)
        {
            return (other is AttackerMatch) && ((AttackerMatch)other) == this;
        }
        public static bool operator ==(AttackerMatch lhs, AttackerMatch rhs)
        {
            return lhs.typeAttack == rhs.typeAttack && lhs.typeDefense == rhs.typeDefense;
        }
        public static bool operator !=(AttackerMatch lhs, AttackerMatch rhs)
        {
            return !(lhs.typeAttack == rhs.typeAttack && lhs.typeDefense == rhs.typeDefense);
        }

        private static int GetAttackHash(AttackerType type)
        {
            switch (type)
            {
                case AttackerType.Infantry:
                    return 13;
                case AttackerType.Rider:
                    return 17;
                case AttackerType.Piker:
                    return 19;
                default:
                    return 1;
            }
        }
        private static int GetDefendHash(AttackerType type)
        {
            switch (type)
            {
                case AttackerType.Infantry:
                    return 3;
                case AttackerType.Rider:
                    return 5;
                case AttackerType.Piker:
                    return 7;
                default:
                    return 1;
            }
        }


        public override int GetHashCode()
        {
            return GetAttackHash(typeAttack) * GetDefendHash(typeDefense);
        }
        AttackerType typeAttack;
        AttackerType typeDefense;
    }

    private Dictionary<AttackerMatch, float> bonusModifiers = new Dictionary<AttackerMatch, float>();

    [SerializeField]
    public List<AttackerEditorEntry> modifierList;

    public float GetModifier(AttackerType attacker, AttackerType defender)
    {
        float modifier;
        AttackerMatch match = new AttackerMatch(attacker,defender);
        if(bonusModifiers.TryGetValue(match,out modifier))
        {
            return modifier;
        }
        return 1f;
    }

    private void Awake()
    {
        foreach(AttackerEditorEntry entry in modifierList)
        {
            AttackerMatch match = new AttackerMatch(entry.typeAttack, entry.typeDefense);
            bonusModifiers.Add(match, entry.modifier);
        }
    }
}
