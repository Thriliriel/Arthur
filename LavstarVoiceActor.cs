using UnityEngine;

namespace Humelo
{
    public enum VoiceActorGender ///< As the name implies.
    {
        NA,
        MALE,
        FEMALE
    }

    [System.Serializable]
    public class LavstarVoiceActor
    {
        [Tooltip("Actor Name")]
        public string Name; ///< As the name implies.

        [Tooltip("Gender Info")]
        public VoiceActorGender Gender; ///< As the name implies.

        [Tooltip("Actor Culture")]
        public string Culture; ///< As the name implies.

        public LavstarVoiceActor(string name, VoiceActorGender gender, string culture)
        {
            Name = name;
            Gender = gender;
            Culture = culture;
        }
    }
}
// © 2019-2020 Humelo Inc.