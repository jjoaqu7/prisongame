using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Unarmed attacks, agreed September 27: LMB punch, V kick (slightly more damage), F shove.
    public enum AttackKind
    {
        Punch,
        Kick,
        Shove,
    }

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

        // Metres the target is pushed along Direction; applied by the target's movement adapter.
        public readonly float Knockback;

        // Freeze applied to the attacker's and the target's combat timelines when the hit lands.
        public readonly float HitStopSeconds;

        public DamageInfo(StableId attacker, string weaponId, AttackKind kind, float amount,
            Vector3 point, Vector3 direction, double time, float knockback = 0f, float hitStopSeconds = 0f)
        {
            Attacker = attacker;
            WeaponId = weaponId;
            Kind = kind;
            Amount = amount;
            Point = point;
            Direction = direction;
            Time = time;
            Knockback = knockback;
            HitStopSeconds = hitStopSeconds;
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
