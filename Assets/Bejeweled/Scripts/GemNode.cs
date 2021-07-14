using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.p4bloGames.Bejeweled
{
    public class GemNode : MonoBehaviour
    {
        #region SerializeFields

        [SerializeField] float swapMoveAnimTime = 0.15f;
        [SerializeField] GameObject selectedSpriteRenderer;
        [SerializeField] Color onHoverColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] AudioSource audioSource;

        #endregion

        #region Private fields

        NodesManager nodesManager;
        GemScriptableObject currentGem;
        SpriteRenderer spriteRenderer;
        bool isSelected;

        #endregion

        #region Public fields

        public System.Guid Id { get; private set; }
        public bool IsBusy { get; private set; }
        public GemType CurrentGemType => currentGem.gemType;

        #endregion

        #region MonoBehaviour methods

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            Id = System.Guid.NewGuid();
            nodesManager = GameObject.Find("Nodes").GetComponent<NodesManager>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            IsBusy = false;
            isSelected = false;
        }

        void Update()
        {
#if UNITY_EDITOR
            name = $"({transform.position.x}, {transform.position.y})";
#endif
        }

        void OnEnable()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            spriteRenderer.color = Color.white;
            SetInInitialPosition();
        }

        void OnMouseUpAsButton()
        {
            if (GameManager.Instance.CurrentGameState != GameState.Running || IsBusy)
                return;

            // If nothing is selected OR we clicked the selected Node
            if (GameManager.Instance.NodeSelected == null
                    || GameManager.Instance.NodeSelected == this)
            {
                // just toggle the state
                ToggleSelected();
            }
            else
            {
                ManageSecondSelection();
            }
        }

        void OnMouseEnter()
        {
            if (GameManager.Instance.CurrentGameState != GameState.Running)
                return;

            spriteRenderer.color = onHoverColor;

            if (GameManager.Instance.NodeDragging != null)
            {
                if (GameManager.Instance.NodeSelected != null)
                    GameManager.Instance.NodeSelected.ToggleSelected();

                GemNode nodeDragging = GameManager.Instance.NodeDragging;
                if (AreNodesAdjacent(transform.position, nodeDragging.transform.position))
                {
                    StartCoroutine(CheckMatchesAfterMoving(nodeDragging));
                    audioSource.Play();
                }

                GameManager.Instance.NodeDragging = null;
            }
        }

        void OnMouseExit()
        {
            if (GameManager.Instance.CurrentGameState == GameState.Running)
                spriteRenderer.color = Color.white;
        }

        void OnMouseDown()
        {
            if (GameManager.Instance.CurrentGameState == GameState.Running)
                GameManager.Instance.NodeDragging = this;
        }

        void OnMouseUp()
        {
            GameManager.Instance.NodeDragging = null;
        }

        #endregion

        #region Private methods

        void SetCurrentGem(GemScriptableObject currentGem)
        {
            this.currentGem = currentGem;
            spriteRenderer.sprite = currentGem.sprite;
        }

        void ToggleSelected()
        {
            isSelected = !isSelected;
            selectedSpriteRenderer.SetActive(isSelected);

            GameManager.Instance.NodeSelected = isSelected ? this : null;
        }

        void ManageSecondSelection()
        {
            GemNode nodeSelected = GameManager.Instance.NodeSelected;
            // Checks if the second selection was on the left, top, right or below the first
            if (AreNodesAdjacent(transform.position, nodeSelected.transform.position))
            {
                audioSource.Play();
                StartCoroutine(CheckMatchesAfterMoving(nodeSelected));

                GameManager.Instance.NodeSelected.ToggleSelected();
            }
            else
            {
                //  Not adjacent; change selection to the last selected (this)
                GameManager.Instance.NodeSelected.ToggleSelected();
                ToggleSelected();
            }
        }

        bool AreNodesAdjacent(Vector3 position1, Vector3 position2)
        {
            if (position1.x == position2.x
                    && (position1.y == position2.y + 1 || position1.y == position2.y - 1))
            {
                return true;
            }

            if (position1.y == position2.y
                    && (position1.x == position2.x + 1 || position1.x == position2.x - 1))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Coroutines

        IEnumerator MoveToTargetPosition(Vector3 targetPosition, float moveAnimTime)
        {
            IsBusy = true;
            float elapsedTime = 0f;
            Vector3 startingPosition = transform.position;

            while (elapsedTime < moveAnimTime)
            {
                transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / moveAnimTime);
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            transform.position = targetPosition;
            IsBusy = false;
        }

        IEnumerator CheckMatchesAfterMoving(GemNode nodeSelected)
        {
            Vector3 originalPosition = transform.position;
            Vector3 nodeSelectedPosition = nodeSelected.transform.position;

            SetTargetPosition(nodeSelectedPosition, swapMoveAnimTime);
            nodeSelected.SetTargetPosition(originalPosition, swapMoveAnimTime);

            yield return new WaitUntil(() => IsBusy == false && nodeSelected.IsBusy == false);

            if (nodesManager.CheckMatches())
            {
                // There were matches thanks to this selection
                nodesManager.RemoveMatches();

                if (GameManager.Instance.IsTimerRunning == false)
                {
                    GameManager.Instance.IsTimerRunning = true;
                }
            }
            else
            {
                SetTargetPosition(originalPosition);
                nodeSelected.SetTargetPosition(nodeSelectedPosition);
                GameManager.Instance.CurrentGameState = GameState.Running;
            }
        }

        #endregion

        #region Public methods

        public void SetCurrentGem(GemType gemType)
        {
            SetCurrentGem(GameManager.Instance.GetGemsAvaiableList().Find(g => g.gemType == gemType));
        }

        public void SetRandomGem()
        {
            SetCurrentGem(GameManager.Instance.GetGemsAvaiableList()[
                    Random.Range(0, GameManager.Instance.GetGemsAvaiableList().Count)
                ]);
        }

        public void SetInInitialPosition()
        {
            transform.position = new Vector3(transform.position.x, nodesManager.GetInitialPositionY());
        }

        public void SetTargetPosition(Vector3 targetPosition, float moveAnimTime = 0f)
        {
            if (moveAnimTime == 0f) moveAnimTime = swapMoveAnimTime;

            //this.targetPosition = targetPosition;
            StartCoroutine(MoveToTargetPosition(targetPosition, moveAnimTime));
        }

        public void RunRemoveAnimation()
        {
            IsBusy = true;
            spriteRenderer.color = new Color(1, 1, 1, 0);
            foreach (Transform child in transform)
            {
                var particles = child.GetComponent<ParticleSystem>();
                if (particles == null) continue;

                if (particles.name.StartsWith(CurrentGemType.ToString()))
                {
                    particles.gameObject.SetActive(true);
                    break;
                }
            }
        }

        #endregion
    }
}
