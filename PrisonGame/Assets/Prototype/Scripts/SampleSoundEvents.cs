using System;
using UnityEngine;

namespace PrisonGame.Prototype
{
    public enum SampleSound { Pickup, Place, Crackers, Fruit, Wrap, Sale, Door, Shelf, Restock }

    // Successful actions announce a cue and actor; local audio decides how to render it.
    public static class SampleSoundEvents
    {
        public static event Action<PlayerInteraction, SampleSound, Vector3> Played;
        public static void Emit(PlayerInteraction actor, SampleSound sound, Vector3 position) => Played?.Invoke(actor, sound, position);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Played = null;
    }
}
