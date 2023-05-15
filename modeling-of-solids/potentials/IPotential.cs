namespace modeling_of_solids.potentials;

public enum PotentialType
{
    LJ,
    MLJ
}

public interface IPotential
{
    /// <summary>
    /// 1 эВ в Дж.
    /// </summary>
    public const double Ev = 1.602176634e-19;

    /// <summary>
    /// Межатомная сила взаимодействия в потенциале (Дж * м).
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public object Force(object[] args);

    /// <summary>
    /// Потенциальная энергия двух атомов (Дж).
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public object PotentialEnergy(object[] args);
}