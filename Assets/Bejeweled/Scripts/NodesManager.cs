using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.p4bloGames.Bejeweled
{
    public class NodesManager : MonoBehaviour
    {
        #region SerializeFields

        [SerializeField] int sizeX = 9;
        [SerializeField] int sizeY = 9;
        [SerializeField] int initialPositionY = 12;
        [SerializeField] GameObject gemPrefab;
        [SerializeField] float enableMoveAnimTime = 1.5f;
        [SerializeField] float removeAnimTime = 1f;
        [SerializeField] int gemValue = 100;
        [SerializeField] float gemsMultiplier = 0.25f;
        [SerializeField] float maxMultiplier = 2f;

        [SerializeField] GemsLevelScriptableObject LevelSO;

        #endregion

        #region Private fields

        List<GemNode> gemNodes;
        List<List<GemNode>> listOfMatches;

        AudioSource audioSource;

        #endregion

        #region MonoBehaviour methods

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            gemNodes = new List<GemNode>();
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    var gemObj = Instantiate(gemPrefab, new Vector3(i, j), Quaternion.identity, transform);
                    GemNode gemNode = gemObj.GetComponent<GemNode>();

                    gemNode.SetTargetPosition(new Vector3(i, j), enableMoveAnimTime);
                    gemNodes.Add(gemNode);
                }
            }

            if (LevelSO == null)
            {
                SetRandomNodes();
            }
            else
            {
                SetNodesFromLevel();
            }

            GameManager.Instance.IsTimerRunning = false;
        }

        #endregion

        #region Private methods

        void SetRandomNodes()
        {
            foreach (GemNode gemNode in gemNodes)
            {
                gemNode.SetRandomGem();
            }
        }

        void SetNodesFromLevel()
        {
            for (int i = 0; i < LevelSO.LevelGemsList.Count; i++)
            {
                GemLevel gemLevel = LevelSO.LevelGemsList[i];
                GemNode gemNode = gemNodes[i];

                gemNode.SetCurrentGem(gemLevel.GemType);
            }
        }

        /// <summary>
        /// directions = 0: up - 1: down - 2: left - 3: right
        /// </summary>
        /// <param name="gemNode"></param>
        /// <param name="availableGemNodes"></param>
        /// <param name="direction"></param>
        List<GemNode> FindMatches(GemNode gemNode, Vector3 direction, List<GemNode> matchList = null)
        {
            // Next gem to compare
            Vector3 posToCompare = gemNode.transform.position + direction;
            if (posToCompare.x < 0 || posToCompare.y < 0)
            {
                return matchList ?? new List<GemNode>(); // out of bounds
            }

            // If nothing found, then foundGemNode will be Null
            GemNode foundGemNode = gemNodes.FirstOrDefault(g => g.transform.position == posToCompare);
            //GemNode foundGemNode = availableGemNodes.FirstOrDefault(g => g.transform.position == posToCompare);

            // If found something, check if they are the same CurrentGemType
            if (foundGemNode != null && foundGemNode.CurrentGemType == gemNode.CurrentGemType)
            {
                // First match
                if (matchList == null)
                {
                    // Initialize and add the first GemNode
                    matchList = new List<GemNode> { gemNode };
                }

                matchList.Add(foundGemNode);
                // Recursively find the next match
                matchList = FindMatches(foundGemNode, direction, matchList);

                // If the first recursive match is Not the same type, then there are not enough matches
                if (matchList.Count < 3)
                {
                    matchList.Clear(); // not enough matches
                }
                //else
                //{
                //    availableGemNodes.RemoveAll(m => matchList.Select(n => n.Id).Contains(m.Id));
                //}
            }

            return matchList ?? new List<GemNode>();
        }

        #endregion

        #region Coroutines

        IEnumerator RemoveAnimation(GemNode gemNode)
        {
            gemNode.RunRemoveAnimation();

            yield return new WaitForSeconds(removeAnimTime);
            gemNode.SetInInitialPosition();
            gemNode.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.15f);

            gemNode.gameObject.SetActive(true);
            gemNode.SetRandomGem();
            GameManager.Instance.CurrentGameState = GameState.Busy;
        }

        IEnumerator MoveNodesDown(List<GemNode> allMatches)
        {
            yield return new WaitUntil(() => GameManager.Instance.CurrentGameState != GameState.RemovingNodes);

            for (int col = 0; col < sizeX; col++)
            {
                for (int row = 0; row < sizeY; row++)
                {
                    Vector3 nextPosition = new Vector3(col, row);
                    GemNode gemNode = FindNodeViaPosition(nextPosition);

                    // has a gem on it
                    if (gemNode != null) continue; // keep looking up

                    // Found first null cell for this column
                    List<Vector3> positions = new List<Vector3>();
                    List<GemNode> gemsToMove = new List<GemNode>();

                    for (int y = (int)nextPosition.y; y < sizeY; y++)
                    {
                        positions.Add(nextPosition);
                        nextPosition += Vector3.up;

                        GemNode nextGem = FindNodeViaPosition(nextPosition);
                        if (nextGem != null) gemsToMove.Add(nextGem);
                    }

                    var matchesInCol = allMatches.Where(a => a.transform.position.x == nextPosition.x).ToList();
                    gemsToMove = gemsToMove.Union(matchesInCol).ToList();

                    for (int i = 0; i < gemsToMove.Count; i++)
                    {
                        gemsToMove[i].SetTargetPosition(positions[i]);
                    }
                    // break to the next column
                    break;
                }
            }
        }

        IEnumerator CheckAnimationsDone()
        {
            yield return new WaitUntil(() =>
            {
                return gemNodes.Any(g => g.IsBusy) == false;
            });

            if (CheckMatches())
            {
                RemoveMatches();
            }
            else
            {
                GameManager.Instance.CurrentGameState = GameState.Running;
            }
        }

        #endregion

        #region Public methods

        public List<GemNode> GetGemNodes()
        {
            return gemNodes;
        }

        public int GetInitialPositionY()
        {
            return initialPositionY;
        }

        public GemNode FindNodeViaPosition(Vector3 positionToFind)
        {
            var found = gemNodes.FirstOrDefault(g => g.gameObject.transform.position == positionToFind);
            return found;
        }

        public bool CheckMatches()
        {
            GameManager.Instance.CurrentGameState = GameState.Busy;

            bool foundAnyMatch = false;
            //availableGemNodes = new List<GemNode>(gemNodes);
            listOfMatches = new List<List<GemNode>>();

            foreach (var gemNode in gemNodes)
            {
                //// If this gemNode is already been added to a matchList, continue to the next
                //if (availableGemNodes.Any(g => g.Id == gemNode.Id) == false)
                //    continue;

                // Find all matches in all four directions
                List<GemNode> upMatchList = FindMatches(gemNode, Vector3.up);
                List<GemNode> downMatchList = FindMatches(gemNode, Vector3.down);
                List<GemNode> leftMatchList = FindMatches(gemNode, Vector3.left);
                List<GemNode> rightMatchList = FindMatches(gemNode, Vector3.right);

                // Merge all matchLists, avoiding duplicates
                var matchList = upMatchList.Union(downMatchList).Union(leftMatchList).Union(rightMatchList).ToList();
                if (matchList.Count >= 3)
                {
                    listOfMatches.Add(matchList);
                    //// remove matches from availables list
                    //availableGemNodes.RemoveAll(a => matchList.Select(m => m.Id).Contains(a.Id));

                    foundAnyMatch = true;
                }
            }
            return foundAnyMatch;
        }

        public void RemoveMatches()
        {
            int score = 0; // TODO: DEBUG ONLY; REMOVE

            List<GemNode> allMatches = new List<GemNode>();
            GameManager.Instance.CurrentGameState = GameState.RemovingNodes;

            foreach (List<GemNode> matches in listOfMatches)
            {
                allMatches = allMatches.Union(matches).ToList();
                // match 3 = 1; every number extra adds extra points
                float multiplier = 1 + (gemsMultiplier * (matches.Count - 3));
                multiplier = Mathf.Clamp(multiplier, 1f, maxMultiplier);

                int scoreToAdd = (int)(matches.Count * gemValue * multiplier);
                GameManager.Instance.AddToScore(scoreToAdd);

                score += scoreToAdd; // TODO: DEBUG ONLY; REMOVE

                foreach (var gemNode in matches)
                {
                    StartCoroutine(RemoveAnimation(gemNode));
                }

                audioSource.Play();
            }

            StartCoroutine(MoveNodesDown(allMatches));
            StartCoroutine(CheckAnimationsDone());
        }

        #endregion
    }
}
