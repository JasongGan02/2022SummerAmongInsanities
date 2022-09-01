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
        public static string PICK_UP_DETECTOR = "PickUpDetector";
        public static string INVENTORY_UI = "InventoryUI";
        public static string INVENTORY_GRID = "InventoryGrid";
    }


    public enum TowerType{
        typeOne,
        typeTwo,
        typeThree,
        typeFour,
        noShadow
    };
}
