namespace Iceberg.Map.DependencyMapper.Filters;

/// <summary>
/// A generic interface used to defining filters.
/// </summary>
/// <typeparam name="T">The type which is being filtered.</typeparam>
public interface IFilter<T> where T : class
{
    /// <summary>
    /// Filters the input values based on the filtering implementation.
    /// </summary>
    /// <param name="input">The input values of the specified type <typeparamref name="T"/>.</param>
    /// <returns>The filtered results.</returns>
    IEnumerable<T> Filter(IEnumerable<T> input);

    /// <summary>
    /// Returns the <see cref="bool"/> result of the filtering predicate for an individual value.
    /// </summary>
    /// <param name="input">The input value of the specified type <typeparamref name="T"/>.</param>
    /// <returns>The <see cref="bool"/> result of the predicate.</returns>
    bool Predicate(T input);
}