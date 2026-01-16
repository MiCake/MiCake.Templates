namespace StandardWeb.Common.Time;

/// <summary>
/// Represents a range between two DateTime values.
/// </summary>
public class DateTimeRange
{
    /// <summary>
    /// The start date of the range.
    /// </summary>
    public required DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the range.
    /// </summary>
    public required DateTime EndDate { get; set; }

    public DateTimeRange(DateTime startDate, DateTime endDate)
    {       
        if (endDate < startDate)
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
    }
}
