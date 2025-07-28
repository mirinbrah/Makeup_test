using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Интерактивные вкладки")]
    public GameObject creamTabVisual; 
    public GameObject makeupTabsContainer; 
    public List<TabsSwitcher> makeupTabs;

    [Header("Объекты для фазы Румян")]
    public BrushTool blushBrush;

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
        UpdateTabsVisual(newPhase);
    }

    private void UpdateTabsVisual(GamePhase activePhase)
    {
        bool isCreamPhase = (activePhase == GamePhase.Acne);

        if (creamTabVisual != null)
        {
            creamTabVisual.SetActive(isCreamPhase);
        }
        if (makeupTabsContainer != null)
        {
            makeupTabsContainer.SetActive(!isCreamPhase);
        }
        if (!isCreamPhase)
        {
            foreach (var tabActivator in makeupTabs)
            {
                bool isActive = (tabActivator.phase == activePhase);
                tabActivator.UpdateVisual(isActive);
            }
        }
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

    public void OnBlushColorSelected(ColorSource selectedColor)
    {
        // Проверяем, что мы в нужной фазе и не заняты другой анимацией
        if (isBusy || currentPhase != GamePhase.Blush || selectedColor.itemPhase != GamePhase.Blush)
        {
            return;
        }

        Debug.Log("Начинаем последовательность для румян. Выбран цвет: " + selectedColor.itemColor);
        isBusy = true;
        hand.isDraggable = false;
        ChangeState(GameState.AnimatingToItem, currentPhase); // Меняем состояние на "анимация"

        // --- НАЧАЛО ПОСЛЕДОВАТЕЛЬНОСТИ ДЕЙСТВИЙ ---

        // Шаг 1: Рука двигается к кисточке
        hand.MoveTo(blushBrush.gripPoint.position, () =>
        {
            // Шаг 2: Рука "берет" кисточку
            hand.AttachTool(blushBrush);
            Debug.Log("Кисточка взята.");

            // --- ЗДЕСЬ ПОКА ОСТАНОВИМСЯ ---
            // В будущем здесь будет продолжение: движение к палетке, потом к лицу.
            // Пока что мы просто завершаем последовательность для проверки.
            isBusy = false;
            // Можно, например, сразу перевести руку в состояние PlayerControl для теста
            // hand.isDraggable = true;
            // ChangeState(GameState.PlayerControl, currentPhase);

            // Или просто вернуть в состояние Idle
            ChangeState(GameState.Idle, currentPhase);
            Debug.Log("Первая часть последовательности завершена.");
        });
    }
}