using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<GamePhase, GameState> OnPhaseStateChanged;

    [SerializeField] private GamePhase currentPhase;
    private GameState currentState;

    public HandController hand;
    public GameObject acneSprite;

    private bool isBusy = false;
    private bool targetReached = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SwitchToPhase(GamePhase.Acne, true);
    }

    private void ChangeState(GameState newState, GamePhase phaseForEvent)
    {
        currentState = newState;
        OnPhaseStateChanged?.Invoke(phaseForEvent, currentState);
    }

    public void SwitchToPhase(GamePhase newPhase, bool forceChange = false)
    {
        if (!forceChange && (isBusy || currentPhase == newPhase)) return;
        currentPhase = newPhase;
        ChangeState(GameState.Idle, newPhase);
    }

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy || item.itemPhase != currentPhase) return;

        isBusy = true;
        hand.isDraggable = false;
        targetReached = false;
        ChangeState(GameState.AnimatingToItem, currentPhase);

        hand.MoveTo(item.transform.position, () => {
            hand.AttachItem(item);
            hand.MoveTo(item.dragStartPosition.position, () => {
                hand.isDraggable = true;
                isBusy = false;
                ChangeState(GameState.PlayerControl, currentPhase);
            });
        });
    }

    public void OnItemReachedTargetZone() { targetReached = true; }
    public void OnItemLeftTargetZone() { targetReached = false; }

    public void OnDragEnded()
    {
        if (isBusy) return;

        if (targetReached)
        {
            hand.isDraggable = false;
            ApplyAction();
        }
        else
        {
            ClickableItem item = hand.GetAttachedItem();
            if (item != null)
            {
                hand.isDraggable = false;
                hand.MoveTo(item.dragStartPosition.position, () => {
                    hand.isDraggable = true;
                    ChangeState(GameState.PlayerControl, currentPhase);
                });
            }
        }
        targetReached = false;
    }

    public void PerformReset()
    {
        if (currentState != GameState.PlayerControl) return;
        ResetAction();
    }

    private void ApplyAction()
    {
        isBusy = true;
        ChangeState(GameState.Applying, currentPhase);

        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator != null)
        {
            handAnimator.AnimateApplyAndReturn(OnApplySequenceFinished);
        }
        else
        {
            Debug.LogError("HandAnimator не найден!");
            OnApplySequenceFinished();
        }
    }

    private void OnApplySequenceFinished()
    {
        if (currentPhase == GamePhase.Acne)
        {
            if (acneSprite != null) acneSprite.SetActive(false);
            isBusy = false;
            SwitchToPhase(GamePhase.Blush);
        }
        else
        {
            isBusy = false;
            ChangeState(GameState.Idle, currentPhase);
        }
    }

    private void ResetAction()
    {
        isBusy = true;
        hand.isDraggable = false;
        ChangeState(GameState.ReturningSequence, currentPhase);

        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator != null)
        {
            handAnimator.AnimateReturnOnly(() => {
                isBusy = false;
                ChangeState(GameState.Idle, currentPhase);
            });
        }
    }
}