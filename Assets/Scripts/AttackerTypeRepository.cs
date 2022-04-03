using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackerType { Rider, Infantry, Piker };

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

        AttackerType typeAttack;
        AttackerType typeDefense;
    }

    private Dictionary<AttackerMatch, float> bonusModifiers;

    public

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
}
