using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat
{
    public enum AttackKind { Quick, Charged, Shove }

    // One resolved hit, decided by the attacker's hit check. Holds IDs rather than
    // GameObject references so it can be logged, tested and later sent to a co-op host.
    public readonly struct DamageInfo
    {
        public readonly StableId Attacker;
        public readonly string WeaponId;
        public readonly AttackKind Kind;
        public readonly float Amount;
        public readonly Vector3 Point;
        public readonly Vector3 Direction;
        public readonly double Time;

        public DamageInfo(StableId attacker, string weaponId, AttackKind kind, float amount,
            Vector3 point, Vector3 direction, double time)
        {
            Attacker = attacker;
            WeaponId = weaponId;
            Kind = kind;
            Amount = amount;
            Point = point;
            Direction = direction;
            Time = time;
        }
    }

    public readonly struct DamageResult
    {
        public readonly float Applied;
        public readonly bool WentDown;
        // True when the target was already down or dead, so the hit changed nothing.
        public readonly bool Ignored;

        public DamageResult(float applied, bool wentDown, bool ignored)
        {
            Applied = applied;
            WentDown = wentDown;
            Ignored = ignored;
        }

        public static DamageResult IgnoredHit => new DamageResult(0f, false, true);
    }
}
