namespace modeling_of_solids.potentials;

public enum PotentialType
{
    LJ,
    MLJ
}

public interface IPotential
{
    /// <summary>
    /// 1 эВ в Дж с нм.
    /// </summary>
    public const double Ev = 0.1602176634;

    /// <summary>
    /// Постоянная Больцмана (эВ/К).
    /// </summary>
    public const double Kb = 8.61733262e-5;

    /// <summary>
    /// Межатомная сила взаимодействия в потенциале.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public object Force(object[] args);

    /// <summary>
    /// Потенциальная энергия двух атомов.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public object PotentialEnergy(object[] args);
}