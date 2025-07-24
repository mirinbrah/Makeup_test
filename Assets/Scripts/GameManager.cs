using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Этапы Игры")]
    [SerializeField] private GamePhase currentPhase;

    [Header("Объекты сцены")]
    public HandController hand; // << ОСТАЛАСЬ ТОЛЬКО ОДНА ССЫЛКА
    public GameObject acneSprite;
    public Collider2D faceZone;

    private bool isBusy = false;
    private bool targetReached = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentPhase = GamePhase.Acne;
    }

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy || item.itemPhase != currentPhase) return;
        StartCoroutine(PickupAndPrepareSequence(item));
    }

    public void OnItemReachedTargetZone()
    {
        if (!hand.isDraggable) return;
        targetReached = true;
    }

    public void OnItemLeftTargetZone()
    {
        targetReached = false;
    }

    public void OnDragEnded()
    {
        if (!hand.isDraggable) return;

        if (targetReached)
        {
            hand.isDraggable = false;
            StartCoroutine(ApplyAndReturnSequence());
        }
        else
        {
            hand.isDraggable = false;
            hand.MoveTo(hand.GetAttachedItem().dragStartPosition.position, () => {
                hand.isDraggable = true;
            });
        }
        targetReached = false;
    }

    private IEnumerator PickupAndPrepareSequence(ClickableItem item)
    {
        isBusy = true;
        hand.isDraggable = false;
        targetReached = false;

        bool finishedMove = false;
        hand.MoveTo(item.transform.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        hand.AttachItem(item);

        finishedMove = false;
        hand.MoveTo(item.dragStartPosition.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        hand.isDraggable = true;
    }

    private IEnumerator ApplyAndReturnSequence()
    {
        // Получаем аниматор с того же объекта, где и контроллер
        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator == null)
        {
            Debug.LogError("На объекте руки отсутствует компонент HandAnimator!");
            isBusy = false;
            yield break;
        }

        bool animationFinished = false;
        StartCoroutine(handAnimator.AnimateApplyAndReturn(() => {
            animationFinished = true;
        }));
        yield return new WaitUntil(() => animationFinished);

        if (currentPhase == GamePhase.Acne && acneSprite != null)
        {
            acneSprite.SetActive(false);
        }

        AdvanceToNextPhase();

        isBusy = false;
    }

    private void AdvanceToNextPhase()
    {
        int nextPhaseIndex = (int)currentPhase + 1;
        if (nextPhaseIndex < System.Enum.GetValues(typeof(GamePhase)).Length)
        {
            currentPhase = (GamePhase)nextPhaseIndex;
        }
        else
        {
            Debug.Log("Все этапы завершены!");
        }
    }
}