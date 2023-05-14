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
                    D = 0.01029;
                    Sigma = 0.3408;
                    break;
                case AtomType.Cu:
                    D = 0.00102;
                    Sigma = 0.34635;
                    break;
                case AtomType.Fe:
                    D = 0.00172;
                    Sigma = 0.25;
                    break;
                case AtomType.Au:
                    D = 2.3058;
                    Sigma = 0.484648;
                    break;
                case AtomType.Si:
                    D = 0.00916;
                    Sigma = 0.372;
                    break;
                case AtomType.Ge:
                    D = 0.0053;
                    Sigma = 0.378;
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
        
        // var ri = Sigma / r;
        // var ri3 = ri * ri * ri;
        // var ri6 = ri3 * ri3;
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
        
        // var ri = Sigma / r;
        // var ri3 = ri * ri * ri;
        // var ri6 = ri3 * ri3;
        var ri2 = Sigma * Sigma / r2;
        var ri6 = ri2 * ri2 * ri2;

        // return 24 * D * Ev * ri6 * (2 * ri6 - 1) / (r * r);
        return 24 * D * IPotential.Ev * ri6 * (2 * ri6 - 1) / r2;
    }
}