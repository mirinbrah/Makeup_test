using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<GamePhase, GameState> OnPhaseStateChanged;

    [Header("Основные ссылки")]
    public HandController hand;
    public MakeupDatabase makeupDatabase;
    public EventSystem eventSystem;

    [Header("Управление вкладками")]
    public GameObject creamTab;
    public GameObject makeupTabsContainer;
    public List<TabsSwitcher> makeupTabs;

    [Header("Сброс макияжа")]
    public List<Transform> makeupContainers;

    [Header("Объекты для фазы Крема")]
    public GameObject acneSprite;

    [Header("Объекты для фазы Румян")]
    public BrushTool blushBrush;
    public Transform blushApplyPositionMarker;

    private GamePhase currentPhase;
    private GameState currentState;

    private bool isBusy = false;
    private bool targetReached = false;
    private ColorSource activeColorSource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SwitchToPhase(GamePhase.Acne, true);
    }

    private void SetBusyState(bool busy)
    {
        isBusy = busy;
        if (eventSystem != null)
        {
            eventSystem.enabled = !busy;
        }
    }

    public void RemoveAllMakeup()
    {
        if (isBusy) return;

        foreach (Transform container in makeupContainers)
        {
            if (container != null)
            {
                foreach (Transform makeupChild in container)
                {
                    makeupChild.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ChangeState(GameState newState, GamePhase phaseForEvent)
    {
        currentState = newState;
        OnPhaseStateChanged?.Invoke(phaseForEvent, currentState);
    }

    public void SwitchToPhase(GamePhase newPhase, bool forceChange = false)
    {
        if (!forceChange && (isBusy || currentPhase == newPhase)) return;

        currentPhase = newPhase;
        ChangeState(GameState.Idle, newPhase);
        UpdateTabsVisual(newPhase);
    }

    private void UpdateTabsVisual(GamePhase activePhase)
    {
        bool isCreamPhase = (activePhase == GamePhase.Acne);
        if (creamTab != null) creamTab.SetActive(isCreamPhase);
        if (makeupTabsContainer != null) makeupTabsContainer.SetActive(!isCreamPhase);

        if (!isCreamPhase)
        {
            foreach (var tab in makeupTabs)
            {
                bool isActive = (tab.phase == activePhase);
                tab.UpdateVisual(isActive);
            }
        }
    }

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy || item.itemPhase != currentPhase) return;
        SetBusyState(true);
        hand.isDraggable = false;
        ChangeState(GameState.AnimatingToItem, currentPhase);
        hand.MoveTo(item.transform.position, () => {
            hand.AttachItem(item);
            hand.MoveTo(item.dragStartPosition.position, () => {
                hand.isDraggable = true;
                SetBusyState(false);
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
            if (currentPhase == GamePhase.Acne)
            {
                ApplyAction();
            }
        }
        else
        {
            if (hand.GetAttachedItem() != null)
            {
                SetBusyState(true);
                hand.GetComponent<HandAnimator>().AnimateItemReturn(() => {
                    hand.isDraggable = true;
                    SetBusyState(false);
                    ChangeState(GameState.PlayerControl, currentPhase);
                });
            }
        }
        targetReached = false;
    }

    public void PerformReset()
    {
        if (isBusy || currentState != GameState.PlayerControl) return;
        ResetAction();
    }

    private void ApplyAction()
    {
        SetBusyState(true);
        ChangeState(GameState.Applying, currentPhase);
        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator != null)
        {
            handAnimator.AnimateCreamApplication(OnApplySequenceFinished);
        }
    }

    private void OnApplySequenceFinished()
    {
        if (currentPhase == GamePhase.Acne)
        {
            if (acneSprite != null) acneSprite.SetActive(false);
            SetBusyState(false);
            SwitchToPhase(GamePhase.Blush);
        }
        else
        {
            SetBusyState(false);
            ChangeState(GameState.Idle, currentPhase);
        }
    }

    private void ResetAction()
    {
        SetBusyState(true);
        hand.isDraggable = false;
        ChangeState(GameState.ReturningSequence, currentPhase);
        HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
        if (handAnimator != null)
        {
            handAnimator.AnimateItemReturn(() => {
                SetBusyState(false);
                ChangeState(GameState.Idle, currentPhase);
            });
        }
    }

    public void OnBlushColorSelected(ColorSource selectedColor)
    {
        if (isBusy || currentPhase != GamePhase.Blush) return;
        SetBusyState(true);
        activeColorSource = selectedColor;
        ChangeState(GameState.Applying, currentPhase);
        hand.MoveTo(blushBrush.gripPoint.position, () =>
        {
            hand.AttachTool(blushBrush);
            HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
            handAnimator.AnimateBlushPickup(selectedColor.transform.position, () => {
                blushBrush.SetTipColor(selectedColor.itemColor);

                handAnimator.AnimateBlushApplication(
                    blushApplyPositionMarker.position,
                    () => {
                        if (makeupDatabase != null)
                        {
                            GameObject faceObject = makeupDatabase.GetFaceObjectFor(activeColorSource);
                            if (faceObject != null)
                            {
                                faceObject.SetActive(true);
                            }
                        }
                    },
                    () => {
                        activeColorSource = null;
                        SetBusyState(false);
                        ChangeState(GameState.Idle, currentPhase);
                    }
                );
            });
        });
    }
}