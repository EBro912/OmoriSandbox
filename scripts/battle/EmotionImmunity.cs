using OmoriSandbox.Battle.Emotions;
using System.Collections.Generic;

namespace OmoriSandbox.Battle;

internal sealed class EmotionImmunity
{
    private readonly HashSet<string> EmotionImmunities = [];
    private readonly HashSet<string> EmotionImmunityExceptions = [];
    private bool ImmuneToAllEmotions;

    internal void Add(string[] emotionIds)
    {
        foreach (string id in emotionIds ?? [])
        {
            if (!string.IsNullOrWhiteSpace(id))
                EmotionImmunities.Add(id);
        }
    }

    internal void AddAllExcept(string[] exceptEmotionIds)
    {
        ImmuneToAllEmotions = true;
        foreach (string id in exceptEmotionIds ?? [])
        {
            if (!string.IsNullOrWhiteSpace(id))
                EmotionImmunityExceptions.Add(id);
        }
    }

    internal bool Blocks(Emotion emotion)
    {
        return EmotionImmunities.Contains(emotion.Id) || (ImmuneToAllEmotions && !EmotionImmunityExceptions.Contains(emotion.Id));
    }
}
