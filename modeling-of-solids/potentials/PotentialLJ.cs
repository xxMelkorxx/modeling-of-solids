using System;

namespace modeling_of_solids.potentials;

public class PotentialLJ : IPotential
{
    /// <summary>
    /// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
    /// </summary>
    public double D;

    /// <summary>
    /// Расстояние между центрами атомов при котором U(sigma) = 0.
    /// </summary>
    public double Sigma;

    /// <summary>
    /// Тип атома.
    /// </summary>
    public AtomType Type
    {
        set
        {
            _type = value;
            switch (_type)
            {
                case AtomType.Ar:
                    D = 0.01029 * IPotential.Ev;
                    Sigma = 0.3408e-9;
                    break;
                case AtomType.Cu:
                    D = 0.8176 * IPotential.Ev;
                    Sigma = 0.2893e-9;
                    break;
                case AtomType.Fe:
                    D = 0.7864 * IPotential.Ev;
                    Sigma = 0.2745e-9;
                    break;
                case AtomType.Au:
                    D = 0.1791 * IPotential.Ev;
                    Sigma = 0.288e-9;
                    break;
                case AtomType.Si:
                    D = 0.00916 * IPotential.Ev;
                    Sigma = 0.372e-9;
                    break;
                case AtomType.Ge:
                    D = 0.0053 * IPotential.Ev;
                    Sigma = 0.378e-9;
                    break;
                default: throw new NullReferenceException();
            }
        }
    }

    private AtomType _type;

    public object Force(object[] args) => Flj((double)args[0]) * (Vector)args[1];

    public object PotentialEnergy(object[] args) => Plj((double)args[0]);

    /// <summary>
    /// Потенциал Леннарда-Джонса.
    /// </summary>
    /// <param name="r2">Расстояние между частицами.</param>
    /// <returns></returns>
    private double Plj(double r2)
    {
        if (r2 == 0)
            throw new DivideByZeroException();
        
        var ri2 = Sigma * Sigma / r2;
        var ri6 = ri2 * ri2 * ri2;

        return 4 * D * ri6 * (ri6 - 1);
    }

    /// <summary>
    /// Cила в потенциале Леннарда-Джонса.
    /// </summary>
    /// <param name="r2">Расстояние между частицами.</param>
    /// <returns></returns>
    private double Flj(double r2)
    {
        if (r2 == 0)
            throw new DivideByZeroException();
        
        var ri2 = Sigma * Sigma / r2;
        var ri6 = ri2 * ri2 * ri2;

        return 24 * D * ri6 * (2 * ri6 - 1) / r2;
    }
}