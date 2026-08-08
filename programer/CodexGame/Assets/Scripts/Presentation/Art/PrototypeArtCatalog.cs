using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodexGame.Presentation.Art
{
    public enum PrototypeArtKey
    {
        CardFrontBase,
        CardBack,
        SuitSpade,
        SuitDiamond,
        SuitHeart,
        SuitClub,
        RankAce,
        RankTwo,
        RankThree,
        RankFour,
        RankFive,
        RankSix,
        RankSeven,
        RankEight,
        RankNine,
        RankTen,
        RankJack,
        RankQueen,
        RankKing,
        SkullOne,
        SkullTwo,
        SkullThree,
        BellIdle,
        BellHover,
        BellPressed,
        BellWrong,
        LeftPileArea,
        RightPileArea,
        FlipArea,
        CardSelectable,
        CardSelected,
        CardConfirmable,
        CardDisabled,
        PlayerHalliWin,
        AiHalliWin,
        PlayerHealth,
        AiHealth,
        PublicCardOpenSlot,
        PublicCardLockedSlot,
        FlipTimer,
        SelectionTimer,
        HandLocked,
        PredictionWin,
        PredictionLose,
        ResultWin,
        ResultLose,
        Bullet,
        ItemSlot
    }

    [Serializable]
    public sealed class PrototypeArtEntry
    {
        [SerializeField] private PrototypeArtKey key;
        [SerializeField] private Sprite sprite;

        public PrototypeArtEntry(PrototypeArtKey key)
        {
            this.key = key;
        }

        public PrototypeArtKey Key => key;
        public Sprite Sprite => sprite;
    }

    [CreateAssetMenu(
        fileName = "PrototypeArtCatalog",
        menuName = "Codex Game/Art/Prototype Art Catalog")]
    public sealed class PrototypeArtCatalog : ScriptableObject
    {
        [SerializeField] private List<PrototypeArtEntry> entries = new List<PrototypeArtEntry>();

        public IReadOnlyList<PrototypeArtEntry> Entries => entries;

        public bool TryGetSprite(PrototypeArtKey key, out Sprite sprite)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                PrototypeArtEntry entry = entries[index];
                if (entry != null && entry.Key == key && entry.Sprite != null)
                {
                    sprite = entry.Sprite;
                    return true;
                }
            }

            sprite = null;
            return false;
        }

#if UNITY_EDITOR
        public void EnsureAllKeys()
        {
            HashSet<PrototypeArtKey> registeredKeys = new HashSet<PrototypeArtKey>();
            for (int index = 0; index < entries.Count; index++)
            {
                PrototypeArtEntry entry = entries[index];
                if (entry != null)
                {
                    registeredKeys.Add(entry.Key);
                }
            }

            Array keys = Enum.GetValues(typeof(PrototypeArtKey));
            foreach (PrototypeArtKey key in keys)
            {
                if (registeredKeys.Add(key))
                {
                    entries.Add(new PrototypeArtEntry(key));
                }
            }
        }
#endif
    }
}
