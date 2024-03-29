using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Puzzle
{
    public enum BoardState
    {
        Ready,
        Action
    }
    
    public class Board : MonoBehaviour
    {
        public int width;
        public int height;

        public BoardState boardState = BoardState.Ready;

        [SerializeField] private ActionTile[] actionTilesPrefabs;

        public ActionTile[,] ActionTiles;
        public float actionTileSpeed;

        public MatchFinder matchFinder;

        private void Awake()
        {
            matchFinder = GetComponent<MatchFinder>();
        }

        private void Start()
        {
            ActionTiles = new ActionTile[width, height];
        
            SpawnTiles();
        }
        
        private void SpawnTiles()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var randomActionTile = Random.Range(0, actionTilesPrefabs.Length);
                    
                    var iterationLimiter = 0;
                    while (CheckMatchAt(new Vector2Int(x, y), actionTilesPrefabs[randomActionTile]) && iterationLimiter < 100)
                    {
                        randomActionTile = Random.Range(0, actionTilesPrefabs.Length);
                        iterationLimiter ++;
                    }

                    if (iterationLimiter >= 100)
                    {
                        Debug.LogError("Iterator is out of range");
                    }
                
                    SpawnTile(new Vector2Int(x, y), actionTilesPrefabs[randomActionTile]);
                }
            }
        }

        private void SpawnTile(Vector2Int positionToSpawn, ActionTile actionTileToSpawn)
        {
            var spawnedActionTile = Instantiate(actionTileToSpawn, new Vector3(positionToSpawn.x, positionToSpawn.y + height, 0.0f), 
                Quaternion.identity, transform);
            
            spawnedActionTile.name = "Tile - " + positionToSpawn.x + ", " + positionToSpawn.y;
        
            ActionTiles[positionToSpawn.x, positionToSpawn.y] = spawnedActionTile;
            spawnedActionTile.SetupGem(positionToSpawn, this);
        }

        private bool CheckMatchAt(Vector2Int positionToCheck, ActionTile tileToCheck)
        {
            if (positionToCheck.x > 1)
            {
                if (ActionTiles[positionToCheck.x - 1, positionToCheck.y].actionType == tileToCheck.actionType && 
                    ActionTiles[positionToCheck.x - 2, positionToCheck.y].actionType == tileToCheck.actionType)
                {
                    return true;
                }
            }
            
            if (positionToCheck.y > 1)
            {
                if (ActionTiles[positionToCheck.x, positionToCheck.y - 1].actionType == tileToCheck.actionType && 
                    ActionTiles[positionToCheck.x, positionToCheck.y - 2].actionType == tileToCheck.actionType)
                {
                    return true;
                }
            }

            return false;
        }

        private void DestroyMatchedTilesAt(Vector2Int position)
        {
            if (ActionTiles[position.x, position.y] != null)
            {
                if (ActionTiles[position.x, position.y].isMatched)
                {
                    Destroy(ActionTiles[position.x, position.y].gameObject);
                    ActionTiles[position.x, position.y] = null;
                }
            }
        }

        public void DestroyMatches()
        {
            foreach (var matchedTile in matchFinder.currentMatches)
            {
                if (matchedTile != null)
                {
                    DestroyMatchedTilesAt(matchedTile.position);
                }
            }

            StartCoroutine(DropRow());
        }

        private IEnumerator DropRow()
        {
            yield return new WaitForSeconds(.2f);

            var nullCounter = 0;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (ActionTiles[x, y] == null)
                    {
                        nullCounter++;
                    }
                    else if (nullCounter > 0)
                    {
                        ActionTiles[x, y].position.y -= nullCounter;
                        ActionTiles[x, y - nullCounter] = ActionTiles[x, y];
                        ActionTiles[x, y] = null;
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FillBoard());
        }

        private IEnumerator FillBoard()
        {
            yield return new WaitForSeconds(0.5f);
            
            RefillBoard();
            
            yield return new WaitForSeconds(0.5f);
            
            matchFinder.FindAllMatches();

            if (matchFinder.currentMatches.Count > 0)
            {
                yield return new WaitForSeconds(0.5f);
                DestroyMatches();
            }
            else
            {
                yield return new WaitForSeconds(0.5f);

                boardState = BoardState.Ready;
            }
        }

        private void RefillBoard()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (ActionTiles[x, y] != null) continue;
                    
                    var randomActionTile = Random.Range(0, actionTilesPrefabs.Length);
                    
                    SpawnTile(new Vector2Int(x, y), actionTilesPrefabs[randomActionTile]);
                }
            }
            
            CheckMisplacedTiles();
        }
        
        private void CheckMisplacedTiles()
        {
            var foundTiles = new List<ActionTile>();
            
            foundTiles.AddRange(FindObjectsOfType<ActionTile>());

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (foundTiles.Contains(ActionTiles[x, y]))
                    {
                        foundTiles.Remove(ActionTiles[x, y]);
                    }
                }
            }

            foreach (var tile in foundTiles)
            {
                Destroy(tile.gameObject);
            }
        }
    }
}
