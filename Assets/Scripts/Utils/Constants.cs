using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static class Layer
    {
        public static int DEFAULT = 0;
        public static int GROUND = 6;
    }

    public static class Tag
    {
        public static string GROUND = "ground";
    }

    public static class Animator
    {
        public static string SPEED = "speed";
        public static string IN_AIR = "inAir";
    }

    public enum TowerType{
        typeOne,
        typeTwo,
        typeThree,
        typeFour,
        noShadow
    };
}
