using UnityEngine;
using System.Collections;
using System; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GamePhase, GameState> OnPhaseStateChanged;

    [Header("Этапы Игры")]
    [SerializeField] private GamePhase currentPhase;

    private GameState currentState;

    [Header("Объекты сцены")]
    public HandController hand;
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
        ChangeState(GameState.Idle); 
    }

    private void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        OnPhaseStateChanged?.Invoke(currentPhase, currentState);
    }

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy || item.itemPhase != currentPhase) return;
        StartCoroutine(PickupAndPrepareSequence(item));
    }

    public void OnItemReachedTargetZone()
    {
        if (currentState != GameState.PlayerControl) return;
        targetReached = true;
    }

    public void OnItemLeftTargetZone()
    {
        targetReached = false;
    }

    public void OnDragEnded()
    {
        if (currentState != GameState.PlayerControl) return;

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
                ChangeState(GameState.PlayerControl); // Возвращаемся в состояние контроля
            });
        }
        targetReached = false;
    }

    // --- ПУБЛИЧНЫЙ МЕТОД ДЛЯ UIManager ---
    public void PerformReset()
    {
        if (currentState != GameState.PlayerControl) return;
        StartCoroutine(ResetSequence());
    }

    private IEnumerator PickupAndPrepareSequence(ClickableItem item)
    {
        isBusy = true;
        hand.isDraggable = false;
        targetReached = false;
        ChangeState(GameState.AnimatingToItem);

        bool finishedMove = false;
        hand.MoveTo(item.transform.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        hand.AttachItem(item);

        finishedMove = false;
        ChangeState(GameState.AnimatingToDragPos);
        hand.MoveTo(item.dragStartPosition.position, () => { finishedMove = true; });
        yield return new WaitUntil(() => finishedMove);

        hand.isDraggable = true;
        ChangeState(GameState.PlayerControl); // << Сообщаем UI, что можно показать кнопку
    }

    private IEnumerator ApplyAndReturnSequence()
    {
        ChangeState(GameState.Applying);
        isBusy = true;
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
        ChangeState(GameState.Idle);
    }

    private IEnumerator ResetSequence()
    {
        isBusy = true;
        hand.isDraggable = false;
        ChangeState(GameState.ReturningSequence);

        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator == null)
        {
            Debug.LogError("На объекте руки отсутствует компонент HandAnimator!");
            isBusy = false;
            yield break;
        }

        bool returnFinished = false;
        StartCoroutine(handAnimator.AnimateReturnOnly(() => {
            returnFinished = true;
        }));
        yield return new WaitUntil(() => returnFinished);

        isBusy = false;
        ChangeState(GameState.Idle);
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