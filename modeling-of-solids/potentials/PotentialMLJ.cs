﻿using System;

namespace modeling_of_solids.potentials;

public class PotentialMLJ : IPotential
{
    /// <summary>
    /// Модуль потенциальной энергии взаимодействия между атомами при равновесии (м).
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
    /// Модуль потенциальной энергии взаимодействия между атомами при равновесии (Дж).
    /// </summary>
    public double D;

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

    public object Force(object[] args)
    {
        var r = (double)args[0];
        var dxdydz = (Vector)args[1];

        return (r < R1) ? Flj(r) * dxdydz : (r > R2) ? Vector.Zero : Flj(r) * dxdydz * K(r);
    }

    public object PotentialEnergy(object[] args)
    {
        var r2 = (double)args[0];
        return r2 < R1 ? Plj(r2) : r2 > R2 ? 0 : Plj(r2) * K(double.Sqrt(r2));
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