using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Puzzle
{
    public enum ActionType
    {
        Bow,
        Key,
        Shield,
        Sword,
        MagicWand
    }
    
    public class ActionTile : MonoBehaviour
    {
        public ActionType actionType;
        
        public Vector2Int position;
        private Board _board;

        private Vector2 _startedTouchPosition;
        private Vector2 _endedTouchPosition;

        private bool _isMousePressed;
        private float _moveAngle;

        private ActionTile _tileToMove;
        public bool isMatched;

        private Vector2Int _previousPosition;

        public void SetupGem(Vector2Int position, Board board)
        {
            this.position = position;
            _board = board;
        }

        private void Update()
        {
            if (Vector2.Distance(transform.position, position) > .01f)
                transform.position = Vector2.Lerp(transform.position, position, _board.actionTileSpeed * Time.deltaTime);
            else
            {
                transform.position = new Vector3(position.x, position.y);
                _board.ActionTiles[position.x, position.y] = this;
            }
        }

        private void OnMouseDown()
        {
            _startedTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _isMousePressed = true;
        }

        private void OnMouseUp()
        {
            if (_isMousePressed && Input.GetMouseButtonUp(0))
            {
                _isMousePressed = false;
                _endedTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }

        private void CalculateAngle()
        {
            _moveAngle = Mathf.Atan2(_endedTouchPosition.y - _startedTouchPosition.y,
                _endedTouchPosition.x - _startedTouchPosition.x);

            _moveAngle *= 180 / Mathf.PI;
        
            if (Vector3.Distance(_startedTouchPosition, _endedTouchPosition) > .5f)
                MoveActionTile();
        }

        private void MoveActionTile()
        {
            _previousPosition = position;
            
            if (_moveAngle is < 45f and > -45f && position.x < _board.width - 1)
            {
                _tileToMove = _board.ActionTiles[position.x + 1, position.y];
                _tileToMove.position.x--;
                position.x++;
            }
            else if (_moveAngle is < 135f and > 45f && position.y < _board.height - 1)
            {
                _tileToMove = _board.ActionTiles[position.x, position.y + 1];
                _tileToMove.position.y--;
                position.y++;
            }
            else if (_moveAngle is < -45f and >= -135f && position.y > 0)
            {
                _tileToMove = _board.ActionTiles[position.x, position.y - 1];
                _tileToMove.position.y++;
                position.y--;
            }
            else if (_moveAngle is < -135f or >= 135f && position.x > 0)
            {
                _tileToMove = _board.ActionTiles[position.x - 1, position.y];
                _tileToMove.position.x++;
                position.x--;
            }
            else
            {
                return;
            }

            _board.ActionTiles[position.x, position.y] = this;
            _board.ActionTiles[_tileToMove.position.x, _tileToMove.position.y] = _tileToMove;

            StartCoroutine(CheckMove());
        }

        private IEnumerator CheckMove()
        {
            yield return new WaitForSeconds(0.5f);
            
            _board.matchFinder.FindAllMatches();

            if (_tileToMove != null)
            {
                if (!isMatched && !_tileToMove.isMatched)
                {
                    _tileToMove.position = position;
                    position = _previousPosition;
                    
                    _board.ActionTiles[position.x, position.y] = this;
                    _board.ActionTiles[_tileToMove.position.x, _tileToMove.position.y] = _tileToMove;
                }
                else
                {
                    _board.DestroyMatches();
                }
            }
        }
    }
}
