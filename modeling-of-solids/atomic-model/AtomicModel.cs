using System;
using System.Collections.Generic;
using modeling_of_solids.potentials;

namespace modeling_of_solids.atomic_model;

public partial class AtomicModel
{
    /// <summary>
    /// Список атомов.
    /// </summary>
    public List<Atom> Atoms { get; }

    /// <summary>
    /// Размер расчётной ячейки.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Размер расчётной ячейки (м).
    /// </summary>
    public double BoxSize => Size * Lattice;

    /// <summary>
    /// Число атомов.
    /// </summary>
    public int CountAtoms => Atoms.Count;

    /// <summary>
    /// Кинетическая энергия системы (эВ).
    /// </summary>
    public double Ke => _ke / Ev;

    private double _ke;

    /// <summary>
    /// Потенциальная энергия системы (эВ).
    /// </summary>
    public double Pe => _pe / Ev;

    private double _pe;

    /// <summary>
    /// Полная энергия системы (эВ).
    /// </summary>
    public double Fe => (_pe + _ke) / Ev;

    /// <summary>
    /// Температура системы.
    /// </summary>
    public double T => 2 * _ke / (3 * Kb * CountAtoms);

    /// <summary>
    /// Давление системы, рассчитанный через вириал (Па).
    /// </summary>
    public double P1 => (_ke + _virial / CountAtoms) / (3 * V);

    private double _virial;

    /// <summary>
    /// Давление системы (Па).
    /// </summary>
    public double P2 => (Flux.X + Flux.Y + Flux.Z) / (6 * BoxSize * BoxSize * Dt);

    public Vector Flux;

    /// <summary>
    /// Объём системы (м³).
    /// </summary>
    public double V => BoxSize * BoxSize * BoxSize;

    /// <summary>
    /// Тип атомов.
    /// </summary>
    public AtomType AtomsType { get; }

    /// <summary>
    /// Параметр решётки (м)
    /// </summary>
    public double Lattice => AtomsType switch
    {
        AtomType.Ar => 0.526e-9,
        AtomType.Cu => 0.3615e-9,
        AtomType.Fe => 0.2866e-9,
        AtomType.Au => 0.40781e-9,
        AtomType.Si => 0.54307e-9,
        AtomType.Ge => 0.566e-9,
        _ => throw new ArgumentNullException()
    };

    /// <summary>
    /// Масса атома (кг).
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
    /// Величина временного шага (c).
    /// </summary>
    public double Dt { get; set; }

    /// <summary>
    /// Текущий временной шаг.
    /// </summary>
    public int CurrentStep { get; set; }

    // Константы.
    /// <summary>
    /// 1 эВ в Дж.
    /// </summary>
    private const double Ev = 1.602176634e-19;

    /// <summary>
    /// Постоянная Больцмана (Дж/К).
    /// </summary>
    private const double Kb = 1.380649e-23;

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
            PotentialType.LJ => new PotentialLJ { Type = AtomsType },
            PotentialType.MLJ => new PotentialMLJ { Type = AtomsType },
            _ => throw new ArgumentNullException()
        };
    }
}