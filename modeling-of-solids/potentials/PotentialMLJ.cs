using System;

namespace modeling_of_solids.potentials;

public class PotentialMLJ : Potential
{
    /// <summary>
    /// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
    /// </summary>
    public double Sigma;

    /// <summary>
    /// Равновесное расстояние между центрами атомов
    /// </summary>
    public double R0 => Sigma * Math.Pow(2, 1.0 / 6.0);

    /// <summary>
    /// Ближний радиус обрезания потенциала.
    /// </summary>
    public double R1 => 1.2 * R0;

    /// <summary>
    /// Дальний радиус обрезания потенциала.
    /// </summary>
    public double R2 => 1.8 * R0;

    /// <summary>
    /// Модуль потенциальной энергии взаимодействия между атомами при равновесии.
    /// </summary>
    public double D;

    /// <summary>
    /// Тип атома.
    /// </summary>
    public override AtomType Type
    {
        get => _type;
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
                    D = 0.00198;
                    Sigma = 0.288;
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

    /// <summary>
    /// Вычисление силы.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override object Force(object[] args)
    {
        var r = (double)args[0];
        var dxdydz = (Vector)args[1];

        return (r < R1) ? Flj(r) * dxdydz : (r > R2) ? Vector.Zero : Flj(r) * dxdydz * K(r);
    }

    /// <summary>
    /// Вычисление потенциала.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override object PotentialEnergy(object[] args)
    {
        var r = (double)args[0];
        return (r < R1) ? Plj(r) : ((r > R2) ? 0 : Plj(r) * K(r));
    }

    /// <summary>
    /// Функция обрезания потенциала.
    /// </summary>
    /// <param name="r">Расстояние между частицами.</param>
    /// <returns></returns>
    private double K(double r) => Math.Pow(1 - (r - R1) * (r - R1) / (R1 - R2) / (R1 - R2), 2);

    /// <summary>
    /// Потенциал Леннарда-Джонса.
    /// </summary>
    /// <param name="r">Расстояние между частицами.</param>
    /// <returns></returns>
    private double Plj(double r)
    {
        var ri = Sigma / r;
        var ri3 = ri * ri * ri;
        var ri6 = ri3 * ri3;

        return 4 * D * ri6 * (ri6 - 1);
    }

    /// <summary>
    /// Cила в потенциале Леннарда-Джонса.
    /// </summary>
    /// <param name="r">Расстояние между частицами.</param>
    /// <returns></returns>
    private double Flj(double r)
    {
        var ri = Sigma / r;
        var ri3 = ri * ri * ri;
        var ri6 = ri3 * ri3;

        return 24 * D * Ev * ri6 * (2 * ri6 - 1) / (r * r);
    }
}