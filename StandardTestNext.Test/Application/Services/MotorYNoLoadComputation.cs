namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYNoLoadComputedPoint
{
    public double U0 { get; init; }
    public double U0DivideUn { get; init; }
    public double U0DivideUnSquare { get; init; }
    public double I0 { get; init; }
    public double P0 { get; init; }
    public double Theta0 { get; init; }
    public double R0 { get; init; }
    public double DeltaI0 { get; init; }
    public double P0cu1 { get; init; }
    public double Pcon { get; init; }
}

internal sealed class MotorYNoLoadComputationResult
{
    public MotorYNoLoadComputedPoint? RatedPoint { get; init; }
    public IReadOnlyList<MotorYNoLoadComputedPoint> AdjustedPoints { get; init; } = Array.Empty<MotorYNoLoadComputedPoint>();
    public IReadOnlyList<MotorYNoLoadComputedPoint> PfwFitSamples { get; init; } = Array.Empty<MotorYNoLoadComputedPoint>();
    public bool PfwFitWindowReady { get; init; }
    public double ComputedTheta0 { get; init; }
    public double ComputedR0 { get; init; }
    public double Pfw { get; init; }
    public double[] CoefficientOfPfe { get; init; } = Array.Empty<double>();
    public double Pfe { get; init; }
    public double FittedI0AtRated { get; init; }
    public double FittedDeltaI0AtRated { get; init; }
    public double FittedP0AtRated { get; init; }
    public double FittedPcuAtRated { get; init; }
}

internal static class MotorYNoLoadComputation
{
    public static MotorYNoLoadComputationResult Compute(
        IReadOnlyList<MotorYNoLoadComputedPoint> points,
        double ratedVoltage,
        int order,
        int decimalPlaces,
        double r1c,
        double theta1c,
        double k1,
        int rConverseType,
        double initialR0 = 0d)
    {
        var computedTheta0 = rConverseType == 1
            ? Math.Round(initialR0 / Math.Max(r1c, 0.0001d) * (k1 + theta1c) - k1, decimalPlaces)
            : Math.Round(points.Max(x => x.Theta0), decimalPlaces);
        var computedR0 = rConverseType == 1
            ? Math.Round(initialR0, decimalPlaces)
            : Math.Round(r1c * (k1 + computedTheta0) / (k1 + theta1c), decimalPlaces);

        var adjustedPoints = points
            .Select(x =>
            {
                var ratio = ratedVoltage == 0 ? 0d : Math.Round(x.U0 / ratedVoltage, decimalPlaces);
                var ratioSquare = Math.Round(Math.Pow(ratio, 2), decimalPlaces);
                var pointR0 = Math.Round(r1c * (k1 + x.Theta0) / (k1 + theta1c), decimalPlaces);
                var p0cu1 = Math.Round(1.5 * pointR0 * x.I0 * x.I0, decimalPlaces);
                var pcon = Math.Round(x.P0 - p0cu1, decimalPlaces);
                return new MotorYNoLoadComputedPoint
                {
                    U0 = Math.Round(x.U0, decimalPlaces),
                    U0DivideUn = ratio,
                    U0DivideUnSquare = ratioSquare,
                    I0 = Math.Round(x.I0, decimalPlaces),
                    P0 = Math.Round(x.P0, decimalPlaces),
                    Theta0 = Math.Round(x.Theta0, decimalPlaces),
                    R0 = pointR0,
                    DeltaI0 = Math.Round(x.DeltaI0, decimalPlaces),
                    P0cu1 = p0cu1,
                    Pcon = pcon
                };
            })
            .ToArray();

        var ratedPoint = adjustedPoints
            .OrderBy(x => Math.Abs(x.U0DivideUn - 1d))
            .ThenBy(x => Math.Abs(x.U0 - ratedVoltage))
            .FirstOrDefault();

        var pfwFitSamples = adjustedPoints.Where(x => x.U0DivideUn < 0.51).ToArray();
        var pfwFitWindowReady = pfwFitSamples.Length >= 2;
        var pfw = pfwFitWindowReady
            ? Math.Round(FitLineIntercept(
                pfwFitSamples.Select(x => x.U0DivideUnSquare).ToArray(),
                pfwFitSamples.Select(x => x.Pcon).ToArray()), decimalPlaces)
            : 0d;

        var pfeValues = adjustedPoints
            .Select(x => Math.Round(Math.Max(0d, x.Pcon - pfw), decimalPlaces))
            .ToArray();
        var coefficientOfPfe = FitPolynomialCoefficients(
            adjustedPoints.Select(x => x.U0DivideUn).ToArray(),
            pfeValues,
            Math.Min(order, Math.Max(0, adjustedPoints.Length - 1)));
        var pfe = ratedPoint is null
            ? 0d
            : Math.Round(EvaluatePolynomial(coefficientOfPfe, 1d), decimalPlaces);

        var i0Coefficients = FitPolynomialCoefficients(
            adjustedPoints.Select(x => x.U0DivideUn).ToArray(),
            adjustedPoints.Select(x => x.I0).ToArray(),
            Math.Min(order, Math.Max(0, adjustedPoints.Length - 1)));
        var deltaI0Coefficients = FitPolynomialCoefficients(
            adjustedPoints.Select(x => x.U0DivideUn).ToArray(),
            adjustedPoints.Select(x => x.DeltaI0).ToArray(),
            Math.Min(order, Math.Max(0, adjustedPoints.Length - 1)));
        var p0Coefficients = FitPolynomialCoefficients(
            adjustedPoints.Select(x => x.U0DivideUn).ToArray(),
            adjustedPoints.Select(x => x.P0).ToArray(),
            Math.Min(order, Math.Max(0, adjustedPoints.Length - 1)));
        var pcuCoefficients = FitPolynomialCoefficients(
            adjustedPoints.Select(x => x.U0DivideUn).ToArray(),
            adjustedPoints.Select(x => x.P0cu1).ToArray(),
            Math.Min(order, Math.Max(0, adjustedPoints.Length - 1)));

        var fittedI0AtRated = ratedPoint is null ? 0d : Math.Round(EvaluatePolynomial(i0Coefficients, 1d), decimalPlaces);
        var fittedDeltaI0AtRated = ratedPoint is null ? 0d : Math.Round(EvaluatePolynomial(deltaI0Coefficients, 1d), decimalPlaces);
        var fittedP0AtRated = ratedPoint is null ? 0d : Math.Round(EvaluatePolynomial(p0Coefficients, 1d), decimalPlaces);
        var fittedPcuAtRated = ratedPoint is null ? 0d : Math.Round(EvaluatePolynomial(pcuCoefficients, 1d), decimalPlaces);

        return new MotorYNoLoadComputationResult
        {
            RatedPoint = ratedPoint,
            AdjustedPoints = adjustedPoints,
            PfwFitSamples = pfwFitSamples,
            PfwFitWindowReady = pfwFitWindowReady,
            ComputedTheta0 = computedTheta0,
            ComputedR0 = computedR0,
            Pfw = pfw,
            CoefficientOfPfe = coefficientOfPfe,
            Pfe = pfe,
            FittedI0AtRated = fittedI0AtRated,
            FittedDeltaI0AtRated = fittedDeltaI0AtRated,
            FittedP0AtRated = fittedP0AtRated,
            FittedPcuAtRated = fittedPcuAtRated
        };
    }

    public static double FitLineIntercept(double[] xValues, double[] yValues)
    {
        if (xValues.Length == 0 || yValues.Length == 0 || xValues.Length != yValues.Length)
        {
            return 0d;
        }

        if (xValues.Length == 1)
        {
            return yValues[0];
        }

        var xMean = xValues.Average();
        var yMean = yValues.Average();
        var denominator = xValues.Sum(x => Math.Pow(x - xMean, 2));
        if (Math.Abs(denominator) < 1e-12)
        {
            return yMean;
        }

        var numerator = xValues
            .Select((x, index) => (x - xMean) * (yValues[index] - yMean))
            .Sum();
        var slope = numerator / denominator;
        return yMean - slope * xMean;
    }

    public static double[] FitPolynomialCoefficients(double[] xValues, double[] yValues, int order)
    {
        if (xValues.Length == 0 || yValues.Length == 0 || xValues.Length != yValues.Length)
        {
            return new[] { 0d };
        }

        var degree = Math.Max(0, Math.Min(order, xValues.Length - 1));
        var size = degree + 1;
        var matrix = new double[size, size];
        var vector = new double[size];

        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                matrix[row, col] = xValues.Sum(x => Math.Pow(x, row + col));
            }

            vector[row] = xValues
                .Select((x, index) => yValues[index] * Math.Pow(x, row))
                .Sum();
        }

        return SolveLinearSystem(matrix, vector);
    }

    public static double EvaluatePolynomial(IReadOnlyList<double> coefficients, double x)
    {
        double sum = 0d;
        for (var i = 0; i < coefficients.Count; i++)
        {
            sum += coefficients[i] * Math.Pow(x, i);
        }

        return sum;
    }

    private static double[] SolveLinearSystem(double[,] matrix, double[] vector)
    {
        var size = vector.Length;
        var augmented = new double[size, size + 1];
        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                augmented[row, col] = matrix[row, col];
            }

            augmented[row, size] = vector[row];
        }

        for (var pivot = 0; pivot < size; pivot++)
        {
            var maxRow = pivot;
            for (var row = pivot + 1; row < size; row++)
            {
                if (Math.Abs(augmented[row, pivot]) > Math.Abs(augmented[maxRow, pivot]))
                {
                    maxRow = row;
                }
            }

            if (Math.Abs(augmented[maxRow, pivot]) < 1e-12)
            {
                continue;
            }

            if (maxRow != pivot)
            {
                for (var col = pivot; col <= size; col++)
                {
                    (augmented[pivot, col], augmented[maxRow, col]) = (augmented[maxRow, col], augmented[pivot, col]);
                }
            }

            var pivotValue = augmented[pivot, pivot];
            for (var col = pivot; col <= size; col++)
            {
                augmented[pivot, col] /= pivotValue;
            }

            for (var row = 0; row < size; row++)
            {
                if (row == pivot)
                {
                    continue;
                }

                var factor = augmented[row, pivot];
                for (var col = pivot; col <= size; col++)
                {
                    augmented[row, col] -= factor * augmented[pivot, col];
                }
            }
        }

        var result = new double[size];
        for (var row = 0; row < size; row++)
        {
            result[row] = Math.Round(augmented[row, size], 8, MidpointRounding.AwayFromZero);
        }

        return result;
    }
}
