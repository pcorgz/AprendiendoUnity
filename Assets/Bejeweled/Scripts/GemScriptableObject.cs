using System;
using UnityEngine;

namespace com.p4bloGames.Bejeweled
{
    [CreateAssetMenu(fileName = "New Gem", menuName = "Bejeweled/Create new Gem", order = 0)]
    public class GemScriptableObject : ScriptableObject
    {
        public GemType gemType;
        public Sprite sprite;
    }

    public enum GemType
    {
        BLUE = 1, GREEN, ORANGE, PURPLE, RED, YELLOW
    }
}
