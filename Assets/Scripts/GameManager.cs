using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<GamePhase, GameState> OnPhaseStateChanged;

    [Header("Состояние игры")]
    [SerializeField] private GamePhase currentPhase;
    private GameState currentState;

    [Header("Основные ссылки")]
    public HandController hand;
    public GameObject acneSprite;

    // --- ВОТ ПРАВИЛЬНЫЙ БЛОК УПРАВЛЕНИЯ ВКЛАДКАМИ ---
    [Header("Управление вкладками")]
    [Tooltip("Объект, представляющий вкладку с кремом и ее контент")]
    public GameObject creamTab;
    [Tooltip("Контейнер для всех вкладок макияжа (румяна, помада и т.д.)")]
    public GameObject makeupTabsContainer;
    [Tooltip("Список переключателей для вкладок ВНУТРИ контейнера макияжа")]
    public List<TabsSwitcher> makeupTabs;
    // ----------------------------------------------------

    [Header("Базы данных и Зоны")]
    public MakeupDatabase makeupDatabase;

    [Header("Объекты для фазы Румян")]
    public BrushTool blushBrush;
    public Transform blushApplyPositionMarker;

    private bool isBusy = false;
    private bool targetReached = false;
    private ColorSource activeColorSource;

    void Awake()
    {
        Instance = this;
        if (makeupDatabase != null)
        {
            foreach (var mapping in makeupDatabase.blushMappings)
            {
                if (mapping.faceObject != null)
                {
                    mapping.faceObject.SetActive(false);
                }
            }
        }
    }

    void Start()
    {
        SwitchToPhase(GamePhase.Acne, true);
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

    // --- ВОТ ПРАВИЛЬНЫЙ МЕТОД УПРАВЛЕНИЯ ГИБРИДНОЙ СИСТЕМОЙ ВКЛАДОК ---
    private void UpdateTabsVisual(GamePhase activePhase)
    {
        // 1. Переключаем видимость главных контейнеров
        bool isCreamPhase = (activePhase == GamePhase.Acne);
        if (creamTab != null) creamTab.SetActive(isCreamPhase);
        if (makeupTabsContainer != null) makeupTabsContainer.SetActive(!isCreamPhase);

        // 2. Если мы в фазе макияжа, обновляем состояние вкладок внутри контейнера
        if (!isCreamPhase)
        {
            foreach (var tab in makeupTabs)
            {
                bool isActive = (tab.phase == activePhase);
                tab.UpdateVisual(isActive);
            }
        }
    }
    // --------------------------------------------------------------------

    public void OnItemClicked(ClickableItem item)
    {
        if (isBusy || item.itemPhase != currentPhase) return;
        isBusy = true;
        hand.isDraggable = false;
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
            if (currentPhase == GamePhase.Acne)
            {
                ApplyAction();
            }
        }
        else
        {
            if (hand.GetAttachedItem() != null)
            {
                hand.GetComponent<HandAnimator>().AnimateItemReturn(() => {
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
            handAnimator.AnimateCreamApplication(OnApplySequenceFinished);
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
            handAnimator.AnimateItemReturn(() => {
                isBusy = false;
                ChangeState(GameState.Idle, currentPhase);
            });
        }
    }

    public void OnBlushColorSelected(ColorSource selectedColor)
    {
        if (isBusy || currentPhase != GamePhase.Blush) return;
        isBusy = true;
        activeColorSource = selectedColor;
        ChangeState(GameState.Applying, currentPhase);
        hand.MoveTo(blushBrush.gripPoint.position, () =>
        {
            hand.AttachTool(blushBrush);
            HandAnimator handAnimator = hand.GetComponent<HandAnimator>();
            handAnimator.AnimateBlushPickup(selectedColor.transform.position, () => {
                blushBrush.SetTipColor(selectedColor.itemColor);

                // --- ИЗМЕНЕННЫЙ ВЫЗОВ ---
                handAnimator.AnimateBlushApplication(
                    blushApplyPositionMarker.position,
                    // 1. Что делать СРАЗУ ПОСЛЕ нанесения:
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
                    // 2. Что делать ПОСЛЕ ВОЗВРАТА кисточки:
                    () => {
                        activeColorSource = null;
                        isBusy = false;
                        ChangeState(GameState.Idle, currentPhase);
                    }
                );
            });
        });
    }
}