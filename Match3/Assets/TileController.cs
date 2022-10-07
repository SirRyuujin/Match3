using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    public Gem Gem { get; private set; }

    [Header("References")]
    public GemTypesHolder GemTypes;
    public GemCollections GemCollections;
    public GemType SpecialGemType;
    public GemType NormalGemType;
    public Image GemSpriteHolder;

    [Header("Preview")]
    public TileController[] Neighbours = new TileController[4];
    [SerializeField] private int x;
    [SerializeField] private int y; 

    private BoardController _boardController;

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    public void AssignGem(Gem gem)
    {
        Gem = gem;
        RenderGemSprtieOnTile();
    }

    private void RenderGemSprtieOnTile()
    {
        GemSpriteHolder.sprite = Gem.Sprite;
    }

    public Gem GetGemOfRandomType(int chanceForSpecialType)
    {
        Gems gems = GetGemCollectionOfType(NormalGemType);
        if (Random.Range(1, 101) <= chanceForSpecialType)
            gems = GetGemCollectionOfType(SpecialGemType);

        return GetRandomGemFromCollection(gems);
    }
     
    public Gem GetRandomGemFromCollection(Gems collection)
    {
        return collection.GemsCollection[Random.Range(0, collection.GemsCollection.Length)];
    }

    public Gems GetGemCollectionOfType(GemType type)
    {
        for (int i = 0; i < GemCollections.AllCollections.Length; i++)
            if (GemCollections.AllCollections[i].Type == type)
                return GemCollections.AllCollections[i];

        return null;
    }

    public void SetRandomizedGemType(int chanceToSpawnSpecial)
    {
      //  _gem = (Random.Range(1, 101) <= chanceToSpawnSpecial) ? GemType.Special : GemType.Normal;
      
    }

    public void SetPosition(Vector2 position)
    {
        //_position = position;
        x = (int)position.x;
        y = (int)position.y;
    }

    public void SetRandomSprite()
    {
        // track current gem per row
        // count them if they're in a succession; no more than MIN_FOR_MATCH - 1
        // additionally check bottom top and add them in to the count if there're a match
        // if count is up: decrease spawn pool

        // check neighbours:
        // if neighbour is of the same type: add count and check his neighbours

        //_gemImage.sprite = (_gem is GemType.Normal)
        //? NormalGemSprites.Sprites[Random.Range(0, NormalGemSprites.Sprites.Length)]
        //: SpecialGemSprites.Sprites[Random.Range(0, SpecialGemSprites.Sprites.Length)];
    }

    public void SetBoardControllerReference(BoardController boardController)
    {
        _boardController = boardController;
    }
}