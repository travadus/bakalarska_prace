using UnityEngine;

public class TimePresenter : MonoBehaviour
{
    [Header("View Reference")]
    [SerializeField] private TimeView view;

    // Instance Modelu
    private TimeModel _model;

    // Rychlost simulace (lze mìnit v editoru nebo pøes UI)
    [Range(0, 10)]
    public float simulationSpeed = 1f;

    private void Awake()
    {
        // Inicializace modelu
        _model = new TimeModel();

        // Zde se pozdìji pøihlásíš k odbìru tickù pro simulaci sítì
        _model.OnHourTick += HandleHourlyTick;
    }

    private void Update()
    {
        if (view == null) return;

        // 1. Posunout èas v modelu
        _model.AdvanceTime(Time.deltaTime * simulationSpeed);

        // 2. Aktualizovat View
        view.UpdateVisuals(
            _model.GetNormalizedTime(),
            _model.CurrentDay,
            _model.CurrentHour
        );
    }

    // Tady budeš pozdìji volat logiku sítì (Grid logic)
    private void HandleHourlyTick(int day, int hour)
    {
        // Pøíklad:
        // gridManager.CalculateEconomy(hour);
        // dataLogger.LogData(day, hour);
    }
}