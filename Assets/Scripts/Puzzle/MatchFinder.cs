using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzle
{
    public class MatchFinder : MonoBehaviour
    {
        public List<ActionTile> currentMatches;
        
        private Board _board;

        private void Awake()
        {
            _board = GetComponent<Board>();
        }

        public void FindAllMatches()
        {
            currentMatches.Clear();
            
            for (var x = 0; x < _board.width; x++)
            {
                for (var y = 0; y < _board.height; y++)
                {
                    var currentActionTile = _board.ActionTiles[x, y];

                    if (currentActionTile != null)
                    {
                        if (x > 0 && x < _board.width - 1)
                        {
                            var leftActionTile = _board.ActionTiles[x - 1, y];
                            var rightActionTile = _board.ActionTiles[x + 1, y];

                            if (leftActionTile != null && rightActionTile != null)
                            {
                                if (leftActionTile.actionType == currentActionTile.actionType &&
                                    rightActionTile.actionType == currentActionTile.actionType)
                                {
                                    currentActionTile.isMatched = true;
                                    leftActionTile.isMatched = true;
                                    rightActionTile.isMatched = true;
                                    
                                    currentMatches.Add(currentActionTile);
                                    currentMatches.Add(leftActionTile);
                                    currentMatches.Add(rightActionTile);
                                }
                            }
                        }
                        
                        if (y > 0 && y < _board.height - 1)
                        {
                            var aboveActionTile = _board.ActionTiles[x, y + 1];
                            var bellowActionTile = _board.ActionTiles[x, y - 1];

                            if (aboveActionTile != null && bellowActionTile != null)
                            {
                                if (aboveActionTile.actionType == currentActionTile.actionType &&
                                    bellowActionTile.actionType == currentActionTile.actionType)
                                {
                                    currentActionTile.isMatched = true;
                                    aboveActionTile.isMatched = true;
                                    bellowActionTile.isMatched = true;
                                    
                                    currentMatches.Add(currentActionTile);
                                    currentMatches.Add(aboveActionTile);
                                    currentMatches.Add(bellowActionTile);
                                }
                            }
                        }
                    }
                }
            }

            if (currentMatches.Count > 0)
            {
                currentMatches = currentMatches.Distinct().ToList();
            }
        }
    }
}
