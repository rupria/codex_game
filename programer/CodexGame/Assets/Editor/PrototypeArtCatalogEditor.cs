using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CodexGame.Presentation.Art;
using UnityEditor;
using UnityEngine;

namespace CodexGame.Editor
{
    public static class PrototypeArtCatalogEditor
    {
        private const string ArtRoot = "Assets/Art/Prototype/";
        private const string CatalogFolder = "Assets/Art/Prototype";
        private const string CatalogPath = CatalogFolder + "/PrototypeArtCatalog.asset";

        private static readonly Regex SnakeCaseName =
            new Regex("^[a-z0-9]+(?:_[a-z0-9]+)*$", RegexOptions.Compiled);

        private static readonly PrototypeArtKey[] RequiredP0Keys =
        {
            PrototypeArtKey.CardFrontBase,
            PrototypeArtKey.CardBack,
            PrototypeArtKey.SuitSpade,
            PrototypeArtKey.SuitDiamond,
            PrototypeArtKey.SuitHeart,
            PrototypeArtKey.SuitClub,
            PrototypeArtKey.RankAce,
            PrototypeArtKey.RankTwo,
            PrototypeArtKey.RankThree,
            PrototypeArtKey.RankFour,
            PrototypeArtKey.RankFive,
            PrototypeArtKey.RankSix,
            PrototypeArtKey.RankSeven,
            PrototypeArtKey.RankEight,
            PrototypeArtKey.RankNine,
            PrototypeArtKey.RankTen,
            PrototypeArtKey.RankJack,
            PrototypeArtKey.RankQueen,
            PrototypeArtKey.RankKing,
            PrototypeArtKey.SkullOne,
            PrototypeArtKey.SkullTwo,
            PrototypeArtKey.SkullThree,
            PrototypeArtKey.BellIdle,
            PrototypeArtKey.BellHover,
            PrototypeArtKey.BellPressed,
            PrototypeArtKey.BellWrong,
            PrototypeArtKey.LeftPileArea,
            PrototypeArtKey.RightPileArea,
            PrototypeArtKey.FlipArea,
            PrototypeArtKey.CardSelectable,
            PrototypeArtKey.CardSelected,
            PrototypeArtKey.CardConfirmable,
            PrototypeArtKey.CardDisabled,
            PrototypeArtKey.PlayerHalliWin,
            PrototypeArtKey.AiHalliWin,
            PrototypeArtKey.PlayerHealth,
            PrototypeArtKey.AiHealth,
            PrototypeArtKey.PublicCardOpenSlot,
            PrototypeArtKey.PublicCardLockedSlot,
            PrototypeArtKey.FlipTimer,
            PrototypeArtKey.SelectionTimer
        };

        [MenuItem("Tools/Codex Game/Art/Create or Refresh Prototype Catalog")]
        public static void CreateOrRefreshCatalog()
        {
            PrototypeArtCatalog catalog = EnsureCatalog();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log("Prototype art catalog is ready at " + CatalogPath + ".", catalog);
        }

        public static void CreateOrRefreshCatalogForBatch()
        {
            PrototypeArtCatalog catalog = EnsureCatalog();
            Debug.Log("Prototype art catalog batch setup completed at " + CatalogPath + ".", catalog);
        }

        [MenuItem("Tools/Codex Game/Art/Validate Prototype Catalog")]
        public static void ValidateCatalog()
        {
            PrototypeArtCatalog catalog = AssetDatabase.LoadAssetAtPath<PrototypeArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogWarning(
                    "Prototype art catalog is missing. Run Tools/Codex Game/Art/Create or Refresh Prototype Catalog.");
                return;
            }

            Dictionary<PrototypeArtKey, PrototypeArtEntry> entries =
                new Dictionary<PrototypeArtKey, PrototypeArtEntry>();
            int issueCount = 0;

            foreach (PrototypeArtEntry entry in catalog.Entries)
            {
                if (entry == null)
                {
                    issueCount++;
                    Debug.LogWarning("Prototype art catalog contains an empty entry.", catalog);
                    continue;
                }

                if (entries.ContainsKey(entry.Key))
                {
                    issueCount++;
                    Debug.LogWarning("Duplicate prototype art key: " + entry.Key + ".", catalog);
                    continue;
                }

                entries.Add(entry.Key, entry);
                if (entry.Sprite == null)
                {
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(entry.Sprite).Replace('\\', '/');
                if (!assetPath.StartsWith(ArtRoot, System.StringComparison.OrdinalIgnoreCase))
                {
                    issueCount++;
                    Debug.LogWarning(
                        entry.Key + " must reference a sprite under " + ArtRoot + ": " + assetPath,
                        entry.Sprite);
                }

                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                if (!SnakeCaseName.IsMatch(fileName))
                {
                    issueCount++;
                    Debug.LogWarning(
                        "Prototype art file names must use lowercase snake_case: " + assetPath,
                        entry.Sprite);
                }
            }

            foreach (PrototypeArtKey key in RequiredP0Keys)
            {
                PrototypeArtEntry entry;
                if (!entries.TryGetValue(key, out entry) || entry.Sprite == null)
                {
                    issueCount++;
                    Debug.LogWarning("Required P0 prototype art is not assigned: " + key + ".", catalog);
                }
            }

            if (issueCount == 0)
            {
                Debug.Log("Prototype art catalog validation passed.", catalog);
            }
            else
            {
                Debug.LogWarning(
                    "Prototype art catalog validation found " + issueCount +
                    " issue(s). Missing art is expected until the first handoff.",
                    catalog);
            }
        }

        private static PrototypeArtCatalog EnsureCatalog()
        {
            if (!AssetDatabase.IsValidFolder(CatalogFolder))
            {
                throw new System.InvalidOperationException(
                    "Prototype art root is missing: " + CatalogFolder + ".");
            }

            PrototypeArtCatalog catalog = AssetDatabase.LoadAssetAtPath<PrototypeArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PrototypeArtCatalog>();
                catalog.EnsureAllKeys();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            else
            {
                catalog.EnsureAllKeys();
                EditorUtility.SetDirty(catalog);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }
    }
}
