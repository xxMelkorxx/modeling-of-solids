using System;
using System.Collections.Generic;
using modeling_of_solids.potentials;

namespace modeling_of_solids.atomic_model;

public partial class AtomicModel
{
    /// <summary>
    /// Список атомов.
    /// </summary>
    public List<Atom> Atoms { get; set; }

    /// <summary>
    /// Размер расчётной ячейки.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Размер расчётной ячейки (нм).
    /// </summary>
    public double BoxSize { get; set; }

    /// <summary>
    /// Число атомов.
    /// </summary>
    public int CountAtoms => Atoms.Count;

    /// <summary>
    /// Кинетическая энергия системы.
    /// </summary>
    public double Ke { get; set; }

    /// <summary>
    /// Потенциальная энергия системы.
    /// </summary>
    public double Pe { get; set; }

    /// <summary>
    /// Полная энергия системы.
    /// </summary>
    public double Fe => Pe + Ke;

    /// <summary>
    /// Температура системы.
    /// </summary>
    public double T => 2d / 3d * Ke / (Kb / Ev * CountAtoms);

    /// <summary>
    /// Давление системы, рассчитанный через вириал.
    /// </summary>
    public double P1 => (CountAtoms * Kb * T + _virial / (CountAtoms * 3)) / V * 1e9;
    private double _virial;

    /// <summary>
    /// Давление системы.
    /// </summary>
    public double P2 => (Flux.X + Flux.Y + Flux.Z) / (2 * BoxSize * BoxSize) * 1e18 / Dt;

    public Vector Flux;

    /// <summary>
    /// Объём системы.
    /// </summary>
    public double V => BoxSize * BoxSize * BoxSize;

    /// <summary>
    /// Тип атомов.
    /// </summary>
    public AtomType AtomsType { get; set; }

    /// <summary>
    /// Параметр решётки
    /// </summary>
    public double Lattice => AtomsType switch
    {
        AtomType.Ar => 0.526,
        AtomType.Cu => 0.3615,
        AtomType.Fe => 0.2866,
        AtomType.Au => 0.40781,
        AtomType.Si => 0.54307,
        AtomType.Ge => 0.566,
        _ => throw new ArgumentNullException()
    };

    /// <summary>
    /// Масса атома.
    /// </summary>
    public double WeightAtom => AtomsType switch
    {
        AtomType.Ar => 39.948 * 1.66054e-27,
        AtomType.Cu => 63.546 * 1.66054e-27,
        AtomType.Fe => 55.845 * 1.66054e-27,
        AtomType.Au => 196.966 * 1.66054e-27,
        AtomType.Si => 28.086 * 1.66054e-27,
        AtomType.Ge => 72.63 * 1.66054e-27,
        _ => throw new ArgumentNullException()
    };

    /// <summary>
    /// Класс потенциала.
    /// </summary>
    private IPotential _potential;

    // Параметры симуляции.
    /// <summary>
    /// Величина временного шага.
    /// </summary>
    public double Dt { get; set; }

    /// <summary>
    /// Текущий временной шаг.
    /// </summary>
    public int CurrentStep { get; set; }

    // Константы.
    /// <summary>
    /// 1 эВ в Дж с нм.
    /// </summary>
    private const double Ev = 0.1602176634;

    /// <summary>
    /// Постоянная Больцмана (Дж/К с нм).
    /// </summary>
    private const double Kb = 1.380649e-5;

    /// <summary>
    /// Генератор случайных чисел.
    /// </summary>
    private readonly Random _rnd = new(DateTime.Now.Millisecond);

    /// <summary>
    /// Список начальных координат атомов.
    /// </summary>
    private List<Vector> _rt0;

    /// <summary>
    /// Список скоростей атомв в разные моменты времени.
    /// </summary>
    private List<List<Vector>> _vtList;

    /// <summary>
    /// Число отсчётов.
    /// </summary>
    public int CountNumberAcf;

    /// <summary>
    /// Число повторений.
    /// </summary>
    public int CountRepeatAcf;

    /// <summary>
    /// Шаг повторений подсчёта.
    /// </summary>
    public int StepRepeatAcf;

    /// <summary>
    /// Создание атомной модели.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="atomType"></param>
    /// <param name="latticeType"></param>
    /// <param name="potentialType"></param>
    public AtomicModel(int size, AtomType atomType, LatticeType latticeType, PotentialType potentialType)
    {
        Atoms = new List<Atom>();
        AtomsType = atomType;
        Size = size;
        BoxSize = size * Lattice;
        CurrentStep = 1;

        // Выбор типа решётки.
        InitPlacement(latticeType);

        // Выбор типа потенциала.
        InitPotential(potentialType);

        // Начальная инициализация параметров.
        InitCalculation();

        _rt0 = GetPosNpAtoms();
        _vtList = new List<List<Vector>> { GetVelocitiesAtoms() };
    }

    /// <summary>
    /// Инициализация начального размещения атомов.
    /// </summary>
    /// <param name="typeLattice"></param>
    private void InitPlacement(LatticeType typeLattice)
    {
        switch (typeLattice)
        {
            case LatticeType.SC:
                InitPlacementSc();
                break;
            case LatticeType.BCC:
                InitPlacementBcc();
                break;
            case LatticeType.FCC:
                InitPlaсementFcc();
                break;
            case LatticeType.Diamond:
                InitPlaсementDiamond();
                break;
            default: throw new ArgumentNullException();
        }
    }

    /// <summary>
    /// Инициализация потенциала.
    /// </summary>
    public void InitPotential(PotentialType potentialType)
    {
        _potential = potentialType switch
        {
            PotentialType.LJ => new PotentialLJ {Type = AtomsType},
            PotentialType.MLJ => new PotentialMLJ { Type = AtomsType },
            _ => throw new ArgumentNullException()
        };
    }
}