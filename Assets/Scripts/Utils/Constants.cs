using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static class Layer
    {
        public static int DEFAULT = 0;
        public static int GROUND = 6;
        public static int RESOURCE = 7;
    }

    public static class Tag
    {
        public static string GROUND = "ground";
        public static string PLAYER = "Player";
        public static string RESOURCE = "resource";
        public static string SPAWN = "Spawn";
    }

    public static class Animator
    {
        public static string SPEED = "speed";
        public static string IN_AIR = "inAir";
        public static string MELEE_TOOL = "MeleeTool";
    }

    public static class Name
    {
        public static string PLAYER = "Player";
        public static string CANVAS = "Canvas";

        public static string PICK_UP_DETECTOR = "PickUpDetector";
        public static string INVENTORY_GRID = "InventoryUI";
        public static string CHESTINVENTORY_GRID = "ChestInventoryUI";
        public static string WEAPONINVENTORY_GRID = "WeaponInventoryUI";
        public static string HEALTH_BAR = "HealthBarUI";
        public static string BACKGROUND = "BG";
        public static string SUN = "Sun";
        public static string MOON = "Moon";
        public static string RED_MOON = "RedMoon";
        public static string BACKGROUND_LIGHT = "BackgroundLight";
        public static string TIME_TEXT = "TimeText";
        public static string CALENDAR_TEXT = "CalendarText";

        public static string CURRENT_ITEM_UI = "Icon";
    }


    public static CharacterAtlas character;
}
