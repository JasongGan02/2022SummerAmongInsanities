using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu(menuName = "VinTools/Custom Tiles/Advanced Rule Tile")]
public class NewCustomRuleTile : RuleTile<NewCustomRuleTile.Neighbor>
{
    public TileBase[] Specified;
    public TileBase[] Any;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Any = 3;
        public const int Specified = 4;
        public const int NotSpecified = 5;
        public const int Nothing = 6;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This: return CheckThis(tile);
            case Neighbor.NotThis: return CheckNotThis(tile);
            case Neighbor.Any: return CheckAny(tile);
            case Neighbor.Specified: return CheckSpecified(tile);
            case Neighbor.NotSpecified: return CheckNotSpecified(tile);
            case Neighbor.Nothing: return CheckNothing(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }

    private bool CheckNothing(TileBase tile)
    {
        return tile is null;
    }

    private bool CheckSpecified(TileBase tile)
    {
        return Specified.Contains(tile);
    }


    private bool CheckAny(TileBase tile)
    {
        return Any.Contains(tile);
    }

    private bool CheckNotSpecified(TileBase tile)
    {
        return !Specified.Contains(tile);
    }

    private bool CheckNotThis(TileBase tile)
    {
        return tile != this;
    }

    private bool CheckThis(TileBase tile)
    {
        return tile == this;
    }
}