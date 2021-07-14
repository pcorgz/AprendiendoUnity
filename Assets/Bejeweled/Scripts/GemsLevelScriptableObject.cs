using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.Bejeweled
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Bejeweled/Create new Level", order = 1)]
    public class GemsLevelScriptableObject : ScriptableObject
    {
        public List<GemLevel> LevelGemsList;
    }

    [System.Serializable]
    public class GemLevel
    {
        public GemType GemType;
        public Vector2 Position;
    }
}
