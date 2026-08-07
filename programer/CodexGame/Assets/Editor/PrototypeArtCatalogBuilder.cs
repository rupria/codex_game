using System;
using System.Collections.Generic;
using CodexGame.Presentation.Art;
using UnityEditor;
using UnityEngine;

namespace CodexGame.Editor
{
    internal static class PrototypeArtCatalogBuilder
    {
        private const string CatalogAssetPath = "Assets/Art/Prototype/PrototypeArtCatalog.asset";
        private const string CardComponentRoot = "Assets/Art/Prototype/Cards/components/";

        private static readonly IReadOnlyDictionary<PrototypeArtKey, string> CardSpritePaths =
            new Dictionary<PrototypeArtKey, string>
            {
                { PrototypeArtKey.CardFrontBase, CardComponentRoot + "card_front_base.png" },
                { PrototypeArtKey.CardBack, CardComponentRoot + "card_back.png" },
                { PrototypeArtKey.SuitSpade, CardComponentRoot + "suit_spades.png" },
                { PrototypeArtKey.SuitDiamond, CardComponentRoot + "suit_diamonds.png" },
                { PrototypeArtKey.SuitHeart, CardComponentRoot + "suit_hearts.png" },
                { PrototypeArtKey.SuitClub, CardComponentRoot + "suit_clubs.png" },
                { PrototypeArtKey.RankAce, CardComponentRoot + "rank_a.png" },
                { PrototypeArtKey.RankTwo, CardComponentRoot + "rank_2.png" },
                { PrototypeArtKey.RankThree, CardComponentRoot + "rank_3.png" },
                { PrototypeArtKey.RankFour, CardComponentRoot + "rank_4.png" },
                { PrototypeArtKey.RankFive, CardComponentRoot + "rank_5.png" },
                { PrototypeArtKey.RankSix, CardComponentRoot + "rank_6.png" },
                { PrototypeArtKey.RankSeven, CardComponentRoot + "rank_7.png" },
                { PrototypeArtKey.RankEight, CardComponentRoot + "rank_8.png" },
                { PrototypeArtKey.RankNine, CardComponentRoot + "rank_9.png" },
                { PrototypeArtKey.RankTen, CardComponentRoot + "rank_10.png" },
                { PrototypeArtKey.RankJack, CardComponentRoot + "rank_j.png" },
                { PrototypeArtKey.RankQueen, CardComponentRoot + "rank_q.png" },
                { PrototypeArtKey.RankKing, CardComponentRoot + "rank_k.png" },
                { PrototypeArtKey.SkullOne, CardComponentRoot + "skull_01.png" },
                { PrototypeArtKey.SkullTwo, CardComponentRoot + "skull_02.png" },
                { PrototypeArtKey.SkullThree, CardComponentRoot + "skull_03.png" }
            };

        [MenuItem("Codex Game/Art/Build Prototype Art Catalog")]
        private static void Build()
        {
            PrototypeArtCatalog catalog = AssetDatabase.LoadAssetAtPath<PrototypeArtCatalog>(CatalogAssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PrototypeArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
            }

            catalog.EnsureAllKeys();
            SerializedObject serializedCatalog = new SerializedObject(catalog);
            SerializedProperty entries = serializedCatalog.FindProperty("entries");
            if (entries == null)
            {
                throw new InvalidOperationException("PrototypeArtCatalog.entries was not found.");
            }

            int assignedCount = 0;
            for (int index = 0; index < entries.arraySize; index++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(index);
                SerializedProperty keyProperty = entry.FindPropertyRelative("key");
                SerializedProperty spriteProperty = entry.FindPropertyRelative("sprite");
                PrototypeArtKey key = (PrototypeArtKey)keyProperty.enumValueIndex;

                if (!CardSpritePaths.TryGetValue(key, out string spritePath))
                {
                    continue;
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite == null)
                {
                    throw new InvalidOperationException("Card sprite was not found or was not imported as Sprite: " + spritePath);
                }

                spriteProperty.objectReferenceValue = sprite;
                assignedCount++;
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Prototype art catalog card entries assigned: " + assignedCount + "/" + CardSpritePaths.Count);
        }

        public static void BuildForBatch()
        {
            Build();
        }

        [MenuItem("Codex Game/Art/Validate Prototype Card Sprites")]
        private static void ValidateCardSprites()
        {
            List<string> missing = new List<string>();
            foreach (KeyValuePair<PrototypeArtKey, string> mapping in CardSpritePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(mapping.Value) == null)
                {
                    missing.Add(mapping.Key + " -> " + mapping.Value);
                }
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException("Missing card sprites:\n" + string.Join("\n", missing));
            }

            Debug.Log("Prototype card sprite validation passed: " + CardSpritePaths.Count + " component sprites.");
        }
    }
}
