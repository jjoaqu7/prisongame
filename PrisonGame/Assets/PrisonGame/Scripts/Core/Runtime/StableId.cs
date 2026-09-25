using System;
using UnityEngine;

namespace PrisonGame.Core
{
    // Stable identity for a player slot, NPC or world item. Saved state and combat
    // records use this instead of GameObject names, instance IDs or Unity 6's
    // UnityEngine.EntityId, all of which change between sessions.
    [Serializable]
    public struct StableId : IEquatable<StableId>
    {
        [SerializeField] private string value;

        public StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Entity ID must not be empty.", nameof(value));
            this.value = value;
        }

        public string Value => value;
        public bool IsValid => !string.IsNullOrWhiteSpace(value);

        public static StableId New() => new StableId(Guid.NewGuid().ToString("N"));

        public bool Equals(StableId other) => string.Equals(value, other.value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is StableId other && Equals(other);
        public override int GetHashCode() => value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
        public override string ToString() => IsValid ? value : "(none)";
        public static bool operator ==(StableId a, StableId b) => a.Equals(b);
        public static bool operator !=(StableId a, StableId b) => !a.Equals(b);
    }
}
