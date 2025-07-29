using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
    public Transform creamApplyPositionMarker;

    [Header("Объекты для фазы Румян")]
    public BrushTool blushBrush;

    [Header("Объекты для фазы Теней")]
    public BrushTool eyeshadowBrush;

    [Header("Объекты для фазы Помады")]
    public List<ClickableItem> lipsticks;

    private GamePhase currentPhase;
    private GameState currentState;

    private bool isBusy = false;
    private bool targetReached = false;
    private ColorSource activeColorSource;
    private ClickableItem activeItem;
    private HandAnimator handAnimator;

    void Awake()
    {
        Instance = this;
        if (hand != null)
        {
            handAnimator = hand.GetComponent<HandAnimator>();
        }
    }

    void Start()
    {
        if (eventSystem == null)
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }
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
        bool isHandHoldingSomething = (activeItem != null || hand.GetAttachedTool() != null);
        if (!forceChange && (isBusy || currentPhase == newPhase || isHandHoldingSomething))
        {
            return;
        }

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
        if (isBusy) return;

        if (activeItem != null && activeItem != item)
        {
            SetBusyState(true);
            hand.SetOrderActive(true);
            handAnimator.AnimateItemReturn(
                () => {
                    activeItem = null;
                },
                () => {
                    hand.SetOrderActive(false);
                    TakeItem(item);
                }
            );
            return;
        }

        if (activeItem == null)
        {
            TakeItem(item);
        }
    }

    private void TakeItem(ClickableItem item)
    {
        if (item.itemPhase != currentPhase) return;

        SetBusyState(true);
        hand.SetOrderActive(true);
        hand.isDraggable = false;
        activeItem = item;
        ChangeState(GameState.AnimatingToItem, currentPhase);

        hand.MoveTo(item.gripPoint.position, () => {
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
            else if (currentPhase == GamePhase.Lipstick)
            {
                ApplyLipstick();
            }
        }
        else
        {
            if (hand.GetAttachedItem() != null)
            {
                SetBusyState(true);
                hand.SetOrderActive(true); 
                handAnimator.AnimateItemReturn(
                    () => {
                        activeItem = null;
                        hand.isDraggable = false;
                        ChangeState(GameState.Idle, currentPhase);
                    },
                    () => {
                        hand.SetOrderActive(false);
                        SetBusyState(false);
                    }
                );
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
        if (handAnimator != null)
        {
            handAnimator.AnimateItemApplication(
                GamePhase.Acne,
                () => {
                    if (acneSprite != null) acneSprite.SetActive(false);
                },
                () => {
                    activeItem = null;
                    hand.SetOrderActive(false);
                    SetBusyState(false);
                    SwitchToPhase(GamePhase.Blush);
                }
            );
        }
    }

    private void ApplyLipstick()
    {
        if (activeItem == null) return;

        SetBusyState(true);
        ChangeState(GameState.Applying, currentPhase);

        handAnimator.AnimateItemApplication(
            GamePhase.Lipstick,
            () => {
                if (makeupDatabase != null)
                {
                    GameObject faceObject = makeupDatabase.GetFaceObjectFor(activeItem);
                    if (faceObject != null)
                    {
                        faceObject.SetActive(true);
                    }
                }
            },
            () => {
                activeItem = null;
                hand.SetOrderActive(false);
                SetBusyState(false);
                ChangeState(GameState.Idle, currentPhase);
            }
        );
    }

    private void ResetAction()
    {
        SetBusyState(true);
        hand.SetOrderActive(true);
        hand.isDraggable = false;
        ChangeState(GameState.ReturningSequence, currentPhase);
        if (handAnimator != null)
        {
            handAnimator.AnimateItemReturn(
                () => {
                    activeItem = null;
                    ChangeState(GameState.Idle, currentPhase);
                },
                () => {
                    hand.SetOrderActive(false);
                    SetBusyState(false);
                }
            );
        }
    }

    public void OnMakeupColorSelected(ColorSource selectedColor)
    {
        if (isBusy || currentPhase != selectedColor.itemPhase) return;

        BrushTool currentBrush = null;

        if (currentPhase == GamePhase.Blush)
        {
            currentBrush = blushBrush;
        }
        else if (currentPhase == GamePhase.Eyeshadow)
        {
            currentBrush = eyeshadowBrush;
        }

        if (currentBrush == null)
        {
            return;
        }

        SetBusyState(true);
        hand.SetOrderActive(true);
        activeColorSource = selectedColor;
        ChangeState(GameState.Applying, currentPhase);

        hand.MoveTo(currentBrush.gripPoint.position, () =>
        {
            hand.AttachTool(currentBrush);

            handAnimator.AnimateMakeupPickup(selectedColor.transform.position, () => {

                currentBrush.SetTipColor(selectedColor.itemColor);

                handAnimator.AnimateMakeupApplication(
                    currentPhase,
                    () => {
                        if (makeupDatabase != null)
                        {
                            GameObject faceObject = makeupDatabase.GetFaceObjectFor(activeColorSource, currentPhase);
                            if (faceObject != null)
                            {
                                faceObject.SetActive(true);
                            }
                        }
                    },
                    () => {
                        activeColorSource = null;
                        hand.SetOrderActive(false);
                        SetBusyState(false);
                        ChangeState(GameState.Idle, currentPhase);
                    }
                );
            });
        });
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}